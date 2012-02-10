using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EncoderConfiguration {
	[Serializable]
	public class Uploader {

		/// <summary>
		/// HTTP Live Streaming: Ftp server connection for video files (excluding filename prefix).<br/>
		/// Smooth Streaming: The IIS playlist URL (used for WebDAV point delivery).
		/// </summary>
		public string VideoDestinationRoot { get; set; }

		/// <summary>
		/// HTTP Live Streaming: Ftp server connection for Index files (can be the same as 'VideoDestinationRoot')<br/>
		/// Smooth Streaming: Backup FTP for delivering Publishing Points.
		/// </summary>
		public string IndexFtpRoot { get; set; }

		/// <summary>
		/// HTTP Live Streaming: Index file name.<br/>
		/// Smooth Streaming: Unused.
		/// </summary>
		public string IndexName { get; set; }

		/// <summary>
		/// HTTP Live Streaming only. Unused by Smooth Streaming.<br/>
		/// Root for inclusion in Index files.
		/// This should be just the filename prefix if the playlist and files are in the same location.
		/// Otherwise, it should be an HTTP location plus the filename prefix.
		/// </summary>
		public string ServerLookupRoot { get; set; }

		/// <summary>
		/// Name of output handler. Should be either "HTTP Live" or "IIS Smooth"
		/// </summary>
		public string UploadHandlerName { get; set; }
	}
}
