using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.FragmentBoxes {
	/// <summary>
	/// Media fragment header box
	/// </summary>
	public class mfhd: FullBox {
		/// <summary>
		/// Create a new header box
		/// </summary>
		public mfhd (UInt32 SequenceNumber): base(0,0, "mfhd") {
			// I think this box is a total waste of up to 24 bytes.
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);
			ous.Write(SequenceNumber);
			// that's it. Done...
		}
		public override void Prepare () {
		}
	}
}
