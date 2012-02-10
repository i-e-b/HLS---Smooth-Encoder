using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HCS_Encoder.Inputs.Buffers {

	/// <summary>
	/// Interface from buffer handlers to encoder loop
	/// </summary>
	public interface IEncoderBuffer {

		double NextCaptureTime { get; }

		int QueueLength { get; }

		void WipeBuffer ();

		void WipeBufferUntil (double AbandonTime);
		
		void LoadToFrame (ref MediaFrame Frame);

		void UnloadFrame (ref MediaFrame Frame);

		void RebufferCapturedFrames ();
	}
}
