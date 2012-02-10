using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	/// <summary>
	/// overall duration of movie.
	/// </summary>
	public class mehd : FullBox {

		public mehd () : base (0x01, 0, "mehd") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			ous.Write((UInt64)0); // test value
		}

		public override void Prepare () {
		}
	}
}
