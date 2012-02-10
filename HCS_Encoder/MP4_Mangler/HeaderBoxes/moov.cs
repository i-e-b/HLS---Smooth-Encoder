using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MP4_Mangler.HeaderBoxes {

	/// <summary>
	/// Root level media format box.
	/// Sibling of ftyp, moof, mdat, mvex and mfra
	/// </summary>
	public class moov: Box {
		protected mvhd head;
		protected trak structure;
			
		public moov(params MediaStream[] Streams):base("moov") {
			// Add a basic header:
			head = new mvhd();
			AddChild(head);

			// Add a track record for each stream (specific to format)
			foreach (var stream in Streams) {
				structure = new trak(stream.Width, stream.Height, stream.TrackId);
				AddChild(structure);
			}

			// add extensions to mark as fragmented:
			mvex extensions = new mvex(Streams);
			AddChild(extensions);
		}

		public override void Prepare () {
		}
	}
}
