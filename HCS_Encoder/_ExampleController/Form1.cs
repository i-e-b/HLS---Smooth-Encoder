using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HCS_Encoder.Utilities;
using EncoderConfiguration;
using System.IO;

namespace _ExampleController {
	public partial class Form1 : Form {
		Configuration config;
		EncoderController encoder;
		ToneDetector _toneDetector;
		VideoOverlay _videoOverlay;
		TimecodeOverlay _timeOverlay;

		private EncoderController PrepareEncoder () {
			var new_encoder = new EncoderController(config);
			new_encoder.UseAbsoluteTime = true;
			
			if (usePlugsCheck.Checked) {

				// Plug-ins for UK Parliament encoding
				if (_toneDetector == null) {
					_toneDetector = new ToneDetector(980);
					_toneDetector.PowerGate = 15;
					_toneDetector.TemporalSmoothing = 350;
				}

				//new_encoder.RegisterPlugin(_toneDetector);

				if (_timeOverlay == null) _timeOverlay = new TimecodeOverlay();
				new_encoder.RegisterPlugin(_timeOverlay);

				if (_videoOverlay == null) _videoOverlay = new VideoOverlay(_toneDetector);
				new_encoder.RegisterPlugin(_videoOverlay);
			}
			return new_encoder;
		}

		#region Status bits
		/// <summary>
		/// Update the status view
		/// </summary>
		private void StatusTimer_Tick (object sender, EventArgs e) {
			try {
				StatusIdle();

				if (encoder != null && encoder.EncoderRunning) {
					StatusActive();
				}

				if (_toneDetector != null) {
					TonePct.Text = (_toneDetector.ToneConfidence * 100.0).ToString("0.00") + "%";
					if (_toneDetector.ToneConfidence >= 0.5) TonePct.ForeColor = Color.Red;
					else TonePct.ForeColor = Color.Black;
					if (previewCheck.Checked) {
						try {
							if (tbox.Image != null) tbox.Image.Dispose();
							tbox.Image = new Bitmap(_toneDetector.DrawOscillator());
						} catch { }
					}
				} else {
					TonePct.Text = "INACTIVE";
					TonePct.ForeColor = Color.Gray;
				}

				if (_videoOverlay != null) {
					if (previewCheck.Checked) {
						try {
							if (pbox.Image != null) pbox.Image.Dispose();
							pbox.Image = new Bitmap(_videoOverlay.Thumbnail);
						} catch { }
					}
				}
			} catch { }
		}

		private void StatusActive () {
			RunningStatus.Text = "Encoding chunk "+encoder.FragmentNumber+"\r\n(" + encoder.FrameCount.ToString("00000000") + " frames so far)";

			EncFPS.Text = encoder.AverageFramesPerSecond.ToString("0.00");
			tV.Text = encoder.VideoSampleTime.ToString();
			tA.Text = encoder.AudioSampleTime.ToString();
			qA.Text = encoder.AudioQueueLength.ToString();
			qV.Text = encoder.VideoQueueLength.ToString();

			ExtraFields();
		}

		private void ExtraFields () {
			string ef = "";

			if (!encoder.EncoderRunning) ef += "not ";
			ef += "running; ";

			if (encoder.DryRun) ef += "dry run; ";
			if (encoder.CleanupDeliveredChunks) ef = "clean up; ";
			if (encoder.EnableOutput) ef += "output; ";
			ef += "buf " + encoder.MinimumBufferPopulation + "; ";

			ef += "job count "+encoder.JobCount + "; ";

			infoField.Text = ef;
		}

		private void StatusIdle () {
			RunningStatus.Text = "Waiting";
			infoField.Text = "system is idle";

			// Process and memory info:
			var proc = System.Diagnostics.Process.GetCurrentProcess();

			ManagedMem.Text = (GC.GetTotalMemory(false) / 1048576.0).ToString("0.00") +" MB";
			SysMem.Text = (proc.WorkingSet64 / 1048576.0).ToString("0.00") + " MB";


			EncFPS.Text = "0.00";
			tV.Text = "00:00:00";
			tA.Text = "00:00:00";
			qA.Text = "0";
			qV.Text = "0";
		}

		#endregion

		#region UI bits

		public Form1 () {
			InitializeComponent();
		}

		private void LoadConfig_Click (object sender, EventArgs e) {
			OpenConfigDialog.ShowDialog();
		}

		private void ToggleEncoder_Click (object sender, EventArgs e) {
			if (encoder != null && encoder.EncoderRunning) {
				encoder.Stop();
				ToggleEncoder.Text = "Start Encoding";
				usePlugsCheck.Enabled = true;
				encoder.Dispose();
				encoder = null;
				GC.Collect();
				//GC.WaitForFullGCComplete();
			} else {
				if (encoder != null) {
					encoder.Dispose();
					encoder = null;
				}
				encoder = PrepareEncoder();
				usePlugsCheck.Enabled = false;
				encoder.Start(); // actual encode will happen on a different thread.
				ToggleEncoder.Text = "Stop Encoding";
			}
		}

		private void OpenConfigDialog_FileOk (object sender, CancelEventArgs e) {
			try {
				config = Configuration.LoadFromFile(OpenConfigDialog.FileName);
			} catch {
				config = null;
			}

			if (config == null) {
				ToggleEncoder.Enabled = false;
				MessageBox.Show("Sorry, can't open that file.\r\nPlease check that it is valid, or recreate using the configuration tool.", "HCS Encoder Demo");
			} else {
				ToggleEncoder.Enabled = true;
				this.Text = "HCS Encoder: "+ Path.GetFileNameWithoutExtension(OpenConfigDialog.FileName);
			}
		}

		private void Form1_FormClosing (object sender, FormClosingEventArgs e) {
			if (encoder != null && encoder.EncoderRunning) {
				encoder.Stop();
			}
		}
		#endregion

		private void PauseOutputBtn_Click (object sender, EventArgs e) {
			if (encoder != null && encoder.EncoderRunning) encoder.EnableOutput = false;
		}

		private void ResumeOutputBtn_Click (object sender, EventArgs e) {
			if (encoder == null) return;
			int target_chunk = encoder.FragmentNumber - 1;
			if (encoder.EncoderRunning) encoder.ContinueOutput(target_chunk);
		}
	}
}
