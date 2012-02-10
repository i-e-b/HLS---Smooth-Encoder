using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace HCS_Encoder {
	// This file holds the encoder-based structures and enumerations that don't fit nicely anywhere else.

	public enum AudioSampleRate : int {
		Default = 0,	// Whatever is supported.
		_11kHz = 11025, // Very low, not recommended for anything anymore.
		_22kHz = 22050, // Low, suitable for speech and simple audio
		_32kHz = 32000, // MP3 low.
		_44kHz = 44100, // CD Audio, suitable for high quality audio and MP3s
		_48kHz = 48000  // Broadcast low. Suitable most most audio signals.
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct EncoderJob {
		[MarshalAs(UnmanagedType.Bool)]
		public bool IsValid;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		public string BaseDirectory;

		public Int32 FrameRate;
		public UInt64 FrameCount;
		public Int32 Bitrate;	// Video bitrate.

		public Int32 Width, Height;

		public double SegmentDuration;
		public Int32 SegmentNumber, OldSegmentNumber;

		#region Internal Handles
		// Don't play with these, they're dangerous!
		private IntPtr fmt, oc, audio_st, video_st;
		private IntPtr audio_outbuf;
		private IntPtr picture;
		private IntPtr video_outbuf;
		private int audio_buf_size;
		private int video_outbuf_size;
		private UInt64 SplitNextKey;
		private UInt64 a_pts, v_pts;
		private IntPtr tswriter;
		#endregion
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DecoderJob {
		public int IsValid;				// true if the job was created properly.

		public Int32 videoWidth, videoHeight;
		public double Framerate;

		public Int32 AudioSampleRate;
		public Int32 AudioChannels;
		public Int32 MinimumAudioBufferSize;
		public UInt64 FrameCount, SampleCount;

		#region Internal Handles
		private IntPtr pFormatCtx;
		private int videoStream, audioStream;
		private IntPtr pCodecCtx;
		private IntPtr pCodec;
		private IntPtr pFrame;

		private IntPtr aCodecCtx;
		private IntPtr aCodec;
		#endregion
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MediaFrame {
		public UInt64 VideoSize; // number of bytes of Y plane (u & v will be VideoSize / 4)
		public IntPtr Yplane;
		public IntPtr Uplane, Vplane;
		public Double VideoSampleTime; // Capture time (for sync)

		public UInt64 AudioSamplesConsumed; // Video is always used. Audio will be used when appropriate.

		public UInt64 AudioSize; // size of AudioBuffer
		public IntPtr AudioBuffer; // raw audio data
		public Double AudioSampleTime; // Capture time (for sync)
		public Int32 AudioSampleRate; // Samples per second. Must be correct.

		public UInt64 DataSize; // number of bytes of non-AV data
		public IntPtr DataStreamData; // raw non-AV data
		public Int32 DataStreamTrack; // Track ID for the data-stream.
		public Double DataStreamTime; // Target packet time

		public byte ForceAudioConsumption; // Allows audio to desync from video. More audio may be consumed.
	}

	public static class EncoderBridge {
		#region Encoding
		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int InitialiseEncoderJob (ref EncoderJob JobSpec, int Width, int Height,
			string BaseDirectory, int FrameRate, int Bitrate, double SegmentDuration);

		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		[System.Security.SuppressUnmanagedCodeSecurity] // this is for performance. Only makes a difference in tight loops.
		public static extern void EncodeFrame (ref EncoderJob JobSpec, ref MediaFrame Frame);

		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void CloseEncoderJob (ref EncoderJob JobSpec);

		/// <summary>
		/// Returns the codec's startup data (other than in frames).
		/// This may be null or empty.
		/// </summary>
		public static byte[] GetVideoCodecData (EncoderJob JobSpec) {
			int bufsz = EncoderBridge.GetVideoCodecDataSize(ref JobSpec);
			byte[] buffer = null;
			if (bufsz > 0) {
				buffer = new byte[bufsz];				
				GCHandle bufh = GCHandle.Alloc(buffer, GCHandleType.Pinned);
				try {
					EncoderBridge.GetVideoCodecData(ref JobSpec, bufh.AddrOfPinnedObject());
				} finally {
					bufh.Free();
				}
			}
			return buffer;
		}

		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int GetVideoCodecDataSize (ref EncoderJob JobSpec);
		
		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void GetVideoCodecData (ref EncoderJob JobSpec, IntPtr Buffer);
		#endregion

		#region Utilities

		/// <summary>Convert a interleaved 24bpp RGB image into a planar 24bpp YUV image</summary>
		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		[System.Security.SuppressUnmanagedCodeSecurity] // this is for performance. Only makes a difference in tight loops.
		public unsafe static extern void Rgb2YuvIS (int w, int h, IntPtr RgbSrc, IntPtr Y, IntPtr U, IntPtr V);

		/// <summary>
		/// Rescale a single plane of a 8-bit per sample planar image
		/// </summary>
		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		[System.Security.SuppressUnmanagedCodeSecurity] // this is for performance. Only makes a difference in tight loops.
		public unsafe static extern void PlanarScale (IntPtr Src, IntPtr Dst, int SrcWidth, int SrcHeight, int DstWidth, int DstHeight, bool HighQuality);

		/// <summary>
		/// Rescale an interleaved 3 channel 24bpp image.
		/// </summary>
		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		[System.Security.SuppressUnmanagedCodeSecurity] // this is for performance. Only makes a difference in tight loops.
		public unsafe static extern void InterleavedScale (IntPtr Src, IntPtr Dst, int SrcWidth, int SrcHeight, int DstWidth, int DstHeight, bool HighQuality);

		
		#endregion

		#region Decoding
		/// <summary>
		/// Prepare a decoder for an existing media file
		/// </summary>
		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int InitialiseDecoderJob (ref DecoderJob jobSpec, string Filepath);

		/// <summary>
		/// Read a frame from a decode job. Returns 0 until complete.
		/// </summary>
		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int DecodeFrame (ref DecoderJob jobSpec, ref MediaFrame frame);

		/// <summary>
		/// Close a previously opened decoder job.
		/// </summary>
		[DllImport("MpegTS_ChunkEncoder.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void CloseDecoderJob (ref DecoderJob jobSpec);
		#endregion
	}
}
