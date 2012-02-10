using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MP4_Mangler;
using MP4_Mangler.FragmentBoxes;
using HCS_Encoder.Outputs.SmoothStream.Multiplexing;
using System.Threading;
using HCS_Encoder.Utilities;

namespace HCS_Encoder.Outputs.SmoothStream {
	public class ChunkTransformer: IOutputHandler {
		private string PublishPoint { get; set; }
		public IisSmoothPush PushServer { get; set; }
		private Dictionary<int, long> TrackDurations { get; set; }
		private Dictionary<int, long> TrackOffsets { get; set; }
		private MediaStream[] Streams = null;
		private FileRoot Mp4fFile;
		private ulong targetDuration;
		private MpegTS_Demux[] Demuxer;
		private EncoderConfiguration.Configuration Config;
		private readonly Queue<ChunkDetail> WaitingChunks;
		private readonly object SyncRoot;

		/// <summary>
		/// Create a new chunk transformer.
		/// Will try to create a new IIS Live Smooth Streaming publishing point matching
		/// the configuration settings.
		/// Will try both WebDAV and FTP. If neither are not correctly enabled on the destination
		/// server, the point must already exist.
		/// </summary>
		public ChunkTransformer (EncoderConfiguration.Configuration Configuration, bool pubPointCreationIsOptional) {
			SyncRoot = new object();
			WaitingChunks = new Queue<ChunkDetail>();
			Config = Configuration;
			try {
				PreparePublishingPoint(Config);
			} catch {
				if (!pubPointCreationIsOptional) throw;
			}
		}

		private static void PreparePublishingPoint (EncoderConfiguration.Configuration Configuration) {
			string dest_root = null;
			IisPointCreator pc;

			try {
				dest_root = Configuration.Upload.IndexFtpRoot;
				if (!String.IsNullOrEmpty(dest_root)) {
					pc = PreparePointCreator(dest_root);
					pc.CreatePoint();
				} else {
					dest_root = Configuration.Upload.VideoDestinationRoot;
					pc = PreparePointCreator(dest_root);
					pc.CreatePoint();
				}
			} catch (Exception ex) {
				throw new Exception("Could not establish a publishing point at " + dest_root, ex);
			}
		}

		private static IisPointCreator PreparePointCreator (string dest_root) {
			var sp = dest_root.LastIndexOf('/');
			var dest = new Uri(dest_root.Substring(0, sp), UriKind.Absolute);
			var ppname = dest_root.Substring(sp + 1);

			var pc = new IisPointCreator(dest, ppname);
			return pc;
		}

		/// <summary>
		/// Read the supplied configuration and prepare the transformer for work.
		/// </summary>
		private void PrepareTransformer (EncoderConfiguration.Configuration Configuration, List<EncoderPackage> Packages) {
			Config = Configuration;
			TimeSpan TargetDuration = TimeSpan.FromSeconds(Config.EncoderSettings.FragmentSeconds);

			PublishPoint = Config.Upload.VideoDestinationRoot;
			if (String.IsNullOrEmpty(PublishPoint)) throw new ArgumentException("Publishing point must not be empty", "PublishUrl");

			PushServer = new IisSmoothPush(new Uri(PublishPoint));
			TrackDurations = new Dictionary<int, long>();
			TrackOffsets = new Dictionary<int, long>();

			targetDuration = (ulong)TargetDuration.Ticks;
			Streams = new MediaStream[Packages.Count];

			foreach (var pkg in Packages) {
				if (pkg.Specification.HasVideo && pkg.Specification.HasAudio) {
					throw new NotSupportedException("IIS Smooth output doesn't support pre-muxed streams");
				}

				if (pkg.Specification.HasAudio) {
					Streams[pkg.JobIndex] = new MediaStream(); // for now, stream 0 is audio, and all others are video.
					Streams[pkg.JobIndex].TrackId = pkg.JobIndex + 1;
					Streams[pkg.JobIndex].FourCC = "mp3a"; // MP3
					//Streams[pkg.JobIndex].FourCC = "mp4a"; // AAC
					Streams[pkg.JobIndex].Height = 0;
					Streams[pkg.JobIndex].Width = 0;
					Streams[pkg.JobIndex].Bitrate = 96000; //pkg.Job.Bitrate; // later!
				} else if (pkg.Specification.HasVideo) {
					Streams[pkg.JobIndex] = new MediaStream(); // for now, stream 0 is audio, and all others are video.
					Streams[pkg.JobIndex].TrackId = pkg.JobIndex + 1;
					Streams[pkg.JobIndex].FourCC = "H264"; // this is the M$ format, not iso (which is 'avc1')
					Streams[pkg.JobIndex].Height = Config.EncoderSettings.OutputHeight; // the actual size may be different due to scaling factor.
					Streams[pkg.JobIndex].Width = Config.EncoderSettings.OutputWidth;
					Streams[pkg.JobIndex].Bitrate = pkg.Job.Bitrate;
				}
			}

			Mp4fFile = new FileRoot(Streams);
			Demuxer = new MpegTS_Demux[Packages.Count];
			for (int di = 0; di < Demuxer.Length; di++) {
				Demuxer[di] = new MpegTS_Demux();
			}
		}

		public event EventHandler<FileEventArgs> FileConsumed;

		public void OnFileConsumed (object sender, FileEventArgs args) {
			var tmp = FileConsumed;
			if (tmp != null) {
				tmp(sender, args);
			}
		}

		/// <summary>
		/// Transform an MPEG-TS file into a MP4f file.
		/// This all needs to be stripped out and each stream needs it's own IisSmoothPush connection.
		/// </summary>
		private void Transform (string SourceFile, int StreamIndex, int ChunkIndex) {
			// Check for chunk:
			string src = SourceFile;
			if (!File.Exists(src)) throw new Exception("Source file missing");

			// Get demuxer for THIS set of chunks (they can't be shared across different feeds)
			MpegTS_Demux demux = Demuxer[StreamIndex];

			double min_time = Math.Max(0, ChunkIndex - 10) * Config.EncoderSettings.FragmentSeconds;

			// Parse TS file into frames:
			using (var fs = new FileStream(src, FileMode.Open)) {
				lock (demux) {
					demux.FeedTransportStream(fs, min_time);
				}
			}

			List<GenericMediaFrame> aud_frames = null;
			List<GenericMediaFrame> vid_frames = null;

			// Read frames that are ready
			int idx = StreamIndex;
			lock (demux) {
				aud_frames = demux.GetAvailableAudio();
				vid_frames = demux.GetAvailableVideo();


				if (aud_frames != null && aud_frames.Count > 0) {
					Streams[idx].Frames = aud_frames; // for now, all audio is stream 0.
					PushStream(Streams[idx], Mp4fFile);
				}
				
				if (vid_frames != null && vid_frames.Count > 0) {
					Streams[idx].Frames = vid_frames;
					PushStream(Streams[idx], Mp4fFile);
				}
			}

			// File is no longer needed
			OnFileConsumed(this, new FileEventArgs { ReferencedFile = new FileInfo(SourceFile) });
		}

		/// <summary>
		/// Pushes a set of frames to IIS. Will trigger a connect if needed.
		/// </summary>
		private void PushStream (MediaStream stream, FileRoot TargetMp4fFile) {
			if (stream == null || stream.Frames == null) return; // no frames.

			SanitiseStream(stream);
			if (stream.Frames.Count < 1) return; // no frames.

			if (!PushServer.IsConnected(stream.TrackId))
				ConnectAndPushHeaders(stream, TargetMp4fFile);

			// set start-of-fragment time from PTS
			stream.Offset = stream.Frames[0].FramePresentationTime - stream.Frames[0].FrameDuration;

			// Push the fragment
			var fragment_handler = TargetMp4fFile.GenerateFragment(stream);
			PushServer.PushData(stream.TrackId, fragment_handler.MoofData());
			PushServer.PushData(stream.TrackId, fragment_handler.MdatData());
		}

		/// <summary>
		/// Remove any frames with zero length.
		/// Zero length frames kill the live stream.
		/// </summary>
		private void SanitiseStream (MediaStream stream) {
			lock (SyncRoot) {
				var src = new List<GenericMediaFrame>(stream.Frames);
				stream.Frames.Clear();
				foreach (var frame in src) {
					if (frame.FrameDuration < 1) continue;
					stream.Frames.Add(frame);
				}
			}
		}

		/// <summary>
		/// Used once per connection, this opens a long-life HTTP stream
		/// and pushes the very basic MP4 parts needed to get IIS working.
		/// </summary>
		private void ConnectAndPushHeaders (MediaStream stream, FileRoot TargetMp4fFile) {
			SmilGenerator smil = new SmilGenerator("HCS Encoder by Iain Ballard.", stream);

			smil.ApproxBitrate = stream.Bitrate;
			MP4_Mangler.ExtraBoxes.SmoothSmil ssmil = new MP4_Mangler.ExtraBoxes.SmoothSmil(smil.Generate());
			PushServer.Connect(stream.TrackId); // This pushes to the subpath: Streams({id}-stream{index})

			// push headers (only done once per track)
			// each one needs it's own HTTP Chunk, so don't concat!
			PushServer.PushData(stream.TrackId, TargetMp4fFile.GenerateFileSpec());
			PushServer.PushData(stream.TrackId, ssmil.deepData());
			PushServer.PushData(stream.TrackId, TargetMp4fFile.GenerateHeaders());
		}


		protected void UploadAction (object o) {
			Exception last_exception = new Exception("Unknown error");
			for (int i = 0; i < 10; i++) {
				try {
					ChunkDetail cd = null;
					lock (SyncRoot) {
						lock (WaitingChunks) {
							cd = WaitingChunks.Peek();
							Transform(cd.SourceFilePath, cd.StreamIndex, cd.ChunkIndex);
							WaitingChunks.Dequeue();
							return;
						}
					}
				} catch (Exception ex) {
					last_exception = ex;
					System.Diagnostics.Debug.WriteLine("ChunkTransformer.cs: IIS Push failed", ex.Message + "\r\n" + ex.StackTrace);
				}
			}
			WaitingChunks.Dequeue(); //drop the failing fragment.
			System.Diagnostics.Debug.Fail("ChunkTransformer.cs: IIS Push failed", last_exception.Message + "\r\n" + last_exception.StackTrace);
			System.Threading.Thread.CurrentThread.Abort();
		}

		public void ConsumeChunk (int ChunkIndex, int StreamIndex, string FilePath, double chunkDuration) {
			lock (SyncRoot) {
				lock (WaitingChunks) {
					var message = new ChunkDetail(FilePath, ChunkIndex, StreamIndex, chunkDuration);
					WaitingChunks.Enqueue(message);
				}
			}
			UploadAction(null);
		}

		/// <summary>
		/// Shut down the streams and close connections.
		/// </summary>
		public void Close () {
			byte[] EOS = Mp4fFile.GenerateFooters(); // End-Of-Stream signal

			// Close off all streams, taking them out of live mode:
			foreach (var stream in Streams) {
				try {
					PushServer.PushData(stream.TrackId, EOS);
				} catch { }
			}

			// Final close:
			PushServer.Close();
		}


		public StreamMapping GetStreamMappingType () {
			return StreamMapping.SingleTypeStreams;
		}

		public void Prepare (List<EncoderPackage> Packages) {
			PrepareTransformer(Config, Packages);
		}
	}
}
