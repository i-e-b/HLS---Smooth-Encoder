﻿using System;

namespace EncoderConfiguration {
	[Serializable]
	public class VideoCapture {
		public int CaptureDeviceNumber { get; set; }
		public int InputWidth { get; set; }
		public int InputHeight { get; set; }
		public int InputFrameRate { get; set; }
	}
}
