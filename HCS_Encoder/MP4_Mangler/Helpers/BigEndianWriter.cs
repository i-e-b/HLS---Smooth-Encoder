using System;
using System.Collections.Generic;
using System.IO;

namespace System.IO {

	/// <summary>
	/// A class for writing to BigEndian streams (i.e. Network or Video).
	/// Designed to be a drop-in replacement for System.IO.BinaryWriter
	/// </summary>
	public class BigEndianWriter {

		/// <summary>Holds the underlying stream.</summary>
		protected Stream OutStream;

		/// <summary>
		/// Initializes a new instance of the System.IO.BinaryWriter class that writesto a stream.
		/// </summary>
		protected BigEndianWriter () {
		}

		/// <summary>
		/// Initializes a new instance of the System.IO.BinaryWriter class based on the
		/// supplied stream and using UTF-8 as the encoding for strings.
		/// </summary>
		/// <param name="output">The output stream</param>
		public BigEndianWriter (Stream output) {
			OutStream = output;
		}

		/// <summary>Gets the underlying stream of the System.IO.BinaryWriter.</summary>
		public virtual Stream BaseStream { get { return OutStream; } }

		/// <summary>Closes the current System.IO.BinaryWriter and the underlying stream.</summary>
		public virtual void Close () {
			OutStream.Close();
		}

		/// Clears all buffers for the current writer and causes any buffered data to
		/// be written to the underlying device.
		/// </summary>
		public virtual void Flush () {
			OutStream.Flush();
		}
		
		/// <summary>
		/// Not supported
		/// </summary>
		public virtual long Seek (int offset, SeekOrigin origin) {
			throw new NotSupportedException();
		}

		// Please excuse the lack of method XML comments.
		// Unless noted, the behaviour of each method 
		//  is as in BinaryWriter.

		/// <summary>Writes a BYTE (0 or 1) value</summary>
		public virtual void Write (bool value) {
			OutStream.WriteByte((byte)(value ? (1) : (0)));
		}
		public virtual void Write (byte value) {
			OutStream.WriteByte(value);
		}

		/// <summary>
		/// Write a byte buffer. Current order is preserved (no endian-swap)
		/// </summary>
		public virtual void Write (byte[] buffer) {
			OutStream.Write(buffer, 0, buffer.Length);
		}
		public virtual void Write (char ch) {
			if (BitConverter.IsLittleEndian) { // I could maybe do this with a switched delegate?
				WriteReverse(BitConverter.GetBytes(ch));
			} else {
				WriteForward(BitConverter.GetBytes(ch));
			}
		}

		public virtual void Write (char[] chars) {
			foreach (char c in chars) this.Write(c); // characters still go forwards, even if bytes go backward.
		}

		public virtual void Write (decimal value) {
			int[] bits = decimal.GetBits(value);
			if (BitConverter.IsLittleEndian) { // I could maybe do this with a switched delegate?
				for (int i = bits.Length - 1; i >= 0; i--) {
					WriteReverse(BitConverter.GetBytes(bits[i]));
				}
			} else {
				for (int i = 0; i < bits.Length; i++) {
					WriteForward(BitConverter.GetBytes(bits[i]));
				}
			}
		}
		public virtual void Write (double value) {
			if (BitConverter.IsLittleEndian) {
				WriteReverse(BitConverter.GetBytes(value));
			} else {
				WriteForward(BitConverter.GetBytes(value));
			}
		}
		public virtual void Write (float value) {
			if (BitConverter.IsLittleEndian) {
				WriteReverse(BitConverter.GetBytes(value));
			} else {
				WriteForward(BitConverter.GetBytes(value));
			}
		}
		public virtual void Write (int value) {
			if (BitConverter.IsLittleEndian) {
				WriteReverse(BitConverter.GetBytes(value));
			} else {
				WriteForward(BitConverter.GetBytes(value));
			}
		}
		public virtual void Write (long value) {
			if (BitConverter.IsLittleEndian) {
				WriteReverse(BitConverter.GetBytes(value));
			} else {
				WriteForward(BitConverter.GetBytes(value));
			}
		}
		public virtual void Write (sbyte value) {
			if (BitConverter.IsLittleEndian) {
				WriteReverse(BitConverter.GetBytes(value));
			} else {
				WriteForward(BitConverter.GetBytes(value));
			}
		}
		public virtual void Write (short value) {
			if (BitConverter.IsLittleEndian) {
				WriteReverse(BitConverter.GetBytes(value));
			} else {
				WriteForward(BitConverter.GetBytes(value));
			}
		}
		/// <summary>
		/// Not Supported
		/// </summary>
		public virtual void Write (string value) {
			throw new NotSupportedException("Please use an Encoder to write strings big-endian");
		}
		public virtual void Write (uint value) {
			if (BitConverter.IsLittleEndian) {
				WriteReverse(BitConverter.GetBytes(value));
			} else {
				WriteForward(BitConverter.GetBytes(value));
			}
		}
		public virtual void Write (ulong value) {
			if (BitConverter.IsLittleEndian) {
				WriteReverse(BitConverter.GetBytes(value));
			} else {
				WriteForward(BitConverter.GetBytes(value));
			}
		}
		public virtual void Write (ushort value) {
			if (BitConverter.IsLittleEndian) {
				WriteReverse(BitConverter.GetBytes(value));
			} else {
				WriteForward(BitConverter.GetBytes(value));
			}
		}

		/// <summary>
		/// Write a byte buffer. Current order is preserved (no endian-swap)
		/// </summary>
		public virtual void Write (byte[] buffer, int index, int count) {
			for (int i = index; i < index + count; i++) {
				OutStream.WriteByte(buffer[i]);
			}
		}
		public virtual void Write (char[] chars, int index, int count) {
			for (int i = index; i < index + count; i++) {
				this.Write(chars[i]);
			}
		}

		/// <summary>
		/// Not supported
		/// </summary>
		protected void Write7BitEncodedInt (int value) {
			throw new NotSupportedException();
		}

		/// <summary>
		/// Write the given byte array in 0..n order
		/// </summary>
		public void WriteForward (byte[] data) {
			OutStream.Write(data, 0, data.Length);
		}

		/// <summary>
		/// Write the given byte array in 0..n (reverse) order
		/// </summary>
		public void WriteReverse (byte[] data) {
			for (int i = data.Length - 1; i >= 0; i--) {
				OutStream.WriteByte(data[i]);
			}
		}

		public void Write (Guid uuid_head) {
			this.WriteForward(uuid_head.ToByteArray()); // test this: might need to be 'WriteReverse()'
		}
	}
}
