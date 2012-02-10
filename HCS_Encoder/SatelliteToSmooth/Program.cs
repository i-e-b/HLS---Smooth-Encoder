using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HCS_Encoder;
using HCS_Encoder.Outputs.SmoothStream.Multiplexing;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using HCS_Encoder.Utilities;
using MP4_Mangler;
using System.Threading;
using HCS_Encoder.Inputs.Buffers;

namespace SatelliteToSmooth {
	static class Program {
		static TranscodeTimeOverlay plug_in = null;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main () {
			/*Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());*/

			
			byte[] data = File.ReadAllBytes(@"C:\temp\sample.ts");
			MemoryStream ms = new MemoryStream(data);
			

			EncoderConfiguration.Configuration config = EncoderConfiguration.Configuration.LoadFromFile(@"C:\temp\dummy_only.xml");
			EncoderController encoder = new EncoderController(config);
			#region Trick mode: encoder with no capture devices (so we can spoon-feed it content)
			encoder.DryRun = true;
			encoder.Start();
			encoder.PauseCapture();
			encoder.ClearBuffers();
			encoder.DryRun = false;
			encoder.MinimumBufferPopulation = 15; // to allow re-ordering of B-frames
			#endregion

			plug_in = new TranscodeTimeOverlay();
			encoder.RegisterPlugin(plug_in); // show captured time over recorded time.

			MpegTS_Demux demux = new MpegTS_Demux();
			demux.FeedTransportStream(ms, 0L);

			DecoderJob decode = new DecoderJob();
			EncoderBridge.InitialiseDecoderJob(ref decode, @"C:\temp\sample.ts");

			Console.WriteLine(decode.videoWidth + "x" + decode.videoHeight);
			double a_time = -1, v_time = -1;
			MediaFrame mf = new MediaFrame();

			byte[] IMAGE = new byte[decode.videoWidth * decode.videoHeight * 16];
			short[] AUDIO = new short[decode.MinimumAudioBufferSize * 2];

			List<GenericMediaFrame> AudioFrames = demux.GetAvailableAudio();
			List<GenericMediaFrame> VideoFrames = demux.GetAvailableVideo();
			VideoFrames.Sort((a, b) => a.FramePresentationTime.CompareTo(b.FramePresentationTime));
			AudioFrames.Sort((a, b) => a.FramePresentationTime.CompareTo(b.FramePresentationTime));

			double dv_time = p2d((long)VideoFrames.Average(a => a.FrameDuration));
			double da_time = p2d((long)AudioFrames.Average(a => a.FrameDuration));

			GCHandle pinX = GCHandle.Alloc(IMAGE, GCHandleType.Pinned);
			mf.Yplane = pinX.AddrOfPinnedObject();

			GCHandle pinY = GCHandle.Alloc(AUDIO, GCHandleType.Pinned);
			mf.AudioBuffer = pinY.AddrOfPinnedObject();

			int i = 0, j=0;
			while (EncoderBridge.DecodeFrame(ref decode, ref mf) >= 0) {
				if (mf.VideoSize > 0) {
					Bitmap img = new Bitmap(decode.videoWidth, decode.videoHeight, decode.videoWidth * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, mf.Yplane);
					img.RotateFlip(RotateFlipType.RotateNoneFlipY); // because decode put things the TL->BR, where video capture is BL->TR.

					if (v_time < 0) v_time = p2d(VideoFrames[i].FramePresentationTime);
					else v_time += dv_time; // p2d(VideoFrames[i].FrameDuration); // using dv_time smooths things

					encoder.ForceInsertFrame(img, v_time);
					Console.Write("v");
					i++;
				}

				if (mf.AudioSize > 0) {
					if (mf.AudioSize > 441000) {
						Console.Write("@"); // protect ourselves from over-size packets!
					} else {
						short[] samples = new short[mf.AudioSize];
						Marshal.Copy(mf.AudioBuffer, samples, 0, samples.Length);
						
						if (a_time < 0) a_time = p2d(AudioFrames[j].FramePresentationTime);
						else a_time += p2d(AudioFrames[j].FrameDuration);

						encoder.ForceInsertFrame(new TimedSample(samples, a_time));
						Console.Write("a");
					}
					j++;
				}

				Application.DoEvents();
				mf.VideoSize = 0;
				mf.AudioSize = 0;
			}

			pinX.Free();
			pinY.Free();

			encoder.MinimumBufferPopulation = 1; // let the buffers empty out

			Console.WriteLine("\r\nEND\r\n");

			Thread.Sleep(2000);
			encoder.Stop();
			EncoderBridge.CloseDecoderJob(ref decode);
		}

		private static double p2d (long p) {
			return TimeSpan.FromTicks(p).TotalSeconds;
		}
	}
}
