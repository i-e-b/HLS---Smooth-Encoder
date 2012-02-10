using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HCS_Encoder.Inputs.Buffers {
	public class TimedImage : IComparable<TimedImage> {
		public byte[] Luma; // Also known as 'Y'
		public byte[] Cr; // Chroma Red, 'u'
		public byte[] Cb; // Chroma Blue, 'v'

		public int Width, Height;

		public double Seconds { get; set; }

		public TimedImage (double time, int width, int height) {
			Luma = new byte[(width * height) + 256]; // a little bit of spare room
			Cr = new byte[(width * height) + 256];
			Cb = new byte[(width * height) + 256];

			Width = width;
			Height = height;

			Seconds = time;
		}

		public int CompareTo (TimedImage other) {
			return this.Seconds.CompareTo(other.Seconds);
		}
	}
}
