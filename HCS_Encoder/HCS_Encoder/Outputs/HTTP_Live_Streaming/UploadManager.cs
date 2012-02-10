using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HCS_Encoder.Outputs;

namespace HCS_Encoder {
	/// <summary>
	/// Cludgy way of transporting HTTP Chunk Streaming files about.
	/// </summary>
	public class UploadManager:IOutputHandler {

		public event EventHandler<ChunkUploadedEventArgs> UploadComplete;
		public event EventHandler<FileEventArgs> FileConsumed;
		private object SyncRoot;

		protected void OnUploadComplete (object sender, ChunkUploadedEventArgs e) {
			var tmp = UploadComplete;
			if (tmp != null) {
				tmp(sender, e);
			}

			var tmp2 = FileConsumed;
			if (tmp2 != null) {
				tmp2(sender, new FileEventArgs { ReferencedFile = new FileInfo(e.SourceFilePath) });
			}


			// Trigger playlist writer
			plw.AddChunk(e.ChunkIndex, e.ChunkDuration);
		}


		protected string src, dest, pre;
		protected Queue<ChunkDetail> waitingChunks;
		protected PlaylistWriter plw = null;

		/// <summary>
		/// Create a new upload manager to transport HCS fragments and write a playlist in the configured location.
		/// The playlist will be re-written every time a fragment is successfully uploaded.
		/// </summary>
		/// <param name="Config">Job configuration.</param>
		public UploadManager (EncoderConfiguration.Configuration Config){
			// Setup some some common strings and the upload queue
			pre = Config.EncoderSettings.LocalSystemFilePrefix;
			src = "file://" + Path.Combine(Config.EncoderSettings.LocalSystemOutputFolder, Config.EncoderSettings.LocalSystemFilePrefix);
			dest = Config.Upload.VideoDestinationRoot;

			if (String.IsNullOrEmpty(pre)) throw new Exception("Configuration is invalid. Check 'LocalSystemPrefix'");
			if (String.IsNullOrEmpty(src)) throw new Exception("Configuration is invalid. Check output paths");
			if (String.IsNullOrEmpty(dest)) throw new Exception("Configuration is invalid. Check 'VideoDestinationRoot'");

			waitingChunks = new Queue<ChunkDetail>();

			// Setup and connect the playlist writer
			plw = new PlaylistWriter();
			plw.ServerRoot = Config.Upload.ServerLookupRoot;
			plw.PlaylistDestination = Config.Upload.IndexFtpRoot + Config.Upload.IndexName;
			plw.PlaylistFilename = Config.Upload.IndexName;
			plw.BaseDirectory = Config.EncoderSettings.LocalSystemOutputFolder;
			plw.IsClosed = false;

			
			SyncRoot = new object();
		}

		public int WaitingUploadCount { get { return waitingChunks.Count; } }

		protected void UploadAction (object o) {
			lock (waitingChunks) {
				while (waitingChunks.Count > 0) {
					Exception last_exception = new Exception("Unknown error");
					for (int i = 0; i < 10; i++) {
						try {
							ChunkDetail cd = null;
							lock (SyncRoot) {
								cd = waitingChunks.Peek();
								SendChunk(cd);
								waitingChunks.Dequeue();
								return;
							}
						} catch (Exception ex) {
							last_exception = ex;
							System.Diagnostics.Debug.WriteLine("UploadManager.cs: HTTP Live transfer failed", ex.Message + "\r\n" + ex.StackTrace);
						}
					}
					waitingChunks.Dequeue(); //drop the failing fragment.
					System.Diagnostics.Debug.Fail("UploadManager.cs: HTTP Live transfer failed", last_exception.Message + "\r\n" + last_exception.StackTrace);
					System.Threading.Thread.CurrentThread.Abort();
				}
			}
		}

		private void SendChunk (ChunkDetail chunk) {
			string frag = "-" + chunk.ChunkIndex.ToString("00000") + ".ts";
			string fn = pre + frag;


			using (Stream fs = File.Open(chunk.SourceFilePath, FileMode.Open)) {
				TransferAgent.Transfer(fs, new Uri(dest + fn));
			}
			OnUploadComplete(this, new ChunkUploadedEventArgs() { ChunkIndex = chunk.ChunkIndex, ChunkDuration = chunk.ChunkDuration, SourceFilePath = chunk.SourceFilePath });

		}

		public void ConsumeChunk (int ChunkIndex, int StreamIndex, string FilePath, double chunkDuration) {

			lock (SyncRoot) {
				lock (waitingChunks) {
					waitingChunks.Enqueue(new ChunkDetail(FilePath, ChunkIndex, StreamIndex, chunkDuration));
					System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(UploadAction));
				}
			}
		}

		public void Close () {
			if (plw != null) plw.Close();
			int safety = 0;
			while ((WaitingUploadCount > 0 || plw.WaitingUploads > 0) && safety < 500) {
				System.Threading.Thread.Sleep(100);
				safety++;
			}
			if (safety >= 450) {
				Console.WriteLine("Timed out trying to finish uploads!");
				System.Threading.Thread.Sleep(3000);
			}
			
		}

		public StreamMapping GetStreamMappingType () {
			return StreamMapping.AllTypeStreams;
		}

		public void Prepare (List<HCS_Encoder.Utilities.EncoderPackage> Packages) {
			// No extra prep required.
		}
	}
}
