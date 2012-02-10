using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HCS_Encoder.Inputs.Processing;
using HCS_Encoder;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace _ExampleController {
	public class VideoOverlay : IVideoProcessor {
		ToneDetector detector;
		Watermark wm;
		RectangleF wmbound = new RectangleF();
		private int frame_count = 0;
		public Bitmap Thumbnail = null;
		Font f = new Font("Stencil", 12.0f);
		Brush K = new SolidBrush(Color.Black);
		Brush Y = new SolidBrush(Color.Yellow);

		public VideoOverlay (ToneDetector Tone) {
			detector = Tone;
			Thumbnail = new Bitmap(256, 192);
		}

		public void ProcessFrame (System.Drawing.Image Frame, double CaptureTime) {
			if (Frame == null) return;
			double confidence = -1.0;
			if (detector != null) confidence = detector.ToneConfidence * 100.0;



			//string msg = "HCS Encoder (Tone confidence: " + confidence.ToString("0.00") + "%)";
			//if (confidence < 0.0) {
			//	msg = "HCS Encoder System";
			//}

			//string msg = "HCS Encoder System Internal Testing";


			using (Graphics g = Graphics.FromImage(Frame)) {
				g.Transform = new Matrix(1, 0, 0, -1, 0, Frame.Height); // flip - Y

				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
				//g.DrawString(msg, f, K, 10.0f, 10.0f);
				//g.DrawString(msg, f, Y, 9.0f, 9.0f);

				try {
					if (wm == null) wm = new Watermark(null, 40);
					wmbound = wm.Bounds;
					// store target location
					wmbound.X = Frame.Width - wmbound.Width - 10.0f;
					wmbound.Y = Frame.Height - wmbound.Height - 10.0f;
				} catch { }

				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.TranslateTransform(wmbound.X, wmbound.Y);
				var br = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
				foreach (var path in wm.paths) {
					g.FillPath(br, path);
				}

				//g.ResetTransform();
				g.Flush();
				g.Dispose();
			}

			frame_count++;
			if (frame_count > 25) { // take a snap-shot at regular intervals
				frame_count = 0;
				using (Graphics gt = Graphics.FromImage(Thumbnail)) {
					gt.Clear(Color.Black);
					gt.Transform = new System.Drawing.Drawing2D.Matrix(1, 0, 0, -1, 0, Thumbnail.Height); // flip - Y
					gt.DrawImage(Frame, new RectangleF(0, 0, 256, 192));
					gt.Flush();
					gt.Dispose();
				}
			}
		}
	}
}
