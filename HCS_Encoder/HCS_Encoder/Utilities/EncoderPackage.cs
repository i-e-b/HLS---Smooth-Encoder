using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EncoderConfiguration;
using HCS_Encoder;
using System.IO;
using System.Threading;
using HCS_Encoder.Inputs.Processing;
using MP4_Mangler;
using System.Runtime.InteropServices;
using System.Drawing;
using HCS_Encoder.Inputs.Buffers;

namespace HCS_Encoder.Utilities {

	/// <summary>
	/// Class for describing an encoder package
	/// </summary>
	public class PackageSpec {
		/// <summary>
		/// Types of buffer that are known.
		/// </summary>
		[Flags]
		public enum BufferType {
			Video = 1, Audio = 2, Data = 4
		}

		#region Creators
		public PackageSpec () {
		}

		public PackageSpec (BufferType Types, Size VideoSize) {
			RequiredBuffers = Types;
			this.VideoSize = VideoSize;
		}

		public PackageSpec (BufferType Types, double Quality) {
			RequiredBuffers = Types;
			this.Quality = Quality;
		}


		public PackageSpec (BufferType Types, Size VideoSize, double Quality) {
			RequiredBuffers = Types;
			this.VideoSize = VideoSize;
			this.Quality = Quality;
		}
		#endregion

		public bool HasVideo {
			get {
				return (RequiredBuffers & PackageSpec.BufferType.Video) == PackageSpec.BufferType.Video;
			}
		}

		public bool HasAudio {
			get {
				return (RequiredBuffers & PackageSpec.BufferType.Audio) == PackageSpec.BufferType.Audio;
			}
		}

		public BufferType RequiredBuffers { get; set; }

		public Size VideoSize { get; set; }

		public double Quality { get; set; }
	}

	/// <summary>
	/// EncoderPackage bundles a set of classes and structures for the core encoder loop of EncoderController
	/// </summary>
	public class EncoderPackage {
		/// <summary>
		/// Gets the source specification for this package
		/// </summary>
		public PackageSpec Specification { get; private set; }

		/// <summary>
		/// Returns true if any of the package's buffers are empty.
		/// </summary>
		public bool BuffersEmpty (uint MinimumBufferPopulation) {
			return Buffers.Any(a => a.QueueLength < MinimumBufferPopulation);
		}

		/// <summary>
		/// Job reference number.
		/// </summary>
		public int JobIndex { get; set; }

		/// <summary>
		/// All buffers to be encoded in this package.
		/// There should be no overlap in data-types (a call to each to the same MediaFrame should be OK).
		/// </summary>
		public List<IEncoderBuffer> Buffers { get; set; }

		/// <summary>
		/// Correctly configured encoder job.
		/// </summary>
		public EncoderJob Job;

		/// <summary>
		/// Prepared media frame for loading and unloading.
		/// </summary>
		public MediaFrame Frame;

		/// <summary>
		/// Create a new EncoderPackage
		/// </summary>
		public EncoderPackage (PackageSpec SrcSpec, int Index, EncoderJob Job, MediaFrame Frame) {
			Buffers = new List<IEncoderBuffer>();
			this.JobIndex = Index;
			this.Job = Job;
			this.Frame = Frame;
			Specification = SrcSpec;
		}

		/// <summary>
		/// Load and Lock all buffers to the MediaFrame.
		/// IMPORTANT: You must call UnloadAllFrames after this method is called.
		/// For efficiency, unload as soon as possible.
		/// </summary>
		public void LoadAllBuffers () {
			try {
				foreach (var buf in Buffers) {
					buf.LoadToFrame(ref Frame);
				}
			} catch {
				foreach (var buf in Buffers) {
					try { buf.UnloadFrame(ref Frame); } catch { }
				}
				throw;
			}
		}

		/// <summary>
		/// Release memory previously locked by LoadToFrame()
		/// </summary>
		public void UnloadAllBuffers () {
			foreach (var buf in Buffers) {
				try { buf.UnloadFrame(ref Frame); } catch { }
			}
		}
	}
}
