using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EncoderConfiguration {
	[Serializable]
	public class AudioCapture {

		public int CaptureDeviceNumber { get; set; }
		public int SampleRate { get; set; }
		public int Channels { get; set; }
	}
}
