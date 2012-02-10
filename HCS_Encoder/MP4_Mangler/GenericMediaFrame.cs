using System;
using System.Collections.Generic;
using System.IO;

namespace MP4_Mangler {
	/// <summary>
	/// The bare minimum data to add frames into MP4 files.
	/// </summary><remarks>
	/// Note: FrameTicks here is the frame duration (1/fps),
	/// PTS is the integral of FrameTicks.
	/// </remarks>
	public class GenericMediaFrame {
		/// <summary>Duration of frame, in .Net ticks</summary>
		public long FrameDuration { get; set; }

		/// <summary>
		/// PTS for this frame.
		/// </summary>
		public long FramePresentationTime { get; set; }

		/// <summary>
		/// Data for the frame
		/// </summary>
		public byte[] FrameData { get; set; }

		/// <summary>
		/// If &gte; 0, this is the valid length of the frame data.
		/// </summary>
		public int DataLength { get; set; }

		/// <summary>
		/// Not often used, but indicates a media format's track identifier.
		/// </summary>
		public int TrackId { get; set; }
	}
}
