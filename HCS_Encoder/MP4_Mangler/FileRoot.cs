using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace MP4_Mangler {
	/// <summary>
	/// Utility object for building useful MP4f files and file parts.
	/// This is not thread-safe!
	/// </summary>
	public class FileRoot {

		public List<MediaStream> KnownStreams { get; private set; }
		public FileInfo ActiveFile { get; private set; }
		public int FragmentNumber { get; private set; }

		#region Creation and setup
		/// <summary>
		/// Create a new file root with the given streams.
		/// Frame data will be cached until StartFile() is called.
		/// </summary>
		/// <remarks>Once a file is started, the tracks and streams can't be changed.
		/// If you know the tracks in advance, it uses less memory to start the file as early as possible.
		/// If you don't know the tracks in advance, don't StartFile() until all tracks are known.
		/// It is possible to Write all frames in all tracks then StartFile() and EndFile() in direct succession.</remarks>
		/// <param name="MediaStreams">a list or array of streams to handle</param>
		public FileRoot (params MediaStream[] MediaStreams) {
			KnownStreams = new List<MediaStream>(MediaStreams);
			ActiveFile = null;
			FragmentNumber = 1;
		}

		/// <summary>
		/// Start an MP4f file. After a sucessful call, all cached data will be sent to
		/// this file, and frame data will no longer be cached on subsequent WriteStream() calls.
		/// </summary>
		public void StartFile (string FilePath) {
			ActiveFile = new FileInfo(FilePath);

			// write filespec & headers
			using (BinaryWriter wr = new BinaryWriter(ActiveFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read))) {
				wr.Write(GenerateFileSpec()); // Write the 'ftyp' atom
				wr.Write(GenerateHeaders()); // Write the 'moov' atom. The streams are fixed to 'KnownStreams' at this point.
				wr.Flush();
			}
			
			foreach (var stream in KnownStreams) { // Write streams in order -- IIS prefers this to interleaving.
				if (stream.Frames != null && stream.Frames.Count > 0) {
					WriteStream(stream);
				}
			}
		}

		/// <summary>
		/// Close a started MP4f file, writing footer data.
		/// After calling this, WriteStream() will cache data.
		/// </summary>
		public void EndFile () {
			if (ActiveFile == null) throw new NotSupportedException("Attempted to close a file that was not started");

			// does nothing yet -- I haven't written the footer atoms.

			ActiveFile = null;
		}

		#endregion

		#region Writing frames
		
		/// <summary>
		/// Write the frames of a stream to a file if one is open or to memory if not.
		/// </summary>
		public void WriteStream (MediaStream SourceStream) {
			var dst_stream = CacheStreamFrames(SourceStream);

			if (ActiveFile == null) return; // No file to write.

			// Write to file system (writes, flushes and re-closes)
			using (BinaryWriter wr = new BinaryWriter(ActiveFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read))) {
				wr.Write(GenerateFragment(dst_stream).FormatData()); // Write any waiting frames, plus the newly added ones
				wr.Flush();
				wr.Close(); // closes the file as well.
			}
			dst_stream.Frames.Clear(); // OK, we've handled these -- so dump the caches
		}


		/// <summary>
		/// Store the frames of a stream in the KnownStreams cache, adding a new entry if needed.
		/// If there is a file open, then new streams will be refused with an argument exception
		/// </summary>
		private MediaStream CacheStreamFrames (MediaStream SourceStream) {
			var dst_stream = KnownStreams.Where(a => a.TrackId == SourceStream.TrackId).FirstOrDefault();
			if (dst_stream == null) { // not a known stream, so add it.
				if (ActiveFile != null) throw new ArgumentException("Can't add new streams to an open file. Try adding your streams first, then open the file once all streams are known", "SourceStream");
				dst_stream = new MediaStream();
				dst_stream.FourCC = SourceStream.FourCC;
				dst_stream.Frames = new List<GenericMediaFrame>(SourceStream.Frames);
				dst_stream.Height = SourceStream.Height;
				dst_stream.TrackId = SourceStream.TrackId;
				dst_stream.Width = SourceStream.Width;
				KnownStreams.Add(dst_stream);
			} else {
				dst_stream.Frames.AddRange(SourceStream.Frames);
			}
			return dst_stream;
		}

		#endregion

		#region Non-file methods (for sending file parts across network interfaces)
		/// <summary>
		/// Output a 'moov' atom and it's children as raw data
		/// for the current set of streams. Does not write to file.
		/// </summary>
		public byte[] GenerateHeaders () {
			return GenerateMoov();
		}

		/// <summary>
		/// Output a 'moov' atom and it's children as raw data.
		/// </summary>
		private byte[] GenerateMoov () {
			HeaderBoxes.moov header = new HeaderBoxes.moov(KnownStreams.ToArray());
			return header.deepData();
		}


		/// <summary>
		/// Output a 'mfra' atom and it's children as raw data.
		/// </summary>
		public byte[] GenerateFooters () {
			FooterBoxes.mfra footer = new FooterBoxes.mfra();
			return footer.deepData();
		}

		/// <summary>
		/// Output a 'ftyp' atom and it's children as raw data
		/// for the current set of streams. Does not write to file.
		/// </summary>
		public byte[] GenerateFileSpec () {
			HeaderBoxes.ftyp filespec = new HeaderBoxes.ftyp(); // just assumes MP4f type
			return filespec.deepData();
		}

		/// <summary>
		/// Generate a pair of 'moof' and 'mdat' atoms for the given stream.
		/// The stream should contains a populated list of frame data, otherwise output will be empty.
		/// Keeps track of fragment index. Does not write to file. Does not remove frames. Does not adjust offset time.
		/// </summary>
		public FragmentBoxes.MediaFragmentHandler GenerateFragment (MediaStream Stream) {
			if (Stream.Frames == null || Stream.Frames.Count < 1) throw new Exception("Stream was empty");

			FragmentBoxes.MediaFragmentHandler output = new MP4_Mangler.FragmentBoxes.MediaFragmentHandler((uint)Stream.FragmentNumber, Stream.Offset);

			Stream.FragmentNumber++;

			foreach (var frame in Stream.Frames) {
				output.AddFrame((uint)Stream.TrackId, frame);
			}

			Stream.Frames.Clear();

			return output;
		}
		#endregion
	}
}
