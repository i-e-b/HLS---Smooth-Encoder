using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HCS_Encoder.Inputs.Buffers {
	/// <summary>
	/// Capture buffer for audio that writes to a list of encoder buffers.
	/// </summary>
	/// <remarks>Currently not very efficient. Doesn't yet support variable quality/bitrate buffers.</remarks>
	public class AudioBufferList : List<AudioBufferMono>, ICaptureBuffer {
		#region ICaptureBuffer Members

		public AudioBufferList (EncoderConfiguration.Configuration config) {
		}

		public double NextCaptureTime {
			get {
				if (this.Count < 1) return 0.0;
				else return this.Max(a => a.NextCaptureTime); }
		}

		public int QueueLength {
			get { return this.Min(a => a.QueueLength); }
		}

		public void WipeBuffer () {
			foreach (var buf in this) {
				buf.WipeBuffer();
			}
		}

		public void WipeBufferUntil (double AbandonTime) {
			foreach (var buf in this) {
				buf.WipeBufferUntil(AbandonTime);
			}
		}

		public void RegisterPlugin (HCS_Encoder.Inputs.Processing.IAudioProcessor PlugIn) {
			foreach (var buf in this) {
				buf.RegisterPlugin(PlugIn);
			}
		}

		public void RegisterPlugin (HCS_Encoder.Inputs.Processing.IVideoProcessor PlugIn) {
			throw new NotSupportedException("Audio buffer does not support video plug-ins");
		}


		public void HandleCapturedSamples (object sender, AudioDataEventArgs e) {
			foreach (var buf in this) {
				buf.HandleCapturedSamples(sender, e);
			}
		}


		public void HandleCapturedFrame (object sender, VideoDataEventArgs e) {
			throw new NotSupportedException("Audio buffer does not support video frames");
		}

		#endregion
	}
}
