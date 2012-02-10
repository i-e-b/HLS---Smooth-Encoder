using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MP4_Mangler.HeaderBoxes {
	public class mdia: Box {
		protected mdhd header;

		public mdia ():base("mdia") {
			header = new mdhd();
			AddChild(header);
		}

		public override void Prepare () {
		}
	}
}
