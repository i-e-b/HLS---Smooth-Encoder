using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using HCS_Encoder;
using HCS_Encoder.Outputs.SmoothStream.Multiplexing;
using HCS_Encoder.Utilities;
using MP4_Mangler;
using HCS_Encoder.Inputs.Buffers;

namespace ExistingMediaConverter {
	public partial class Form1 : Form {
		public string SourceFile { get; private set; }
		public string DestFile { get; private set; }
		public bool running = true;

		public Form1 () {
			InitializeComponent();
		}

		private void SourceFileDialog_FileOk (object sender, CancelEventArgs e) {
			string path = SourceFileDialog.FileName;

			// Show details and enable save button.
			SourceLabel.Text = "Using: " + path;
			SourceFile = path;
			ChooseDestinationButton.Enabled = true;

			// FUTURE: Pre-check that video is acceptable.
		}

		private void ChooseSourceButton_Click (object sender, EventArgs e) {
			SourceFileDialog.ShowDialog();
		}

		private void ChooseDestinationButton_Click (object sender, EventArgs e) {
			ConfigFileDialog.ShowDialog();
		}

		private void DestFileDialog_FileOk (object sender, CancelEventArgs e) {
			DestFile = ConfigFileDialog.FileName;
			ConvertButton.Enabled = true;
		}

		private void ConvertButton_Click (object sender, EventArgs e) {
			ConvertButton.Enabled = false;
			ChooseSourceButton.Enabled = false;
			ChooseDestinationButton.Enabled = false;

			DoTranscode();

			MessageBox.Show("Complete!");
		}

		private void DoTranscode () {
			// Start decode job (gets some basic information)
			DecoderJob decode = new DecoderJob();
			EncoderBridge.InitialiseDecoderJob(ref decode, SourceFile);

			if (decode.IsValid == 0) {
				MessageBox.Show("Sorry, the source file doesn't appear to be valid");
				return;
			}

			// Load config, then tweak to match input
			EncoderConfiguration.Configuration config = EncoderConfiguration.Configuration.LoadFromFile(DestFile);
			config.Audio.Channels = decode.AudioChannels;
			if (config.Audio.Channels > 0) {
				config.Audio.CaptureDeviceNumber = -2; // dummy
			} else {
				config.Audio.CaptureDeviceNumber = -1; // no audio
			}

			if (decode.videoWidth * decode.videoHeight > 0) {
				config.Video.CaptureDeviceNumber = -2; // dummy device
				config.Video.InputFrameRate = (int)decode.Framerate;
				if (config.Video.InputFrameRate < 1) {
					config.Video.InputFrameRate = 25; // don't know frame rate, so adapt.
				}

				config.EncoderSettings.OutputHeight = decode.videoHeight;
				config.EncoderSettings.OutputWidth = decode.videoWidth;
			} else {
				config.Video.CaptureDeviceNumber = -1; // no video
			}

			#region Start up encoder in a trick mode
			EncoderController encoder = new EncoderController(config);
			encoder.DryRun = true;
			encoder.Start();
			encoder.PauseCapture();
			encoder.ClearBuffers();
			encoder.DryRun = false;
			encoder.MinimumBufferPopulation = 5; // to allow re-ordering of weird frame timings
			#endregion

			Console.WriteLine(decode.videoWidth + "x" + decode.videoHeight);
			double a_time = -1, v_time = -1;
			MediaFrame mf = new MediaFrame();

			byte[] IMAGE = new byte[decode.videoWidth * decode.videoHeight * 16];
			short[] AUDIO = new short[decode.MinimumAudioBufferSize * 2];

			GCHandle pinX = GCHandle.Alloc(IMAGE, GCHandleType.Pinned);
			mf.Yplane = pinX.AddrOfPinnedObject();

			GCHandle pinY = GCHandle.Alloc(AUDIO, GCHandleType.Pinned);
			mf.AudioBuffer = pinY.AddrOfPinnedObject();

			int i = 0, j = 0;
			while (EncoderBridge.DecodeFrame(ref decode, ref mf) >= 0) {
				if (mf.VideoSize > 0) {
					Bitmap img = new Bitmap(decode.videoWidth, decode.videoHeight, decode.videoWidth * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, mf.Yplane);
					img.RotateFlip(RotateFlipType.RotateNoneFlipY); // because decode put things the TL->BR, where video capture is BL->TR.

					v_time = mf.VideoSampleTime;
					//v_time += 1.0 / config.Video.InputFrameRate;
					try { encoder.ForceInsertFrame(img, v_time); } catch { }
					Console.Write("v");
					i++;
				}

				if (mf.AudioSize > 0) {
					if (mf.AudioSize > 441000) {
						Console.Write("@"); // protect ourselves from over-size packets!
					} else {
						short[] samples = new short[mf.AudioSize];
						Marshal.Copy(mf.AudioBuffer, samples, 0, samples.Length);

						a_time = mf.AudioSampleTime;

						encoder.ForceInsertFrame(new TimedSample(samples, a_time));
						Console.Write("a");
					}
					j++;
				}

				//while (encoder.AudioQueueLength > 50 || encoder.VideoQueueLength > 50) {
					if (!encoder.EncoderRunning) throw new Exception("Encoder broken!");
					Thread.Sleep((int)(250 / config.Video.InputFrameRate));
				//}

				this.Text = "V (" + i + "/" + v_time + ") | A (" + j + "/" + a_time + ")";

				Application.DoEvents();

				if (!running) break;

				mf.VideoSize = 0;
				mf.AudioSize = 0;
			}

			pinX.Free();
			pinY.Free();

			encoder.MinimumBufferPopulation = 1; // let the buffers empty out

			Console.WriteLine("\r\nEND\r\n");

			Thread.Sleep(5000);
			encoder.Stop();
			EncoderBridge.CloseDecoderJob(ref decode);
		}

		private void Form1_FormClosing (object sender, FormClosingEventArgs e) {
			running = false;
		}
	}
}
