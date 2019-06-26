using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EncoderConfiguration {
	[Serializable]
	public class Encoder {
		public int OutputWidth { get; set; }
		public int OutputHeight { get; set; }
		public int VideoBitrate { get; set; }
		public int FragmentSeconds { get; set; }

		/// <summary>
		/// Set of 1..0 values. Factors of main size & bitrate to encode.
		/// Single bitrate should have: {1.0};
		/// Example multiple bitrate: {1.0, 0.7, 0.5, 0.3};
		/// </summary>
		[XmlArrayItem(ElementName="f")]
		public List<double> ReductionFactors { get; set; }

		public string LocalSystemOutputFolder { get; set; }
		public string LocalSystemFilePrefix { get; set; }
	}
}
