﻿using System;
using System.Collections.Generic;

namespace HCS_Encoder.Outputs {
	/// <summary>
	/// Dummy output handler. Does nothing.
	/// </summary>
	public class TestingOutputHandler : IOutputHandler {
		public void ConsumeChunk (int ChunkIndex, int StreamIndex, string FilePath, double chunkDuration) {
		}

		public void Close () {
		}

		public event EventHandler<FileEventArgs> FileConsumed;


		public StreamMapping GetStreamMappingType () {
			//return StreamMapping.AllTypeStreams;
			return StreamMapping.SingleTypeStreams;
		}

		public void Prepare (List<HCS_Encoder.Utilities.EncoderPackage> Packages) {
			
		}
	}
}
