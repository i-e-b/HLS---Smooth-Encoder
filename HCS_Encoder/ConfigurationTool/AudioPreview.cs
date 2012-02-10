using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConfigurationTool {
	public partial class AudioPreview : Form {
		private HCS_Encoder.AudioCapture mic = null;
		private Pen white_pen = new Pen(Color.White, 1.5f);
		private Pen red_pen = new Pen(Color.Red);
		private bool working = false;

		public AudioPreview (HCS_Encoder.AudioCapture CaptureDevice) {
			InitializeComponent();

			mic = CaptureDevice;
			mic.FrameAvailable += new EventHandler<HCS_Encoder.AudioDataEventArgs>(mic_FrameAvailable);
			this.SetClientSizeCore(640, 240);
			mic.Start();
		}

		void mic_FrameAvailable (object sender, HCS_Encoder.AudioDataEventArgs e) {
			if (working) return;
			if (e.Samples.Length < 1) return;
			working = true;
			Bitmap bmp = new Bitmap(640, 240);
			Graphics g = Graphics.FromImage(bmp);
			g.Clear(Color.Black);
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			List<PointF> points = new List<PointF>();
			try {
				double dx = 640.0 / e.Samples.Length;
				double ty = 120.0;
				double dy = 1.0 / 320.0;


				int sw = e.Samples.Length / 640;

				for (int i = 0; i < e.Samples.Length; i += sw) {
					float x = (float)(i * dx);
					float y = (float)(ty + (e.Samples[i] * dy));

					points.Add(new PointF(x, y));
				}

				g.DrawLine(red_pen, 0, (float)(ty + (short.MinValue * dy)), 640, (float)(ty + (short.MinValue * dy)));
				g.DrawLine(red_pen, 0, (float)(ty + (short.MaxValue * dy)), 640, (float)(ty + (short.MaxValue * dy)));
				g.DrawLines(white_pen, points.ToArray());

			} catch { }
			g.Dispose();
			pictureBox1.Image = bmp;
			this.Invalidate();
			working = false;
		}

		private void AudioPreview_FormClosing (object sender, FormClosingEventArgs e) {
			working = true;
			mic.Pause();
			mic.Dispose();
		}
	}
}
