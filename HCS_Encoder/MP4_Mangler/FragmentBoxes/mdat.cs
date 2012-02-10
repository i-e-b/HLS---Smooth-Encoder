using System;
using System.Collections.Generic;
using System.IO;

namespace MP4_Mangler.FragmentBoxes {
	/// <summary>
	/// Root level Movie data container box.
	/// Add all video frames before calling tree data methods.
	/// Sibling of moov, ftyp, moof, mvex and mfra
	/// </summary><remarks>
	/// mdat is used to store raw frame data, away from the data struture.
	/// I'm not sure why -- it would seem to make more sense to keep frames chunked,
	/// like MPEG-TS does.
	/// </remarks>
	public class mdat:Box {

		/// <summary>
		/// Prepare a new raw frame data atom.
		/// </summary>
		public mdat ():base("mdat") {
			_data = new MemoryStream();
		}

		/// <summary>
		/// Add a frame to the track
		/// </summary>
		public void AddFrame (UInt32 TrackID, GenericMediaFrame frame) {
			_data.Write(frame.FrameData, 0, frame.FrameData.Length);
		}

		public override void Prepare () {
		}
	}
}
