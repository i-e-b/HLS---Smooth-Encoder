using System;
using System.Collections.Generic;
using System.Linq;
using EncoderConfiguration;
using System.IO;
using System.Threading;
using HCS_Encoder.Inputs.Processing;
using System.Drawing;
using HCS_Encoder.Inputs.Buffers;

namespace HCS_Encoder.Utilities {
	/// <summary>
	/// This class provides a simplified way to start and 
	/// stop an encode process, using a configuration file for the details.
	/// </summary>
	public class EncoderController: IDisposable {
		#region Status Properties (for health checking and monitoring tools)
		/// <summary>
		/// Returns true if the encoder is currently running,
		/// false otherwise.
		/// </summary>
		public bool EncoderRunning {
			get { return Thread.VolatileRead(ref _encoderRunning) == -1; }
			private set { Thread.VolatileWrite(ref _encoderRunning, (value) ? (-1) : (0)); }
		}
		private int _encoderRunning;

		/// <summary>
		/// Gets duration of recording (time since encoder was last started)
		/// </summary>
		public TimeSpan RecordingDuration {
			get {
				if (start <= DateTime.MinValue) return TimeSpan.Zero;
				return DateTime.Now - start;
			}
		}

		/// <summary>
		/// Gets average video frames per seconds.
		/// Only accurate during encoding. Related to frames encoded, not frames captured.
		/// </summary>
		public double AverageFramesPerSecond {
			get {
				if (videoJobs < 1) return 0.0;
				return (FrameCount / (double)videoJobs) / Math.Max(1.0, RecordingDuration.TotalSeconds);
			}
		}

		/// <summary>
		/// Gets the recording time of the last video frame loaded into the encoder.
		/// </summary>
		/// <remarks>'RecordingDuration' should always be higher, and 'RecordingDuration - VideoSampleTime' is latency.</remarks>
		public TimeSpan VideoSampleTime {
			get {
				return TimeSpan.FromSeconds(Packages.Max(a => a.Frame.VideoSampleTime));
			}
		}

		/// <summary>
		/// Gets the recording time of the last audio frame loaded into the encoder.
		/// If video is being encoded, this should always be less-than-or-equal 'VideoSampleTime'
		/// </summary>
		/// <remarks>'RecordingDuration' should always be higher, and 'RecordingDuration - AudioSampleTime' is latency.</remarks>
		public TimeSpan AudioSampleTime {
			get {
				return TimeSpan.FromSeconds(Packages.Max(a => a.Frame.AudioSampleTime));
			}
		}

		/// <summary>
		/// Returns number of video frames captured and waiting to be encoded.
		/// If not capturing video, this will be 0.
		/// </summary>
		public int VideoQueueLength {
			get {
				if (ImageBuffers != null) return ImageBuffers.QueueLength;
				return 0;
			}
		}

		/// <summary>
		/// Returns number of audio frames captured and waiting to be encoded.
		/// If not capturing audio, this will be 0.
		/// </summary>
		/// <remarks>Because audio can be returned unencoded (to ensure A/V sync) this value may not reduce
		/// after a call to the encoder.</remarks>
		public int AudioQueueLength {
			get {
				if (AudioBuffers != null) return AudioBuffers.QueueLength;
				return 0;
			}
		}

		/// <summary>
		/// Gets the current TARGET fragment for the encoder.
		/// </summary>
		public int FragmentNumber {
			get {
				return Packages.Min(a => a.Job.SegmentNumber);
			}
		}

		/// <summary>
		/// If encoding video, this is the count of video frames encoded.
		/// If only encoding audio, this is the number of audio samples encoded (sample duration depends on capture device).
		/// </summary>
		public long FrameCount { get; private set; }

		/// <summary>
		/// Gets the number of jobs being encoded.
		/// </summary>
		public int JobCount {get {return Packages.Count;}}

		#endregion

		#region Control Properties (for special control -- normal start/stop doesn't need these to be set)

		/// <summary>
		/// Gets or Sets. Default = false.
		/// If set to true, capture devices will be run into their
		/// buffers without any encoding (buffers will fill until encoder is stopped, DryRun is set to false, or memory runs out.)
		/// </summary>
		public bool DryRun { get; set; }

		/// <summary>
		/// Gets or sets. Default = zero.
		/// Any samples in the buffer with a capture time less than this 
		/// setting are abandoned without being encoded. They will be removed from the buffers.
		/// This works in DryRun mode.
		/// </summary>
		public double AbandonTime { get; set; }

		/// <summary>
		/// Gets or Sets. Default = true.
		/// If set to false, encoder will run as normal, but output fragments won't be sent.
		/// </summary>
		public bool EnableOutput {
			get {
				return outputRouter.EnableOutput;
			}
			set {
				outputRouter.EnableOutput = value;
			}
		}

		/// <summary>
		/// Gets or Sets. Default = true;
		/// If set to false, temporary files won't be deleted once delivered.
		/// </summary>
		public bool CleanupDeliveredChunks {
			get {
				return outputRouter.ShouldCleanup;
			}
			set {
				outputRouter.ShouldCleanup = value;
			}
		}

		/// <summary>
		/// Gets or Sets. Default = 1;
		/// Minimum number of frames in all active buffers before frames will be encoded.
		/// Buffers are sorted before encoding -- longer buffers can cope with larger input jitter.
		/// If set to 0, A/V may desync. Set higher for unreliable inputs.
		/// </summary>
		public uint MinimumBufferPopulation { get; set; }

		/// <summary>
		/// Gets or Sets. Default = false;
		/// If true, the system clock is used to time frames.
		/// If false, the capture device is used to time frames.
		/// Only takes effect before a call to Start();
		/// </summary>
		/// <remarks>Many capture devices return unstable or incorrect timecodes,
		/// even when frames are delivered at the correct rate. Absolute time attempts to correct this.</remarks>
		public bool UseAbsoluteTime { get; set; }

		#endregion

		/// <summary>
		/// Create and initialise a new encoder job with the given config
		/// </summary>
		public EncoderController (Configuration Config) {
			UseAbsoluteTime = false;
			DryRun = false;
			AbandonTime = 0.0;
			MinimumBufferPopulation = 1;
			if (Config == null) throw new ArgumentException("Config must not be null", "Config");
			config = Config;
			if (config.Video.InputFrameRate < 1) config.Video.InputFrameRate = 1;
			EncoderRunning = false;

			outputRouter = new Outputs.OutputRouter(config);
			CaptureSetup();
			EncoderSetup();
			outputRouter.Prepare(Packages);

			EnableOutput = true;
			CleanupDeliveredChunks = true;
		}

		/// <summary>
		/// Start the encoding process.
		/// This will start to over-write any existing encodes made with the configuration being used.
		/// </summary>
		public void Start () {
			if (EncoderRunning || (coreloops != null && coreloops.Exists(a => a.IsAlive)))
				throw new ThreadStateException("Do not start an Encoder controller while it is running!");
			InnerStartup();
		}

		/// <summary>
		/// Pauses all capture devices, but allows the encoder loop to continue (will sleep while buffers are empty)
		/// </summary>
		public void PauseCapture () {
			if (cam != null) cam.Pause();
			if (mic != null) mic.Pause();
		}

		/// <summary>
		/// Resumes capture devices after a call to 'PauseCapture()'
		/// </summary>
		public void ContinueCapture () {
			if (!EncoderRunning || coreloops == null || !coreloops.Exists(a => a.IsAlive))
				throw new ThreadStateException("Encoder is not running. Please call Start() to start encoder.");

			if (cam != null) cam.Start();
			if (mic != null) mic.Start();
		}

		/// <summary>
		/// Resumes output sending (if 'EnableOutput' is false).
		/// Any chunks with an index below 'MiniumumChunkIndex' will be dropped.
		/// </summary>
		public void ContinueOutput (int MinimumChunkIndex) {
			if (EnableOutput) return; // not turned off

			outputRouter.SendOutput(MinimumChunkIndex, -1);
		}

		/// <summary>
		/// Stop the encoding process
		/// </summary>
		public void Stop () {
			if (cam != null) cam.Pause();
			if (mic != null) mic.Pause();
			EncoderRunning = false;

			if (coreloops != null) {
				foreach (var loop in coreloops) {
					loop.Join(); // wait for the encoder to get the message and actually stop. Easier than screwing it's state data.
				}
			}
			try { cam.Dispose(); } catch { }
			try { mic.Dispose(); } catch { }

			foreach (var pkg in Packages) {
				try {
					EncoderBridge.CloseEncoderJob(ref pkg.Job);
				} catch { }
			}
			if (outputRouter != null) outputRouter.Close();
		}

		/// <summary>
		/// Add a plug-in to the audio buffer's plug-in list
		/// </summary>
		public void RegisterPlugin (IAudioProcessor PlugIn) {
			if (AudioBuffers == null) throw new Exception("Can't register a plug-in: audio buffer has not been configured");

			AudioBuffers.RegisterPlugin(PlugIn);
		}

		/// <summary>
		/// Add a plug-in to the video buffer's plug-in list
		/// </summary>
		public void RegisterPlugin (IVideoProcessor PlugIn) {
			if (ImageBuffers == null) throw new Exception("Can't register a plug-in: video buffer has not been configured");

			ImageBuffers.RegisterPlugin(PlugIn);
		}

		#region Hardcore controls -- things you won't need unless you're doing something odd
		/// <summary>
		/// Force a timed frame into the encoder's buffers.
		/// May cause unexpected operation. Use with caution!
		/// </summary>
		public void ForceInsertFrame (System.Drawing.Bitmap VideoFrame, double SimulatedCaptureTime) {
			if (ImageBuffers != null) ImageBuffers.HandleCapturedFrame(this, new VideoDataEventArgs() { Frame = VideoFrame, CaptureTime = SimulatedCaptureTime });
			else throw new Exception("Can't send video frame to uninitialised buffer. Please include a video device in your config.");
		}
		
		/// <summary>
		/// Force a timed frame into the encoder's buffers.
		/// May cause unexpected operation. Use with caution!
		/// </summary>
		public void ForceInsertFrame (TimedSample AudioFrame) {
			if (AudioBuffers != null) AudioBuffers.HandleCapturedSamples(this, new AudioDataEventArgs() { Samples = AudioFrame.Samples, CaptureTime = AudioFrame.Seconds });
			else throw new Exception("Can't send audio frame to uninitialised buffer. Please include an audio device in your config.");
		}

		/// <summary>
		/// Clear all captured frames from all attached buffers.
		/// Buffers will refill if capture devices are running.
		/// </summary>
		public void ClearBuffers () {
			if (ImageBuffers != null) ImageBuffers.WipeBuffer();
			if (AudioBuffers != null) AudioBuffers.WipeBuffer();
		}
		#endregion

		#region Inner Workings
		// Sources:
		private Configuration config = null;
		private AudioCapture mic = null;
		private VideoCapture cam = null;
		// Buffers:
		private AudioBufferList AudioBuffers = null;
		private ImageBufferList ImageBuffers = null;
		// Encode & output:
		private List<EncoderPackage> Packages;
		private List<Thread> coreloops = null;
		private Outputs.OutputRouter outputRouter = null;
		// Flags & bits:
		private int videoJobs;
		private int frameSleep; // used for Adaptive frame rate
		private DateTime start = DateTime.MinValue;
		private bool synchronised = false; // sync flag for time-code compensation.

		/// <summary>
		/// Make sure our child thread gets killed.
		/// Will still fail on a stack collision.
		/// </summary>
		~EncoderController () {
			
		}


		public void Dispose () {
			if (mic != null) {
				mic.Pause();
				mic.Dispose();
				mic = null;
			}
			if (cam != null) {
				cam.Pause();
				cam.Dispose();
				cam = null;
			}
			if (EncoderRunning) Stop();
			if (coreloops != null) {
				foreach (var loop in coreloops) {
					if (loop == null) continue;
					loop.Join();
				}
				coreloops.Clear();
			}
		}

		/// <summary>
		/// Stop the encode process, from within the core loop
		/// </summary>
		private void Halt () {
			PauseCapture();
			EncoderRunning = false;
			
			outputRouter.Close();
			//Thread.CurrentThread.Abort();
		}

		/// <summary>
		/// Start encoder threads & capture devices for each job.
		/// </summary>
		private void InnerStartup () {
			try {
				EncoderRunning = false;
				synchronised = false;
				//System.Threading.ThreadPool.SetMaxThreads(90, 90);
				// Start a new thread for each job
				coreloops = new List<Thread>();

				foreach (var pkg in Packages) {
					coreloops.Add(NewEncoderThread(pkg));
				}

				if (cam != null) cam.UseAbsoluteTime = this.UseAbsoluteTime;
				if (mic != null) mic.UseAbsoluteTime = this.UseAbsoluteTime;

				// Start capturing! (buffers should start to fill from now on)
				if (cam != null) cam.Start(); // cameras usually take longer to start than microphones, so start in serial and cameras first.
				if (mic != null) mic.Start();

				FrameCount = 0;

				// Sync frames:
				AdjustFrameSleep();
				EncoderRunning = true;
				WaitAndCompensateCaptureTimes();
			} catch {
				EncoderRunning = false;
			}
		}

		/// <summary>
		/// Create, start and return a new core encoder thread.
		/// </summary>
		private Thread NewEncoderThread (EncoderPackage pkg) {
			ParameterizedThreadStart encodeLoop = new ParameterizedThreadStart(EncoderCoreLoop);

			var encloop = new Thread(encodeLoop);
			encloop.IsBackground = true; // When the parent thread ends, the encoder thread will be aborted.

			encloop.SetApartmentState(ApartmentState.MTA);

			// Don't use 'Highest' priority, as that can cause issues with the capture drivers (which will be working hard too)
			encloop.Priority = ThreadPriority.AboveNormal;
			encloop.Start(pkg); // start with the job id
			return encloop;
		}

		/// <summary>
		/// This loop does the actual work of reading caches into the encoder and firing actions.
		/// This loop controls Multiple-bit-rate encoding
		/// </summary>
		private void EncoderCoreLoop (object Package) {
			EncoderPackage pkg = Package as EncoderPackage;
			try {
				#region Start up
				if (pkg == null) throw new Exception("Encoder core loop package was lost");
				double lastVideoTime = 0.0; // used for Adaptive frame rate

				if (cam != null) lastVideoTime = cam.TimecodeStart;

				int loop_frame_incr = 0;
				if (pkg.Specification.HasVideo) {
					loop_frame_incr = 1;
					videoJobs++;
				}
				#endregion

				while (!EncoderRunning) { // wait for the signal!
					System.Threading.Thread.Sleep(1000);
				}

				WaitForSyncFlag();

				start = DateTime.Now;
				while (EncoderRunning) { // Encode frames until stopped
					#region Frame availability checks
					// Wait for buffers to be populated
					while (pkg.BuffersEmpty(MinimumBufferPopulation) && EncoderRunning) {
						foreach (var buf in pkg.Buffers) buf.RebufferCapturedFrames();
						if (pkg.BuffersEmpty(MinimumBufferPopulation)) System.Threading.Thread.Sleep(frameSleep);
					}

					if (DryRun) {
						System.Threading.Thread.Sleep(frameSleep);
						continue; // don't encode
					}
					#endregion

					pkg.LoadAllBuffers();
					EncoderBridge.EncodeFrame(ref pkg.Job, ref pkg.Frame);
					pkg.UnloadAllBuffers();
					
					if (!pkg.Job.IsValid) throw new Exception("Job became invalid. Possible memory or filesystem error");

					#region Segment switching
					if (pkg.Job.SegmentNumber != pkg.Job.OldSegmentNumber) {
						double real_chunk_duration = pkg.Frame.VideoSampleTime - lastVideoTime;
						lock (outputRouter) {
							outputRouter.NewChunkAvailable(pkg.Job.OldSegmentNumber, pkg.JobIndex, real_chunk_duration);
						}
						lastVideoTime = pkg.Frame.VideoSampleTime;
						pkg.Job.OldSegmentNumber = pkg.Job.SegmentNumber;
					}

					FrameCount += loop_frame_incr;
					#endregion
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.Fail("EncoderController.cs: Core loop fault.", ex.Message + "\r\n" + ex.StackTrace);

				File.WriteAllText(config.EncoderSettings.LocalSystemOutputFolder + @"/error.txt", "Main loop: "+ex.Message + "\r\n" + ex.StackTrace);
			} finally {
				if (pkg != null) {
					EncoderBridge.CloseEncoderJob(ref pkg.Job);// NEVER FORGET THIS!!
				}
				if (EncoderRunning) Halt(); // Don't use 'Stop()' in the core loop, or the system will freeze!
				System.Threading.Thread.CurrentThread.Abort();
			}
		}

		/// <summary>
		/// Wait for first frames in buffer, and use those to compensate timecodes.
		/// </summary>
		private void WaitAndCompensateCaptureTimes () {
			if (cam == null && mic == null) { // can't sync without capture devices.
				synchronised = true;
				return;
			}
			/*
			if (UseAbsoluteTime) { // already synchronised
				synchronised = true;
				return;
			}*/

			while (BuffersEmpty() && EncoderRunning) {
				System.Threading.Thread.Sleep(frameSleep);
			}
			if (ImageBuffers != null) ImageBuffers.WipeBuffer();
			if (AudioBuffers != null) AudioBuffers.WipeBuffer();

			while (BuffersEmpty() && EncoderRunning) {
				System.Threading.Thread.Sleep(frameSleep);
			}

			if (ImageBuffers != null || AudioBuffers != null) { // sync buffers.
				double a = (ImageBuffers != null) ? (ImageBuffers.NextCaptureTime) : (0);
				double b = (AudioBuffers != null) ? (AudioBuffers.NextCaptureTime) : (0);
				
				double capt = Math.Max(a, b);

				if (cam != null) cam.TimecodeStart += capt;
				if (mic != null) mic.TimecodeStart += capt;

				if (ImageBuffers != null) ImageBuffers.WipeBuffer();
				if (AudioBuffers != null) AudioBuffers.WipeBuffer();
			}
			synchronised = true;
		}

		/// <summary>
		/// Wait for the sync flag to be set by WaitAndCompensateCaptureTimes()
		/// </summary>
		private void WaitForSyncFlag () {
			while (EncoderRunning && !synchronised) {
				System.Threading.Thread.Sleep(frameSleep);
			}
		}

		/// <summary>
		/// Initialise an encoder job based on previously setup capture devices.
		/// Need to have one job per 'ReductionFactor' in the config.
		/// </summary>
		private void EncoderSetup () {
			var factors = config.EncoderSettings.ReductionFactors;

			Packages = new List<EncoderPackage>();
			
			int fps = (cam != null) ? (cam.m_frameRate) : (config.Video.InputFrameRate);

			var needed_packages = ListRequiredPackages();

			int pkg_id = 0;
			foreach (var np in needed_packages) {
				EncoderJob job = new EncoderJob();
				job.OldSegmentNumber = 1;
				string joined = Path.Combine(config.EncoderSettings.LocalSystemOutputFolder, config.EncoderSettings.LocalSystemFilePrefix);

				joined += "_" + pkg_id;
				int bitrate = (int)(config.EncoderSettings.VideoBitrate * np.Quality); // linear scale

				int error = EncoderBridge.InitialiseEncoderJob(
					ref job,											// job to complete
					np.VideoSize.Width,									// OUTPUT video width
					np.VideoSize.Height,								// OUTPUT video height
					joined,												// OUTPUT folder + filename prefix
					fps,												// INPUT frame rate (output will match)
					bitrate,											// OUTPUT video bit rate
					config.EncoderSettings.FragmentSeconds);			// Fragment length (seconds)

				if (error != 0) throw new Exception("Encoder setup error #" + error);
				if (!job.IsValid) throw new Exception("Job rejected");

				var mf = new MediaFrame();
				mf.ForceAudioConsumption = (np.HasVideo) ? ((byte)0) : ((byte)1); // don't sync if no video.

				var pkg = new EncoderPackage(np, pkg_id, job, mf);

				ConnectPackageToBuffers(pkg, np);

				Packages.Add(pkg); pkg_id++;
			}
		}

		/// <summary>
		/// Create encoder buffers for each package, and add to capture buffer lists.
		/// </summary>
		private void ConnectPackageToBuffers (EncoderPackage Package, PackageSpec Spec) {
			if (Spec.HasVideo && ImageBuffers != null) {
				var vbuf = new ImageBuffer(Spec.VideoSize.Width, Spec.VideoSize.Height);
				ImageBuffers.Add(vbuf);
				Package.Buffers.Add(vbuf);
			}

			if (Spec.HasAudio && AudioBuffers != null) {
				AudioBufferMono abuf = null;
				if (mic != null) abuf = new AudioBufferMono(mic.SampleRate, mic.Channels);
				else abuf = new AudioBufferMono(config.Audio.SampleRate, config.Audio.Channels);
				AudioBuffers.Add(abuf);
				Package.Buffers.Add(abuf);
			}
		}

		/// <summary>
		/// Creates a list of encoder packages requires to fulfill the configured job.
		/// </summary>
		private List<PackageSpec> ListRequiredPackages () {
			var needed_packages = new List<PackageSpec>();
			var package_type = outputRouter.GetStreamMappingType();
			var sizes = CalculateScales(config.EncoderSettings.OutputWidth, config.EncoderSettings.OutputHeight, config.EncoderSettings.ReductionFactors);

			var f = config.EncoderSettings.ReductionFactors;
			int fi = 0;

			switch (package_type) {
				case HCS_Encoder.Outputs.StreamMapping.AllTypeStreams:
					foreach (var size in sizes) {
						needed_packages.Add(new PackageSpec(
							PackageSpec.BufferType.Audio | PackageSpec.BufferType.Video,
							size,
							f[fi++])
							);
					}
					break;
				case HCS_Encoder.Outputs.StreamMapping.SingleTypeStreams:
					foreach (var size in sizes) {
						needed_packages.Add(new PackageSpec(PackageSpec.BufferType.Video, size, f[fi++]));
					}
					needed_packages.Add(new PackageSpec(PackageSpec.BufferType.Audio, 1.0)); // only 1 audio quality at present
					break;
			}
			return needed_packages;
		}

		/// <summary>
		/// Initialise capture devices and connect the appropriate buffers
		/// </summary>
		private void CaptureSetup () {
			// Try setting up the capture devices.
			if (config.Audio.CaptureDeviceNumber >= 0) InitAudioCapture();
			if (config.Video.CaptureDeviceNumber >= 0) InitVideoCapture();

			// Set up buffers
			if (mic != null) {
				AudioBuffers = new AudioBufferList(config);
				mic.FrameAvailable += new EventHandler<AudioDataEventArgs>(AudioBuffers.HandleCapturedSamples);
			} else if (config.Audio.CaptureDeviceNumber == -2) { // dummy mode: buffers but no capture
				AudioBuffers = new AudioBufferList(config);
			}

			if (cam != null) {
				ImageBuffers = new ImageBufferList(config);
				cam.TargetFrameSize = new Size(config.EncoderSettings.OutputWidth, config.EncoderSettings.OutputHeight);
				cam.FrameAvailable += new EventHandler<VideoDataEventArgs>(ImageBuffers.HandleCapturedFrame);
			} else if (config.Video.CaptureDeviceNumber == -2) { // dummy mode: buffers but no capture
				ImageBuffers = new ImageBufferList(config);
			}

			if (ImageBuffers == null && AudioBuffers == null) throw new Exception("Neither Audio or Video capture was specified");
		}

		/// <summary>
		/// Given a set of linear scale factors, create a set of scaled (w*h) sizes.
		/// </summary>
		/// <remarks>
		/// Scaling the height and width directly will result in pixel count being reduced by the square
		/// of the scale factor, so instead we apply a rough factor to get a linear scaled pixel count.
		/// </remarks>
		private List<Size> CalculateScales (int Width, int Height, List<double> ScaleFactors) {
			var outp = new List<Size>();
			foreach (var factor in ScaleFactors) {
				if (factor > 1.0 || factor < 0.1) throw new Exception("All scale factors should be between 0.1 and 1.0");

				double invSq = 1.0 - ((1.0 - factor) * (1.0 - factor)); // this is only rough, but is near enough

				// scale
				int w = (int)Math.Floor(Width * invSq);
				int h = (int)Math.Floor(Height * invSq);

				// optimise:
				w -= w % 2;
				h -= h % 2;

				outp.Add(new Size(w, h));
			}
			return outp;
		}

		private void InitVideoCapture () {
			try {
				cam = new VideoCapture(
					config.Video.CaptureDeviceNumber,
					config.Video.InputFrameRate,
					config.Video.InputWidth,
					config.Video.InputHeight);
			} catch (Exception ex) {
				throw new Exception("Video capture settings don't work (if you don't want video, remember to set the video capture to (none) in the config tool)", ex);
			}
		}

		private void InitAudioCapture () {
			try {
				mic = new AudioCapture(
					config.Audio.CaptureDeviceNumber,
					config.Audio.SampleRate,
					config.Audio.Channels);
			} catch (Exception ex) {
				throw new Exception("Audio capture settings don't work (if you don't want audio, remember to set the audio capture to (none) in the config tool)", ex);
			}
		}

		/// <summary>
		/// Work out a resonable sleep duration (over which we'd expect a new frame in at least one buffer)
		/// </summary>
		private void AdjustFrameSleep () {
			if (cam != null) frameSleep = (int)(1000.0 * (0.5 / cam.m_frameRate));
			else frameSleep = 50;
			if (frameSleep < 1) frameSleep = 1;
			if (frameSleep > 250) frameSleep = 250;
		}

		/// <summary>
		/// Returns true if any of the active capture buffers are below their encoding theshold.
		/// </summary>
		private bool BuffersEmpty() {
			if (Packages.Any(a => a.BuffersEmpty(MinimumBufferPopulation))) {
				foreach (var pkg in Packages) {
					foreach (var buf in pkg.Buffers) buf.RebufferCapturedFrames();
				}
			}
			return Packages.Any(a => a.BuffersEmpty(MinimumBufferPopulation));
		}

		#endregion
	}
}
