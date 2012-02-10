using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.ExtraBoxes {
	public class SmoothSmil:Box {

		/// <summary>
		/// Create a new SMIL container box. Please supply your own SMIL file.
		/// </summary>
		public SmoothSmil (byte[] SmilFile):base("uuid") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			// I have no idea what this GUID relates to or what it means.
			// I took this value from a socket capture. Many Bothans died to bring us this information.
			//Guid uuid_head = new Guid("A5 D4 0B 30 E8 14 11 DD BA 2F 08 00 20 0C 9A 66");
			ous.WriteForward(new byte[] { 0xA5, 0xD4, 0x0B, 0x30, 0xE8, 0x14, 0x11, 0xDD, 0xBA, 0x2F, 0x08, 0x00, 0x20, 0x0C, 0x9A, 0x66 });

			UInt32 blank = 0U;
			ous.Write(blank);
			ous.WriteForward(SmilFile);
		}

		public override void Prepare () {
		}
	}
}
