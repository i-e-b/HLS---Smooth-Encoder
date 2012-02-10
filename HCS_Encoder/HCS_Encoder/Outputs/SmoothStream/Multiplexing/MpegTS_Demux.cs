using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using MP4_Mangler;
using System.Collections;

namespace HCS_Encoder.Outputs.SmoothStream.Multiplexing {
	/// <summary>
	/// Class to handle the input of MPEG Transport files.
	/// Keeps a buffer of the Video and Audio streams.
	/// Output frames are suitable for remuxing into MP4 files.
	/// </summary>
	public class MpegTS_Demux {
		public class DemuxException : Exception {public DemuxException (string msg):base(msg) {}}

		protected int VideoPID, AudioPID; // first seen video & audio PIDs.
		public int VideoStreamId { get { return VideoPID; } }
		public int AudioStreamId { get { return AudioPID; } }

		protected Dictionary<int, Queue<GenericMediaFrame> > WaitingFrames; // PID => queue of completed frames
		protected Dictionary<int, Queue<Packet> > Packets; // PID => packet
		protected Dictionary<int, long> Timestamps; // PID => last known timestamp
		protected Dictionary<int, MemoryStream> Payloads; // PID => aggregated payload data.

		/// <summary>
		/// Create a new demuxer, prepared for stream feeding.
		/// Timestamps will be converted from MPEG timing (90kHz) to .Net timing (10MHz).
		/// 
		/// The demuxer should be kept and fed data when available.
		/// Creating a new demuxer for each data fragment will cause information loss.
		/// </summary>
		public MpegTS_Demux () {
			WaitingFrames = new Dictionary<int, Queue<GenericMediaFrame>>();
			Packets = new Dictionary<int, Queue<Packet>>();
			Payloads = new Dictionary<int, MemoryStream>();
			Timestamps = new Dictionary<int, long>();
			VideoPID = -1;
			AudioPID = -1;
		}

		/// <summary>
		/// Feed a data stream into the demuxer.
		/// Stream MUST be aligned to packets.
		/// This method is NOT THREAD SAFE -- lock Demux before calling.
		/// Feed must come from one logical transport stream only (to decode another stream, start another demux object).
		/// </summary>
		/// <param name="TransportStream">Source data stream. Must be readable, does not need to be seekable.</param>
		/// <param name="MinimumTimecode">A known minimum time (in seconds), or zero. Helps work around M2TS limitations.</param>
		public void FeedTransportStream (Stream TransportStream, double MinimumTime) {
			byte[] packet = new byte[188];

			// Decompose packets:
			while (TransportStream.Read(packet, 0, 188) == 188) {
				Packet p = new Packet(packet);
				if (p.PID == 0x1FFF) continue; // this PID means 'null packet' and is used for bit-rate control.

				if (!Packets.ContainsKey(p.PID)) Packets.Add(p.PID, new Queue<Packet>());
				Packets[p.PID].Enqueue(p);
			}
			MapVideoAudioPIDs();

			if (VideoPID > 0) {
				Queue<Packet> pkts = (Packets.ContainsKey(VideoPID)) ? (Packets[VideoPID]) : (new Queue<Packet>());
				AgregatePayload(VideoPID, pkts, MinimumTime);
			}
			if (AudioPID > 0) {
				Queue<Packet> pkts = (Packets.ContainsKey(AudioPID)) ? (Packets[AudioPID]) : (new Queue<Packet>());
				AgregatePayload(AudioPID, pkts, MinimumTime);
			}

			//By this point, any completed frames should be available to dequeue.

			// Kill off non-agregated packets: (remove this for doing analysis)
			foreach (var pid in Packets.Keys) {
				if (pid == VideoPID || pid == AudioPID) continue;
				Packets[pid].Clear();
			}
		}

		/// <summary>
		/// Return all completed video frames (removes them from completion queue)
		/// </summary>
		public List<GenericMediaFrame> GetAvailableVideo () {
			return GetCompletedFrames(VideoPID);
		}

		/// <summary>
		/// Return all completed video frames (removes them from completion queue)
		/// </summary>
		public List<GenericMediaFrame> GetAvailableAudio () {
			return GetCompletedFrames(AudioPID);
		}


		#region Helper methods
		/// <summary>
		/// Return complete frames for the given PID, clearing the queue.
		/// </summary>
		private List<GenericMediaFrame> GetCompletedFrames (int PID) {
			var fs = new List<GenericMediaFrame>();
			lock (WaitingFrames) {
				if (WaitingFrames.ContainsKey(PID)) {
					var q = WaitingFrames[PID];
					while (q.Count > 0) fs.Add(q.Dequeue());
				}
			}
			return fs;
		}

		/// <summary>
		/// Concatenate packet payloads, splitting at start-of-payload points.
		/// Passes these to DecodeFrame()
		/// </summary>
		/// <param name="PES_PID">PID to agregate</param>
		/// <param name="Pkts">List of packets for the PES PID.</param>
		private void AgregatePayload (int PES_PID, Queue<Packet> Pkts, double MinimumTime) {
			// CASES:
			// 1) We have a stream for this pid, and first pkt is not a start.
			//    --> continue to fill stream
			// 2) We don't have a stream for this pid, and first pkt is not a start.
			//    --> abandon pkts until we get a start
			// 3) First pkt is a start (regardless of current stream)
			//    --> end current stream (if there is one)
			//    --> start new stream and start filling it.

			if (!Payloads.ContainsKey(PES_PID)) Payloads.Add(PES_PID, null);
			MemoryStream ms = Payloads[PES_PID];
			while (Pkts.Count > 0) {
				var pkt = Pkts.Dequeue();
				if (!pkt.HasPayload) continue;

				if (pkt.StartIndicator) { // close old ms, start new.
					DecodeFrame(PES_PID, ms, MinimumTime); // this will handle the (ms == null) case
					Payloads[PES_PID] = new MemoryStream();
					ms = Payloads[PES_PID];
				}

				if (ms != null) {
					ms.Write(pkt.payload, 0, pkt.payload.Length);
				}
			}
		}

		/// <summary>
		/// Called by 'AgregatePayload()' as soon as a frame has been completed.
		/// Adds a GenericMediaFrame to the WaitingFrames queue if possible.
		/// </summary>
		private void DecodeFrame (int PES_PID, MemoryStream StreamData, double MinimumTime) {
			if (StreamData == null || StreamData.Length <= 9) return; // no useful data.

			byte[] payload = StreamData.ToArray();
			PES pes = new PES(payload);

			long new_timestamp = pes.PTS;
			long minimum_timestamp = (long)(MinimumTime * 90000.0);
			while (new_timestamp < minimum_timestamp) {// fix for 32-bit issues:
				new_timestamp += 0xFFFFFFFFL; // compensate for overflow
			}

			// **** EVERYTHING ABOVE THIS LINE IS 90kHz CLOCK ****
			new_timestamp = MpegToNetTime(new_timestamp);
			// **** EVERYTHING BELOW THIS LINE IS 10MHz CLOCK ****

			long old_timestamp = Timestamps[PES_PID];
			if (new_timestamp > old_timestamp) {
				Timestamps[PES_PID] = new_timestamp;
			}
			if (old_timestamp < 0) old_timestamp = new_timestamp; // prevent very long frames if clock is not zero-based.

			GenericMediaFrame frame = new GenericMediaFrame();
			frame.FrameData = pes.FrameData;
			frame.DataLength = frame.FrameData.Length;
			frame.FramePresentationTime = Timestamps[PES_PID];
			frame.FrameDuration = Timestamps[PES_PID] - old_timestamp; // First frame duration = 0

			WaitingFrames[PES_PID].Enqueue(frame);

			// try to fix first frame:
			if (WaitingFrames[PES_PID].Count == 2) {
				var first = WaitingFrames[PES_PID].Peek();

				// if bad duration, guess that it'll be the same as the next.
				if (first.FrameDuration <= 1) {
					first.FrameDuration = frame.FrameDuration;
				}
			}
		}

		/// <summary>
		/// Convert from MPEG 90kHz clock to .Net 10MHz clock.
		/// </summary>
		private long MpegToNetTime (long timestamp) {
			return (timestamp / 9L) * 1000L;
		}

		/// <summary>
		/// Read PAT table, find PMT table, and read that to get the *FIRST* video & audio PIDs.
		/// Will not change the PIDs after they've first been mapped (to prevent accidental switching).
		/// </summary>
		private void MapVideoAudioPIDs () {
			if (VideoPID > 0 && AudioPID > 0) return; // Both already mapped
			// Should have at least one packet in PID = 0;
			// Read PAT to find PMT PID:
			if (!Packets.ContainsKey(0)) throw new DemuxException("Did not find a PAT packet stream");
			int pmt_pid = DecodePAT();

			foreach (var pat_pkt in Packets[pmt_pid]) {
				PMT pmt = new PMT(pat_pkt.payload);
				if (pmt.ReverseMap.Count < 1) continue; // no fields
				if (pmt.ReverseMap.ContainsKey(PMT.StreamType.Audio)) { // map audio
					if (AudioPID <= 0) { // not already mapped
						AudioPID = pmt.ReverseMap[PMT.StreamType.Audio];
						WaitingFrames.Add(AudioPID, new Queue<GenericMediaFrame>());
						Timestamps.Add(AudioPID, -1);
					}
				}
				if (pmt.ReverseMap.ContainsKey(PMT.StreamType.Video)) { // map audio
					if (VideoPID <= 0) { // not already mapped
						VideoPID = pmt.ReverseMap[PMT.StreamType.Video];
						WaitingFrames.Add(VideoPID, new Queue<GenericMediaFrame>());
						Timestamps.Add(VideoPID, -1);
					}
				}
			}
		}

		/// <summary>
		/// Scan for the first non-empty PAT, and use it to read the PID for the PMT table.
		/// </summary>
		/// <returns>PID of PMT</returns>
		private int DecodePAT () {
			int pmt_pid = -1;
			foreach (var pat_pkt in Packets[0]) {
				PAT pat = new PAT(pat_pkt.payload);
				int rq_count = 1;
				if (pat.Map.ContainsKey(0)) rq_count++;
				if (pat.Map.Count < rq_count) continue; // doesn't contain a non-network PID.

				pmt_pid = pat.Map.First(a => a.Key != 0).Value;
			}

			if (pmt_pid < 1 || !Packets.ContainsKey(pmt_pid)) {
				throw new DemuxException("PAT refered to a PID that was not in the stream");
			}
			return pmt_pid;
		}
		#endregion

		#region Decoding classes
		/// <summary>
		/// Class to handle the top-level transport packets (ITU-T Rec H.222.0 (2000 E) -- page 18, 22)
		/// </summary>
		protected class Packet {
			public byte[] payload;

			public bool PayloadIs_PES;
			public int TableId;

			#region Packet head
			public bool Error;
			public bool StartIndicator;
			public bool HighPriority;
			public int PID;
			public int ScrambleCode;
			public bool HasAdaptionField;
			public bool HasPayload;
			public int Counter;
			public long PCR;
			#endregion

			#region Adaption fields
			public bool Discont, KeyFrame, ES_Prio, HasPCR, HasOPCR, HasSplice, PrivateFlag, AdapExtFlag;
			#endregion

			/// <summary>Digest data into structured packet</summary>
			public Packet (byte[] RawPacket) {
				if (RawPacket[0] != 0x47) throw new DemuxException("Sync byte missing");

				PCR = -1; TableId = -1; PayloadIs_PES = false;

				BitSplitter bs = new BitSplitter(RawPacket);
				ReadTransportHeader(bs);

				if (PID == 0x1FFF) return; // null packet

				CheckAdaptionField(bs);

				payload = bs.RemainingBytes();

				CheckPayloadType(bs);
			}

			private void ReadTransportHeader (BitSplitter bs) {
				bs.SkipToNextByte(); // Sync byte
				Error = bs.GetFlag();
				StartIndicator = bs.GetFlag();
				HighPriority = bs.GetFlag();
				PID = (int)bs.GetInteger(13);
				ScrambleCode = (int)bs.GetInteger(2);
				HasAdaptionField = bs.GetFlag();
				HasPayload = bs.GetFlag();
				Counter = (int)bs.GetInteger(4);
			}

			private void CheckPayloadType (BitSplitter bs) {
				if (payload.Length > 4) {
					if (payload[0] == 0 && payload[1] == 0 && payload[2] == 0x01) {
						PayloadIs_PES = true;
					}
				}
				if (!PayloadIs_PES && HasPayload && StartIndicator) {
					if (payload[0] != 0x00) throw new DemuxException("Non-zero pointer values are not yet supported!");
					TableId = payload[1];
				}
			}

			private void CheckAdaptionField (BitSplitter bs) {

				if (HasAdaptionField) {
					int adaption_end = (int)bs.GetInteger(8);
					adaption_end += bs.ByteOffset;
					Discont = bs.GetFlag();
					KeyFrame = bs.GetFlag();
					ES_Prio = bs.GetFlag();
					HasPCR = bs.GetFlag();
					HasOPCR = bs.GetFlag();
					HasSplice = bs.GetFlag();
					PrivateFlag = bs.GetFlag();
					AdapExtFlag = bs.GetFlag();
					if (bs.BitOffset != 0) throw new Exception("bit align problem");

					if (HasPCR) {
						PCR = (long)bs.GetInteger(33);
						bs.SkipBits(15); // throw away useless sync stuff.
					}
					if (HasOPCR) bs.SkipBits(48); // throw away useless "old" timecode
					if (HasSplice) bs.SkipBits(8); // throw away splice counter
					if (PrivateFlag) {
						int priv_len = (int)bs.GetInteger(8);
						bs.SkipBytes(priv_len); // skip private data
					}
					// ignore the rest of the adaption field (it's mostly to support stuff we ignore)
					int skip_len = adaption_end - bs.ByteOffset;
					bs.SkipBytes(skip_len);
				}
			}

			public override string ToString () {
				return ((StartIndicator) ? ("start of ") : ("         ")) + PID.ToString()
					+ (HasAdaptionField ? "A":" ")
					+ (HasPayload ? "P" : " ")
					+ ((StartIndicator) ? (PayloadIs_PES ? "E" : ("T:" + TableId)) : "...");
			}
		}

		/// <summary>
		/// Class to handle 2nd level PAT tables (ITU-T Rec H.222.0 (2000 E) -- page 43)
		/// </summary>
		protected class PAT {
			public Dictionary<int, int> Map; // program number => PID
			private bool SectionSyntax;
			private int SectionLength;
			private int TransportID; // this is a private field, recorded only for interest
			private int Version;
			private bool IsCurrent;
			private int SectionNumber, LastSection; // should always be zero

			/// <summary>
			/// Digest a packet payload into structured table. Payload should be from the pointer field onward.
			/// Does not yet handle multi-packet tables
			/// </summary>
			public PAT (byte[] RawPayload) {
				Map = new Dictionary<int,int>();
				BitSplitter bs = new BitSplitter(RawPayload);
				ValidateTable(bs);

				bs.SkipBits(2); // reserved;
				SectionLength = (int)bs.GetInteger(12);
				TransportID = (int)bs.GetInteger(16);
				bs.SkipBits(2); // reserved
				Version = (int)bs.GetInteger(5);
				IsCurrent = bs.GetFlag();
				SectionNumber = (int)bs.GetInteger(8);
				LastSection = (int)bs.GetInteger(8);

				int bits_left = (SectionLength - 5) - 4; // remaining length in bytes, excluding CRC
				int items = bits_left / 4;
				for (int i = 0; i < items; i++) {
					int prog = (int)bs.GetInteger(16);
					bs.SkipBits(3);
					int pid = (int)bs.GetInteger(13);
					if (!Map.ContainsKey(prog))
						Map.Add(prog, pid);
					else throw new DemuxException("Invalid PAT: program number specified more than once (" + prog + ")");
				}

				// Ignoring CRC.
			}

			private void ValidateTable (BitSplitter bs) {

				int pointer = (int)bs.GetInteger(8);
				if (pointer != 0) throw new DemuxException("Non-zero pointers are not currently supported");

				int table_id = (int)bs.GetInteger(8);
				if (table_id != 0) throw new DemuxException("Wrong table ID for PAT");

				SectionSyntax = bs.GetFlag();
				if (!SectionSyntax) throw new DemuxException("Invalid PAT: incorrect section syntax");
				bool zero = bs.GetFlag();
				if (zero) throw new DemuxException("Invalid PAT: zero bit wasn't zero");
			}
		}

		/// <summary>
		/// Class to handle 2nd level PMT tables (ITU-T Rec H.222.0 (2000 E) -- page 46)
		/// </summary>
		protected class PMT {

			public enum StreamType { // (ITU-T Rec H.222.0 (2000 E) -- page 48)
				Audio, Video, Other
			}

			public Dictionary<int, int> Map; // PID => stream type code
			public Dictionary<StreamType, int> ReverseMap; // Stream type => PID

			private bool SectionSyntax;
			private int SectionLength;
			private int ProgramNumber;
			public int Version;
			public bool IsCurrent;
			public int SectionNumber, LastSection; // should both be 0x00, but I don't validate this.
			public int PCR_PID;
			private int ProgInfoLength;

			/// <summary>
			/// Digest a packet payload into structured table. Payload should be from the pointer field onward.
			/// Does not yet handle multi-packet tables
			/// </summary>
			public PMT (byte[] RawPayload) {
				BitSplitter bs = new BitSplitter(RawPayload);
				Map = new Dictionary<int, int>();
				ReverseMap = new Dictionary<StreamType, int>();

				ValidateTable(bs);

				bs.SkipBits(2);
				SectionLength = (int)bs.GetInteger(12); // total length after this, in bytes; includes 4 byte CRC.
				ProgramNumber = (int)bs.GetInteger(16);
				bs.SkipBits(2);

				Version = (int)bs.GetInteger(5);
				IsCurrent = bs.GetFlag();
				SectionNumber = (int)bs.GetInteger(8);
				LastSection = (int)bs.GetInteger(8);
				bs.SkipBits(3);
				PCR_PID = (int)bs.GetInteger(13); // Either the PID of a channel timecode stream, or 0x1FFF for none.
				bs.SkipBits(4);

				ProgInfoLength = (int)bs.GetInteger(12); // number of bytes of descriptors.
				if (bs.BitOffset != 0) throw new DemuxException("Byte alignment error (internal)");
				bs.SkipBytes(ProgInfoLength); // ignore descriptors.

				int info_bytes = (SectionLength - ProgInfoLength) - 13; // bytes of descriptor.

				while (info_bytes > 0) { // descriptions can be variable length
					int stream_type = (int)bs.GetInteger(8);
					bs.SkipBits(3);
					int pid = (int)bs.GetInteger(13);
					bs.SkipBits(4);

					if (!Map.ContainsKey(pid)) Map.Add(pid, stream_type); // more complete map of pid types
					else throw new DemuxException("Invalid PMT: PID specified more than once");

					StreamType st = DecodeStreamType(stream_type);
					if (!ReverseMap.ContainsKey(st)) ReverseMap.Add(st, pid); // store first pid of each type

					int es_info_length = (int)bs.GetInteger(12);
					bs.SkipBytes(es_info_length);
					info_bytes -= 5 + es_info_length;
				}
				if (bs.BitOffset != 0) throw new DemuxException("Invalid PMT: program info length didn't match data");

				// ignoring CRC.
			}

			/// <summary>
			/// Decode the various random stream types into useful info.
			/// Add new stream types as they are discovered.
			/// </summary>
			private StreamType DecodeStreamType (int stream_type) {
				switch (stream_type) {
					default: return StreamType.Other; // don't know, or unused data streams.

					case 0x01: // mpeg 2 video
					case 0x02: // mpeg 2 video
					case 0x80: // mpeg 2 video
					case 0x1B: // H.264
					case 0xEA: // Microsoft VC-1
						return StreamType.Video;

					case 0x81: // AC-3 audio
					case 0x06: // AC-3 audio
					case 0x83: // AC-3 audio
					case 0x03: // MP3 audio
					case 0x04: // MP3 audio
					case 0x50: // AAC audio?
						return StreamType.Audio;
				}
			}

			private void ValidateTable (BitSplitter bs) {
				int pointer = (int)bs.GetInteger(8);
				if (pointer != 0) throw new DemuxException("Non-zero pointers are not currently supported");

				int table_id = (int)bs.GetInteger(8);
				if (table_id != 0x02) throw new DemuxException("Wrong table ID for PMT");

				SectionSyntax = bs.GetFlag();
				if (!SectionSyntax) throw new DemuxException("Invalid PMT: incorrect section syntax");
				bool zero = bs.GetFlag();
				if (zero) throw new DemuxException("Invalid PMT: zero bit wasn't zero");
			}
		}

		/// <summary>
		/// Class to handle 2nd level packetised elementary streams  (ITU-T Rec H.222.0 (2000 E) -- page 31..33)
		/// </summary>
		protected class PES {
			public byte[] FrameData;
			public long PTS;
			public long DTS;
			public int StreamId;
			private int PacketLength;
			public int HeaderLength;

			#region Flags
			public int ScrambleControl;
			public bool HighPriority, HasAlignment, IsCopyright, IsOriginal, HasPTS, HasDTS, HasESCR;
			public bool HasEsRate, UsesTrickMode, MoreCopyright, HasPesCRC, HasPesExtension;
			#endregion

			/// <summary>
			/// Digest a PES payload into structured table.
			/// Does not handle split-payloads -- agregate payloads before calling
			/// </summary>
			public PES (byte[] RawPayload) {
				BitSplitter bs = new BitSplitter(RawPayload);

				int start_code = (int)bs.GetInteger(24);
				if (start_code != 1) throw new DemuxException("Invalid PES: start code prefix missing");

				PTS = DTS = -1;

				StreamId = (int)bs.GetInteger(8);
				PacketLength = (int)bs.GetInteger(16);

				// Both these methods set 'FrameData'
				if (SpecialStream(StreamId)) {
					ReadSpecialForm(bs);
				} else {
					DecodeElementaryStream(bs);
				}
			}
			/// <summary>
			/// If true, the stream uses the alternate PES header form.
			/// If false, the stream uses the default PES header form.
			/// </summary>
			private bool SpecialStream (int StreamId) {
				switch (StreamId) {
					default: return false;
					case 0xBC: // PSM
					case 0xBD: // Padding
					case 0xBF: // Private 2
					case 0xF0: // ECM
					case 0xF1: // EMM
					case 0xFF: // PSD
					case 0xF2: // DSMCC
					case 0xF8: // H.222 Type E
						return true;
				}
			}


			private void DecodeElementaryStream (BitSplitter bs) {
				int marker = (int)bs.GetInteger(2);
				if (marker != 2) throw new DemuxException("Invalid PES: first marker missing");
				ReadFlags(bs);
				if (bs.BitOffset != 0) throw new DemuxException("Alignment problem in PES (internal)");
				HeaderLength = (int)bs.GetInteger(8);

				int head_start = bs.ByteOffset;
				if (HasPTS && HasDTS) {
					ReadDTS_PTS(bs);
				} else if (HasPTS) {
					ReadPTS(bs);
				}

				if (HasESCR) bs.SkipBytes(6); // not currently used.
				if (HasEsRate) bs.SkipBits(24); // not currently used.
				if (UsesTrickMode) bs.SkipBytes(1); // ignored
				if (MoreCopyright) bs.SkipBytes(1); // ignored
				if (HasPesCRC) bs.SkipBytes(2); // ignored

				if (HasPesExtension) ReadExtendedHeader(bs);

				// skip anything that's left
				int head_end = bs.ByteOffset;
				int to_skip = HeaderLength - (head_end - head_start);
				if (to_skip < 0) throw new DemuxException("Invalid PES: declared header length did not match measured length");
				bs.SkipBytes(to_skip);

				// Now, the remaining bytes are data and padding
				int data_length = PacketLength - (HeaderLength + to_skip);
				if (data_length > 3) data_length -= 3; // no idea where the '3' is coming from...
				byte[] data = bs.RemainingBytes();

				if (PacketLength == 0) { // video is allowed to not specify
					data_length = data.Length;
				}

#if DEBUG
				if (data.Length < data_length)
					throw new DemuxException("Invalid PES: packet shorter than described");

				if (data_length < 0) throw new DemuxException("Invalid PES: Negative packet length");
#else
				if (data.Length < data_length || data_length < 0) data_length = 0;
#endif

				MemoryStream ms = new MemoryStream(data, 0, data_length);
				FrameData = ms.ToArray();
			}

			private void ReadExtendedHeader (BitSplitter bs) {
				// not yet implemented
			}

			/// <summary>
			/// Decode the bizzare PTS format
			/// </summary>
			private void ReadPTS (BitSplitter bs) {
				int marker = (int)bs.GetInteger(4);
				if (marker != 2) throw new DemuxException("Invalid PES: PTS marker incorrect");
				long part_1 = (long)bs.GetInteger(3);
				bs.SkipBits(1);
				long part_2 = (long)bs.GetInteger(15);
				bs.SkipBits(1);
				long part_3 = (long)bs.GetInteger(15);
				bs.SkipBits(1);
				unchecked { // allow overflow so we can catch it later:
					PTS = (UInt32)(part_3 + (part_2 << 15) + (part_1 << 30));
				}
			}

			/// <summary>
			/// Decode the bizzare PTS+DTS format
			/// </summary>
			private void ReadDTS_PTS (BitSplitter bs) {
				int marker = (int)bs.GetInteger(4);
				if (marker != 3) throw new DemuxException("Invalid PES: PTS-(PTS/DTS) marker incorrect");
				long part_1 = (long)bs.GetInteger(3);
				bs.SkipBits(1);
				long part_2 = (long)bs.GetInteger(15);
				bs.SkipBits(1);
				long part_3 = (long)bs.GetInteger(15);
				unchecked { // allow overflow so we can catch it later:
					PTS = (UInt32)(part_3 + (part_2 << 15) + (part_1 << 30));
				}
				bs.SkipBits(1);

				marker = (int)bs.GetInteger(4);
				if (marker != 1) throw new DemuxException("Invalid PES: DTS-(PTS/DTS) marker incorrect");

				part_1 = (long)bs.GetInteger(3);
				bs.SkipBits(1);
				part_2 = (long)bs.GetInteger(15);
				bs.SkipBits(1);
				part_3 = (long)bs.GetInteger(15);
				bs.SkipBits(1);
				unchecked {
					DTS = (UInt32)(part_3 + (part_2 << 15) + (part_1 << 30));
				}
			}

			/// <summary>
			/// Reads the long list of flags in the default PES header.
			/// </summary>
			private void ReadFlags (BitSplitter bs) {
				ScrambleControl = (int)bs.GetInteger(2);
				HighPriority = bs.GetFlag();
				HasAlignment = bs.GetFlag();
				IsCopyright = bs.GetFlag();
				IsOriginal = bs.GetFlag();
				HasPTS = bs.GetFlag();
				HasDTS = bs.GetFlag();

				if (HasDTS && !HasPTS) throw new DemuxException("Invalid PES: DTS without PTS is not allowed");

				HasESCR = bs.GetFlag();
				HasEsRate = bs.GetFlag();
				UsesTrickMode = bs.GetFlag();
				MoreCopyright = bs.GetFlag();
				HasPesCRC = bs.GetFlag();
				HasPesExtension = bs.GetFlag();
			}

			private void ReadSpecialForm (BitSplitter bs) {
				byte[] data = bs.RemainingBytes();
				if (data.Length < PacketLength) throw new DemuxException("Invalid PES: packet shorter than described");
				MemoryStream ms = new MemoryStream(data, 0, PacketLength);
				FrameData = ms.ToArray();
			}

		}
		#endregion
	}
}
