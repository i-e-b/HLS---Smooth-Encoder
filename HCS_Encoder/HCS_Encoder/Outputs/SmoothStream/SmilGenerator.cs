using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MP4_Mangler;
using System.IO;

namespace HCS_Encoder.Outputs.SmoothStream {
	/// <summary>
	/// Helper class for generating SMIL files to send to the IIS Live Smoothstreaming snap-in
	/// </summary>
	public class SmilGenerator {
		protected int _channel;
		protected string _fourCC;
		protected string _privateData; // Either "CodecPrivateData" or "WaveFormatEx"
		protected string _fileRoot;

		/// <summary>
		/// Approximate bitrate (used for switching, but not for playing)
		/// </summary>
		public long ApproxBitrate { get; set; }

		/// <summary>
		/// Native video width (if video -- ignored for audio)
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Native video height (if video -- ignored for audio)
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Prepate a SMIL generator
		/// </summary>
		/// <param name="FileRoot">
		/// Start of the media file name (used by IIS to create archive files).
		/// This will be appended with bitrate and dot-extension.
		/// </param>
		public SmilGenerator (string FileRoot, MediaStream SourceStream) {
			switch (SourceStream.FourCC) {
				case "mp4a":
					_fourCC = "mp4a";
					Height = 0;
					Width = 0;
					break;

				case "mp3a":
					_fourCC = "mp3a";
					Height = 0;
					Width = 0;
					break;

				case "avc1":
				case "H264":
					_fourCC = "H264"; // use the Microsoft form, as this is for sending to IIS & Silverlight.
					Height = SourceStream.Height;
					Width = SourceStream.Width;
					break;

				default: throw new ArgumentException("Stream type " + SourceStream.FourCC + " is not supported", "SourceStream");
			}
			_channel = SourceStream.TrackId;
			_fileRoot = FileRoot;
		}

		/// <summary>
		/// Create a SMIL document with the details provided.
		/// </summary>
		/// <remarks>"Private data" is created at this point.</remarks>
		public byte[] Generate () {
			MemoryStream ms = new MemoryStream();
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = Encoding.UTF8;
			settings.Indent = true;

			var xml = XmlWriter.Create(ms, settings);

			xml.WriteStartDocument();
			xml.WriteStartElement("smil", "http://www.w3.org/2001/SMIL20/Language" );
			xml.WriteStartElement("head");
			xml.WriteStartElement("meta");
			xml.WriteAttributeString("name", "creator");
			xml.WriteAttributeString("content", "pushEncoder");
			xml.WriteEndElement(); // meta
			xml.WriteEndElement(); // head

			xml.WriteStartElement("body");
			xml.WriteStartElement("switch");

			switch (_fourCC) {
				case "avc1":
				case "H264":
					_privateData = GenerateH264_Private();
					WriteVideo(xml);
					break;
				case "mp3a":
					_privateData = GenerateMP3_Private();
					WriteAudio(xml, 0x55);
					break;
				case "mp4a":
					_privateData = GenerateAAC_Private();
					WriteAudio(xml, 0x1610);
					break;
			}
			
			xml.WriteEndElement(); // switch
			xml.WriteEndElement(); // body
			xml.WriteEndDocument();
			xml.Flush();
			xml.Close();

			return ms.ToArray(); 
		}

		/// <summary>
		/// Generate a "Codec Private Data" string for an MP3 stream (values approximate).
		/// </summary>
		private string GenerateMP3_Private () {
			MpegLayer3WaveFormat wfmt = new MpegLayer3WaveFormat();
			wfmt.BitratePaddingMode = 0;
			wfmt.BlockSize = 312;
			wfmt.CodecDelay = 0;
			wfmt.FramesPerBlock = 1;
			wfmt.Id = 1;

			wfmt.WaveFormatExtensible = new WaveFormatExtensible();
			wfmt.WaveFormatExtensible.AverageBytesPerSecond = 96000 / 8;
			wfmt.WaveFormatExtensible.BitsPerSample = 0;
			wfmt.WaveFormatExtensible.BlockAlign = 1;
			wfmt.WaveFormatExtensible.Channels = 1;
			wfmt.WaveFormatExtensible.FormatTag = 85;
			wfmt.WaveFormatExtensible.SamplesPerSec = 44100;
			wfmt.WaveFormatExtensible.Size = 12;

			return wfmt.ToHexString();
		}


		/// <summary>
		/// Generate a "Codec Private Data" string for an AAC stream (values approximate).
		/// </summary>
		private string GenerateAAC_Private () {
			MpegLayer3WaveFormat wfmt = new MpegLayer3WaveFormat();
			wfmt.BitratePaddingMode = 0;
			wfmt.BlockSize = 312;
			wfmt.CodecDelay = 0;
			wfmt.FramesPerBlock = 1;
			wfmt.Id = 1;

			wfmt.WaveFormatExtensible = new WaveFormatExtensible();
			wfmt.WaveFormatExtensible.AverageBytesPerSecond = 96000 / 8;
			wfmt.WaveFormatExtensible.BitsPerSample = 0;
			wfmt.WaveFormatExtensible.BlockAlign = 1;
			wfmt.WaveFormatExtensible.Channels = 1;
			wfmt.WaveFormatExtensible.FormatTag = 0xFF;
			wfmt.WaveFormatExtensible.SamplesPerSec = 44100;
			wfmt.WaveFormatExtensible.Size = 12;

			return wfmt.ToHexString();
		}

		// Eventually, there should be an integrated pipeline between the encoder and final output -- and this data should be transported to here.
		private string GenerateH264_Private () {
			return "";
		}

		#region XML specific crap
		private void WriteAudio (XmlWriter xml, int AudioFormat) {
			xml.WriteStartElement("audio");
			
			xml.WriteAttributeString("src", _fileRoot + "_" + ApproxBitrate.ToString() + ".ismv");
			xml.WriteAttributeString("systemBitrate", ApproxBitrate.ToString());

			WriteParam(xml, "systemBitrate", ApproxBitrate.ToString());
			WriteParam(xml, "trackID", _channel.ToString());
			WriteParam(xml, "FourCC", "" /*_fourCC*/); // dunno why this is passed blank, but it is.
			WriteParam(xml, "CodecPrivateData", _privateData);
			WriteParam(xml, "AudioTag", AudioFormat.ToString() /*"85"*/); // is this the WaveFormatEx format tag?
			WriteParam(xml, "Channels", "1"); // hard coded for now, fix later.
			WriteParam(xml, "SamplingRate", "44100"); // Always hardcoded in HCS.
			WriteParam(xml, "BitsPerSample", "0"); // Always hardcoded in HCS.
			WriteParam(xml, "PacketSize", "1152"); // guess...
			WriteParam(xml, "Subtype", "mpegaudio"); // don't know the right value

			xml.WriteEndElement(); // video
		}

		private void WriteVideo (XmlWriter xml) {

			xml.WriteStartElement("video");

			xml.WriteAttributeString("src", _fileRoot + "_" + ApproxBitrate.ToString() + ".ismv");
			xml.WriteAttributeString("systemBitrate", ApproxBitrate.ToString());

			WriteParam(xml, "systemBitrate", ApproxBitrate.ToString());
			WriteParam(xml, "trackID", _channel.ToString());
			WriteParam(xml, "FourCC", _fourCC);
			WriteParam(xml, "CodecPrivateData", _privateData);
			WriteParam(xml, "MaxWidth", Width.ToString());
			WriteParam(xml, "MaxHeight", Height.ToString());
			WriteParam(xml, "DisplayWidth", Width.ToString());
			WriteParam(xml, "DisplayHeight", Height.ToString());
			WriteParam(xml, "Subtype", _fourCC);

			xml.WriteEndElement(); // video
		}

		private void WriteParam (XmlWriter xml, string name, string value) {
			xml.WriteStartElement("param");
			xml.WriteAttributeString("name", name);
			xml.WriteAttributeString("value", value);
			xml.WriteAttributeString("valuetype", "data");
			xml.WriteEndElement(); // param
		}
		#endregion
	}
}
