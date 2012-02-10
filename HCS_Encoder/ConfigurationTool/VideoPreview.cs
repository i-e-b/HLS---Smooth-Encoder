using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConfigurationTool {
	public partial class VideoPreview : Form {
		private HCS_Encoder.VideoCapture cam;
		private bool working = false;

		public VideoPreview (HCS_Encoder.VideoCapture CaptureDevice) {
			InitializeComponent();

			cam = CaptureDevice;
			cam.FrameAvailable += new EventHandler<HCS_Encoder.VideoDataEventArgs>(cam_FrameAvailable);
			this.SetClientSizeCore(cam.TargetFrameSize.Width, cam.TargetFrameSize.Height);
			cam.Start();
		}

		void cam_FrameAvailable (object sender, HCS_Encoder.VideoDataEventArgs e) {
			if (working) return;
			working = true;
			e.Frame.RotateFlip(RotateFlipType.RotateNoneFlipY);

			Bitmap newImg = new Bitmap(e.Frame);
			pictureBox1.Image = newImg;
			this.Invalidate();
			working = false;
		}

		private void VideoPreview_FormClosing (object sender, FormClosingEventArgs e) {
			working = true;
			cam.Pause();
			cam.Dispose();
		}
	}
}
