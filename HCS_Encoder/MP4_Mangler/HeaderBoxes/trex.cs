using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {
	
	public class trex : FullBox {

		/// <summary>
		/// Create a blank 'trex' for the given track id.
		/// </summary>
		/// <param name="TrackId"></param>
		public trex (int TrackId) : base(0, 0, "trex") {
			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);

			ous.Write((UInt32)TrackId);	// Track
			ous.Write((UInt32)0);		// Default sample description index
			ous.Write((UInt32)0);		// Default sample duration
			ous.Write((UInt32)0);		// Default sample size
			ous.Write((UInt32)0);		// Default sample flags
		}

		public override void Prepare () {
		}
	}
}
