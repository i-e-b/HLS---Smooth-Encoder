using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MP4_Mangler.HeaderBoxes {
	/// <summary>
	/// Media info container box.
	/// </summary>
	public class minf: Box {

		/// <summary>
		/// Create a new track info box. Determines audio or video based on (width*height > 0?)
		/// </summary>
		public minf (int Width, int Height, int TrackId):base("minf") {
			int pixels = Width * Height;

			if (pixels > 0) {
				vmhd video_header = new vmhd();
				AddChild(video_header);
			} else {
				smhd audio_header = new smhd();
				AddChild(audio_header);
			}

			// Add 'dinf'
			dinf data_info = new dinf(); // automatically does the minimal structure
			AddChild(data_info);
			// Add 'stbl'
			stbl sample_table = new stbl();
			AddChild(sample_table);
			// add 'stsd'
			if (pixels > 0) {
				stsd_h264 video_startup = new stsd_h264();
				sample_table.AddChild(video_startup);
			} else {
				stsd_mp3 audio_startup = new stsd_mp3();
				sample_table.AddChild(audio_startup);
			}
		}

		public override void Prepare () {
		}
	}
}
