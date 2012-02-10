using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MP4_Mangler.HeaderBoxes {

	/// <summary>
	/// Root level fragment signalling box.
	/// Sibling of ftyp, moof, mdat and mfra
	/// </summary>
	public class mvex : Box {

		/// <summary>
		/// Creates an 'mvex' box, which signals that the containing file is
		/// at least partially fragmented.
		/// Also adds blank 'trex' boxes for each track ID specified
		/// </summary>
		public mvex (params MediaStream[] MediaStreams) : base ("mvex") {
			// 'mvex' itself is just a signaller. The 'trex' boxes are meant to save size, but they actually don't save very much.

			mehd header = new mehd();
			AddChild(header);

			foreach (var track in MediaStreams) {
				trex t = new trex(track.TrackId);
				AddChild(t);
			}
		}

		public override void Prepare () {
		}
	}
}
