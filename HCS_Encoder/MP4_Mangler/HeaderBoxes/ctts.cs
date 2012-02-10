using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	/// <summary>
	/// An empty composition-time-to-sample box.
	/// </summary>
	public class ctts : FullBox {

		/// <summary>
		/// Creates a stts with no data (empty box)
		/// </summary>
		public ctts () : base(0, 0, "ctts") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			ous.Write((UInt32)0); // Entry count

		}

		public override void Prepare () {
		}
	}
}
