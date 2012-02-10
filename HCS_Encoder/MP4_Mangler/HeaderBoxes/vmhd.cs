using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	public class vmhd: FullBox {

		/// <summary>
		/// Create a new Video media header
		/// </summary>
		/// <remarks>The spec says flags should be 1, with no info why.</remarks>
		public vmhd (): base(0, 0x0001, "vmhd") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			UInt16 graphics_mode = 0; // plain ol' copy. 
			UInt16 op_red = 0; // a bit odd that this is RGB when most video is Y'CrCb...
			UInt16 op_green = 0;
			UInt16 op_blue = 0;

			ous.Write(graphics_mode);
			ous.Write(op_red);
			ous.Write(op_green);
			ous.Write(op_blue);
		}

		public override void Prepare () {
		}
	}
}
