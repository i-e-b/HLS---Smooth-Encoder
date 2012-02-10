using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace HCS_Encoder {
	public class TransferAgent {

		/// <summary>
		/// Transfer from a source Uri to a destination Uri.
		/// Uris need not be the same type or on the same server. Uris may contain
		/// username and password information -- this will be used to make the connection.
		/// Destination files will be overwritten if needed and possible.
		/// </summary>
		/// <remarks>
		/// This method DOES NOT try to create missing directories.
		/// All of the directories in the destination path should exist and
		/// have proper permissions.
		/// </remarks>
		/// <param name="From">Source Uri. Should resolve to a file.</param>
		/// <param name="To">Destination Uri. Should resolve to a file.</param>
		public static void Transfer (Uri From, Uri To) {
			// Get requests
			WebRequest src = WebRequest.Create(From);
			WebRequest dst = WebRequest.Create(To);

			WebProxy proxy = new WebProxy();
			proxy.UseDefaultCredentials = true;

			dst.Proxy = proxy; // this gets around a probleb with 'Fiddler'

			// Get incoming stream
			WebResponse rx = src.GetResponse();
			Stream @in = rx.GetResponseStream();

			// Set outgoing method
			switch (To.Scheme.ToUpper()) {
				case "FILE":
				case "HTTP":
				case "HTTPS":
					dst.Method = "POST";
					break;
				case "FTP":
					dst.Method = "STOR";
					break;
				default:
					throw new ArgumentException("Can't handle the scheme \"" + To.Scheme + "\"");
			}

			Stream @out = dst.GetRequestStream();
			try {
				if (!@out.CanWrite) throw new Exception("Non-writable stream");

				// read all available data across streams:
				int BUFFER_SIZE = 230500; // 225K.
				byte[] buffer = new byte[BUFFER_SIZE];
				int read_bytes = 0;
				do {
					read_bytes = @in.Read(buffer, 0, BUFFER_SIZE);
					@out.Write(buffer, 0, read_bytes);
				} while (read_bytes > 0);

			} finally {
				@in.Close();
				@out.Flush();
				@out.Close();
			}
		}

		/// <summary>
		/// Transfer from a source data stream to destination Uri.
		/// Uris need not be the same type or on the same server. Uris may contain
		/// username and password information -- this will be used to make the connection.
		/// Destination files will be overwritten if needed and possible.
		/// </summary>
		/// <param name="From">Source data stream</param>
		/// <param name="To">Destination Uri. Should resolve to a file.</param>
		public static void Transfer (Stream From, Uri To) {
			// Get request
			WebRequest dst = WebRequest.Create(To);

			// Set outgoing method
			switch (To.Scheme.ToUpper()) {
				case "FILE":
				case "HTTP":
				case "HTTPS":
					dst.Method = "POST";
					break;
				case "FTP":
					dst.Method = "STOR";
					break;
				default:
					break;
			}

			Stream @out = dst.GetRequestStream();
			try {
				if (!@out.CanWrite) throw new Exception("Non-writable stream");

				// read all available data across streams:

				int BUFFER_SIZE = 230500; // 225K.
				byte[] buffer = new byte[BUFFER_SIZE];
				int read_bytes = 0;
				do {
					read_bytes = From.Read(buffer, 0, BUFFER_SIZE);
					@out.Write(buffer, 0, read_bytes);
				} while (read_bytes > 0);

			} finally {
				@out.Flush();
				@out.Close();
			}
		}

		/// <summary>
		/// Transfer from a source Uri stream to destination Stream.
		/// Uris may contain username and password information
		/// -- this will be used to make the connection.
		/// </summary>
		/// <param name="From">Source data stream</param>
		/// <returns>An in memory stream containing the complete data
		/// read from 'From'</returns>
		public static Stream Transfer (Uri From) {
			// Get requests
			WebRequest src = WebRequest.Create(From);

			// Get incoming stream
			WebResponse rx = src.GetResponse();
			Stream @in = rx.GetResponseStream();
			List<byte> buffer = new List<byte>(230500);

			try {
				// read all available data across streams:

				int BUFFER_SIZE = 230500; // 225K.
				byte[] bbuffer = new byte[BUFFER_SIZE];
				int read_bytes = 0;
				do {
					read_bytes = @in.Read(bbuffer, 0, BUFFER_SIZE);

					for (int i = 0; i < read_bytes; i++) {
						buffer.Add(bbuffer[i]); // really inefficient, but I just want it to work!
					}

				} while (read_bytes > 0);
			} finally {
				@in.Close();
			}
			return new MemoryStream(buffer.ToArray(), true);
		}
	}
}
