using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;

namespace ConfigurationTool {
	public partial class MainForm : Form {
		public MainForm () {
			InitializeComponent();
			EnumerateDevices();
		}

		/// <summary>
		/// List available audio & video devices into the selection boxes.
		/// </summary>
		private void EnumerateDevices () {
			AudioDeviceMenu.Items.Clear();
			var audio_devices = DsDevice.GetDevicesOfCat(FilterCategory.AudioInputDevice);

			if (audio_devices.Length < 1) {
				AudioDeviceMenu.Enabled = false;
				AudioDeviceProps.Enabled = false;
			} else {
				foreach (var audio_device in audio_devices) {
					AudioDeviceMenu.Items.Add(audio_device.Name);
				}
			}
			AudioDeviceMenu.Items.Add("(none)");

			VideoDeviceMenu.Items.Clear();
			var video_devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

			if (video_devices.Length < 1) {
				VideoDeviceMenu.Enabled = false;
				VideoDeviceProps.Enabled = false;
			} else {
				foreach (var video_device in video_devices) {
					VideoDeviceMenu.Items.Add(video_device.Name);
				}
			}
			VideoDeviceMenu.Items.Add("(none)");

		}

		#region API
		//A (modified) definition of OleCreatePropertyFrame found here: http://groups.google.no/group/microsoft.public.dotnet.languages.csharp/browse_thread/thread/db794e9779144a46/55dbed2bab4cd772?lnk=st&q=[DllImport(%22olepro32.dll%22)]&rnum=1&hl=no#55dbed2bab4cd772
		[DllImport(@"oleaut32.dll")]
		public static extern int OleCreatePropertyFrame (
			IntPtr hwndOwner,
			int x,
			int y,
			[MarshalAs(UnmanagedType.LPWStr)] string lpszCaption,
			int cObjects,
			[MarshalAs(UnmanagedType.Interface, ArraySubType = UnmanagedType.IUnknown)] 
			ref object ppUnk,
			int cPages,
			IntPtr lpPageClsID,
			int lcid,
			int dwReserved,
			IntPtr lpvReserved);
		#endregion

		private void VideoDeviceProps_Click (object sender, EventArgs e) {
			string dev = VideoDeviceMenu.SelectedItem as string;
			if (String.IsNullOrEmpty(dev)) return;

			IBaseFilter filt = CreateFilter(FilterCategory.VideoInputDevice, dev);
			if (filt != null) DisplayPropertyPage(filt);
		}

		private void AudioDeviceProps_Click (object sender, EventArgs e) {
			string dev = AudioDeviceMenu.SelectedItem as string;
			if (String.IsNullOrEmpty(dev)) return;

			IBaseFilter filt = CreateFilter(FilterCategory.AudioInputDevice, dev);
			if (filt != null) DisplayPropertyPage(filt);
		}

		/// <summary>
		/// Displays a property page for a filter
		/// </summary>
		/// <param name="dev">The filter for which to display a property page</param>
		private void DisplayPropertyPage (IBaseFilter dev) {
			//Get the ISpecifyPropertyPages for the filter
			ISpecifyPropertyPages pProp = dev as ISpecifyPropertyPages;
			int hr = 0;

			if (pProp == null) {
				//If the filter doesn't implement ISpecifyPropertyPages, try displaying IAMVfwCompressDialogs instead!
				IAMVfwCompressDialogs compressDialog = dev as IAMVfwCompressDialogs;
				if (compressDialog != null) {

					hr = compressDialog.ShowDialog(VfwCompressDialogs.Config, IntPtr.Zero);
					DsError.ThrowExceptionForHR(hr);
				}
				return;
			}

			//Get the name of the filter from the FilterInfo struct
			FilterInfo filterInfo;
			hr = dev.QueryFilterInfo(out filterInfo);
			DsError.ThrowExceptionForHR(hr);

			// Get the propertypages from the property bag
			DsCAUUID caGUID;
			hr = pProp.GetPages(out caGUID);
			DsError.ThrowExceptionForHR(hr);

			// Check for property pages on the output pin
			IPin pPin = DsFindPin.ByDirection(dev, PinDirection.Output, 0);
			ISpecifyPropertyPages pProp2 = pPin as ISpecifyPropertyPages;
			if (pProp2 != null) {
				DsCAUUID caGUID2;
				hr = pProp2.GetPages(out caGUID2);
				DsError.ThrowExceptionForHR(hr);

				if (caGUID2.cElems > 0) {
					int soGuid = Marshal.SizeOf(typeof(Guid));

					// Create a new buffer to hold all the GUIDs
					IntPtr p1 = Marshal.AllocCoTaskMem((caGUID.cElems + caGUID2.cElems) * soGuid);

					// Copy over the pages from the Filter
					for (int x = 0; x < caGUID.cElems * soGuid; x++) {
						Marshal.WriteByte(p1, x, Marshal.ReadByte(caGUID.pElems, x));
					}

					// Add the pages from the pin
					for (int x = 0; x < caGUID2.cElems * soGuid; x++) {
						Marshal.WriteByte(p1, x + (caGUID.cElems * soGuid), Marshal.ReadByte(caGUID2.pElems, x));
					}

					// Release the old memory
					Marshal.FreeCoTaskMem(caGUID.pElems);
					Marshal.FreeCoTaskMem(caGUID2.pElems);

					// Reset caGUID to include both
					caGUID.pElems = p1;
					caGUID.cElems += caGUID2.cElems;
				}
			}

			// Create and display the OlePropertyFrame
			object oDevice = (object)dev;
			hr = OleCreatePropertyFrame(this.Handle, 0, 0, filterInfo.achName, 1, ref oDevice, caGUID.cElems, caGUID.pElems, 0, 0, IntPtr.Zero);
			DsError.ThrowExceptionForHR(hr);

			// Release COM objects
			Marshal.FreeCoTaskMem(caGUID.pElems);
			Marshal.ReleaseComObject(pProp);
			if (filterInfo.pGraph != null) {
				Marshal.ReleaseComObject(filterInfo.pGraph);
			}
		}

		/// <summary>
		/// Enumerates all filters of the selected category and returns the IBaseFilter for the 
		/// filter described in friendlyname
		/// </summary>
		/// <param name="category">Category of the filter</param>
		/// <param name="friendlyname">Friendly name of the filter</param>
		/// <returns>IBaseFilter for the device</returns>
		private IBaseFilter CreateFilter (Guid category, string friendlyname) {
			object source = null;
			Guid iid = typeof(IBaseFilter).GUID;
			foreach (DsDevice device in DsDevice.GetDevicesOfCat(category)) {
				if (device.Name.CompareTo(friendlyname) == 0) {
					device.Mon.BindToObject(null, null, ref iid, out source);
					break;
				}
			}

			return (IBaseFilter)source;
		}

		private void AudioCaptureRate_ValueChanged (object sender, EventArgs e) {
			if (AudioCaptureRate.Value == 36975) AudioCaptureRate.Value = 44100; // fix rate
		}

		private void OutputHandlerMenu_SelectedIndexChanged (object sender, EventArgs e) {
			if (OutputHandlerMenu.Text.ToLower().StartsWith("iis")) {
				VideoDestinationLabel.Text = "Publishing Point (including username && password - must be a Windows account authorised for WebDAV and Live IIS)";
				IndexFTPRootLabel.Text = "Publishing Point FTP delivery (including username && password, may be blank)";
				IndexFTPRoot.Enabled = true;
				IndexName.Enabled = false;
				ServerLookupRoot.Enabled = false;
			} else {
				VideoDestinationLabel.Text = "Video FTP Root (including username && password)";
				IndexFTPRootLabel.Text = "Index FTP Root (including username && password)";
				IndexFTPRoot.Enabled = true;
				IndexName.Enabled = true;
				ServerLookupRoot.Enabled = true;
			}
		}

		private void LoadConfigDialog_FileOk (object sender, CancelEventArgs e) {
			try {
				// load a config file
				EncoderConfiguration.Configuration config = null;
				string msg = "";
				try {
					config = EncoderConfiguration.Configuration.LoadFromFile(LoadConfigDialog.FileName);
				} catch (Exception ex) {
					msg = ex.Message;
				}
				if (config == null) {
					MessageBox.Show("Not a valid configuration file\r\n" + msg, "HCS Config");
					return;
				}

				LoadedFileLabel.Text = (LoadConfigDialog.SafeFileName);

				if (config.Audio.CaptureDeviceNumber < 0) {
					AudioDeviceMenu.SelectedIndex = AudioDeviceMenu.Items.Count - 1; // "(none)" at end of list
				} else {
					AudioDeviceMenu.SelectedIndex = Math.Min(config.Audio.CaptureDeviceNumber, AudioDeviceMenu.Items.Count - 1);
				}
				AudioChannelCount.Value = config.Audio.Channels;
				AudioCaptureRate.Value = config.Audio.SampleRate;

				FragmentSize.Value = config.EncoderSettings.FragmentSeconds;
				FilePrefix.Text = config.EncoderSettings.LocalSystemFilePrefix;
				LocalFilesystemOutputFolder.Text = config.EncoderSettings.LocalSystemOutputFolder;
				OutputHeight.Value = config.EncoderSettings.OutputHeight;
				OutputWidth.Value = config.EncoderSettings.OutputWidth;
				VideoBitrate.Value = config.EncoderSettings.VideoBitrate;

				// MBR bits effect this:
				var factors = config.EncoderSettings.ReductionFactors;
				if (factors == null) factors = new List<double>();
				factors.Remove(1.0);
				for (int i = 0; i < MBRChecklist.Items.Count; i++) {
					double val = double.Parse(MBRChecklist.Items[i].ToString().Replace("%", "")) / 100.0;
					if (factors.Contains(val)) {
						MBRChecklist.SetItemChecked(i, true);
						factors.Remove(val);
					} else {
						MBRChecklist.SetItemChecked(i, false);
					}
				}
				foreach (var factor in factors) { // any extra non-standard factors:
					string wrd = (factor * 100.0).ToString("0") + "%";
					MBRChecklist.Items.Add(wrd, true);
				}

				IndexFTPRoot.Text = config.Upload.IndexFtpRoot;
				IndexName.Text = config.Upload.IndexName;
				ServerLookupRoot.Text = config.Upload.ServerLookupRoot;
				VideoFTPRoot.Text = config.Upload.VideoDestinationRoot;

				if (config.Video.CaptureDeviceNumber < 0) {
					VideoDeviceMenu.SelectedIndex = VideoDeviceMenu.Items.Count - 1; // "(none)" at end of list
				} else {
					VideoDeviceMenu.SelectedIndex = Math.Min(config.Video.CaptureDeviceNumber, VideoDeviceMenu.Items.Count - 1);
				}
				FrameRate.Value = config.Video.InputFrameRate;
				CaptureHeight.Value = config.Video.InputHeight;
				CaptureWidth.Value = config.Video.InputWidth;

				OutputHandlerMenu.Text = config.Upload.UploadHandlerName;
			} catch (Exception ex) {
				MessageBox.Show("Couldn't open: " + ex.Message);
			}
			SaveConfigDialog.FileName = LoadConfigDialog.SafeFileName; // next save is assumed to overwrite.
		}

		private void LoadSettingsButton_Click (object sender, EventArgs e) {
			LoadConfigDialog.ShowDialog();
		}

		private void SaveConfigDialog_FileOk (object sender, CancelEventArgs e) {
			// save config
			EncoderConfiguration.Configuration config = new EncoderConfiguration.Configuration();

			if (AudioDeviceMenu.SelectedItem.ToString() == "(none)") {
				config.Audio.CaptureDeviceNumber = -1;
			} else {
				config.Audio.CaptureDeviceNumber = Math.Max(0, AudioDeviceMenu.SelectedIndex);
			}
			config.Audio.Channels = (int)AudioChannelCount.Value;
			config.Audio.SampleRate = (int)AudioCaptureRate.Value;

			config.EncoderSettings.FragmentSeconds = (int)FragmentSize.Value;
			config.EncoderSettings.LocalSystemFilePrefix = FilePrefix.Text;
			config.EncoderSettings.LocalSystemOutputFolder = LocalFilesystemOutputFolder.Text;
			config.EncoderSettings.OutputHeight = (int)OutputHeight.Value;
			config.EncoderSettings.OutputWidth = (int)OutputWidth.Value;
			config.EncoderSettings.VideoBitrate = (int)VideoBitrate.Value;

			// MBR bits effect this:
			config.EncoderSettings.ReductionFactors = new List<double>();
			config.EncoderSettings.ReductionFactors.Add(1.0);
			foreach (var item in MBRChecklist.CheckedItems) {
				double val = double.Parse(item.ToString().Replace("%", "")) / 100.0;
				config.EncoderSettings.ReductionFactors.Add(val);
			}

			config.Upload.IndexFtpRoot = IndexFTPRoot.Text;
			config.Upload.IndexName = IndexName.Text;
			config.Upload.ServerLookupRoot = ServerLookupRoot.Text;
			config.Upload.VideoDestinationRoot = VideoFTPRoot.Text;

			if (VideoDeviceMenu.SelectedItem.ToString() == "(none)") {
				config.Video.CaptureDeviceNumber = -1;
			} else {
				config.Video.CaptureDeviceNumber = Math.Max(0, VideoDeviceMenu.SelectedIndex);
			}
			config.Video.InputFrameRate = (int)FrameRate.Value;
			config.Video.InputHeight = (int)CaptureHeight.Value;
			config.Video.InputWidth = (int)CaptureWidth.Value;

			
			config.Upload.UploadHandlerName = OutputHandlerMenu.Text;

			config.SaveToFile(SaveConfigDialog.FileName);
		}

		private void SaveSettingsButton_Click (object sender, EventArgs e) {
			SaveConfigDialog.ShowDialog();
		}

		private void OutputWidth_ValueChanged (object sender, EventArgs e) {
			OutputWidth.Value -= OutputWidth.Value % OutputWidth.Increment;
		}

		private void TestVideoSettings_Click (object sender, EventArgs e) {
			if (VideoDeviceMenu.SelectedIndex < 0) {
				MessageBox.Show("Please select a capture device first", "HCS Config");
				return;
			}
			HCS_Encoder.VideoCapture cam = null;
			try {
				cam = new HCS_Encoder.VideoCapture(VideoDeviceMenu.SelectedIndex,
					(int)FrameRate.Value, (int)CaptureWidth.Value, (int)CaptureHeight.Value);

				FrameRate.Value = cam.m_frameRate;
				CaptureHeight.Value = cam.Height;
				CaptureWidth.Value = cam.Width;

				MessageBox.Show("Video device initialised OK.\r\nCapture settings may have been updated", "HCS Config");
			} catch {
				MessageBox.Show("Those settings don't work.\r\n(capture device refused to initialise, please check your device's documentation)", "HCS Config");
			} finally {
				if (cam != null) cam.Dispose();
			}
		}

		private void TestAudioSettings_Click (object sender, EventArgs e) {
			if (AudioDeviceMenu.SelectedIndex < 0) {
				MessageBox.Show("Please select a capture device first", "HCS Config");
				return;
			}
			HCS_Encoder.AudioCapture mic = null;
			try {
				mic = new HCS_Encoder.AudioCapture(AudioDeviceMenu.SelectedIndex,
					(int)AudioCaptureRate.Value, (int)AudioChannelCount.Value);

				AudioChannelCount.Value = mic.Channels;
				AudioCaptureRate.Value = mic.SampleRate;

				MessageBox.Show("Audio device initialised OK.\r\nCapture settings may have been updated", "HCS Config");
			} catch {
				MessageBox.Show("Those settings don't work.\r\n(capture device refused to initialise, please check your device's documentation)", "HCS Config");
			} finally {
				if (mic != null) mic.Dispose();
			}
		}

		private void VideoDevicePreview_Click (object sender, EventArgs e) {
			if (VideoDeviceMenu.SelectedIndex < 0) {
				MessageBox.Show("Please select a capture device first", "HCS Config");
				return;
			}
			HCS_Encoder.VideoCapture cam = null;
			try {
				cam = new HCS_Encoder.VideoCapture(VideoDeviceMenu.SelectedIndex,
					(int)FrameRate.Value, (int)CaptureWidth.Value, (int)CaptureHeight.Value);

				FrameRate.Value = cam.m_frameRate;
				CaptureHeight.Value = cam.Height;
				CaptureWidth.Value = cam.Width;

				// preview with scaling:
				cam.TargetFrameSize = new Size((int)OutputWidth.Value, (int)OutputHeight.Value);

				VideoPreview prev = new VideoPreview(cam);
				prev.ShowDialog();
			} catch {
				MessageBox.Show("Those settings don't work.\r\n(capture device refused to initialise, please check your device's documentation)", "HCS Config");
				if (cam != null) cam.Dispose();
			} 
		}

		private void AudioPreviewButton_Click (object sender, EventArgs e) {
			if (AudioDeviceMenu.SelectedIndex < 0) {
				MessageBox.Show("Please select a capture device first", "HCS Config");
				return;
			}
			HCS_Encoder.AudioCapture mic = null;
			try {
				mic = new HCS_Encoder.AudioCapture(AudioDeviceMenu.SelectedIndex,
					(int)AudioCaptureRate.Value, (int)AudioChannelCount.Value);

				AudioChannelCount.Value = mic.Channels;
				AudioCaptureRate.Value = mic.SampleRate;

				AudioPreview prev = new AudioPreview(mic);
				prev.ShowDialog();
			} catch {
				MessageBox.Show("Those settings don't work.\r\n(capture device refused to initialise, please check your device's documentation)", "HCS Config");
				if (mic != null) mic.Dispose();
			} 
		}

		private void BalanceAspectButton_Click (object sender, EventArgs e) {
			decimal scale = OutputWidth.Value / CaptureWidth.Value;
			OutputHeight.Value = CaptureHeight.Value * scale;
		}

		private void UriHelper_Click (object sender, EventArgs e) {
			(new UriHelper()).Show();
		}

		private void MainForm_Load (object sender, EventArgs e) {

		}

	}
}
