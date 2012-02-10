using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	public class smhd : FullBox{

		public smhd () : base(0, 0, "smhd") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			UInt16 balance = 0x0000; // 8.8 real; 0xFF00 == left, 0x0100 == right.
			UInt16 reserved = 0;

			ous.Write(balance);
			ous.Write(reserved);
		}

		public override void Prepare () {
		}
	}
}
