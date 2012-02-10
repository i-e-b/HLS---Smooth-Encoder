using System;
using System.Linq;
using System.Collections.Generic;
using HCS_Encoder.Inputs.Processing;
using HCS_Encoder.Inputs.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HCS_Encoder.Inputs.Buffers {
	/// <summary>
	/// Handy container for a list of Image_Buffers that work together.
	/// If a method isn't supported here, you should be using the contained 
	/// buffers individually.
	/// </summary>
	public class ImageBufferList : List<ImageBuffer>, ICaptureBuffer {
		public int SourceWidth { get; private set; }
		public int SourceHeight { get; private set; }

		/// <summary>
		/// List of video processors to apply.
		/// </summary>
		public List<IVideoProcessor> ProcessorQueue { get; private set; }

		/// <summary>
		/// Queue of captured frames
		/// </summary>
		public SortedSubscriberQueue<TimedImage> WaitingCaptures { get; private set; }

		public ImageBufferList (EncoderConfiguration.Configuration config)
			: base() {
			SourceWidth = config.Video.InputWidth;
			SourceHeight = config.Video.InputHeight;
			WaitingCaptures = new SortedSubscriberQueue<TimedImage>();
		}

		/// <summary>
		/// Number of frames buffered and waiting to be encoded in all buffers.
		/// </summary>
		public int QueueLength {
			get {
				lock (this) {
					return this.Sum(a => a.QueueLength + a.WaitingCaptureCount);
				}
			}
		}

		/// <summary>
		/// Capture time of next frame to be encoded (this should be the minimum of all buffered capture times)
		/// </summary>
		public double NextCaptureTime {
			get {
				lock (this) {
					return this.Min(a => a.NextCaptureTime);
				}
			}
		}

		/// <summary>
		/// Add a plug-in to the end of the processor queue
		/// </summary>
		public void RegisterPlugin (HCS_Encoder.Inputs.Processing.IVideoProcessor PlugIn) {
			lock (this) {
				if (PlugIn == null) return;
				if (ProcessorQueue == null) ProcessorQueue = new List<IVideoProcessor>();
				ProcessorQueue.Add(PlugIn);
			}
		}


		/// <summary>
		/// Respond to capture event. Frame and timings are passed to each buffer.
		/// </summary>
		public void HandleCapturedFrame (object sender, VideoDataEventArgs e) {
			// Run plug-ins, convert to YUV, then send YUV buffers to individual buffers for scaling.
			if (e.Frame != null) {
				try {
					if (ProcessorQueue != null) { // process the source image
						foreach (var item in ProcessorQueue) {
							item.ProcessFrame(e.Frame, e.CaptureTime);
						}
					}
				} catch { }

				var ti = new TimedImage(e.CaptureTime, e.Frame.Width, e.Frame.Height);
				ResampleBuffer(e.Frame, ti);

				WaitingCaptures.Enqueue(ti);

				foreach (var buffer in this) {
					if (buffer.WaitingCaptures != WaitingCaptures) {
						buffer.WaitingCaptures = WaitingCaptures;
					}
				}
			}
		}

		/// <summary>
		/// Emergeny clear-out. Drop all queued frames
		/// </summary>
		public void WipeBuffer () {
			lock (this) {
				this.WaitingCaptures.Clear();
				foreach (var buffer in this) {
					buffer.WipeBuffer();
				}
			}
		}

		/// <summary>
		/// Remove all frames captured before the given capture time. They will not be encoded.
		/// </summary>
		public void WipeBufferUntil (double AbandonTime) {
			lock (this) {
				foreach (var buffer in this) {
					buffer.WipeBufferUntil(AbandonTime);
				}
			}
		}


		#region Inner Workings

		/// <summary>
		/// Convert a GDI image into an equal-sized YUV planar image.
		/// </summary>
		private void ResampleBuffer (Bitmap img, TimedImage ti) {
			if (img == null) return;
			Rectangle r = new Rectangle(0, 0, img.Width, img.Height);
			BitmapData bmp = img.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb); // for speed, always use the bitmap's real pixel format!

			// Do the RGB -> YUV conversion
			try {
				GCHandle _Lx = default(GCHandle), _Crx = default(GCHandle), _Cbx = default(GCHandle);
				try {
					_Lx = GCHandle.Alloc(ti.Luma, GCHandleType.Pinned);
					IntPtr _L = _Lx.AddrOfPinnedObject();
					_Crx = GCHandle.Alloc(ti.Cr, GCHandleType.Pinned);
					IntPtr _Cr = _Crx.AddrOfPinnedObject();
					_Cbx = GCHandle.Alloc(ti.Cb, GCHandleType.Pinned);
					IntPtr _Cb = _Cbx.AddrOfPinnedObject();

					// Convert, but don't scale. No sub-sampling here.
					EncoderBridge.Rgb2YuvIS(ti.Width, ti.Height,
						bmp.Scan0, _L, _Cb, _Cr);

				} finally {
					if (_Lx.IsAllocated) _Lx.Free();
					if (_Crx.IsAllocated) _Crx.Free();
					if (_Cbx.IsAllocated) _Cbx.Free();
				}
			} finally {
				img.UnlockBits(bmp);
			}
		}

		#endregion

		#region ICaptureBuffer Members


		public void RegisterPlugin (IAudioProcessor PlugIn) {
			throw new NotSupportedException();
		}

		public void HandleCapturedSamples (object sender, AudioDataEventArgs e) {
			throw new NotSupportedException();
		}

		#endregion
	}
}
