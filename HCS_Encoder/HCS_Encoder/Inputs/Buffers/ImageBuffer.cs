using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using HCS_Encoder.Inputs.Processing;

namespace HCS_Encoder.Inputs.Buffers {

	public class ImageBuffer : IEncoderBuffer {
		/// <summary>Output buffer width</summary>
		public int Width { get { return width; } }

		/// <summary>Output buffer height</summary>
		public int Height { get { return height; } }

		/// <summary>
		/// Capture time of next frame to be encoded (this should be the minimum of all buffered capture times)
		/// </summary>
		public double NextCaptureTime {
			get {
				if (WaitingFrames.Count < 1) {
					try {
						if (!WaitingCaptures.DataAvailable(this)) return 0.0;
						return WaitingCaptures.Peek(this).Seconds;
					} catch {
						return 0.0;
					}
				}
				return WaitingFrames[0].Seconds;
			}
		}

		/// <summary>
		/// Number of frames buffered and waiting to be encoded.
		/// </summary>
		public int QueueLength {
			get { return WaitingFrames.Count/* + WaitingCaptures.Count*/; }
		}

		/// <summary>
		/// Number of frames received from capture and waiting to be processed
		/// </summary>
		public int WaitingCaptureCount {
			get { return WaitingCaptures.Count(this); }
		}


		/// <summary>
		/// Prepare a set of buffers to accept the images.
		/// Incoming frames will be scaled to match the given width and height.
		/// For best results, capture and buffer sizes should match.
		/// Rescaling does not preserve aspect ratio.
		/// </summary>
		/// <param name="SourcePixels">height * width of the capture device.</param>
		public ImageBuffer (int Width, int Height) {
			height = Height;
			width = Width;

			WaitingFrames = new List<TimedImage>(100);
			WaitingCaptures = new SortedSubscriberQueue<TimedImage>(); // dummy one until a proper queue is added.
		}

		/// <summary>
		/// Emergeny clear-out. Drop all queued frames
		/// </summary>
		public void WipeBuffer () {
			lock (WaitingFrames) {
				WaitingFrames.Clear();
			}
			GC.Collect();
		}


		/// <summary>
		/// Remove all frames captured before the given capture time. They will not be encoded.
		/// </summary>
		public void WipeBufferUntil (double AbandonTime) {
			lock (this) {
				lock (WaitingCaptures) {
					while (WaitingCaptures.DataAvailable(this)) {
						var ti = WaitingCaptures.Peek(this);
						if (ti != null && ti.Seconds < AbandonTime) {
							WaitingCaptures.Dequeue(this);

						} else break;
					}
				}
			}

			lock (WaitingFrames) {
				while (WaitingFrames.Count > 0 && WaitingFrames[0].Seconds < AbandonTime) {
					WaitingFrames.RemoveAt(0);
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

		/// <summary>
		/// Gets or Sets the captured images source to buffer from
		/// </summary>
		public SortedSubscriberQueue<TimedImage> WaitingCaptures {
			get {
				return _waitingCaptures;
			}
			set {
				if (_waitingCaptures != null) {
					if (_waitingCaptures.IsSubscribed(this)) _waitingCaptures.Unsubscribe(this);
				}
				_waitingCaptures = value;
				if (_waitingCaptures != null) {
					if (!_waitingCaptures.IsSubscribed(this)) _waitingCaptures.Subscribe(this);
				}
			}
		}

		#region Inner workings
		private List<TimedImage> WaitingFrames;
		private SortedSubscriberQueue<TimedImage> _waitingCaptures;
		private int height, width;
		private byte[] Y, u, v;

		/// <summary>
		/// Convert a captured YUV buffer into a scaled YUV buffer
		/// </summary>
		public void RebufferCapturedFrames() {
			TimedImage ti = null;
			if (!WaitingCaptures.DataAvailable(this)) return;
			ti = WaitingCaptures.Dequeue(this);
			if (ti == null) return;

			int twidth = Math.Max(width, ti.Width); // allocate with enough room for in-place scaling
			int theight = Math.Max(height, ti.Height); // allocate with enough room for in-place scaling
			TimedImage sti = null;
			//try {
				sti = new TimedImage(ti.Seconds, twidth, theight);
			//} catch (OutOfMemoryException) {
			//	GC.Collect();
			//	return; //dropped a frame
			//}
			sti.Width = width; // set scaling to target size
			sti.Height = height;

			RescaleBuffers(ti, sti); // do the scaling
			lock (WaitingFrames) {
				WaitingFrames.Add(sti);
				WaitingFrames.Sort((a, b) => a.Seconds.CompareTo(b.Seconds));
			}
		}

		private GCHandle pinY, pinU, pinV;

		private void RescaleBuffers (TimedImage Src, TimedImage Dst) {
			ScalePlane(Src.Luma, Dst.Luma, Src.Width, Src.Height, Dst.Width, Dst.Height, true);
			ScalePlane(Src.Cr, Dst.Cr, Src.Width, Src.Height, Dst.Width / 2, Dst.Height / 2, false);
			ScalePlane(Src.Cb, Dst.Cb, Src.Width, Src.Height, Dst.Width / 2, Dst.Height / 2, false);
		}

		private void ScalePlane (byte[] Src, byte[] Dst, int src_w, int src_h, int dst_w, int dst_h, bool HQ) {
			GCHandle _Src = default(GCHandle), _Dst = default(GCHandle);
			try {
				_Src = GCHandle.Alloc(Src, GCHandleType.Pinned);
				IntPtr _A = _Src.AddrOfPinnedObject();
				_Dst = GCHandle.Alloc(Dst, GCHandleType.Pinned);
				IntPtr _B = _Dst.AddrOfPinnedObject();

				EncoderBridge.PlanarScale(_A, _B, src_w, src_h, dst_w, dst_h, HQ);

			} finally {
				if (_Src.IsAllocated) _Src.Free();
				if (_Dst.IsAllocated) _Dst.Free();
			}
		}

		#endregion
	}
}
