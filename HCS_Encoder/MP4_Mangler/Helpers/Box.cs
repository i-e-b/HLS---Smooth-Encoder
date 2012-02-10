using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler {

	/// <summary>
	/// Box is the top-level data class in MP4 files.
	/// This abstract class implements most of the ISO file system requirements.
	/// It does not yet handle parsing (input from files).
	/// </summary>
	public abstract class Box {
		// the size and long-size values are calculated on-the-fly in my model.
		protected UInt32 _fourCC;      // four-cc code of the box. Must be correct!
		protected MemoryStream _data;  // Stored data of this box (not including header data or child data).
		protected byte[] _flags;      // Flags for 'FullBox'; null if not used
		protected List<Box> _children; // List of child boxes. All child box data is stored in order, and after this boxes' data & headers.

		#region Helper methods
		/// <summary>
		/// Gives a date as a number of seconds since 1904-01-01 T 00:00
		/// </summary>
		public static UInt64 BoxDate (DateTime date) {
			DateTime _1904 = new DateTime(1904, 01, 01);
			TimeSpan ts = date - _1904;
			return (UInt64)ts.TotalSeconds;
		}

		/// <summary>
		/// Convert a four character code string into a 32 bit int, suitable
		/// for writing with 'BigEndianWriter'
		/// </summary>
		public static UInt32 FourCC (string fourcc) {
			byte[] b = Encoding.ASCII.GetBytes(fourcc);
			return (UInt32)((b[0] << 24) + (b[1] << 16) + (b[2] << 8) + (b[3]));
		}
		#endregion

		/// <summary>
		/// Create a generic box
		/// </summary>
		public Box (string FOURCC) {
			_fourCC = Box.FourCC(FOURCC);

			_flags = null;
			_data = null;
		}

		/// <summary>
		/// Add a child box into this box.
		/// Child box should be a sub-class of Box (i.e. not a generic 'Box')
		/// </summary>
		/// <param name="child"></param>
		public void AddChild (Box child) {
			if (_children == null) _children = new List<Box>();

			_children.Add(child);
		}

		/// <summary>
		/// Size of self, disregarding children (in bytes)
		/// Override this to add extra size to base() if your sub-class adds extra fields
		/// </summary>
		protected virtual ulong selfSize () {
			ulong l = 0;
			if (_data != null) {
				l += (ulong)_data.Length;
			}
			return l;
		}

		/// <summary>
		/// Size of self plus all children (container size).
		/// Includes size for FOURCC and basic 'FullBox' flags.
		/// </summary>
		/// <remarks>You probably don't need to override this</remarks>
		public virtual ulong deepSize () {
			Prepare();
			ulong size = selfSize() + 4; // four for FOURCC
			if (_flags != null) size += 4;
			if (_children != null) {
				foreach (Box child in _children) {
					size += child.deepSize();
				}
			}

			if (size >= UInt32.MaxValue) {
				size += 4 + 8; // 'size' + 'largesize'
			} else {
				size += 4; // 'size'
			}

			return size;
		}

		/// <summary>
		/// Output the header data for this Box.
		/// Override this to add extra data if your sub-class adds extra fields
		/// </summary>
		/// <returns></returns>
		protected virtual byte[] selfData () {
			ulong size = deepSize();
			MemoryStream ms = new MemoryStream();

			BigEndianWriter ous = new BigEndianWriter(ms);

			if (size >= UInt32.MaxValue) {
				ous.Write((UInt32)1U);
				ous.Write((UInt32)_fourCC);
				ous.Write((UInt64)size);
			} else {
				ous.Write((UInt32)size);
				ous.Write((UInt32)_fourCC);
			}
			if (_flags != null) ous.WriteForward(_flags);
			return ms.ToArray();
		}

		/// <summary>
		/// Return data for this and all children, in correct order.
		/// </summary>
		/// <remarks>You probably don't need to override this</remarks>
		/// <returns>Raw data to add to filestream</returns>
		public virtual byte[] deepData () {
			MemoryStream ms = new MemoryStream();

			byte[] sd = selfData();

			if (sd != null) {
				ms.Write(sd, 0, sd.Length);
			}

			if (_data != null) {
				byte[] buf = _data.ToArray();
				if (buf != null && buf.LongLength > 0) ms.Write(buf, 0, buf.Length);
			}

			if (_children != null) {
				foreach (Box child in _children) {
					byte[] buffer = child.deepData();
					if (buffer != null && buffer.LongLength > 0) ms.Write(buffer, 0, buffer.Length);
				}
			}
			

			return ms.ToArray();
		}

		/// <summary>
		/// Prepare for data gathering.
		/// Your '_data' members should be populated here at the latest.
		/// </summary>
		public abstract void Prepare ();

		/// <summary> Convert .Net ticks (100ns) to Mpeg ticks (90MHz) </summary>
		protected ulong NetToMpgTicks (ulong MpgTicks) {
			return (MpgTicks * 9UL) / 1000UL;
		}
	}
}
