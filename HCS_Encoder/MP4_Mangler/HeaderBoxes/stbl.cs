using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MP4_Mangler.HeaderBoxes {
	/// <summary>
	/// Container for 'stsd', 'stts' and 'ctts'
	/// </summary>
	public class stbl : Box{
		private bool _childrenAdded = false;

		/// <summary>
		/// Create an empty stbl.
		/// Add your 'stsd' after creation.
		/// Empty 'stts' and 'ctts' are added at Prepare()
		/// </summary>
		public stbl():base("stbl") {

		}


		/// <summary>
		/// Create and add a pair of empty composition tables (not used in fragment mode)
		/// </summary>
		public override void Prepare () {
			if (!_childrenAdded) {
				stts sample_to_time = new stts();
				AddChild(sample_to_time);
				ctts composition_to_time = new ctts(); // spec says not needed, but every file I inspect has an empty one.
				AddChild(composition_to_time);

				_childrenAdded = true;
			}
		}
	}
}
