using System;

namespace HCS_Encoder {

	public class ChunkDetail : IComparable<ChunkDetail> {
		public int StreamIndex { get; set; }
		public int ChunkIndex { get; set; }
		public double ChunkDuration { get; set; }
		public string SourceFilePath { get; set; }
		public ChunkDetail (int chunkIndex, int streamIndex, double duration) {
			ChunkIndex = chunkIndex;
			StreamIndex = streamIndex;
			ChunkDuration = duration;
		}
		public ChunkDetail (string path, int chunkIndex, int streamIndex, double duration) {
			ChunkIndex = chunkIndex;
			StreamIndex = streamIndex;
			ChunkDuration = duration;
			SourceFilePath = path;
		}

		#region IComparable<ChunkDetail> Members

		public int CompareTo (ChunkDetail other) {
			return ChunkIndex.CompareTo(other.ChunkIndex);
		}

		#endregion
	}

	public class ChunkUploadedEventArgs : EventArgs {
		public int ChunkIndex { get; set; }
		public int StreamIndex { get; set; }
		public double ChunkDuration { get; set; }
		public string SourceFilePath { get; set; }
	}
}
