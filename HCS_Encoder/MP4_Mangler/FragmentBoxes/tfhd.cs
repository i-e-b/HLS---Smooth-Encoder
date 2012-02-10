using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.FragmentBoxes {
	/// <summary>
	/// Header for track data.
	/// Currently doesn't handle anything beyond track IDs.
	/// </summary>
	public class tfhd: FullBox {
		protected UInt32 DataSize { get; set; }
		protected UInt32 FrameCount { get; set; }

		/// <summary>
		/// Make a defaut header with just track id
		/// </summary>
		/// <param name="TrackID"></param>
		public tfhd (UInt32 TrackID)
			: base(0x00, /*Flags*/0, "tfhd") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);
			ous.Write(TrackID);
		}

		public void AddFrame (GenericMediaFrame gmf) {
			DataSize += (uint)gmf.FrameData.Length;
			FrameCount++;
		}

		public override void Prepare () {
		}
	}
}
