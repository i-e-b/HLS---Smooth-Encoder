using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MP4_Mangler.FragmentBoxes {
	/// <summary>
	/// Track fragment container.
	/// Frames sent here by 'moof'
	/// </summary>
	public class traf: Box {
		public UInt32 TrackId { get; set; }
		protected tfhd Header = null;

		/// <summary>
		/// Create a new track container, with appropriate ID.
		/// </summary>
		public traf (UInt32 TrackID): base("traf") {
			_children = new List<Box>();
			TrackId = TrackID;
			Header = new tfhd(TrackId);
			AddChild(Header);
		}

		/// <summary>
		/// Add a frame to this traf
		/// </summary>
		public void AddFrame (GenericMediaFrame frame) {
			if (_children.Last() is trun) { // looking at .ismv files in a hex editor, they use multiple frames/trun.
				trun ot = _children.Last() as trun;
				ot.AddFrame(frame);
			} else {
				var t = new trun();
				t.AddFrame(frame);
				AddChild(t);
			}
			Header.AddFrame(frame);
		}


		public override void Prepare () {
		}
	}
}
