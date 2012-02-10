using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	public class mdhd: FullBox {

		public mdhd () : base(1, 0, "mdhd") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			// Version = 1 means a few 64-bit values:
			UInt64 creation_time = Box.BoxDate(DateTime.Now);
			UInt64 modified_time = creation_time;
			UInt32 timescale = 10000000; // This is the .Net value. Normal MPEG might need 900000;
			UInt64 duration = 0; // This is unknown for live, and currently unused by Silverlight.

			ous.Write(creation_time);
			ous.Write(modified_time);
			ous.Write(timescale);
			ous.Write(duration);

			UInt16 language = 0x55C4; // this is both the 1 bit pad and the 5 bit x 3 char code for 'und';
			ous.Write(language);

			ous.Write((UInt16)0); // Why have 'language' so crushed, and yet have this waste?
		}

		public override void Prepare () {
		}
	}
}
