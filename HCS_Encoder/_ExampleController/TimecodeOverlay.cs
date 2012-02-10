using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HCS_Encoder.Inputs.Processing;
using HCS_Encoder;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace _ExampleController {
	public class TimecodeOverlay : IVideoProcessor{
		Font f = new Font("Consolas", 12.0f);
		Brush K = new SolidBrush(Color.Black);
		Brush Y = new SolidBrush(Color.PowderBlue);

		public void ProcessFrame (Image Frame, double CaptureTime) {
			

			TimeSpan ct = TimeSpan.FromSeconds(CaptureTime);
			string msg = ct.ToString();

			using (Graphics g = Graphics.FromImage(Frame)) {
				g.Transform = new System.Drawing.Drawing2D.Matrix(1, 0, 0, -1, 0, Frame.Height); // flip - Y

				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
				g.DrawString(msg, f, K, 10.0f, Frame.Height - 18.0f);
				g.DrawString(msg, f, Y, 9.0f, Frame.Height - 19.0f);

				//g.ResetTransform();
				g.Flush();
				g.Dispose();
			}
		}
	}
}
