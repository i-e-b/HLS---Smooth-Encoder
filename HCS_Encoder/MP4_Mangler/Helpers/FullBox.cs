using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler {
	/// <summary>
	/// Full box is a container box which has flags and version data
	/// </summary>
	public abstract class FullBox : Box {

		/// <summary>
		/// Create a 'Full' box.
		/// This shouldn't be used directly, but can be subclassed where needed.
		/// </summary>
		public FullBox (byte Version, UInt32 Flags, string fourcc):base(fourcc) {
			// 0x01, 0x??234567
			// ==>
			// 01 23 45 67;

			_flags = new byte[] {
				Version,
				(byte)((Flags & 0x00FF0000u) >> 16),
				(byte)((Flags & 0x0000FF00u) >> 8),
				(byte)((Flags & 0x000000FFu))
			};
		}
	}
}
