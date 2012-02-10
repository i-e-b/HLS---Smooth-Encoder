using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace EncoderConfiguration {

	[Serializable]
	public class Configuration {
		#region IO helpers
		/// <summary>
		/// Load a configuration object from a file
		/// </summary>
		public static Configuration LoadFromFile (string Filepath) {
			XmlSerializer sz = new XmlSerializer(typeof(Configuration));
			Configuration conf = null;
			try {
				using (Stream reader = File.OpenRead(Filepath)) {
					conf = sz.Deserialize(reader) as Configuration;
				}
				return conf;
			} catch {
				return null;
			}
		}

		/// <summary>
		/// Save this configuration to a file
		/// </summary>
		public void SaveToFile (string Filepath) {
			XmlSerializer sz = new XmlSerializer(typeof(Configuration));
			
			// Some nasty hacks to get around serializer issues.
			EncoderSettings.LocalSystemOutputFolder = CDATAfy(EncoderSettings.LocalSystemOutputFolder);
			EncoderSettings.LocalSystemFilePrefix = CDATAfy(EncoderSettings.LocalSystemFilePrefix);
			Upload.VideoDestinationRoot = CDATAfy(Upload.VideoDestinationRoot);
			Upload.IndexFtpRoot = CDATAfy(Upload.IndexFtpRoot);
			Upload.IndexName = CDATAfy(Upload.IndexName);
			Upload.ServerLookupRoot = CDATAfy(Upload.ServerLookupRoot);

			if (File.Exists(Filepath)) {
				File.Delete(Filepath);
			}

			StringBuilder sb = new StringBuilder();

			using (
			StringWriter writer = new StringWriter(sb)) {
				sz.Serialize(writer, this);
			}

			string fmt_txt = sb.ToString().Replace("&lt;![CDATA[", "<![CDATA[").Replace("]]&gt;", "]]>");

			File.WriteAllText(Filepath,
				fmt_txt.Replace( // The XML serialiser can get in knickers in a terrible twist...
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>",
				"<?xml version=\"1.0\"?>")
				);
		}

		private string CDATAfy (string input) {
			if (!input.StartsWith("<![CDATA[")) {
				return "<![CDATA[" + input + "]]>";
			} else {
				return input;
			}
		}
		#endregion

		/// <summary>
		/// Create a new blank configuration script
		/// </summary>
		public Configuration () {
			Audio = new EncoderConfiguration.AudioCapture();
			EncoderSettings = new EncoderConfiguration.Encoder();
			Upload = new EncoderConfiguration.Uploader();
			Video = new EncoderConfiguration.VideoCapture();

			EncoderSettings.LocalSystemFilePrefix = "";
			EncoderSettings.LocalSystemOutputFolder = "";

			Upload.IndexFtpRoot = "";
			Upload.IndexName = "";
			Upload.ServerLookupRoot = "";
			Upload.VideoDestinationRoot = "";
		}

		public AudioCapture Audio { get; set; }
		public VideoCapture Video { get; set; }
		public Encoder EncoderSettings { get; set; }
		public Uploader Upload { get; set; }
	}
}
