using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using DirectShowLib;
using HCS_Encoder.Inputs.Processing;

namespace HCS_Encoder {
	public class VideoDataEventArgs : EventArgs {
		public Bitmap Frame;
		public double CaptureTime;
	}

	/// <summary>
	/// Callback based video capture class
	/// </summary>
	public class VideoCapture : ISampleGrabberCB, IDisposable {
		#region Member variables

		/// <summary> graph builder interface. </summary>
		private IFilterGraph2 m_FilterGraph = null;
		private IMediaControl m_mediaCtrl = null;

		/// <summary> so we can wait for the async job to finish </summary>
		private ManualResetEvent m_PictureReady = null;

		/// <summary> Indicates the status of the graph </summary>
		private bool m_bRunning = false;

		/// <summary> Dimensions of the image, calculated once in constructor. </summary>
		private IntPtr m_handle = IntPtr.Zero;
		private int m_videoWidth;
		private int m_videoHeight;
		private int m_stride;
		public int m_frameRate = 0;
		public int m_Dropped = 0;
		protected bool m_IncrementTimecodes = false;
		protected double m_TimecodeBase = 0.0;

		/// <summary>
		/// Sets the true zero-point of capture time.
		/// Used to compensate for start-up drift.
		/// </summary>
		public double TimecodeStart {
			get { return m_TimecodeBase; }
			set {
				m_TimecodeBase = value;
			}
		}

		/// <summary>
		/// Gets or Sets; default = capture size.
		/// Captured frames will be rescaled to this size (useful for adjusting incorrect aspect ratios).
		/// </summary>
		public Size TargetFrameSize { get; set; }

		/// <summary>
		/// Gets or Sets. Default = true.
		/// If true, the system clock is used to time frames.
		/// If false, the capture device is used to time frames.
		/// </summary>
		public bool UseAbsoluteTime { get; set; }

		private DateTime _startDay;

		#endregion

		#region API

		[DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
		private static extern void CopyMemory (IntPtr Destination, IntPtr Source, int Length);

		#endregion

		#region Events
		public event EventHandler<VideoDataEventArgs> FrameAvailable;

		protected void OnFrameAvailable (object sender, VideoDataEventArgs e) {
			if (FrameAvailable != null) {
				FrameAvailable(sender, e);
			}
		}
		#endregion

		/// <summary> Use capture device zero, default frame rate and size</summary>
		public VideoCapture () {
			_Capture(0, 0, 0, 0);
			TargetFrameSize = new Size(this.Width, this.Height);
		}
		/// <summary> Use specified capture device, default frame rate and size</summary>
		public VideoCapture (int iDeviceNum) {
			_Capture(iDeviceNum, 0, 0, 0);
			TargetFrameSize = new Size(this.Width, this.Height);
		}
		/// <summary> Use specified capture device, specified frame rate and default size</summary>
		public VideoCapture (int iDeviceNum, int iFrameRate) {
			_Capture(iDeviceNum, iFrameRate, 0, 0);
			TargetFrameSize = new Size(this.Width, this.Height);
		}
		/// <summary> Use specified capture device, specified frame rate and size</summary>
		public VideoCapture (int iDeviceNum, int iFrameRate, int iWidth, int iHeight) {
			_Capture(iDeviceNum, iFrameRate, iWidth, iHeight);
			TargetFrameSize = new Size(iWidth, iHeight); // results in correct size, regardless of capture device.
		}

		/// <summary>
		/// Start a frame-by-frame capture of a source file.
		/// Must be one that can be played in Windows Media Player.
		/// </summary>
		public VideoCapture (string Filename) {
			try {
				SetupGraph(Filename);
			} catch {
				Dispose();
				throw;
			}
		}

		#region Guff
		/// <summary> release everything. </summary>
		public void Dispose () {
			CloseInterfaces();
			if (m_PictureReady != null) {
				m_PictureReady.Close();
				m_PictureReady = null;
			}
		}
		// Destructor
		~VideoCapture () {
			Dispose();
		}

		public int Width {
			get {
				return m_videoWidth;
			}
		}
		public int Height {
			get {
				return m_videoHeight;
			}
		}
		public int Stride {
			get {
				return m_stride;
			}
		}
		#endregion

		// Start the capture graph
		public void Start () {
			if (!m_bRunning) {
				int hr = m_mediaCtrl.Run();
				DsError.ThrowExceptionForHR(hr);

				m_bRunning = true;
			}
		}
		// Pause the capture graph.
		// Running the graph takes up a lot of resources.  Pause it when it
		// isn't needed.
		public void Pause () {
			if (m_bRunning) {
				int hr = m_mediaCtrl.Pause();
				DsError.ThrowExceptionForHR(hr);

				m_bRunning = false;
			}
		}


		// Internal capture
		private void _Capture (int iDeviceNum, int iFrameRate, int iWidth, int iHeight) {
			UseAbsoluteTime = true;
			_startDay = DateTime.Today;
			DsDevice[] capDevices;

			// Get the collection of video devices
			capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

			if (iDeviceNum + 1 > capDevices.Length) {
				throw new Exception("No video capture devices found at that index!");
			}

			for (int i = 0; i < capDevices.Length; i++) {
				Console.WriteLine("Video device " + i + " is " + capDevices[i].Name);
			}
			Console.WriteLine("Using video device " + iDeviceNum + " at " + capDevices[iDeviceNum].DevicePath);

			try {
				// Set up the capture graph
				SetupGraph(capDevices[iDeviceNum], iFrameRate, iWidth, iHeight);
			} catch {
				Dispose();
				throw;
			}
		}

		/// <summary> build the capture graph for grabber. </summary>
		private void SetupGraph (DsDevice dev, int iFrameRate, int iWidth, int iHeight) {
			int hr;

			ISampleGrabber sampGrabber = null;
			IBaseFilter capFilter = null;
			ICaptureGraphBuilder2 capGraph = null;

			// Get the graphbuilder object
			m_FilterGraph = (IFilterGraph2)new FilterGraph();
			m_mediaCtrl = m_FilterGraph as IMediaControl;
			try {
				// Get the ICaptureGraphBuilder2
				capGraph = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

				// Get the SampleGrabber interface
				sampGrabber = (ISampleGrabber)new SampleGrabber();

				// Start building the graph
				hr = capGraph.SetFiltergraph(m_FilterGraph);
				DsError.ThrowExceptionForHR(hr);

				// Add the video device
				hr = m_FilterGraph.AddSourceFilterForMoniker(dev.Mon, null, "Video input", out capFilter);
				DsError.ThrowExceptionForHR(hr);

				IBaseFilter baseGrabFlt = (IBaseFilter)sampGrabber;
				ConfigureSampleGrabber(sampGrabber);

				// Add the frame grabber to the graph
				hr = m_FilterGraph.AddFilter(baseGrabFlt, "Ds.NET Grabber");
				DsError.ThrowExceptionForHR(hr);

				// If any of the default config items are set
				if (iFrameRate + iHeight + iWidth > 0) {
					SetConfigParms(capGraph, capFilter, iFrameRate, iWidth, iHeight);
				}

				hr = capGraph.RenderStream(PinCategory.Capture, MediaType.Video, capFilter, null, baseGrabFlt);
				DsError.ThrowExceptionForHR(hr);

				SaveSizeInfo(sampGrabber);
			} finally {
				if (capFilter != null) {
					Marshal.ReleaseComObject(capFilter);
					capFilter = null;
				}
				if (sampGrabber != null) {
					Marshal.ReleaseComObject(sampGrabber);
					sampGrabber = null;
				}
				if (capGraph != null) {
					Marshal.ReleaseComObject(capGraph);
					capGraph = null;
				}
			}
		}


		public IMediaEvent m_MediaEvent = null;
		private void SetupGraph (string FileName) {
			int hr;

			ISampleGrabber sampGrabber = null;
			IBaseFilter baseGrabFlt = null;
			IBaseFilter capFilter = null;
			IBaseFilter nullrenderer = null;

			// Get the graphbuilder object
			m_FilterGraph = new FilterGraph() as IFilterGraph2;
			m_mediaCtrl = m_FilterGraph as IMediaControl;
			m_MediaEvent = m_FilterGraph as IMediaEvent;

			IMediaFilter mediaFilt = m_FilterGraph as IMediaFilter;

			try {

				// Add the video source
				hr = m_FilterGraph.AddSourceFilter(FileName, "Ds.NET FileFilter", out capFilter);
				DsError.ThrowExceptionForHR(hr);

				// Get the SampleGrabber interface
				sampGrabber = new SampleGrabber() as ISampleGrabber;
				baseGrabFlt = sampGrabber as IBaseFilter;

				ConfigureSampleGrabber(sampGrabber);

				// Add the frame grabber to the graph
				hr = m_FilterGraph.AddFilter(baseGrabFlt, "Ds.NET Grabber");
				DsError.ThrowExceptionForHR(hr);

				// ---------------------------------
				// Connect the file filter to the sample grabber

				// Hopefully this will be the video pin, we could check by reading it's mediatype
				IPin iPinOut = DsFindPin.ByDirection(capFilter, PinDirection.Output, 0);

				// Get the input pin from the sample grabber
				IPin iPinIn = DsFindPin.ByDirection(baseGrabFlt, PinDirection.Input, 0);

				hr = m_FilterGraph.Connect(iPinOut, iPinIn);
				DsError.ThrowExceptionForHR(hr);

				// Add the null renderer to the graph
				nullrenderer = new NullRenderer() as IBaseFilter;
				hr = m_FilterGraph.AddFilter(nullrenderer, "Null renderer");
				DsError.ThrowExceptionForHR(hr);

				// ---------------------------------
				// Connect the sample grabber to the null renderer

				iPinOut = DsFindPin.ByDirection(baseGrabFlt, PinDirection.Output, 0);
				iPinIn = DsFindPin.ByDirection(nullrenderer, PinDirection.Input, 0);

				hr = m_FilterGraph.Connect(iPinOut, iPinIn);
				DsError.ThrowExceptionForHR(hr);

				// Turn off the clock.  This causes the frames to be sent
				// thru the graph as fast as possible
				hr = mediaFilt.SetSyncSource(null);
				DsError.ThrowExceptionForHR(hr);

				// Read and cache the image sizes
				SaveSizeInfo(sampGrabber);
			} finally {
				if (capFilter != null) {
					Marshal.ReleaseComObject(capFilter);
					capFilter = null;
				}
				if (sampGrabber != null) {
					Marshal.ReleaseComObject(sampGrabber);
					sampGrabber = null;
				}
				if (nullrenderer != null) {
					Marshal.ReleaseComObject(nullrenderer);
					nullrenderer = null;
				}
			}
		}

		private int VideoIndex (IPin pPin) {
			int hr;
			int bRet = -1;
			AMMediaType[] pmt = new AMMediaType[1];
			IEnumMediaTypes ppEnum;

			// Walk the MediaTypes for the pin
			hr = pPin.EnumMediaTypes(out ppEnum);
			DsError.ThrowExceptionForHR(hr);

			try {
				do {
					bRet++;
					// Just read the first one
					hr = ppEnum.Next(1, pmt, IntPtr.Zero);
					DsError.ThrowExceptionForHR(hr);

				} while (pmt[0].majorType != MediaType.Video);
			} catch {
				bRet = -1;
			} finally {
				Marshal.ReleaseComObject(ppEnum);
			}
			DsUtils.FreeAMMediaType(pmt[0]);

			return bRet;
		}

		/// <summary>
		/// Check if source media has ended. Only makes sense for file input.
		/// </summary>
		public bool IsCompleted () {
			if (m_MediaEvent == null) return false;

			int hr;
			EventCode evCode;
			const int ended = unchecked((int)0x80040227);

			hr = this.m_MediaEvent.WaitForCompletion(100, out evCode);
			//if (evCode == EventCode.Complete) return true;
			if (hr == ended) return true;
			return false;
		}

		private void SaveSizeInfo (ISampleGrabber sampGrabber) {
			int hr;

			// Get the media type from the SampleGrabber
			AMMediaType media = new AMMediaType();
			hr = sampGrabber.GetConnectedMediaType(media);
			DsError.ThrowExceptionForHR(hr);

			if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero)) {
				throw new NotSupportedException("Unknown Grabber Media Format");
			}

			// Grab the size info
			VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
			m_videoWidth = videoInfoHeader.BmiHeader.Width;
			m_videoHeight = videoInfoHeader.BmiHeader.Height;
			m_stride = m_videoWidth * (videoInfoHeader.BmiHeader.BitCount / 8);

			m_frameRate = (int)(10000000 / videoInfoHeader.AvgTimePerFrame);

			DsUtils.FreeAMMediaType(media);
			media = null;
		}

		private void ConfigureSampleGrabber (ISampleGrabber sampGrabber) {
			AMMediaType media;
			int hr;

			// Set the media type to Video/RBG24
			media = new AMMediaType();
			media.majorType = MediaType.Video;
			media.subType = MediaSubType.RGB24;
			media.formatType = FormatType.VideoInfo;
			hr = sampGrabber.SetMediaType(media);
			DsError.ThrowExceptionForHR(hr);

			DsUtils.FreeAMMediaType(media);
			media = null;

			// Configure the samplegrabber
			hr = sampGrabber.SetCallback(this, 1); // buffer callback (0 = sample callback)
			DsError.ThrowExceptionForHR(hr);
		}

		// Set the Framerate, and video size
		private void SetConfigParms (ICaptureGraphBuilder2 capGraph, IBaseFilter capFilter, int iFrameRate, int iWidth, int iHeight) {
			int hr;
			object o;
			AMMediaType media;

			// Find the stream config interface
			hr = capGraph.FindInterface(
				PinCategory.Capture, MediaType.Video, capFilter, typeof(IAMStreamConfig).GUID, out o);

			IAMStreamConfig videoStreamConfig = o as IAMStreamConfig;
			if (videoStreamConfig == null) {
				throw new Exception("Failed to get IAMStreamConfig");
			}

			// Get the existing format block
			hr = videoStreamConfig.GetFormat(out media);
			DsError.ThrowExceptionForHR(hr);

			// copy out the videoinfoheader
			VideoInfoHeader v = new VideoInfoHeader();
			Marshal.PtrToStructure(media.formatPtr, v);

			// if overriding the framerate, set the frame rate
			if (iFrameRate > 0) {
				v.AvgTimePerFrame = 10000000 / iFrameRate;
			}

			// if overriding the width, set the width
			if (iWidth > 0) {
				v.BmiHeader.Width = iWidth;
			}

			// if overriding the Height, set the Height
			if (iHeight > 0) {
				v.BmiHeader.Height = iHeight;
			}

			// Copy the media structure back
			Marshal.StructureToPtr(v, media.formatPtr, false);

			// Set the new format
			hr = videoStreamConfig.SetFormat(media);
			DsError.ThrowExceptionForHR(hr);

			DsUtils.FreeAMMediaType(media);
			media = null;
		}

		/// <summary> Shut down capture </summary>
		private void CloseInterfaces () {
			int hr;

			try {
				if (m_mediaCtrl != null) {
					// Stop the graph
					hr = m_mediaCtrl.Stop();
					m_bRunning = false;
				}
			} catch (Exception ex) {
				Debug.WriteLine(ex);
			}

			if (m_FilterGraph != null) {
				Marshal.ReleaseComObject(m_FilterGraph);
				Marshal.ReleaseComObject(m_mediaCtrl);
				m_mediaCtrl = null;
				m_FilterGraph = null;
			}
		}

		/// <summary> sample callback, NOT USED. </summary>
		int ISampleGrabberCB.SampleCB (double SampleTime, IMediaSample pSample) {
			throw new NotImplementedException();
		}

		private double _lastSampleTime = -1.0;

		/// <summary> buffer callback, COULD BE FROM FOREIGN THREAD. </summary>
		int ISampleGrabberCB.BufferCB (double SampleTime, IntPtr pBuffer, int BufferLen) {

			if (_lastSampleTime < 0.0) {
				_lastSampleTime = SampleTime;
				return 0;
			}
			double frame_duration = SampleTime - _lastSampleTime;
			_lastSampleTime = SampleTime;


			// this method *REQUIRES* that the frame be properly consumed and dropped before returning from this method
			// After this method returns, 'pBuffer' will be freed.
			Bitmap frame = null;

			// If primary video size <> capture size, we rescale here (to make sure plug-ins work as expected)
			if (this.Width != TargetFrameSize.Width || this.Height != TargetFrameSize.Height) {
				frame = RescaleToTargetSize(pBuffer);
			} else {
				frame = new Bitmap(this.Width, this.Height, 3 * this.Width, PixelFormat.Format24bppRgb, pBuffer);
			}

			VideoDataEventArgs args = new VideoDataEventArgs();
			args.Frame = frame;
			if (UseAbsoluteTime) {
				args.CaptureTime = (DateTime.Now - _startDay).TotalSeconds - m_TimecodeBase - frame_duration;
			} else {
				args.CaptureTime = SampleTime - m_TimecodeBase;
				if (args.CaptureTime < 0.0) return 0;
			}

			OnFrameAvailable(this, args);
			frame.Dispose();
			return 0;
		}

		/// <summary>
		/// Scale a 24bpp interleaved image into another 24bpp interleaved image.
		/// </summary>
		private Bitmap RescaleToTargetSize (IntPtr pBuffer) {
			Bitmap frame = new Bitmap(TargetFrameSize.Width, TargetFrameSize.Height, PixelFormat.Format24bppRgb);
			Rectangle r = new Rectangle(Point.Empty, TargetFrameSize);
			BitmapData buf = frame.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			try {
				EncoderBridge.InterleavedScale(pBuffer, buf.Scan0,
					this.Width, this.Height,
					TargetFrameSize.Width, TargetFrameSize.Height,
					true);
			} finally {
				frame.UnlockBits(buf);
			}
			return frame;
		}
	}
}
