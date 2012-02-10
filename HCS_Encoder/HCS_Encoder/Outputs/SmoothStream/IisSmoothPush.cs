
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace HCS_Encoder.Outputs.SmoothStream {
	/// <summary>
	/// Class for pushing MP4f files to the IIS Live Smooth Streaming snap-in
	/// </summary>
	public class IisSmoothPush {

		protected string pointName;
		private Uri destServer;
		private Dictionary<int,Stream> chunkStreams;
		private int pushId;

		public IisSmoothPush (Uri Destination) {
			pointName = Destination.PathAndQuery;
			destServer = Destination;
			pushId = 0x1EB; // ;-)
			chunkStreams = new Dictionary<int, Stream>();
		}

		private void WakeIIS () {

			HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(destServer);
			rq.Headers.Clear();
			rq.Method = "POST";
			rq.ContentLength = 0;
			rq.UserAgent = "NSPlayer/7.0 IIS-LiveStream/7.0";
			rq.KeepAlive = true;

			HttpWebResponse tx = (HttpWebResponse)rq.GetResponse();
			if (tx.StatusCode != HttpStatusCode.OK) throw new Exception("Connection refused: " + tx.StatusDescription);
		}

		/// <summary>
		/// Try to connect to the host server.
		/// Call this method first. Throws exceptions on failure.
		/// After a sucessful call ypu may start sending data chunks.
		/// </summary>
		/// <param name="subpath">sub-path to push to. May be emmpty or null.</param>
		public Stream Connect (int StreamId) {

			System.Net.ServicePointManager.Expect100Continue = false; // Thanks to Lance Olson and Phil Haack for noting this.

			// If we get this far, we should try to push the chunks.
			// First, open a connection, and hold the stream in an instance member.
			// Then "push-data" will throw chunks at it.

			if (chunkStreams == null) chunkStreams = new Dictionary<int, Stream>();
			if (chunkStreams.ContainsKey(StreamId)) { // close off old stream and start again.
				chunkStreams[StreamId].Flush();
				chunkStreams[StreamId].Close();
				chunkStreams.Remove(StreamId);
			}
			
			WakeIIS();

			string full = Path.Combine(pointName, "Streams("+pushId+"-stream"+StreamId+")").Replace("\\", "/");
			

			HttpWebRequest outgoingHTTP;
			outgoingHTTP = (HttpWebRequest)WebRequest.Create(new Uri(destServer, full));
			outgoingHTTP.Headers.Clear();

			outgoingHTTP.ServicePoint.Expect100Continue = false;
			outgoingHTTP.ServicePoint.ConnectionLimit = 8;
			
			outgoingHTTP.ServicePoint.ConnectionLeaseTimeout = Timeout.Infinite;
			outgoingHTTP.ServicePoint.MaxIdleTime = Timeout.Infinite;

			outgoingHTTP.Timeout = Timeout.Infinite;
			outgoingHTTP.AllowWriteStreamBuffering = false;
			
			outgoingHTTP.SendChunked = true;
			outgoingHTTP.KeepAlive = false;
			outgoingHTTP.Method = "POST";
			outgoingHTTP.UserAgent = "NSPlayer/7.0 IIS-LiveStream/7.0";
			outgoingHTTP.ContentLength = 0;
			outgoingHTTP.ReadWriteTimeout = Timeout.Infinite;

			Stream ous = outgoingHTTP.GetRequestStream();
			if (ous != null) {
				if (chunkStreams.ContainsKey(StreamId)) chunkStreams[StreamId] = ous;
				else chunkStreams.Add(StreamId, ous);

			} else throw new Exception("IIS Connection failed to yield a transport stream: check network conditions.");

			return ous;
		}

		/// <summary>
		/// Returns true if the given stream/track ID has a connection open.
		/// </summary>
		public bool IsConnected (int StreamId) {
			return chunkStreams.ContainsKey(StreamId);
		}

		/// <summary>
		/// Push a chunk of raw data to the connected server
		/// </summary>
		public void PushData (int StreamId, byte[] Data) {

			if (chunkStreams == null || !chunkStreams.ContainsKey(StreamId)) throw new Exception("Stream not connected");
			Stream chunkStream = chunkStreams[StreamId];
			if (chunkStream == null || !chunkStream.CanWrite) {
				chunkStream = Connect(StreamId);
				if (chunkStream == null || !chunkStream.CanWrite) throw new Exception("Can't write to stream in current state.");
			}

			try {
				chunkStream.Write(Data, 0, Data.Length);
				chunkStream.Flush();
			} catch {
				for (int i = 0; i < 5; i++) {
					try {
						chunkStream = Connect(StreamId);
						chunkStream.Write(Data, 0, Data.Length);
						chunkStream.Flush();
						break;
					} catch (Exception ex) {
						Debug.Write("Failed to send chunk: " + ex.Message);
					}
				}
			}
		}

		/// <summary>
		/// Close all streams.
		/// Only do this once all fragments have been pushed.
		/// </summary>
		public void Close () {
			foreach (Stream chunkStream in chunkStreams.Values) {
				byte[] empty = new byte[0];
				chunkStream.Write(empty, 0, empty.Length);
				chunkStream.Flush();
				chunkStream.Close();
			}
		}


	}
}
