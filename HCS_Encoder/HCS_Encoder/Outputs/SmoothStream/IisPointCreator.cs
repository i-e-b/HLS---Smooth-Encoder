using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using HCS_Encoder.WebDav;
using System.Threading;

namespace HCS_Encoder.Outputs.SmoothStream {
	/// <summary>
	/// Generates and uploads ISML files to the given Uri.
	/// Uses WebDAV, which must be enabled on the receiving server. 
	/// </summary>
	/// <remarks>The .isml file is placed in the web directory, and controls a publish point.</remarks>
	public class IisPointCreator {
		public string PublishPointName { get; set; }
		public Uri Server { get; set; }
		protected AutoResetEvent autoResetEvent;

		/// <summary>
		/// Prepare a SMIL generator for the given Uri and Point name.
		/// The Uri must have the correct username and password specified, in the form
		///   http://user:password@my.site.net/Path/
		/// </summary>
		public IisPointCreator (Uri Destination, string PointName) {
			this.PublishPointName = PointName;
			this.Server = Destination;
		}

		/// <summary>
		/// Tries to write a publishing point to the destination server, first by FTP then by WebDAV.
		/// </summary>
		public void CreatePoint () {
			if (Server.Scheme.ToLower() == "ftp") {
				CreatePoint_FTP();
			} else {
				CreatePoint_WebDAV();
			}
		}
	
		/// <summary>
		/// Write the file to the destination server.
		/// </summary>
		public void CreatePoint_WebDAV () {
			autoResetEvent = new AutoResetEvent(false);
			string[] auth = Server.UserInfo.Split(':');
			if (auth.Length != 2) throw new Exception("Expected authorisation info was missing");

			string usr = Uri.UnescapeDataString(auth[0]);
			string pass = Uri.UnescapeDataString(auth[1]);

			var c = new WebDavClient();
			c.Server = "http://" + Server.Host;
			c.BasePath = Server.AbsolutePath;

			if (usr.Contains('\\')) {
				string[] u2 = usr.Split('\\');
				usr = u2[1];
				c.Domain = u2[0];
			} else if (usr.Contains('@')) {
				string[] u3 = usr.Split('@');
				usr = u3[0];
				c.Domain = u3[1];
			}
			c.User = usr;
			c.Pass = pass;

			c.UploadComplete += c_UploadComplete;
			c.Upload(Generate(), c.BasePath + PublishPointName, null);
			autoResetEvent.WaitOne();
		}

		/// <summary>
		/// Write the file to the destination server, using FTP (if WebDAV isn't available)
		/// </summary>
		public void CreatePoint_FTP () {
			var dest = new Uri(Server, PublishPointName);
			dest = new Uri(dest.ToString().Replace("http://", "ftp://"));
			var ms = new MemoryStream(Generate());
			TransferAgent.Transfer(ms, dest);
		}

		void c_UploadComplete (int statusCode, object state) {
			autoResetEvent.Set();
		}

		/// <summary>
		/// Create a SMIL document with the details provided.
		/// </summary>
		private byte[] Generate () {
			var ms = new MemoryStream();
			var settings = new XmlWriterSettings();
			settings.Encoding = Encoding.UTF8;
			settings.Indent = true;

			var xml = XmlWriter.Create(ms, settings);

			xml.WriteStartDocument();
			xml.WriteStartElement("smil", "http://www.w3.org/2001/SMIL20/Language");
			xml.WriteStartElement("head");
			WriteMeta(xml, "title", PublishPointName);
			WriteMeta(xml, "module", "liveSmoothStreaming");
			WriteMeta(xml, "sourceType", "Push");
			WriteMeta(xml, "publishing", "Fragments;Streams;Archives");
			WriteMeta(xml, "estimatedTime", "3600");
			WriteMeta(xml, "lookaheadChunks", "2");
			WriteMeta(xml, "manifestWindowLength", "0"); // this means unlimited -- archive everything
			WriteMeta(xml, "startOnFirstRequest", "True"); // needs to be true, otherwise you'd need machine access to turn on.
			WriteMeta(xml, "archiveSegmentLength", "0"); // don't split archives

			// New stuff for IIS MS4:
			WriteMeta(xml, "formats", "m3u8-aapl"); // additional format
			WriteMeta(xml, "m3u8-aapl-segmentlength", "10"); // iPhone chunk length in seconds
			WriteMeta(xml, "m3u8-aapl-maxbitrate", "1600000"); // maximum bit-rate for iPhone
			// end of new stuff

			xml.WriteEndElement(); // head

			xml.WriteStartElement("body");
			xml.WriteEndElement(); // body
			xml.WriteEndDocument();
			xml.Flush();
			xml.Close();

			return ms.ToArray(); 
		}

		private void WriteMeta (XmlWriter xml, string name, string value) {
			xml.WriteStartElement("meta");
			xml.WriteAttributeString("name", name);
			xml.WriteAttributeString("content", value);
			xml.WriteEndElement(); // param
		}
	}
}
