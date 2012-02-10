using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	/// <summary>
	/// An empty time-to-sample box.
	/// </summary>
	public class stts: FullBox {

		/// <summary>
		/// Creates a stts with no data (empty box)
		/// </summary>
		public stts () : base(0, 0, "stts") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			ous.Write((UInt32)0); // Entry count

		}

		public override void Prepare () {
		}
	}
}
