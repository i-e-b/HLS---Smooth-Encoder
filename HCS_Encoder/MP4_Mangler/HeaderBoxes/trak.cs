using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MP4_Mangler.HeaderBoxes {
	/// <summary>
	/// Container for track meta-data
	/// </summary>
	public class trak: Box {
		protected tkhd header;
		protected mdia body;
		protected hdlr handler_ref;
		protected minf media_info;

		public trak (int Width, int Height, int TrackId):base("trak"){
			header = new tkhd(Width, Height, TrackId);
			AddChild(header);
			body = new mdia();
			AddChild(body);

			handler_ref = new hdlr(Width, Height);
			body.AddChild(handler_ref);

			media_info = new minf(Width, Height, TrackId);
			body.AddChild(media_info);
		}

		public override void Prepare () {
		}
	}
}
