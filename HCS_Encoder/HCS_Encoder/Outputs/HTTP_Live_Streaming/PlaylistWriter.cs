using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace HCS_Encoder {
	/// <summary>
	/// Handles writing playlists for encoded chunks.
	/// Writes both uncompressed (Apple standard) and gzip'd (My version)
	/// </summary>
	public class PlaylistWriter {
		/// <summary>
		/// Base location for chunk files.
		/// (example: "http://video.my-cdn.com/acct/demo-video-1/chunk";
		/// outputs "http://video.my-cdn.com/acct/demo-video-1/chunk-00001.ts")
		/// </summary>
		public string ServerRoot { get; set; }

		/// <summary>
		/// Directory for local temporary files.
		/// </summary>
		public string BaseDirectory { get; set; }

		/// <summary>
		/// Write-out location for the playlist file.
		/// Can be either local file system or FTP server.
		/// </summary>
		public string PlaylistDestination { get; set; }

		/// <summary>
		/// Name of the playlist file. Normally "index.m3u8".
		/// A compressed playlist will be named with ".gz" appended.
		/// </summary>
		public string PlaylistFilename { get; set; }

		/// <summary>
		/// If true, playlist won't cause Client-side player to update for new chunks.
		/// If false, Client-side player will poll for updated playlist.
		/// Defaults to false.
		/// </summary>
		public bool IsClosed { get; set; }

		protected List<ChunkDetail> ChunkIndices; // List of completed chunks to be included in playlist
		private object SyncRoot;

		public PlaylistWriter () {
			SyncRoot = new object();
			ChunkIndices = new List<ChunkDetail>();
			IsClosed = false;
		}

		public int WaitingUploads { get; protected set; }

		/// <summary>
		/// Add a chunk to the playlist and write.
		/// </summary>
		public void AddChunk (int index, double duration) {
			lock (SyncRoot) {
				var cd = new ChunkDetail(index, 0, duration);
				if (!ChunkIndices.Contains(cd)) {
					ChunkIndices.Add(cd);
				}
				ChunkIndices.Sort(); // just in case the uploads go out-of-order.
				WritePlaylist();
				TransportPlaylist();
			}
		}

		/// <summary>
		/// Set the playlist to closed and write the final version
		/// </summary>
		public void Close () {
			IsClosed = true;
			WritePlaylist();
			TransportPlaylist();
		}

		protected void UploadAction (object o) {
			try {
				string dir = BaseDirectory;
				string filename = Path.Combine(dir, PlaylistFilename);
				TransferAgent.Transfer(new Uri(filename, UriKind.Absolute), new Uri(PlaylistDestination, UriKind.Absolute));

				filename += ".gz"; // upload compressed version
				TransferAgent.Transfer(new Uri(filename, UriKind.Absolute), new Uri(PlaylistDestination+".gz", UriKind.Absolute));

				WaitingUploads = 0; // any transfer is as good as all.
				Console.WriteLine("Uploaded new playlist");
			} catch {
				// Some sort of error notification might help!
				Console.WriteLine("Missed a playlist upload");
				TransportPlaylist(); // try again!
			}
		}

		private void TransportPlaylist () {
			WaitingUploads++;
			System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(UploadAction));
		}

		private void WritePlaylist () {
			string dur = "5";
			if (ChunkIndices.Count > 0) dur = ChunkIndices[ChunkIndices.Count-1].ChunkDuration.ToString("0");

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("#EXTM3U");
			sb.AppendLine("#EXT-X-TARGETDURATION:"+dur);

			IFormatProvider invar = System.Globalization.NumberFormatInfo.InvariantInfo;

			string infdur = "#EXTINF:";
			int last_idx = -1;
			foreach (ChunkDetail chunk in ChunkIndices) {
				if (last_idx == chunk.ChunkIndex) {
					sb.Append("#FAIL");
				}
				sb.AppendLine(infdur+chunk.ChunkDuration.ToString("0")+","+chunk.ChunkDuration.ToString("0.00000", invar));
				sb.AppendLine(ServerRoot + "-" + chunk.ChunkIndex.ToString("00000") + ".ts");
				last_idx = chunk.ChunkIndex;
			}

			if (IsClosed) {
				sb.AppendLine("#EXT-X-ENDLIST");
			}

			string dir = BaseDirectory;
			string filename = Path.Combine(dir, PlaylistFilename);
			try {
				lock (SyncRoot) {
					File.WriteAllText(filename, sb.ToString());
				}
			} catch { }

			// Write a compressed version:
			filename += ".gz";
			byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
			try {
				using (Stream os = File.Create(filename))
				using (DeflaterOutputStream gzos = new DeflaterOutputStream(os))
					gzos.Write(buffer, 0, buffer.Length);
			} catch { }
			sb = null;
		}
	}
}
