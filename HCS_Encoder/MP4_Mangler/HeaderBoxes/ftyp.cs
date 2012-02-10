using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.HeaderBoxes {

	/// <summary>
	/// Root level File Type box. Sibling of moov, moof, mdat, mvex and mfra
	/// </summary>
	public class ftyp : Box {

		/// <summary>
		/// Create a new ftyp box, assuming isom / iso2 compatibiliy
		/// </summary>
		public ftyp ():base("ftyp") {
			// Write out a string of FOURCC values.
			// There are:
			//		'Major Brand' = "avc1"
			//		'minor version' = 0
			//		'compatible brands' = {"isom", "iso2"}

			UInt32 major_brand = Box.FourCC("avc1");
			UInt32 minor_version = 0U;
			UInt32 cb_brand_1 = Box.FourCC("isom");
			UInt32 cb_brand_2 = Box.FourCC("iso2");

			_data = new System.IO.MemoryStream();
			BigEndianWriter ous = new BigEndianWriter(_data);
			ous.Write(major_brand);
			ous.Write(minor_version);
			ous.Write(cb_brand_1);
			ous.Write(cb_brand_2);
		}

		public override void Prepare () {
		}
	}
}
