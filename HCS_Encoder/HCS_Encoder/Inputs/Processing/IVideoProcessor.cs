using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace HCS_Encoder.Inputs.Processing {
	/// <summary>
	/// Interface for video processing plug-ins.
	/// IMPORTANT: plug-ins are part of the live encode system, and should be as fast as possible
	/// (return quickly, use as little processor as possible)
	/// </summary>
	public interface IVideoProcessor {

		/// <summary>Process a GDI image representation of a frame</summary>
		void ProcessFrame (Image Frame, double CaptureTime);

	}

}
