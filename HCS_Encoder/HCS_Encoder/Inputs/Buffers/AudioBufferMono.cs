using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using HCS_Encoder.Inputs.Processing;

namespace HCS_Encoder.Inputs.Buffers {
	public class TimedSample {
		public short[] Samples { get; set; }
		public double Seconds { get; set; }

		public TimedSample (short[] samples, double time) {
			Samples = samples;
			Seconds = time;
		}
	}

	public class AudioBufferMono : IEncoderBuffer {
		private GCHandle _pinSamples;
		private int _incomingSampleRate;
		private int _channels;
		private List<TimedSample> _sampleBuffer;
		private short[] _samples; // Stored in object to make unmanaged pinning more reliable.

		/// <summary>
		/// Encoder ideal frame size.
		/// The buffer will deliver samples at this size.
		/// </summary>
		public int FrameSize { get; set; }

		public double NextCaptureTime {
			get {
				if (_sampleBuffer.Count < 1) return 0.0;
				return _sampleBuffer[0].Seconds;
			}
		}

		/// <summary>
		/// Number of frames waiting to be encoded.
		/// </summary>
		/// <remarks>This is reduced by 1, as the first frame is used as a sub-buffer to help A/V sync.</remarks>
		public int QueueLength {
			get { lock (_sampleBuffer) { return Math.Max(0, _sampleBuffer.Count - 1); } }
		}

		/// <summary>
		/// Queue of audio processors to apply.
		/// </summary>
		public List<IAudioProcessor> ProcessorQueue { get; private set; }

		public void RegisterPlugin (IVideoProcessor PlugIn) {
			throw new NotSupportedException("Can not apply video plugins to audio buffers");
		}
		public void RegisterPlugin (IAudioProcessor PlugIn) {
			if (PlugIn == null) return;
			if (ProcessorQueue == null) ProcessorQueue = new List<IAudioProcessor>();

			ProcessorQueue.Add(PlugIn);
		}

		/// <summary>
		/// Prepare an audio frame buffer.
		/// Sample rate and channels should match INCOMING signal (from capture device).
		/// Always outputs 44.1kHz 16 bit PCM mono audio.
		/// </summary>
		public AudioBufferMono (int SampleRate, int Channels) {
			_sampleBuffer = new List<TimedSample>(100);
			ProcessorQueue = new List<IAudioProcessor>();
			_channels = Channels;
			_incomingSampleRate = SampleRate;
			FrameSize = 1152; // guess for now, the programmer can set through property later.
		}

		/// <summary>
		/// Respond to capture event.
		/// This should return as fast as possible.
		/// </summary>
		public void HandleCapturedSamples (object sender, AudioDataEventArgs e) {
			if (e.Samples == null || e.Samples.Length <= 0) return;

			int sample_sec = 44100;

			short[] result = null;
			if (_channels == 1) result = ResampleBuffer(e, sample_sec);
			else result = ResampleStereo(e, sample_sec);

			TimedSample ts = new TimedSample(result, e.CaptureTime);

			try {
				if (ProcessorQueue != null) {
					foreach (var item in ProcessorQueue) {
						item.ProcessSample(ts);
					}
				}
			} catch { }

			lock (_sampleBuffer) {
				_sampleBuffer.Add(ts);
			}

		}

		/// <summary>
		/// Not yet implemented.
		/// </summary>
		public void RebufferCapturedFrames () {
		}

		/// <summary>
		/// Error based resampler.
		/// Only works for MONO.
		/// </summary>
		private short[] ResampleBuffer (AudioDataEventArgs e, double sample_sec) {
			int total_samps = e.Samples.Length;
			int out_samples = (int)(total_samps * (sample_sec / (double)(_incomingSampleRate)));
			if (total_samps == out_samples) return e.Samples; // don't bother resampling if it's already correct
			double error_rate = (total_samps / out_samples);

			unchecked {
				short[] result = new short[out_samples];

				short[] source = e.Samples;
				for (int i = 0; i < result.Length; i++) {
					int ec = (int)(i * error_rate);
					ec = Math.Min(ec, e.Samples.Length - 1);
					ec = Math.Max(ec, 0);

					result[i] = e.Samples[ec];
				}
				return result;
			}
			
		}

		/// <summary>
		/// Handles the more complex case of resampling interleaved stereo samples to mono
		/// </summary>
		private short[] ResampleStereo (AudioDataEventArgs e, double sample_sec) {
			if (e.Samples.Length < 2) return e.Samples;
			int total_samps = e.Samples.Length;
			int out_samples = (int)(total_samps * (sample_sec / (double)(_incomingSampleRate)));
			out_samples /= _channels;
			out_samples -= out_samples % _channels;
			if (out_samples < 1) return e.Samples;
			double error_rate = (total_samps / out_samples);

			unchecked {
				short[] result = new short[out_samples];

				short[] source = e.Samples;
				for (int c = 0; c < _channels; c++) {
					for (int i = c; i < result.Length; i+=_channels) {
						int ec = (int)(i * error_rate);

						ec -= ec % _channels; // fix to channel slot
						ec += c;

						ec = Math.Min(ec, e.Samples.Length - 1);
						ec = Math.Max(ec, 0);

						result[i] += (short)(e.Samples[ec] / _channels);

					}

				}
				return result;
			}
		}

		/// <summary>
		/// Load the buffer into a MediaFrame for the encoder.
		/// IMPORTANT: You must call UnloadFrame after this method is called.
		/// For effciency, unload as soon as possible.
		/// </summary>
		public void LoadToFrame (ref MediaFrame Frame) {
			try {
				TimedSample ts = null;
				lock (_sampleBuffer) {
					if (_sampleBuffer.Count < 1) {
						Frame.AudioSize = 0UL;
						Frame.AudioSamplesConsumed = 0;
						return;
					}
					_sampleBuffer.RemoveAll(a => a == null);
					_sampleBuffer.Sort((a, b) => a.Seconds.CompareTo(b.Seconds));

					if (_sampleBuffer[0].Samples.Length < FrameSize) MergeFirstSample(); // Make sure frames are large enough!

					if (_sampleBuffer.Count > 0) {
						ts = _sampleBuffer[0];
					} else {
						Frame.AudioSize = 0;
						Frame.AudioBuffer = IntPtr.Zero;
						return;
					}
				}

				_samples = ts.Samples;

				Frame.AudioSampleTime = ts.Seconds;
				Frame.AudioSize = (ulong)_samples.LongLength;
				Frame.AudioSamplesConsumed = 0;

				// Outgoing sample rate is always 44100, to support iPhone
				Frame.AudioSampleRate = 44100; // this is used to correct timing on the encoder.

				_pinSamples = GCHandle.Alloc(_samples, GCHandleType.Pinned);
				Frame.AudioBuffer = _pinSamples.AddrOfPinnedObject();
			} catch (Exception ex) {
				UnloadFrame(ref Frame);
				Console.WriteLine("Loading audio frame failed: " + ex.Message);
			}
		}

		/// <summary>
		/// Release memory previously locked by LoadToFrame()
		/// </summary>
		public void UnloadFrame (ref MediaFrame Frame) {
			// Check how much was used, and re-integrate unused samples.
			lock (_sampleBuffer) {
				if (_sampleBuffer.Count > 0 && Frame.AudioSamplesConsumed == Frame.AudioSize) {
					_sampleBuffer.RemoveAt(0); // Buffer was fully used. Drop it
				} else {
					PrependUnusedSamples(Frame);
				}
			}

			Frame.AudioBuffer = IntPtr.Zero;
			Frame.AudioSize = 0;
			Frame.AudioSamplesConsumed = 0;

			if (_pinSamples.IsAllocated) _pinSamples.Free();
			_samples = null;
		}

		/// <summary>
		/// Join the first two samples together, making a large first sample.
		/// Resulting sample retains first sample's timecode.
		/// </summary>
		private void MergeFirstSample () {
			lock (_sampleBuffer) {
				if (_sampleBuffer.Count < 2) return; // no samples to merge.

				short[] übersample = _sampleBuffer[0].Samples.Concat(_sampleBuffer[1].Samples).ToArray();

				TimedSample ts = new TimedSample(übersample, _sampleBuffer[0].Seconds);
				_sampleBuffer.RemoveRange(0, 2);
				_sampleBuffer.Insert(0, ts);
			}
		}

		/// <summary>
		/// Add unused samples back into the sample buffer.
		/// </summary>
		/// <remarks>Time stamps need to be properly re-integrated!</remarks>
		private void PrependUnusedSamples (MediaFrame Frame) {
			// checks:
			if (_sampleBuffer == null) return;
			_sampleBuffer.RemoveAll((a) => a == null); // clean out bad transfers
			if (_sampleBuffer.Count < 1) return;
			if ((ulong)_sampleBuffer[0].Samples.LongLength != Frame.AudioSize) throw new Exception("Frames unloaded out-of-sync. Frames must be loaded then unloaded in order and one-at-a-time!"); // wrong frame!
			
			// Build new truncated sample:
			ulong new_sample_count = Frame.AudioSize - Frame.AudioSamplesConsumed;
			if (new_sample_count < 1) {
				_sampleBuffer.RemoveAt(0);
				return;
			}

			short[] cut = new short[new_sample_count]; // pun intended ;-)
			Array.Copy(_sampleBuffer[0].Samples, (long)Frame.AudioSamplesConsumed, cut, 0, (long)new_sample_count);
			double new_time_stamp = Frame.AudioSampleTime + (Frame.AudioSamplesConsumed / 44100.0);
			TimedSample sample = new TimedSample(cut, new_time_stamp);

			lock (_sampleBuffer) {
				// Over-write the old sample with the new, shorter version
				_sampleBuffer[0] = sample;

				// clean out bad transfers:
				_sampleBuffer.Sort((a, b) => a.Seconds.CompareTo(b.Seconds));
			}

			// merge function to join first two samples if the first is small.
			if (_sampleBuffer.Count >= 2) {
				if (_sampleBuffer[0].Samples.Length < 4608) MergeFirstSample(); // 4 frames
			}
		}

		/// <summary>
		/// Emergency frame drop. Removes all waiting samples.
		/// </summary>
		public void WipeBuffer () {
			lock (_sampleBuffer) {
				_sampleBuffer.Clear();
			}
			GC.Collect();
		}

		public void WipeBufferUntil (double AbandonTime) {
			lock (_sampleBuffer) {
				while (_sampleBuffer.Count > 0 && _sampleBuffer[0].Seconds < AbandonTime) {
					_sampleBuffer.RemoveAt(0);
				}
			}
			GC.Collect();
		}
	}
}
