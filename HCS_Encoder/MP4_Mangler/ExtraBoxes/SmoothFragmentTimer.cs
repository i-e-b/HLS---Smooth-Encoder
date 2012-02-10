using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.ExtraBoxes {
	/// <summary>
	/// This special box gives per-fragment timing information
	/// to IIS during Live Smooth Streaming.
	/// </summary>
	/// <remarks>
	/// This structure was deduced from TCP sniffing, so might not
	/// be correct. Many Bothans and all that.
	/// </remarks>
	public class SmoothFragmentTimer : Box {
		/// <summary>
		/// Create a new timing box.
		/// </summary>
		/// <param name="Offset">Start timecode of this fragment (sum of previous 'FragmentTicks')</param>
		/// <param name="FragmentTicks">Total number of .Net ticks of the fragment</param>
		public SmoothFragmentTimer(long Offset, long FragmentTicks):base("uuid") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			// This is the GUID for the timing box. Taken directly from network captures
			ous.WriteForward(new byte[] {
				0x6D, 0x1D, 0x9B, 0x05, 0x42, 0xD5, 0x44, 0xE6,
				0x80, 0xE2, 0x14, 0x1D, 0xAF, 0xF7, 0x57, 0xB2 // 16 bytes of 'type'
			});

			// Don't know what this is, but it's always this data when I've seen it. Maybe flags&version?
			ous.WriteForward(new byte[] {
				0x01, 0x00, 0x00, 0x00
			});

			// This relates to the 't' field of the 'c' element in .ism files.
			ous.Write((UInt64)Offset); // This is presentation offset.

			// This relates to the 'd' field of the 'c' element in .ism files.
			ous.Write((UInt64)FragmentTicks); // This is the fragment duration.
		}

		public override void Prepare () {
		}
	}
}
