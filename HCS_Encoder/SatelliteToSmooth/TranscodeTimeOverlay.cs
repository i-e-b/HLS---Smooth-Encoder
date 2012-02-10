using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HCS_Encoder.Inputs.Processing;
using HCS_Encoder;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SatelliteToSmooth {
	/// <summary>
	/// Used to match with 'TimecodeOverlay' in the example encoder.
	/// </summary>
	public class TranscodeTimeOverlay: IVideoProcessor{
		private long frame = 0;

		public void ProcessFrame (Image Frame, double CaptureTime) {
			
			Font f = new Font("Consolas", 12.0f);
			Brush K = new SolidBrush(Color.Black);
			Brush Y = new SolidBrush(Color.OrangeRed);

			TimeSpan ct = TimeSpan.FromSeconds(CaptureTime);
			string msg = "Frame " + frame + ", capture time = " + ct.ToString();

			frame++;

			using (Graphics g = Graphics.FromImage(Frame)) {
				g.Transform = new System.Drawing.Drawing2D.Matrix(1, 0, 0, -1, 0, Frame.Height); // flip - Y

				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
				g.DrawString(msg, f, K, 10.0f, Frame.Height - 28.0f);
				g.DrawString(msg, f, Y, 9.0f, Frame.Height - 29.0f);

				//g.ResetTransform();
				g.Flush();
				g.Dispose();
			}
		}
	}
}
