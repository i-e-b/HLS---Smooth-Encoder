using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO {
	/// <summary>
	/// Splits a byte array into values, based on a series of bit lengths
	/// Translations are done from network byte order.
	/// </summary>
	public class BitSplitter {
		protected byte[] src;
		protected long offset; // offset number of BITS.

		public BitSplitter (byte[] RawData) {
			offset = 0L;
			src = RawData;
		}

		/// <summary>Byte number to start</summary>
		public int ByteOffset{get{ return (int)(offset / 8L); }}

		/// <summary>Bit number to start inside current byte</summary>
		public int BitOffset{get{ return (int)(offset % 8L); }}

		/// <summary>
		/// Create a byte mask
		/// </summary>
		/// <param name="start">number of bits skipped</param>
		/// <param name="length">run length (bits)</param>
		protected byte[] Mask (int start, int length) {
			if (length < 1) return new byte[] { 0x00 };

			int Bcnt = (int)Math.Ceiling((start + length) / 8.0);
			byte[] output = new byte[Bcnt];
			byte val = 0x80; // top bit;
			int bcnt = start + length;

			for (int i = 0; i < bcnt; i++) {
				int B = i / 8;
				int b = i % 8;

				if (b == 0) val = 0x80;
				else val >>= 1;

				if (i >= start) output[B] |= val;
			}
			return output;
		}

		/// <summary>
		/// Get an unshifted array of bits packed in bytes.
		/// Advances position counter.
		/// </summary>
		protected byte[] GetNext (int l) {
			byte[] m = Mask(BitOffset, l);
			for (int i = 0; i < m.Length; i++) {
				m[i] = (byte)(m[i] & src[ByteOffset + i]);
			}
			offset += l;
			return m;
		}

		/// <summary>
		/// Move forward to next byte edge (moves between 1 and 8 bits)
		/// </summary>
		public void SkipToNextByte () {
			offset += 8 - BitOffset;
		}

		/// <summary>
		/// Move forward a given number of bits.
		/// They are consumed but not processed.
		/// </summary>
		public void SkipBits (int BitCount) {
			offset += BitCount;
		}

		/// <summary>
		/// Move forward a given number of bytes.
		/// They are consumed but not processed.
		/// </summary>
		public void SkipBytes (int ByteCount) {
			offset += ByteCount * 8;
		}

		/// <summary>
		/// Read a single bit, and return true if set to '1'.
		/// Advances position by 1 bit
		/// </summary>
		public bool GetFlag () {
			byte[] v = GetNext(1);
			if (v.Length != 1) throw new Exception("Unexpected bytes!");
			return v[0] != 0;
		}

		/// <summary>
		/// Move to the next byte edge (no action if already on one -- moves between 0 and 7 bits)
		/// </summary>
		public void AlignToByte () {
			offset += 8 - BitOffset;
		}

		/// <summary>
		/// Return a new array containing all remaining whole bytes not consumed.
		/// </summary>
		public byte[] RemainingBytes () {
			MemoryStream ms = new MemoryStream(src.Length - ByteOffset);
			ms.Write(src, ByteOffset, src.Length - ByteOffset);
			return ms.ToArray();
		}

		/// <summary>
		/// Return an unsigned long for the integer bits
		/// </summary>
		public ulong GetInteger (int BitLength) {
			ulong outval = 0L;
			int rshift, lshift;
			
			byte[] v = GetNext(BitLength);
			if (BitOffset == 0) { // landed on a byte-boundary
				rshift = 0;
			} else {
				rshift = 8 - BitOffset;
			}
			lshift = 8 - rshift;


			// This mess shifts the bits of a byte array by up to 8 places
			// and reverses byte order, joining the result into a ulong.
			int psh = 0;
			for (int i = v.Length - 1; i >= 0; i--) {
				int n0 = v[i] >> rshift;
				int n1 = (i > 0) ? (v[i-1] << lshift) : (0);

				outval += (ulong)(((n0 + n1) & 0xFF) << psh);
				psh += 8;
			}

			return outval;
		}
	}
}
