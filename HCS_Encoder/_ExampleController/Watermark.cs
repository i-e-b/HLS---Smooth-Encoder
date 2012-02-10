using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace _ExampleController {
	public class Watermark {
		public List<GraphicsPath> paths;

		public RectangleF Bounds {
			get {
				if (paths.Count < 1) return new RectangleF();
				RectangleF r = paths[0].GetBounds();
				foreach (var path in paths) {
					r = RectangleF.Union(r, path.GetBounds());
				}
				return r;
			}
		}

		/// <summary>
		/// Load a watermark from paths, and scale to the target size
		/// </summary>
		/// <param name="TargetSize">Minimum dimension. Watermark is scaled so that either height or width is
		/// equal to this size. Aspect is maintained.</param>
		public Watermark (string FilePath, float TargetSize) {
			paths = new List<GraphicsPath>();
			if (String.IsNullOrEmpty(FilePath)) {
				LoadPaths();
			} else {
				LoadPathsFromXAML(FilePath);
			}
			ScaleAndFlatten(TargetSize);
		}

		/// <summary>
		/// Resize and reduce to lines (trading extra memory size for better rendering speed)
		/// </summary>
		private void ScaleAndFlatten (float TargetSize) {
			RectangleF bounds = Bounds;
			if (bounds.IsEmpty) return; // can't scale;

			float sw = TargetSize / bounds.Width;
			float sh = TargetSize / bounds.Height;

			float scale = Math.Max(sw, sh);

			Matrix smx = new Matrix();
			smx.Translate(-bounds.Left, -bounds.Top); // position at 0,0
			smx.Scale(scale, scale); // scale proportionately to fit in target size

			foreach (var path in paths) {
				path.Flatten(smx, 0.25f);
			}
		}

		private void LoadPathsFromXAML (string filepath) {
			string[] lines = System.IO.File.ReadAllLines(filepath);
			StringBuilder sb = new StringBuilder();

			foreach (var line in lines) {
				Console.WriteLine(line);

				if (line.Contains("Data=\"")) {
					ProcessLine(line);
				}
			}
		}

		private void ProcessLine (string line) {
			int i = 0;
			while (i >= 0) {
				int p = line.IndexOf("Data=\"", i);
				if (p < 0) {
					break;
				}
				p += 6;

				int r = line.IndexOf('"', p);
				if (r > p) {
					paths.Add(SyntaxToPath(line.Substring(p, r - p)));
				}
				i = r;
			}
		}

		/// <summary>
		/// Load a set of paths.
		/// This should be re-written to load from a file.
		/// </summary>
		private void LoadPaths () { 
			// export a set of xaml paths here
			paths.Add(SyntaxToPath(@"F1 M 41.0927,12.2747L 30.3666,6.08191L 30.3666,18.4674L 41.0927,12.2747 Z")); 
		}

		public GraphicsPath SyntaxToPath (string PathSyntax) {
			List<string> t = Tokenize(PathSyntax);
			if (t.Count < 2) return new GraphicsPath();

			GraphicsPath p = new GraphicsPath();
			int i = 0;
			if (t[0] == "F1") {
				p.FillMode = FillMode.Winding;
				i++;
			} else if (t[0] == "F0") {
				p.FillMode = FillMode.Alternate;
				i++;
			}

			PointF last_point = new PointF(0, 0);

			for (; i < t.Count; i++) {
				switch (t[i]) { // doesn't handle a lot of short-cuts yet. Doesn't support Hh or Vv
					case "M":
						p.StartFigure();
						last_point = GetPointStr(t[i + 1], t[i + 2]);
						i += 2;
						break;
					case "m":
						p.StartFigure();
						last_point = GetPointStrOff(t[i + 1], t[i + 2], last_point);
						i += 2;
						break;

					case "L":
						p.AddLine(last_point, GetPointStr(t[i + 1], t[i + 2]));
						last_point = p.GetLastPoint();
						i += 2;
						break;

					case "l":
						p.AddLine(last_point, GetPointStrOff(t[i + 1], t[i + 2], last_point));
						last_point = p.GetLastPoint();
						i += 2;
						break;

					case "C":
						p.AddBezier(last_point, GetPointStr(t[i + 1], t[i + 2]), GetPointStr(t[i + 3], t[i + 4]), GetPointStr(t[i + 5], t[i + 6]));
						last_point = p.GetLastPoint();
						i += 6;
						break;

					case "Z":
					case "z":
						p.CloseFigure();
						break;

					default:
						throw new Exception("Unsupported symbol: " + t[i]);
				}
			}
			p.CloseAllFigures();

			return p;
		}

		private PointF GetPointStrOff (string x, string y, PointF last_point) {
			return PointF.Add(GetPointStr(x, y), p2s(last_point));
		}

		private SizeF p2s (PointF last_point) {
			return new SizeF(last_point);
		}

		private PointF GetPointStr (string x, string y) {
			return new PointF(float.Parse(x), float.Parse(y));
		}

		private List<string> Tokenize (string PathSyntax) {
			var outp = new List<string>();

			Regex splitter = new Regex(@"([A-Za-z][0-1]*)|([\-0-9.][\-0-9.e]*)"); // split around and including numbers

			string[] bits = splitter.Split(PathSyntax);
			foreach (var bit in bits) {
				string b = bit.Trim();
				if (String.IsNullOrEmpty(b)) continue;
				if (b == ",") continue; // don't bother adding.
				outp.Add(b);
			}

			return outp;
		}

		public void Draw (Graphics g) {

		}
	}
}
