using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HCS_Encoder.Utilities;

namespace HCS_Encoder.Outputs {

	/// <summary>
	/// Handles chunk output, based on encoder configuration.
	/// </summary>
	/// <remarks>This is tightly coupled to the individual uploaders on purpose.</remarks>
	public class OutputRouter {
		/// <summary>
		/// Gets or Sets. Default = true;
		/// If true, output chunks are deleted once they have been sucessfully sent.
		/// If false, otuput chunks are kept on the encoder local disk.
		/// </summary>
		public bool ShouldCleanup { get; set; }

		/// <summary>
		/// Gets or Sets. Default = true;
		/// If true, output chunks are sent as soon as possible.
		/// If false, output chunks are not sent until a call to 'SendOutput()' is made. Outputs will still be queued
		/// even, and will be sent when output is re-enabled.
		/// </summary>
		/// <remarks>
		/// Unsent chunks will not be deleted from the encoder until sent, even if 'ShouldCleanup' is true.
		/// </remarks>
		public bool EnableOutput { get; set; }

		/// <summary>
		/// Prepare a new output, with the given configuration.
		/// </summary>
		public OutputRouter (EncoderConfiguration.Configuration Configuration) {
			SyncLock = new object();
			Config = Configuration;
			string handler = Configuration.Upload.UploadHandlerName ?? "empty";
			ChunksCompleted = new Queue<ChunkDetail>();
			FilesCompleted = new Queue<FileInfo>();
			ShouldCleanup = true;
			EnableOutput = true;

			switch (handler.ToLower()) {
				case "iis smooth":
					Prepare_IIS();
					break;

				case "http live":
					Prepare_HCS();
					break;

				case "test":
					Prepare_Test();
					break;

				default:
					throw new ArgumentException("Upload handler name not recognised", "Configuration");
			}
			if (SelectedHandler == null) throw new Exception("Failed to start output handler");

			SelectedHandler.FileConsumed += new EventHandler<FileEventArgs>(SelectedHandler_FileConsumed);
		}

		/// <summary>
		/// Signal the output to consume the given chunk.
		/// Should only be called by the encoder.
		/// </summary>
		public void NewChunkAvailable (int ChunkIndex, int StreamIndex, double CaptureDuration) {
			if (SelectedHandler == null) throw new Exception("Output handler was not initialised");
			string joined = Path.Combine(Config.EncoderSettings.LocalSystemOutputFolder, Config.EncoderSettings.LocalSystemFilePrefix);
			string chunk_path = joined + "_" + StreamIndex + "-" + ChunkIndex.ToString("00000") + ".ts";

			ChunkDetail chunk = new ChunkDetail(chunk_path, ChunkIndex, StreamIndex, CaptureDuration);
			lock (ChunksCompleted) {
				ChunksCompleted.Enqueue(chunk);
			}

			if (EnableOutput) {
				// Force work on a seperate thread, to prevent any interference with encoding.
				// Encoder output to local system SHOULD be unaffected by delays in output handler.
				// Final output MAY be delayed, but SHOULD be correct.
				System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ChunkAction));
			}
		}

		/// <summary>
		/// Close this output path and clean up any connections
		/// </summary>
		public void Close () {
			if (SelectedHandler != null) {
				SelectedHandler.FileConsumed -= SelectedHandler_FileConsumed;
				SelectedHandler.Close();
			}

			while (FilesCompleted.Count > 0) {
				FilesCompleted.Dequeue().Delete();
			}

			// clean up leftovers:
			if (ShouldCleanup) {
				while (ChunksCompleted.Count > 0) {
					new FileInfo(ChunksCompleted.Dequeue().SourceFilePath).Delete();
				}
			}
		}

		/// <summary>
		/// Send any queued outputs inclusively inside the specified range.
		/// Queued outputs less than the minimum will be dropped.
		/// Setting the maximum to -1 will re-enable output (same as 'EnableOutput = true') and
		/// send all queued outputs.
		/// </summary>
		/// <param name="MinimumChunkIndex">Inclusive minimum chunk to send</param>
		/// <param name="MaximumChunkIndex">Inclusive maximum chunk to send</param>
		/// <remarks>To enable output while clearing all queued outputs, call as 'SendOutput(Job.SegmentNumber, -1)'</remarks>
		public void SendOutput (int MinimumChunkIndex, int MaximumChunkIndex) {
			lock (SyncLock) {
				int max = (MaximumChunkIndex >= 0) ? (MaximumChunkIndex) : (int.MaxValue);

				int jobs_to_run = 0; // number of output workers to start.

				List<ChunkDetail> chunks_waiting = ChunksCompleted.ToList();
				chunks_waiting.Sort((a, b) => a.ChunkIndex.CompareTo(b.ChunkIndex));

				ChunksCompleted.Clear();
				foreach (var chunk in chunks_waiting) {
					if (chunk.ChunkIndex > max) {
						ChunksCompleted.Enqueue(chunk);
						continue;
					}

					if (chunk.ChunkIndex >= MinimumChunkIndex) {
						jobs_to_run++; // note another job to be run.
						ChunksCompleted.Enqueue(chunk);
					} else {
						// drop this file.
						FilesCompleted.Enqueue(new FileInfo(chunk.SourceFilePath));
					}
				}

				// run jobs that are waiting and not removed.
				for (int i = 0; i < jobs_to_run; i++) {
					System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ChunkAction));
				}

				if (MaximumChunkIndex < 0) EnableOutput = true;
			}
		}

		/// <summary>
		/// Mapping type of the connected output.
		/// </summary>
		public StreamMapping GetStreamMappingType () {
			return SelectedHandler.GetStreamMappingType();
		}

		/// <summary>
		/// Connect the output handler to a set of encoder packages
		/// </summary>
		/// <param name="Packages"></param>
		public void Prepare (List<EncoderPackage> Packages) {
			SelectedHandler.Prepare(Packages);
		}

		#region Inner Workings
		private IOutputHandler SelectedHandler { get; set; }
		private Queue<ChunkDetail> ChunksCompleted;
		private Queue<FileInfo> FilesCompleted;
		private object SyncLock;

		/// <summary>
		/// Gets the configuration used to set-up this output router.
		/// </summary>
		private EncoderConfiguration.Configuration Config { get; set; }

		/// <summary>
		/// Gets the encoder job being used by this output router.
		/// </summary>
		private EncoderJob Job { get; set; }

		/// <summary>
		/// Clean up the local filesystem after a SUCCESSFUL file transfer.
		/// </summary>
		void SelectedHandler_FileConsumed (object sender, FileEventArgs e) {
			if (ShouldCleanup) {
				FilesCompleted.Enqueue(e.ReferencedFile);

				while (FilesCompleted.Count > 10) { // allows a few samples to be left about
					try {
						FilesCompleted.Dequeue().Delete();
					} catch { }
				}
			}
		}

		/// <summary>
		/// Called by thread pool in NewChunkAvailable()
		/// </summary>
		protected void ChunkAction (object o) {
			ChunkDetail chunk;
			lock (SyncLock) {
				lock (ChunksCompleted) {
					if (ChunksCompleted.Count < 1) return; // no chunks waiting!
					chunk = ChunksCompleted.Dequeue();
				}
				SelectedHandler.ConsumeChunk(chunk.ChunkIndex, chunk.StreamIndex, chunk.SourceFilePath, chunk.ChunkDuration);
			}
		}

		private void Prepare_HCS() {
			var uploader = new UploadManager(Config);
			SelectedHandler = uploader;
		}

		private void Prepare_IIS () {
			SelectedHandler = new SmoothStream.ChunkTransformer(Config, true);
		}

		private void Prepare_Test () {
			SelectedHandler = new TestingOutputHandler();
		}
		#endregion
	}
}
