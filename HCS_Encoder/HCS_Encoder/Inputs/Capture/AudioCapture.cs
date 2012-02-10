using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

using DirectShowLib;
using System.Runtime.InteropServices.ComTypes;

namespace HCS_Encoder {
	public class AudioDataEventArgs : EventArgs {
		/// <summary>Sample values</summary>
		public short[] Samples { get; set; }

		/// <summary>Number of seconds into capture (for sync with video)</summary>
		public double CaptureTime;
	}

	/// <summary>
	/// Callback based audio capture class.
	/// This is pretty much an exact duplicate of VideoCapture, with a few audio-specific changes.
	/// </summary>
	public class AudioCapture : ISampleGrabberCB, IDisposable {
		#region Member variables

		/// <summary> graph builder interface. </summary>
		private IFilterGraph2 m_FilterGraph = null;
		private IMediaControl m_mediaCtrl = null;

		/// <summary> so we can wait for the async job to finish </summary>
		private ManualResetEvent m_PictureReady = null;

		/// <summary> Indicates the status of the graph </summary>
		private bool m_bRunning = false;

		// Returned actual capture parameters (may not be as requested)
		protected short m_Channels = 0;
		protected short m_BitsPerSample = 0;
		protected int m_SampleRate = 0;
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
		/// Gets or Sets. Default = true.
		/// If true, the system clock is used to time frames.
		/// If false, the capture device is used to time frames.
		/// </summary>
		public bool UseAbsoluteTime { get; set; }

		private DateTime _startDay;
		#endregion

		/// <summary>Number of channels (1 = mono, 2 = stereo)</summary>
		public int Channels { get { return m_Channels; } }

		/// <summary>Bits per sample. Usually 8 or 16.</summary>
		public int BitsPerSample { get { return m_BitsPerSample; } }

		/// <summary>Number of samples per second</summary>
		public int SampleRate { get { return m_SampleRate; } }

		#region API

		[DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
		private static extern void CopyMemory (IntPtr Destination, IntPtr Source, int Length);

		#endregion

		#region Events
		public event EventHandler<AudioDataEventArgs> FrameAvailable;

		protected void OnFrameAvailable (object sender, AudioDataEventArgs e) {
			if (FrameAvailable != null) {
				FrameAvailable(sender, e); 
			}
		}
		#endregion

		/// <summary> Use capture device zero, default frame rate and size</summary>
		public AudioCapture () {
			_Capture(0, 0, 0);
		}
		/// <summary> Use specified capture device, default frame rate and size</summary>
		public AudioCapture (int iDeviceNum) {
			_Capture(iDeviceNum, 0, 0);
		}
		/// <summary> Use specified capture device, specified frame rate and default size</summary>
		public AudioCapture (int iDeviceNum, int iSampleRate) {
			_Capture(iDeviceNum, iSampleRate, 0);
		}
		/// <summary> Use specified capture device, specified frame rate and default size</summary>
		public AudioCapture (int iDeviceNum, int iSampleRate, int iChannels) {
			_Capture(iDeviceNum, iSampleRate, iChannels);
		}

		public AudioCapture (string Filename) {
			try {
				SetupGraph(Filename);
			} catch {
				Dispose();
				throw;
			}
		}

		/// <summary> release everything. </summary>
		public void Dispose () {
			CloseInterfaces();
			if (m_PictureReady != null) {
				m_PictureReady.Close();
				m_PictureReady = null;
			}
		}
		// Destructor
		~AudioCapture () {
			Dispose();
		}

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
		private void _Capture (int iDeviceNum, int iSampleRate, int iChannels) {
			UseAbsoluteTime = true;
			_startDay = DateTime.Today;
			DsDevice[] capDevices;

			// Get the collection of video devices
			capDevices = DsDevice.GetDevicesOfCat(FilterCategory.AudioInputDevice);

			if (iDeviceNum + 1 > capDevices.Length) {
				throw new Exception("No audio capture devices found at that index!");
			}

			for (int i = 0; i < capDevices.Length; i++) {
				Console.WriteLine("Audio device " + i + " is " + capDevices[i].Name);
			}
			Console.WriteLine("Using audio device " + iDeviceNum + " at " + capDevices[iDeviceNum].DevicePath);

			try {
				// Set up the capture graph
				SetupGraph(capDevices[iDeviceNum], iSampleRate, iChannels);
			} catch {
				Dispose();
				throw;
			}
		}

		private void ConfigureSampleGrabber (ISampleGrabber sampGrabber) {
			AMMediaType media;
			int hr;

			media = new AMMediaType();
			
			media.majorType = MediaType.Audio;
			/*media.subType = MediaSubType.WAVE;
			media.formatType = FormatType.WaveEx;*/
			
			hr = sampGrabber.SetMediaType(media);
			DsError.ThrowExceptionForHR(hr);

			DsUtils.FreeAMMediaType(media);
			media = null;

			// Configure the samplegrabber
			hr = sampGrabber.SetCallback(this, 1); // buffer callback (0 = Sample callback)
			DsError.ThrowExceptionForHR(hr);
		}


		/// <summary> build the capture graph for grabber. </summary>
		private void SetupGraph (DsDevice dev, int iSampleRate, int iChannels) {
			int hr;

			ISampleGrabber sampGrabber = null;
			IBaseFilter capFilter = null;
			ICaptureGraphBuilder2 capGraph = null;
			IBaseFilter baseGrabFlt = null;
			IBaseFilter nullrenderer = null;
			IMediaFilter mediaFilt = m_FilterGraph as IMediaFilter;

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

				// Add the audio device
				hr = m_FilterGraph.AddSourceFilterForMoniker(dev.Mon, null, "Audio input", out capFilter);
				DsError.ThrowExceptionForHR(hr);
				
				// If any of the default config items are set
				if (iSampleRate + iChannels > 0) {
					SetConfigParms(capGraph, capFilter, iSampleRate, iChannels);
				}

				// Get the SampleGrabber interface
				sampGrabber = new SampleGrabber() as ISampleGrabber;
				baseGrabFlt = sampGrabber as IBaseFilter;

				ConfigureSampleGrabber(sampGrabber);

				// Add the frame grabber to the graph
				hr = m_FilterGraph.AddFilter(baseGrabFlt, "Ds.NET Grabber");
				DsError.ThrowExceptionForHR(hr);

				
				// ---------------------------------
				// Connect the file filter to the sample grabber

				// Hopefully this will be the audio pin, we could check by reading it's mediatype
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

				// Read and cache the resulting settings
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


		protected IMediaEvent m_MediaEvent = null;
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

				// Hopefully this will be the audio pin, we could check by reading it's mediatype
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

			if ((media.formatType != FormatType.WaveEx) || (media.formatPtr == IntPtr.Zero)) {
				throw new NotSupportedException("Unknown Grabber Audio Format");
			}

			WaveFormatEx infoHeader = (WaveFormatEx)Marshal.PtrToStructure(media.formatPtr, typeof(WaveFormatEx));
			m_Channels = infoHeader.nChannels;
			m_SampleRate = infoHeader.nSamplesPerSec;
			m_BitsPerSample = infoHeader.wBitsPerSample;

			DsUtils.FreeAMMediaType(media);
			media = null;
		}

		// Set the Framerate, and video size
		private void SetConfigParms (ICaptureGraphBuilder2 capGraph, IBaseFilter capFilter, int iSampleRate, int iChannels) {
			int hr;
			object o;
			AMMediaType media;

			// Find the stream config interface
			hr = capGraph.FindInterface(
				PinCategory.Capture, MediaType.Audio, capFilter, typeof(IAMStreamConfig).GUID, out o);

			IAMStreamConfig audioStreamConfig = o as IAMStreamConfig;
			if (audioStreamConfig == null) {
				throw new Exception("Failed to get IAMStreamConfig");
			}

			// Get the existing format block
			hr = audioStreamConfig.GetFormat(out media);
			DsError.ThrowExceptionForHR(hr);

			// copy out the videoinfoheader
			WaveFormatEx i = new WaveFormatEx();
			Marshal.PtrToStructure(media.formatPtr, i);


			i.wFormatTag = 0x0001; // WAVE_FORMAT_PCM
			i.wBitsPerSample = 16;
			i.nSamplesPerSec = 44100;
			i.nChannels = m_Channels;
			i.nBlockAlign = 2;
			i.nAvgBytesPerSec = (i.nSamplesPerSec * i.nBlockAlign);
			i.cbSize = 0;

			// if overriding the framerate, set the frame rate
			if (iSampleRate > 0) {
				i.nSamplesPerSec = iSampleRate;
			}

			// if overriding the width, set the width
			if (iChannels > 0) {
				i.nChannels = (short)iChannels;
			}

			// Copy the media structure back
			Marshal.StructureToPtr(i, media.formatPtr, false);

			// Set the new format
			hr = audioStreamConfig.SetFormat(media);
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

		int ISampleGrabberCB.SampleCB (double SampleTime, IMediaSample pSample) {
			throw new NotImplementedException();
		}

		private double _lastSampleTime = -1.0;

		int ISampleGrabberCB.BufferCB (double SampleTime, IntPtr pBuffer, int BufferLen) {

			if (_lastSampleTime < 0.0) {
				_lastSampleTime = SampleTime;
				return 0;
			}
			double frame_duration = SampleTime - _lastSampleTime;
			_lastSampleTime = SampleTime;


			var args = new AudioDataEventArgs(); 
			
			if (UseAbsoluteTime) {
				args.CaptureTime = (DateTime.Now - _startDay).TotalSeconds - m_TimecodeBase - frame_duration;
			} else {
				args.CaptureTime = SampleTime - m_TimecodeBase;
				if (args.CaptureTime < 0.0) return 0;
			}

			args.Samples = new short[BufferLen / 2];
			Marshal.Copy(pBuffer, args.Samples, 0, BufferLen / 2);
			OnFrameAvailable(this, args);
			return 0;
		}
	}
}
