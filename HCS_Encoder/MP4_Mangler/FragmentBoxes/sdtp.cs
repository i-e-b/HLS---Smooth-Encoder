using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.FragmentBoxes {
	/// <summary>
	/// Sample dependence table
	/// </summary>
	public class sdtp : FullBox {
		protected bool Prepared;
		protected int FrameCount;

		public sdtp ():base(0, 0, "sdtp") {
			Prepared = false;
			FrameCount = 0;
		}

		/// <summary>
		/// Add a frame to this trun
		/// </summary>
		public void AddFrame (GenericMediaFrame f) {
			FrameCount++;
		}


		public override void Prepare () {
			if (Prepared) return;
			Prepared = true;

			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			if (FrameCount < 1) return;

			// First frame is [no depends, is depended on, unknown redundancy]
			ous.Write((byte)0x24);

			for (int i = 1; i < FrameCount; i++) {
				// Other frames are [dependent, (reserved), unknown redundancy] --> this is how M$ do it...
				ous.Write((byte)0x1C);
				//ous.Write((byte)0x24);
			}
		}
	}
}
