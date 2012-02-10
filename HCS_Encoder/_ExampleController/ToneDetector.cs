using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HCS_Encoder.Inputs.Processing;
using HCS_Encoder;
using System.Drawing;
using HCS_Encoder.Inputs.Buffers;

namespace _ExampleController {
	public class ToneDetector : IAudioProcessor {
		/// <summary>
		/// Gets 0..1 value indicating likelyhood of tone.
		/// </summary>
		public double ToneConfidence { get { return Math.Min(500, stable_cycles) / 500.0; } }

		/// <summary>
		/// Gets true when tone has been detected in a sample set.
		/// </summary>
		public bool ToneDetected { get { return ToneConfidence > 0.5; } }

		/// <summary>
		/// Sets smoothing 0..500.
		/// Higher values will find tone in noisier signals, but will be more sensitive to correct frequency
		/// </summary>
		public int TemporalSmoothing {
			set {
				int v = Math.Min(500, Math.Max(0, value));
				decay = (float)(Math.Log10(Math.Max(1.0, v + 1.0)) * 0.37);
				gain = 1.0f - decay;
			}
		}

		/// <summary>
		/// Sets power gate 0..100 (%)
		/// Minimum power to be detected.
		/// </summary>
		public int PowerGate {
			set {
				int v = Math.Min(100, Math.Max(0, value));
				maxg = v / 100.0f;
			}
		}

		/// <summary>
		/// Create a new tone detector.
		/// Target Frequency to be given in Hz (1kHz == 1000)
		/// </summary>
		public ToneDetector (int TargetFrequency) {
			PrepareSampleConversion(TargetFrequency);

			int osclen = tsampr / TargetFrequency;

			osc = new float[osclen]; // sample rate / target freq --> 44000 / 1000
			tracker = new float[osc.Length];
		}

		#region debug helper
		/// <summary>
		/// Draw a visual representation of the tone detector.
		/// </summary>
		public Bitmap DrawOscillator () {
			Bitmap img = new Bitmap(/*width*/ osc.Length, /*height*/32);
			using (Graphics g = Graphics.FromImage(img)) {
				for (int i = 0; i < osc.Length; i++) {
					float power = Math.Min(1.0f, Math.Abs(osc[i]));
					if (osc[i] == 0.0f) {
						g.FillRectangle(Brushes.Black, new Rectangle(i, 0, 2, 16));
					} else if (osc[i] > 0.0f) {
						Color c = Color.FromArgb((int)(255 * power), 0, 0);
						g.FillRectangle(new SolidBrush(c), new Rectangle(i, 0, 2, 16));
					} else {
						Color c = Color.FromArgb(0, 0, (int)(255 * power));
						g.FillRectangle(new SolidBrush(c), new Rectangle(i, 0, 2, 16));
					}
				}

				for (int i = 0; i < tracker.Length; i++) {
					float power = Math.Min(1.0f, Math.Abs(tracker[i]));
					if (tracker[i] == 0.0f) {
						g.FillRectangle(Brushes.Black, new Rectangle(i, 16, 2, 16));
					} else if (tracker[i] > 0.0f) {
						Color c = Color.FromArgb((int)(255 * power), 0, 0);
						g.FillRectangle(new SolidBrush(c), new Rectangle(i, 16, 2, 16));
					} else {
						Color c = Color.FromArgb(0, 0, (int)(255 * power));
						g.FillRectangle(new SolidBrush(c), new Rectangle(i, 16, 2, 16));
					}
				}
			}


			return img;
		}
		#endregion
		#region Inner workings

		#region Sensor values
		float[] osc; // oscillator data (signal is written here)
		float[] tracker; // filtered oscillator data (used for detection)
		int osc_pos = 0; // position

		float decay, gain; // used to balance input to oscillator
		float maxg, max_scale; // gate & power values
		int sign_count = 0; // how many non-zero patches in filtered signal. Must be == 2 for a positive detection.
		int pos_centre = 0, neg_centre = 0; // centres of detected +ve and -ve zones, if 'sign_count' == 2
		int pos_c_new = 0, neg_c_new = 0; // used for drift compensation
		public int stable_cycles = 0; // number of oscillator cycles that pos_centre and neg_centre have been stable to +-2.
		int tsampr = 44100; // target sample rate
		#endregion

		/// <summary>
		/// Find the first sample rate that gives an integral oscillator size
		/// </summary>
		private void PrepareSampleConversion (int TargetFrequency) {
			double fpart = tsampr / (double)TargetFrequency;
			fpart = fpart - ((long)fpart);

			while (fpart != 0.0) {
				tsampr--;
				fpart = tsampr / (double)TargetFrequency;
				fpart = fpart - ((long)fpart);
			}

		}

		/// <summary>
		/// Called by plug-in host (encoder)
		/// </summary>
		public void ProcessSample (TimedSample Sample) {
			float[] samps = ResampleForTone(Sample.Samples);

			foreach (float s in samps) {
				osc[osc_pos] *= decay; // fade old signal, to reduce response time
				osc[osc_pos] += s * gain;
				osc_pos = (osc_pos + 1) % osc.Length;

				if (osc_pos == 0) {
					// Prepare 
					BalanceOscillator();
					FilterOscillator();

					// Tone detection
					DetectTone();
				}
			}
		}

		/// <summary>
		/// Counts the number of non-zero blocks in the filtered oscillator data.
		/// Also finds the centres of the first positive and negative peaks
		/// </summary>
		private void CountGatedSigns () {
			int cs = Math.Sign(tracker[0]);
			int max = tracker.Length;

			List<int> pEdges = new List<int>(2);
			List<int> nEdges = new List<int>(2);

			if (cs != 0) {
				// starts on a peak
				sign_count = 1;

				// count back from end to find possible loop
				int tl = 0;
				for (int i = tracker.Length - 1; i >= 0; i--) {
					if (Math.Sign(tracker[i]) == cs) {
						tl = i;
						max = i;
					} else break;
				}
				if (cs < 0) nEdges.Add(tl - tracker.Length);
				else pEdges.Add(tl - tracker.Length);
			} else sign_count = 0;

			for (int i = 0; i < tracker.Length; i++) {
				int ns = Math.Sign(tracker[i]);
				if (ns != cs && ns != 0 && i < max)
					sign_count++;

				if (nEdges.Count < 2) {
					if (cs >= 0 && ns < 0)
						nEdges.Add(i);
					else if (cs < 0 && ns >= 0)
						nEdges.Add(i);
				}
				if (pEdges.Count < 2) {
					if (cs <= 0 && ns > 0)
						pEdges.Add(i);
					else if (cs > 0 && ns <= 0)
						pEdges.Add(i);
				}

				cs = ns;
			}

			if (pEdges.Count == 2) {
				pos_c_new = (int)pEdges.Average();
				if (pos_c_new < 0) pos_c_new = tracker.Length - pos_c_new;
			} else pos_c_new = -1;

			if (nEdges.Count == 2) {
				neg_c_new = (int)nEdges.Average();
				if (neg_c_new < 0) neg_c_new = tracker.Length - neg_c_new;
			} else neg_c_new = -1;
		}

		private void BalanceOscillator () {
			float max = 0.0f;
			float min = 0.0f;

			// find power centre and re-balance if needed
			for (int i = 0; i < osc.Length; i++) {
				max = Math.Max(max, osc[i]);
				min = Math.Min(min, osc[i]);
			}

			float centre = (max + min) / 2.0f;
			if (centre > 0.01f || centre < -0.01f) {
				max = 0.0f;
				min = 0.0f;
				for (int i = 0; i < osc.Length; i++) {
					osc[i] -= centre;
					max = Math.Max(max, osc[i]);
					min = Math.Min(min, osc[i]);
				}
			}

			// normalise to 1.0...-1.0
			if (max > 0.0f && max > 1.0) {
				float r = 1.0f / max;
				for (int i = 0; i < osc.Length; i++) {
					osc[i] *= r;
				}
			}
		}

		private void JitterOrFail () {
			stable_cycles = Math.Max(0, stable_cycles - 4);
		}

		private void DetectTone () {
			CountGatedSigns();
			if (sign_count != 2) {
				pos_centre = neg_centre = -1;
				JitterOrFail();
				return;
			}

			if (pos_c_new < 0 || neg_c_new < 0) {
				JitterOrFail();
				return;
			}

			int pos_drift = pos_c_new - pos_centre;
			int neg_drift = neg_c_new - neg_centre;
			int drift_allowance = 2;

			// More permissive stability check (allows small drift)
			if ((pos_drift <= drift_allowance && pos_drift >= -drift_allowance)
				&& (neg_drift <= drift_allowance && neg_drift >= -drift_allowance)) {

				stable_cycles++;
				if (stable_cycles > 1000) stable_cycles = 1000; // allows expression of confidence as per-mille

			} else {
				pos_centre = pos_c_new;
				neg_centre = neg_c_new;
			}
		}

		private void FilterOscillator () {
			float max = 0.0f;
			float min = 0.0f;


			// smooth output
			float[] window = new float[3];
			window[0] = osc[osc.Length - 1];
			window[1] = osc[0];
			window[2] = osc[1 % osc.Length];
			for (int i = 1; i <= osc.Length; i++) {
				tracker[i % tracker.Length] = window.Average();
				window[0] = window[1];
				window[1] = window[2];
				window[2] = osc[i % osc.Length];
			}

			// apply level gates
			for (int i = 0; i < tracker.Length; i++) {
				float av = Math.Abs(tracker[i]);
				float sg = Math.Sign(tracker[i]);

				tracker[i] = (av >= maxg) ? (sg) : (0.0f);

				max = Math.Max(max, tracker[i]);
				min = Math.Min(min, tracker[i]);
			}

			// normalise to 1.0...-1.0
			if (max > 0.0f) {
				float r = 1.0f / max;
				for (int i = 0; i < tracker.Length; i++) {
					tracker[i] *= r;
				}
			}
		}

		private float[] ResampleForTone (short[] Samples) {
			short[] source = Samples;
			int total_samps = Samples.Length;
			int out_samples = total_samps * tsampr;
			out_samples /= 44100;

			double error_rate = ((double)total_samps / (double)out_samples);
			float[] result = new float[out_samples];

			float scale = Math.Max(source.Max(), -source.Min());
			if (scale > max_scale) max_scale = scale;
			max_scale -= 1;

			scale = 1.0f / max_scale;

			unchecked {

				for (int i = 0; i < result.Length; i++) {
					int ec = (int)(i * error_rate);
					ec = Math.Min(ec, source.Length - 1);
					ec = Math.Max(ec, 0);

					result[i] = source[ec] * scale;
				}
			}

			return result;
		}

		#endregion
	}
}
