using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	/// <summary>
	/// Handler reference box
	/// </summary>
	public class hdlr: FullBox {

		public hdlr (int Width, int Height) : base(0, 0, "hdlr") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			ous.Write((UInt32)0); // 'pre_defined', which seems to be the same as 'reserved'

			UInt32 handler_type = 0;
			int pixels = Width * Height;
			if (pixels > 0) handler_type = Box.FourCC("vide");
			else handler_type = Box.FourCC("soun");

			ous.Write(handler_type);

			ous.Write((UInt32)0); // a few more reserved fields...
			ous.Write((UInt32)0); // ...
			ous.Write((UInt32)0); // ..!

			ous.Write((UInt32)0); // Null-terminated user readable string. Seems only good for copyrights and secret messages.
		}

		public override void Prepare () {
		}
	}
}
