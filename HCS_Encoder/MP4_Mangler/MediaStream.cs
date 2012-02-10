using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MP4_Mangler {
	/// <summary>
	/// This class holds all the data needed to setup and produce a single MP4 track.
	/// </summary>
	public class MediaStream {
		/// <summary>
		/// TrackId. If this is &#8804; 1, it will be assigned by the FileRoot object.
		/// </summary>
		public int TrackId { get; set; }

		/// <summary>
		/// Start at 1 and increment for each fragment sent to hosting server.
		/// </summary>
		public int FragmentNumber { get; set; }

		/// <summary>
		/// Width of the content. MUST be exactly 0 for non-video streams
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Height of the content. MUST be exactly 0 for non-video streams
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Four character code for the content format.
		/// Should be specified, but not currenly used.
		/// </summary>
		public string FourCC { get; set; }

		/// <summary>
		/// Currently loaded frame data.
		/// May be null or empty.
		/// </summary>
		public List<GenericMediaFrame> Frames { get; set; }

		/// <summary>
		/// Tick count of all frames held
		/// </summary>
		public Int64 Duration {
			get {
				lock (this) {
					long dur = 0;
					foreach (var frame in Frames) {
						dur += frame.FrameDuration;
					}
					return dur;
				}
			}
		}

		/// <summary>
		/// Tick sum of frames already handled (next frame starts at this time).
		/// </summary>
		public Int64 Offset { get; set; }

		/// <summary>
		/// Measured or target bitrate (should be constant throught the stream)
		/// </summary>
		public int Bitrate { get; set; }
	}
}
