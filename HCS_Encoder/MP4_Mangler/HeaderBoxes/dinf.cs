using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	/// <summary>
	/// Data information box.
	/// This generates the bare minimum for sending to IIS.
	/// </summary>
	public class dinf: Box {

		public dinf () : base("dinf") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			byte[] minimum = new byte[] {
				0x00, 0x00, 0x00, 0x1C, 0x64, 0x72, 0x65, 0x66,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
				0x00, 0x00, 0x00, 0x0C, 0x75, 0x72, 0x6C, 0x20,
				0x00, 0x00, 0x00, 0x01
			}; // minimal dref>url (refers only to this file)

			ous.WriteForward(minimum);
		}

		public override void Prepare () {
		}
	}
}
