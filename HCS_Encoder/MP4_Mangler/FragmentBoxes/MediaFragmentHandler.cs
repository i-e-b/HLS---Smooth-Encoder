using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.FragmentBoxes {
	/// <summary>
	/// Handles the creation of MP4 boxes to store movie fragments.
	/// </summary>
	public class MediaFragmentHandler {
		protected moof FragmentInfo;
		protected mdat MovieData;


		public MediaFragmentHandler (uint SequenceNumber, long OffsetTimestamp) {
			FragmentInfo = new moof(SequenceNumber, OffsetTimestamp);
			MovieData = new mdat();
		}

		/// <summary>
		/// Add frames to this fragment.
		/// GOPs should be closed inside a fragment (no external refs)
		/// </summary>
		/// <param name="TrackID">Track ID (should be associated with 'moov' headers)</param>
		/// <param name="frame">Frame data</param>
		public void AddFrame (UInt32 TrackID, GenericMediaFrame frame) {
			FragmentInfo.AddFrame(TrackID, frame);
			MovieData.AddFrame(TrackID, frame);
		}

		public long ClaimedDuration {
			get {
				return FragmentInfo.CalulatedDuration;
			}
		}

		/// <summary>
		/// Returns the current fragment's header data.
		/// </summary>
		public byte[] MoofData () {
			return FragmentInfo.deepData();
		}

		/// <summary>
		/// Returns the current fragment's frame data.
		/// </summary>
		public byte[] MdatData () {
			return MovieData.deepData();
		}

		/// <summary>
		/// Returns the current fragment's storage (header+frame) data.
		/// </summary>
		public byte[] FormatData () {
			byte[] moof_data = FragmentInfo.deepData();
			byte[] mdat_data = MovieData.deepData();

			MemoryStream ms = new MemoryStream(moof_data.Length + mdat_data.Length);
			ms.Write(moof_data, 0, moof_data.Length);
			ms.Write(mdat_data, 0, mdat_data.Length);

			return ms.ToArray();
		}
	}
}
