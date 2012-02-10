using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HCS_Encoder.Inputs.Buffers {

	/// <summary>
	/// Interface from capture devices to buffer handlers
	/// </summary>
	public interface ICaptureBuffer {

		double NextCaptureTime {get;}

		int QueueLength {get;}

		void WipeBuffer ();

		void WipeBufferUntil (double AbandonTime);


		void RegisterPlugin (HCS_Encoder.Inputs.Processing.IVideoProcessor PlugIn);
		void RegisterPlugin (HCS_Encoder.Inputs.Processing.IAudioProcessor PlugIn);


		void HandleCapturedSamples (object sender, AudioDataEventArgs e);
		void HandleCapturedFrame (object sender, VideoDataEventArgs e);
	}
}
