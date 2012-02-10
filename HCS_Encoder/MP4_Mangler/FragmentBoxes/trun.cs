using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MP4_Mangler.FragmentBoxes {
	/// <summary>
	/// Set of frames, sent here by 'traf'
	/// </summary>
	public class trun: FullBox {
		protected List<GenericMediaFrame> _frames;

		/// <summary>
		/// Begin an empty 'trun' box
		/// </summary>
		public trun ()
			: base(0,
			/*Flags: */
				/* Duration-> */ 0x000100
				/* Size-> */ + 0x000200
				/* PTS-> + 0x000800 */, "trun") {
			// The only flags of 'trun' that the ExpressionPlayer seems to use are the duration and size-present
			// so those are the only ones I'm adding.
			// everything else should be deductable from the Elementary Stream data.

			_frames = new List<GenericMediaFrame>();
		}

		/// <summary>
		/// Add a frame to this trun
		/// </summary>
		public void AddFrame (GenericMediaFrame f) {
			_frames.Add(f);
		}


		public override void Prepare () {
			// build the base '_data' item.
			// done here to be as late as possible.

			_data = new MemoryStream();
			BigEndianWriter d = new BigEndianWriter(_data);
			d.Write((UInt32)_frames.Count); // required field

			// frame positions and sizes
			foreach (var frame in _frames) {
				d.Write((UInt32)frame.FrameDuration); // This is in .Net ticks (100ns)
				d.Write((UInt32)frame.FrameData.Length);
				//d.Write((UInt32)frame.FramePresentationTime); // This is in .Net ticks (100ns)
			}
		}

	}
}
