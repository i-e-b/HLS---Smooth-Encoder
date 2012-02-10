using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	public class mvhd:FullBox {

		public mvhd ():base(0x01, 0, "mvhd") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			// Version = 1 means a few 64-bit values:
			UInt64 creation_time = Box.BoxDate(DateTime.Now);
			UInt64 modified_time = creation_time;
			UInt32 timescale = 10000000; // I think Silverlight ignores this.
			UInt64 duration = 0; // This is unknown for live, and currently unused by Silverlight.

			ous.Write(creation_time);
			ous.Write(modified_time);
			ous.Write(timescale);
			ous.Write(duration);

			// These are all constants for now:
			Int32 DisplayRate = 0x00010000; // Fixed point value = 1.0f -> normal playback, forward.
			Int16 Volume = 0x0100; // 100% volume
			Int16 Reserved_1 = 0;
			UInt64 Reserved_2 = 0; // spec has this as Int32[2].

			ous.Write(DisplayRate);
			ous.Write(Volume);
			ous.Write(Reserved_1);
			ous.Write(Reserved_2);

			// Unity matrix: (another waste of 36 bytes)
			ous.Write((Int32)0x00010000); ous.Write((Int32)0x00000000); ous.Write((Int32)0x00000000);
			ous.Write((Int32)0x00000000); ous.Write((Int32)0x00010000); ous.Write((Int32)0x00000000);
			ous.Write((Int32)0x00000000); ous.Write((Int32)0x00000000); ous.Write((Int32)0x40000000);

			// Big chunk of zeros for no know reason:
			for (int i = 0; i < 6; i++) {
				ous.Write((Int32)0x00000000); // why have both version numbers *AND* reserved values?
			}

			// Weird value: "next_track_id";
			// This is meant to be the next, non-zero id that isn't currently used in the file.
			// Thankfully if we set it to 0xFFFFFFFF, it signals that a search should be used.
			ous.Write((UInt32)0xFFFFFFFFu);
		}

		public override void Prepare () {
		}
	}
}
