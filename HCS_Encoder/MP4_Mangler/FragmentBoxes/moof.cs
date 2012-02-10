using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MP4_Mangler.FragmentBoxes {
	/// <summary>
	/// Root level Movie fragment container box.
	/// Add all video frames before calling tree data methods.
	/// Sibling of moov, ftyp, mdat, mvex and mfra
	/// </summary>
	public class moof: Box {
		protected mfhd Header;
		protected long Duration, Offset;
		protected bool Prepared;
		protected traf Tracks;
		protected sdtp Dependencies;

		public long CalulatedDuration { get { return Duration; } }

		/// <summary>
		/// Create a new fragment box.
		/// Generates correct children and includes sequence numbers
		/// </summary>
		public moof (UInt32 SequenceNumber, long BaseTime): base("moof") {
			Header = new mfhd(SequenceNumber);
			AddChild(Header);
			Duration = 0U;
			Offset = BaseTime;
			Prepared = false;
		}

		/// <summary>
		/// Add a frame to the track
		/// </summary>
		public void AddFrame (UInt32 TrackID, GenericMediaFrame frame) {
			// Ensure correct boxes
			if (Tracks == null) {
				Tracks = new traf(TrackID);
				AddChild(Tracks);
			}
			if (TrackID != Tracks.TrackId) throw new Exception("Fragments should only contain a single track");
			if (Dependencies == null) {
				Dependencies = new sdtp();
				AddChild(Dependencies);
			}

			// Add this frame
			Tracks.AddFrame(frame);
			Dependencies.AddFrame(frame);
			// Add duration
			Duration += frame.FrameDuration;
		}


		/// <summary>
		/// Prepare for data gathering. Add duration box.
		/// </summary>
		public override void Prepare () {
			if (Prepared) return;
			Prepared = true;

			ExtraBoxes.SmoothFragmentTimer ftimer = new ExtraBoxes.SmoothFragmentTimer(Offset, Duration);
			Tracks.AddChild(ftimer);
		}
	}
}
