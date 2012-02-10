using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	public class tkhd: FullBox {

		/// <summary>
		/// Create a new track header
		/// </summary>
		public tkhd (int Width, int Height, int TrackId):base(0x01, 0x0007, "tkhd") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			// Version = 1 means a few 64-bit values:
			UInt64 creation_time = Box.BoxDate(DateTime.Now);
			UInt64 modified_time = creation_time;
			UInt32 track_id = (UInt32)TrackId;
			UInt32 reserved_1 = 0;
			UInt64 duration = 0; // This is unknown for live, and currently unused by Silverlight.
			UInt64 reserved_2 = 0; // spec has this as UInt32[2]

			ous.Write(creation_time);
			ous.Write(modified_time);
			ous.Write(track_id);
			ous.Write(reserved_1);
			ous.Write(duration);
			ous.Write(reserved_2);

			// These all seem pointless:
			Int16 layer = 0;
			Int16 alternate_group = 0;
			int pixels = Width * Height;
			Int16 volume = (pixels > 0) ? ((Int16)0x0000) : ((Int16)0x0100); // spec says this should be 0 for video, 0x0100 for audio.
			Int16 reserved_3 = 0;

			ous.Write(layer);
			ous.Write(alternate_group);
			ous.Write(volume);
			ous.Write(reserved_3);

			// Unity matrix: (a waste of 36 bytes that was already wasted in mvhd)
			ous.Write((Int32)0x00010000); ous.Write((Int32)0x00000000); ous.Write((Int32)0x00000000);
			ous.Write((Int32)0x00000000); ous.Write((Int32)0x00010000); ous.Write((Int32)0x00000000);
			ous.Write((Int32)0x00000000); ous.Write((Int32)0x00000000); ous.Write((Int32)0x40000000);

			UInt32 fx_width = (UInt32)((Width & 0xFFFF) << 16);
			UInt32 fx_height = (UInt32)((Height & 0xFFFF) << 16);

			ous.Write(fx_width);
			ous.Write(fx_height);
		}

		public override void Prepare () {
		}
	}
}
