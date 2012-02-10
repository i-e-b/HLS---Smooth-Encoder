using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HCS_Encoder.Outputs {
	public class FileEventArgs: EventArgs {
		public FileInfo ReferencedFile { get; set; }
	}

	/// <summary>
	/// Buffer to encoder to output mapping types.
	/// </summary>
	public enum StreamMapping {
		/// <summary>All stream types are separated (i.e. 1x audio, 3x video, 1x data)</summary>
		/// <remarks>Used for IIS Smooth Streaming -- chunks need to be composited by a smart server.</remarks>
		SingleTypeStreams, 

		/// <summary>
		/// All streams carry all types of content (i.e. High[audio,video,data], Med[audio,video,data], Low[audio,video,data])
		/// </summary>
		/// <remarks>Used for HTTP Live Streaming -- each chunk is totally independent of others.</remarks>
		AllTypeStreams
/*
		/// <summary>
		/// One stream carries all types bases, and other rates override (i.e. Low[audio,video,data], Med[video], High[audio,video])
		/// </summary>
		CascadingStreams*/
	}

	interface IOutputHandler {
		event EventHandler<FileEventArgs> FileConsumed;

		/// <summary>
		/// Returns the kind of stream mapping this output handler requires.
		/// </summary>
		StreamMapping GetStreamMappingType ();

		void ConsumeChunk (int ChunkIndex, int StreamIndex, string FilePath, double chunkDuration);
		void Close ();

		void Prepare (List<HCS_Encoder.Utilities.EncoderPackage> Packages);
	}
}
