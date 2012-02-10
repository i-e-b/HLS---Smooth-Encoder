using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using HCS_Encoder.Inputs.Processing;

namespace HCS_Encoder {

	/// <summary>
	/// Converts System.Drawing images to scaled YUV420p buffers
	/// at 8bpp/planar
	/// </summary>
	public class ImageToYUV_Buffer {
		#region Inner workings
		private int height, width;
		private byte[] Y, u, v;

		public class TimedImage {
			public byte[] Luma; // Also known as 'Y'
			public byte[] Cr; // Chroma Red, 'u'
			public byte[] Cb; // Chroma Blue, 'v'

			public double Seconds { get; set; }

			public TimedImage (double time, int width, int height) {
				Luma = new byte[width * height *2];
				Cr = new byte[(width / 2) * (height / 2) *2];
				Cb = new byte[(width / 2) * (height / 2) * 2];

				Seconds = time;
			}
		}

		/// <summary>Output buffer width</summary>
		public int Width { get { return width; } }

		/// <summary>Output buffer height</summary>
		public int Height { get { return height; } }

		/// <summary>
		/// Capture time of next frame to be encoded (this should be the minimum of all buffered capture times)
		/// </summary>
		public double NextCaptureTime {
			get {
				if (WaitingFrames.Count < 1) return 0.0;
				return WaitingFrames[0].Seconds;
			}
		}

		private List<TimedImage> WaitingFrames;

		/// <summary>
		/// Number of frames buffered and waiting to be encoded.
		/// </summary>
		public int QueueLength {
			get { lock (WaitingFrames) {return WaitingFrames.Count;} }
		}
		private GCHandle pinY, pinU, pinV;

		#endregion

		/// <summary>
		/// Respond to capture event.
		/// This should return as fast as possible.
		/// </summary>
		public void HandleCapturedFrame (object sender, VideoDataEventArgs e) {
			if (e.Frame != null) {
				TimedImage ti = new TimedImage(e.CaptureTime, width, height);

				try {
					if (ProcessorQueue != null) { // process the source image
						foreach (var item in ProcessorQueue) {
							item.ProcessFrame(e.Frame, e.CaptureTime);
						}
					}
				} catch { }

				lock (WaitingFrames) {
					WaitingFrames.Add(ti);
					ResampleBuffer(e.Frame, ref ti.Luma, ref ti.Cr, ref ti.Cb, width, height);
				}

				try {
					if (ProcessorQueue != null) { // process the YUV buffer
						foreach (var item in ProcessorQueue) {
							item.ProcessFrame(ti);
						}
					}
				} catch { }
			}
		}

		/// <summary>
		/// Prepare a set of buffers to accept the images.
		/// Incoming frames will be scaled to match the given width and height.
		/// For best results, capture and buffer sizes should match.
		/// Rescaling does not preserve aspect ratio.
		/// </summary>
		public ImageToYUV_Buffer (int Width, int Height) {
			height = Height;
			width = Width;
			WaitingFrames = new List<TimedImage>(100);
			ProcessorQueue = new List<IVideoProcessor>();
		}

		/// <summary>
		/// Resample an image into a set of YUV420 buffers.
		/// </summary>
		/// <remarks>This is done seperately from frame encoding the improve multi-processor performance</remarks>
		internal unsafe void ResampleBuffer (Bitmap img, ref byte[] Luma, ref byte[] Cr, ref byte[] Cb, int width, int height) {
			if (img == null) return;
			Rectangle r = new Rectangle(0, 0, img.Width, img.Height);
			BitmapData bmp = img.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb); // for speed, always use the bitmap's real pixel format!

			// Do the RGB -> YUV conversion, with chroma sub-sampling & scaling
			try {
				GCHandle _Lx = default(GCHandle), _Crx = default(GCHandle), _Cbx = default(GCHandle);
				try {
					_Lx = GCHandle.Alloc(Luma, GCHandleType.Pinned);
					IntPtr _L = _Lx.AddrOfPinnedObject();
					_Crx = GCHandle.Alloc(Cr, GCHandleType.Pinned);
					IntPtr _Cr = _Crx.AddrOfPinnedObject();
					_Cbx = GCHandle.Alloc(Cb, GCHandleType.Pinned);
					IntPtr _Cb = _Cbx.AddrOfPinnedObject();
					
					EncoderBridge.ScaleAndConvert(
						bmp.Scan0, bmp.Stride, img.Width, img.Height,
						_L, _Cr, _Cb, width, height);

				} finally {
					if (_Lx.IsAllocated) _Lx.Free();
					if (_Crx.IsAllocated) _Crx.Free();
					if (_Cbx.IsAllocated) _Cbx.Free();
				}
			} finally {
				img.UnlockBits(bmp);
			}
		}

		/// <summary>
		/// Emergeny clear-out. Drop all queued frames
		/// </summary>
		internal void WipeBuffer () {
			WaitingFrames.Clear();
			GC.Collect();
		}


		/// <summary>
		/// Remove all frames captured before the given capture time. They will not be encoded.
		/// </summary>
		internal void WipeBufferUntil (double AbandonTime) {
			lock (this) {
				lock (WaitingFrames) {
					while (WaitingFrames.Count > 0 && WaitingFrames[0].Seconds < AbandonTime) {
						WaitingFrames.RemoveAt(0);
					}
				}
			}
			GC.Collect();
		}

		#region Sequential Loading Methods (for camera capture & reliable timed input)

		/// <summary>
		/// Load the buffer into a MediaFrame for the encoder.
		/// IMPORTANT: You must call UnloadFrame after this method is called.
		/// For efficiency, unload as soon as possible.
		/// </summary>
		public void LoadToFrame (ref MediaFrame Frame) {
			try {
				if (WaitingFrames.Count > 0) {
					TimedImage img = null;
					lock (WaitingFrames) {
						WaitingFrames.RemoveAll(a => a == null);
						WaitingFrames.Sort((a, b) => a.Seconds.CompareTo(b.Seconds));

						img = WaitingFrames[0];
						WaitingFrames.RemoveAt(0);
					}

					if (img.Luma == null || img.Cr == null || img.Cb == null) return; // crap frame

					Y = img.Luma;
					u = img.Cr;
					v = img.Cb;

					Frame.VideoSize = (ulong)Y.Length;
					Frame.VideoSampleTime = img.Seconds;

					pinY = GCHandle.Alloc(Y, GCHandleType.Pinned);
					Frame.Yplane = pinY.AddrOfPinnedObject();

					pinU = GCHandle.Alloc(u, GCHandleType.Pinned);
					Frame.Uplane = pinU.AddrOfPinnedObject();

					pinV = GCHandle.Alloc(v, GCHandleType.Pinned);
					Frame.Vplane = pinV.AddrOfPinnedObject();
				} else {
					Frame.Yplane = IntPtr.Zero;
					Frame.Uplane = IntPtr.Zero;
					Frame.Vplane = IntPtr.Zero;
					Frame.VideoSize = 0;
					Console.WriteLine("Frame buffer was empty (in ImageToYUV_Buffer.LoadToFrame())");
				}
			} catch {
				// Drop the bad frame data:
				UnloadFrame(ref Frame); // this can still be sent to the encoder, it should just mean a dropped frame
				Console.WriteLine("Lost a frame (no image)");
			}
		}

		/// <summary>
		/// Release memory previously locked by LoadToFrame()
		/// </summary>
		public void UnloadFrame (ref MediaFrame Frame) {
			if (pinY.IsAllocated) pinY.Free();
			if (pinU.IsAllocated) pinU.Free();
			if (pinV.IsAllocated) pinV.Free();
			Frame.VideoSize = 0;

			Y = null;
			u = null;
			v = null;
		}
		#endregion

		#region Clocked Loading Methods (for where frame rate can be variable)

		/// <summary>
		/// Load the closest matching frame by offset time.
		/// Fills the encoder-ready frame, with given time-code.
		/// WARNING: use this *OR* 'LoadToFrame', but not both!
		/// </summary>
		public void SelectiveLoadFrame (ref MediaFrame Frame, double OffsetSeconds) {
			// This is meant to be used for big frame skips on static bars&tones.
			// will need to be called from a clocked reference, and will give frames
			// a time based on that clock.

			// You should call 'SelectiveDequeue' before updating the reference clock
			int idx = FirstFrameMatchingTime(OffsetSeconds);
			if (idx < 0) { // no frame available
				Frame.Yplane = IntPtr.Zero;
				Frame.Uplane = IntPtr.Zero;
				Frame.Vplane = IntPtr.Zero;
				Frame.VideoSize = 0;
				return;
			}

			try {
				TimedImage img = null;
				lock (WaitingFrames) {
					img = WaitingFrames[idx];
				}
				if (img == null) return; // screw-up
				if (img.Luma == null || img.Cr == null || img.Cb == null) return; // crap frame

				Y = img.Luma;
				u = img.Cr;
				v = img.Cb;

				Frame.VideoSize = (ulong)Y.Length;
				Frame.VideoSampleTime = OffsetSeconds;

				pinY = GCHandle.Alloc(Y, GCHandleType.Pinned);
				Frame.Yplane = pinY.AddrOfPinnedObject();

				pinU = GCHandle.Alloc(u, GCHandleType.Pinned);
				Frame.Uplane = pinU.AddrOfPinnedObject();

				pinV = GCHandle.Alloc(v, GCHandleType.Pinned);
				Frame.Vplane = pinV.AddrOfPinnedObject();
			} catch {
				// Drop the bad frame data:
				UnloadFrame(ref Frame); // this can still be sent to the encoder, it should just mean a dropped frame
				Console.WriteLine("Lost a frame (no image)");
			}
		}

		/// <summary>
		/// Used by SelectiveLoadFrame() to pick a frame from the waiting frames buffer.
		/// </summary>
		private int FirstFrameMatchingTime (double OffsetSeconds) {
			lock (WaitingFrames) {
				WaitingFrames.Sort((a, b) => a.Seconds.CompareTo(b.Seconds)); // make sure bad media doesn't cause skips.
				if (WaitingFrames.Count < 1) return -1;
				if (WaitingFrames.Count == 1) return 0;

				for (int i = 1; i < WaitingFrames.Count; i++) {
					if (WaitingFrames[i].Seconds > OffsetSeconds) return i - 1;
				}
				return WaitingFrames.Count - 1; // no new frame yet. Will cause a pause.
			}
		}

		/// <summary>
		/// Remove un-needed frames when encoding by a reference clock.
		/// </summary>
		public void SelectiveDequeue (double OffsetSeconds) {
			// Find the same index in the frame buffer as 'SelectiveLoadFrame'
			// and remove all before, but NOT INCLUDING that index.

			int idx = FirstFrameMatchingTime(OffsetSeconds);
			lock (WaitingFrames) {
				if (idx < 0 || idx >= WaitingFrames.Count - 1) return;
				WaitingFrames.RemoveRange(0, idx);
			}
		}

		#endregion

	}

}
