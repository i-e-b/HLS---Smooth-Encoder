namespace ConfigurationTool {
	partial class MainForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent () {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.VideoDeviceMenu = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.VideoDeviceProps = new System.Windows.Forms.Button();
			this.AudioDeviceProps = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.AudioDeviceMenu = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.CaptureWidth = new System.Windows.Forms.NumericUpDown();
			this.CaptureHeight = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.FrameRate = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.OutputHeight = new System.Windows.Forms.NumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.OutputWidth = new System.Windows.Forms.NumericUpDown();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.AudioCaptureRate = new System.Windows.Forms.NumericUpDown();
			this.label12 = new System.Windows.Forms.Label();
			this.AudioChannelCount = new System.Windows.Forms.NumericUpDown();
			this.label13 = new System.Windows.Forms.Label();
			this.VideoBitrate = new System.Windows.Forms.NumericUpDown();
			this.label14 = new System.Windows.Forms.Label();
			this.FragmentSize = new System.Windows.Forms.NumericUpDown();
			this.label15 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.LocalFilesystemOutputFolder = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			this.VideoDestinationLabel = new System.Windows.Forms.Label();
			this.VideoFTPRoot = new System.Windows.Forms.TextBox();
			this.IndexFTPRootLabel = new System.Windows.Forms.Label();
			this.IndexFTPRoot = new System.Windows.Forms.TextBox();
			this.label20 = new System.Windows.Forms.Label();
			this.ServerLookupRoot = new System.Windows.Forms.TextBox();
			this.label21 = new System.Windows.Forms.Label();
			this.FilePrefix = new System.Windows.Forms.TextBox();
			this.label22 = new System.Windows.Forms.Label();
			this.IndexName = new System.Windows.Forms.TextBox();
			this.LoadSettingsButton = new System.Windows.Forms.Button();
			this.SaveSettingsButton = new System.Windows.Forms.Button();
			this.LoadConfigDialog = new System.Windows.Forms.OpenFileDialog();
			this.SaveConfigDialog = new System.Windows.Forms.SaveFileDialog();
			this.TestVideoSettings = new System.Windows.Forms.Button();
			this.TestAudioSettings = new System.Windows.Forms.Button();
			this.VideoDevicePreview = new System.Windows.Forms.Button();
			this.AudioPreviewButton = new System.Windows.Forms.Button();
			this.BalanceAspectButton = new System.Windows.Forms.Button();
			this.OutputHandlerMenu = new System.Windows.Forms.ComboBox();
			this.label18 = new System.Windows.Forms.Label();
			this.UriHelper = new System.Windows.Forms.Button();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.VideoTab = new System.Windows.Forms.TabPage();
			this.AudioTab = new System.Windows.Forms.TabPage();
			this.BitratesTab = new System.Windows.Forms.TabPage();
			this.label19 = new System.Windows.Forms.Label();
			this.MBRChecklist = new System.Windows.Forms.CheckedListBox();
			this.OutputTab = new System.Windows.Forms.TabPage();
			this.LoadedFileLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.CaptureWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CaptureHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FrameRate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OutputHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OutputWidth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AudioCaptureRate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AudioChannelCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VideoBitrate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FragmentSize)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.VideoTab.SuspendLayout();
			this.AudioTab.SuspendLayout();
			this.BitratesTab.SuspendLayout();
			this.OutputTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(13, 39);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 17);
			this.label1.TabIndex = 21;
			this.label1.Text = "Important Note:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(13, 60);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(450, 36);
			this.label2.TabIndex = 22;
			this.label2.Text = "This tool must be run on the encoding server. If you change your hardware layout," +
				" please re-run this tool";
			// 
			// VideoDeviceMenu
			// 
			this.VideoDeviceMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.VideoDeviceMenu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.VideoDeviceMenu.FormattingEnabled = true;
			this.VideoDeviceMenu.Location = new System.Drawing.Point(94, 9);
			this.VideoDeviceMenu.Name = "VideoDeviceMenu";
			this.VideoDeviceMenu.Size = new System.Drawing.Size(358, 21);
			this.VideoDeviceMenu.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(5, 12);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(71, 13);
			this.label3.TabIndex = 23;
			this.label3.Text = "Video Device";
			// 
			// VideoDeviceProps
			// 
			this.VideoDeviceProps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.VideoDeviceProps.Location = new System.Drawing.Point(467, 8);
			this.VideoDeviceProps.Name = "VideoDeviceProps";
			this.VideoDeviceProps.Size = new System.Drawing.Size(93, 23);
			this.VideoDeviceProps.TabIndex = 2;
			this.VideoDeviceProps.Text = "Properties...";
			this.VideoDeviceProps.UseVisualStyleBackColor = true;
			this.VideoDeviceProps.Click += new System.EventHandler(this.VideoDeviceProps_Click);
			// 
			// AudioDeviceProps
			// 
			this.AudioDeviceProps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AudioDeviceProps.Location = new System.Drawing.Point(467, 8);
			this.AudioDeviceProps.Name = "AudioDeviceProps";
			this.AudioDeviceProps.Size = new System.Drawing.Size(93, 23);
			this.AudioDeviceProps.TabIndex = 11;
			this.AudioDeviceProps.Text = "Properties...";
			this.AudioDeviceProps.UseVisualStyleBackColor = true;
			this.AudioDeviceProps.Click += new System.EventHandler(this.AudioDeviceProps_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(5, 12);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(71, 13);
			this.label4.TabIndex = 33;
			this.label4.Text = "Audio Device";
			// 
			// AudioDeviceMenu
			// 
			this.AudioDeviceMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.AudioDeviceMenu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.AudioDeviceMenu.FormattingEnabled = true;
			this.AudioDeviceMenu.Location = new System.Drawing.Point(94, 9);
			this.AudioDeviceMenu.Name = "AudioDeviceMenu";
			this.AudioDeviceMenu.Size = new System.Drawing.Size(358, 21);
			this.AudioDeviceMenu.TabIndex = 10;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(5, 46);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(75, 13);
			this.label5.TabIndex = 24;
			this.label5.Text = "Capture Width";
			// 
			// CaptureWidth
			// 
			this.CaptureWidth.Location = new System.Drawing.Point(95, 44);
			this.CaptureWidth.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.CaptureWidth.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.CaptureWidth.Name = "CaptureWidth";
			this.CaptureWidth.Size = new System.Drawing.Size(120, 20);
			this.CaptureWidth.TabIndex = 3;
			this.CaptureWidth.Value = new decimal(new int[] {
            640,
            0,
            0,
            0});
			// 
			// CaptureHeight
			// 
			this.CaptureHeight.Location = new System.Drawing.Point(95, 70);
			this.CaptureHeight.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.CaptureHeight.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.CaptureHeight.Name = "CaptureHeight";
			this.CaptureHeight.Size = new System.Drawing.Size(120, 20);
			this.CaptureHeight.TabIndex = 4;
			this.CaptureHeight.Value = new decimal(new int[] {
            480,
            0,
            0,
            0});
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(5, 72);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(78, 13);
			this.label6.TabIndex = 26;
			this.label6.Text = "Capture Height";
			// 
			// FrameRate
			// 
			this.FrameRate.DecimalPlaces = 1;
			this.FrameRate.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.FrameRate.Location = new System.Drawing.Point(95, 96);
			this.FrameRate.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
			this.FrameRate.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.FrameRate.Name = "FrameRate";
			this.FrameRate.Size = new System.Drawing.Size(120, 20);
			this.FrameRate.TabIndex = 7;
			this.FrameRate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(5, 98);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(54, 13);
			this.label7.TabIndex = 28;
			this.label7.Text = "Framerate";
			// 
			// OutputHeight
			// 
			this.OutputHeight.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.OutputHeight.Location = new System.Drawing.Point(332, 72);
			this.OutputHeight.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.OutputHeight.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.OutputHeight.Name = "OutputHeight";
			this.OutputHeight.Size = new System.Drawing.Size(120, 20);
			this.OutputHeight.TabIndex = 6;
			this.OutputHeight.Value = new decimal(new int[] {
            320,
            0,
            0,
            0});
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(242, 74);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(73, 13);
			this.label8.TabIndex = 27;
			this.label8.Text = "Output Height";
			// 
			// OutputWidth
			// 
			this.OutputWidth.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.OutputWidth.Location = new System.Drawing.Point(332, 46);
			this.OutputWidth.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.OutputWidth.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.OutputWidth.Name = "OutputWidth";
			this.OutputWidth.Size = new System.Drawing.Size(120, 20);
			this.OutputWidth.TabIndex = 5;
			this.OutputWidth.Value = new decimal(new int[] {
            480,
            0,
            0,
            0});
			this.OutputWidth.ValueChanged += new System.EventHandler(this.OutputWidth_ValueChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(242, 48);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(70, 13);
			this.label9.TabIndex = 25;
			this.label9.Text = "Output Width";
			// 
			// label10
			// 
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label10.Location = new System.Drawing.Point(243, 131);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(303, 66);
			this.label10.TabIndex = 29;
			this.label10.Text = "For best performance, input and output size should match.\r\nPreview image should a" +
				"ppear correctly orientated.";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(5, 46);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(108, 13);
			this.label11.TabIndex = 34;
			this.label11.Text = "Capture Sample Rate";
			// 
			// AudioCaptureRate
			// 
			this.AudioCaptureRate.Increment = new decimal(new int[] {
            11025,
            0,
            0,
            0});
			this.AudioCaptureRate.Location = new System.Drawing.Point(119, 44);
			this.AudioCaptureRate.Maximum = new decimal(new int[] {
            48000,
            0,
            0,
            0});
			this.AudioCaptureRate.Minimum = new decimal(new int[] {
            11025,
            0,
            0,
            0});
			this.AudioCaptureRate.Name = "AudioCaptureRate";
			this.AudioCaptureRate.Size = new System.Drawing.Size(120, 20);
			this.AudioCaptureRate.TabIndex = 12;
			this.AudioCaptureRate.Value = new decimal(new int[] {
            44100,
            0,
            0,
            0});
			this.AudioCaptureRate.ValueChanged += new System.EventHandler(this.AudioCaptureRate_ValueChanged);
			// 
			// label12
			// 
			this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label12.Location = new System.Drawing.Point(6, 108);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(540, 32);
			this.label12.TabIndex = 35;
			this.label12.Text = "Output rate is always 44100, input will be resampled";
			// 
			// AudioChannelCount
			// 
			this.AudioChannelCount.Location = new System.Drawing.Point(119, 70);
			this.AudioChannelCount.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.AudioChannelCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.AudioChannelCount.Name = "AudioChannelCount";
			this.AudioChannelCount.Size = new System.Drawing.Size(120, 20);
			this.AudioChannelCount.TabIndex = 13;
			this.AudioChannelCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(5, 72);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(51, 13);
			this.label13.TabIndex = 36;
			this.label13.Text = "Channels";
			// 
			// VideoBitrate
			// 
			this.VideoBitrate.Increment = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.VideoBitrate.Location = new System.Drawing.Point(95, 137);
			this.VideoBitrate.Maximum = new decimal(new int[] {
            1500000,
            0,
            0,
            0});
			this.VideoBitrate.Minimum = new decimal(new int[] {
            64000,
            0,
            0,
            0});
			this.VideoBitrate.Name = "VideoBitrate";
			this.VideoBitrate.Size = new System.Drawing.Size(120, 20);
			this.VideoBitrate.TabIndex = 8;
			this.VideoBitrate.ThousandsSeparator = true;
			this.VideoBitrate.Value = new decimal(new int[] {
            320000,
            0,
            0,
            0});
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(5, 139);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(80, 13);
			this.label14.TabIndex = 30;
			this.label14.Text = "Encoder Bitrate";
			// 
			// FragmentSize
			// 
			this.FragmentSize.Location = new System.Drawing.Point(95, 163);
			this.FragmentSize.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
			this.FragmentSize.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.FragmentSize.Name = "FragmentSize";
			this.FragmentSize.Size = new System.Drawing.Size(67, 20);
			this.FragmentSize.TabIndex = 9;
			this.FragmentSize.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(5, 165);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(74, 13);
			this.label15.TabIndex = 31;
			this.label15.Text = "Fragment Size";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(168, 165);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(47, 13);
			this.label16.TabIndex = 32;
			this.label16.Text = "seconds";
			// 
			// LocalFilesystemOutputFolder
			// 
			this.LocalFilesystemOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LocalFilesystemOutputFolder.Location = new System.Drawing.Point(8, 94);
			this.LocalFilesystemOutputFolder.Name = "LocalFilesystemOutputFolder";
			this.LocalFilesystemOutputFolder.Size = new System.Drawing.Size(543, 20);
			this.LocalFilesystemOutputFolder.TabIndex = 14;
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(5, 78);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(151, 13);
			this.label17.TabIndex = 37;
			this.label17.Text = "Local Filesystem Output Folder";
			// 
			// VideoDestinationLabel
			// 
			this.VideoDestinationLabel.AutoSize = true;
			this.VideoDestinationLabel.Location = new System.Drawing.Point(6, 124);
			this.VideoDestinationLabel.Name = "VideoDestinationLabel";
			this.VideoDestinationLabel.Size = new System.Drawing.Size(240, 13);
			this.VideoDestinationLabel.TabIndex = 38;
			this.VideoDestinationLabel.Text = "Video FTP Root (including username && password)";
			// 
			// VideoFTPRoot
			// 
			this.VideoFTPRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.VideoFTPRoot.Location = new System.Drawing.Point(9, 140);
			this.VideoFTPRoot.Name = "VideoFTPRoot";
			this.VideoFTPRoot.Size = new System.Drawing.Size(542, 20);
			this.VideoFTPRoot.TabIndex = 15;
			// 
			// IndexFTPRootLabel
			// 
			this.IndexFTPRootLabel.AutoSize = true;
			this.IndexFTPRootLabel.Location = new System.Drawing.Point(6, 169);
			this.IndexFTPRootLabel.Name = "IndexFTPRootLabel";
			this.IndexFTPRootLabel.Size = new System.Drawing.Size(239, 13);
			this.IndexFTPRootLabel.TabIndex = 39;
			this.IndexFTPRootLabel.Text = "Index FTP Root (including username && password)";
			// 
			// IndexFTPRoot
			// 
			this.IndexFTPRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.IndexFTPRoot.Location = new System.Drawing.Point(9, 185);
			this.IndexFTPRoot.Name = "IndexFTPRoot";
			this.IndexFTPRoot.Size = new System.Drawing.Size(542, 20);
			this.IndexFTPRoot.TabIndex = 16;
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Location = new System.Drawing.Point(6, 229);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(356, 13);
			this.label20.TabIndex = 40;
			this.label20.Text = "HTTP Video Root (this is just the file prefix if Index and Video are together)";
			// 
			// ServerLookupRoot
			// 
			this.ServerLookupRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ServerLookupRoot.Location = new System.Drawing.Point(9, 245);
			this.ServerLookupRoot.Name = "ServerLookupRoot";
			this.ServerLookupRoot.Size = new System.Drawing.Size(542, 20);
			this.ServerLookupRoot.TabIndex = 17;
			// 
			// label21
			// 
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(5, 275);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(52, 13);
			this.label21.TabIndex = 41;
			this.label21.Text = "File Prefix";
			// 
			// FilePrefix
			// 
			this.FilePrefix.Location = new System.Drawing.Point(8, 291);
			this.FilePrefix.Name = "FilePrefix";
			this.FilePrefix.Size = new System.Drawing.Size(230, 20);
			this.FilePrefix.TabIndex = 18;
			// 
			// label22
			// 
			this.label22.AutoSize = true;
			this.label22.Location = new System.Drawing.Point(242, 275);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(161, 13);
			this.label22.TabIndex = 42;
			this.label22.Text = "Index name (including extension)";
			// 
			// IndexName
			// 
			this.IndexName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.IndexName.Location = new System.Drawing.Point(244, 291);
			this.IndexName.Name = "IndexName";
			this.IndexName.Size = new System.Drawing.Size(307, 20);
			this.IndexName.TabIndex = 19;
			// 
			// LoadSettingsButton
			// 
			this.LoadSettingsButton.Location = new System.Drawing.Point(14, 12);
			this.LoadSettingsButton.Name = "LoadSettingsButton";
			this.LoadSettingsButton.Size = new System.Drawing.Size(75, 23);
			this.LoadSettingsButton.TabIndex = 0;
			this.LoadSettingsButton.Text = "Load...";
			this.LoadSettingsButton.UseVisualStyleBackColor = true;
			this.LoadSettingsButton.Click += new System.EventHandler(this.LoadSettingsButton_Click);
			// 
			// SaveSettingsButton
			// 
			this.SaveSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveSettingsButton.Location = new System.Drawing.Point(514, 12);
			this.SaveSettingsButton.Name = "SaveSettingsButton";
			this.SaveSettingsButton.Size = new System.Drawing.Size(75, 25);
			this.SaveSettingsButton.TabIndex = 20;
			this.SaveSettingsButton.Text = "Save...";
			this.SaveSettingsButton.UseVisualStyleBackColor = true;
			this.SaveSettingsButton.Click += new System.EventHandler(this.SaveSettingsButton_Click);
			// 
			// LoadConfigDialog
			// 
			this.LoadConfigDialog.DefaultExt = "xml";
			this.LoadConfigDialog.FileName = "openFileDialog1";
			this.LoadConfigDialog.Filter = "XML Files|*.xml";
			this.LoadConfigDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.LoadConfigDialog_FileOk);
			// 
			// SaveConfigDialog
			// 
			this.SaveConfigDialog.DefaultExt = "xml";
			this.SaveConfigDialog.FileName = "default.xml";
			this.SaveConfigDialog.Filter = "XML Files|*.xml";
			this.SaveConfigDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveConfigDialog_FileOk);
			// 
			// TestVideoSettings
			// 
			this.TestVideoSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TestVideoSettings.Location = new System.Drawing.Point(467, 37);
			this.TestVideoSettings.Name = "TestVideoSettings";
			this.TestVideoSettings.Size = new System.Drawing.Size(93, 23);
			this.TestVideoSettings.TabIndex = 44;
			this.TestVideoSettings.Text = "Test Settings";
			this.TestVideoSettings.UseVisualStyleBackColor = true;
			this.TestVideoSettings.Click += new System.EventHandler(this.TestVideoSettings_Click);
			// 
			// TestAudioSettings
			// 
			this.TestAudioSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TestAudioSettings.Location = new System.Drawing.Point(467, 37);
			this.TestAudioSettings.Name = "TestAudioSettings";
			this.TestAudioSettings.Size = new System.Drawing.Size(93, 23);
			this.TestAudioSettings.TabIndex = 45;
			this.TestAudioSettings.Text = "Test Settings";
			this.TestAudioSettings.UseVisualStyleBackColor = true;
			this.TestAudioSettings.Click += new System.EventHandler(this.TestAudioSettings_Click);
			// 
			// VideoDevicePreview
			// 
			this.VideoDevicePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.VideoDevicePreview.Location = new System.Drawing.Point(467, 66);
			this.VideoDevicePreview.Name = "VideoDevicePreview";
			this.VideoDevicePreview.Size = new System.Drawing.Size(93, 23);
			this.VideoDevicePreview.TabIndex = 46;
			this.VideoDevicePreview.Text = "Preview";
			this.VideoDevicePreview.UseVisualStyleBackColor = true;
			this.VideoDevicePreview.Click += new System.EventHandler(this.VideoDevicePreview_Click);
			// 
			// AudioPreviewButton
			// 
			this.AudioPreviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AudioPreviewButton.Location = new System.Drawing.Point(467, 66);
			this.AudioPreviewButton.Name = "AudioPreviewButton";
			this.AudioPreviewButton.Size = new System.Drawing.Size(93, 23);
			this.AudioPreviewButton.TabIndex = 47;
			this.AudioPreviewButton.Text = "Preview";
			this.AudioPreviewButton.UseVisualStyleBackColor = true;
			this.AudioPreviewButton.Click += new System.EventHandler(this.AudioPreviewButton_Click);
			// 
			// BalanceAspectButton
			// 
			this.BalanceAspectButton.Location = new System.Drawing.Point(332, 98);
			this.BalanceAspectButton.Name = "BalanceAspectButton";
			this.BalanceAspectButton.Size = new System.Drawing.Size(120, 23);
			this.BalanceAspectButton.TabIndex = 48;
			this.BalanceAspectButton.Text = "Balance Aspect";
			this.BalanceAspectButton.UseVisualStyleBackColor = true;
			this.BalanceAspectButton.Click += new System.EventHandler(this.BalanceAspectButton_Click);
			// 
			// OutputHandlerMenu
			// 
			this.OutputHandlerMenu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OutputHandlerMenu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.OutputHandlerMenu.FormattingEnabled = true;
			this.OutputHandlerMenu.Items.AddRange(new object[] {
            "HTTP Live",
            "IIS Smooth",
            "Test"});
			this.OutputHandlerMenu.Location = new System.Drawing.Point(94, 9);
			this.OutputHandlerMenu.Name = "OutputHandlerMenu";
			this.OutputHandlerMenu.Size = new System.Drawing.Size(457, 21);
			this.OutputHandlerMenu.TabIndex = 49;
			this.OutputHandlerMenu.SelectedIndexChanged += new System.EventHandler(this.OutputHandlerMenu_SelectedIndexChanged);
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(6, 12);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(66, 13);
			this.label18.TabIndex = 50;
			this.label18.Text = "Output Type";
			// 
			// UriHelper
			// 
			this.UriHelper.Location = new System.Drawing.Point(7, 36);
			this.UriHelper.Name = "UriHelper";
			this.UriHelper.Size = new System.Drawing.Size(126, 23);
			this.UriHelper.TabIndex = 51;
			this.UriHelper.Text = "URL Helper";
			this.UriHelper.UseVisualStyleBackColor = true;
			this.UriHelper.Click += new System.EventHandler(this.UriHelper_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.VideoTab);
			this.tabControl1.Controls.Add(this.AudioTab);
			this.tabControl1.Controls.Add(this.BitratesTab);
			this.tabControl1.Controls.Add(this.OutputTab);
			this.tabControl1.Location = new System.Drawing.Point(12, 99);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(577, 360);
			this.tabControl1.TabIndex = 52;
			// 
			// VideoTab
			// 
			this.VideoTab.Controls.Add(this.label3);
			this.VideoTab.Controls.Add(this.label6);
			this.VideoTab.Controls.Add(this.CaptureWidth);
			this.VideoTab.Controls.Add(this.CaptureHeight);
			this.VideoTab.Controls.Add(this.BalanceAspectButton);
			this.VideoTab.Controls.Add(this.label5);
			this.VideoTab.Controls.Add(this.label7);
			this.VideoTab.Controls.Add(this.VideoDevicePreview);
			this.VideoTab.Controls.Add(this.label10);
			this.VideoTab.Controls.Add(this.label14);
			this.VideoTab.Controls.Add(this.TestVideoSettings);
			this.VideoTab.Controls.Add(this.FrameRate);
			this.VideoTab.Controls.Add(this.OutputHeight);
			this.VideoTab.Controls.Add(this.VideoBitrate);
			this.VideoTab.Controls.Add(this.label9);
			this.VideoTab.Controls.Add(this.label8);
			this.VideoTab.Controls.Add(this.label15);
			this.VideoTab.Controls.Add(this.OutputWidth);
			this.VideoTab.Controls.Add(this.VideoDeviceProps);
			this.VideoTab.Controls.Add(this.FragmentSize);
			this.VideoTab.Controls.Add(this.VideoDeviceMenu);
			this.VideoTab.Controls.Add(this.label16);
			this.VideoTab.Location = new System.Drawing.Point(4, 22);
			this.VideoTab.Name = "VideoTab";
			this.VideoTab.Padding = new System.Windows.Forms.Padding(3);
			this.VideoTab.Size = new System.Drawing.Size(569, 334);
			this.VideoTab.TabIndex = 0;
			this.VideoTab.Text = "Video";
			this.VideoTab.UseVisualStyleBackColor = true;
			// 
			// AudioTab
			// 
			this.AudioTab.Controls.Add(this.label4);
			this.AudioTab.Controls.Add(this.label12);
			this.AudioTab.Controls.Add(this.AudioCaptureRate);
			this.AudioTab.Controls.Add(this.label13);
			this.AudioTab.Controls.Add(this.AudioPreviewButton);
			this.AudioTab.Controls.Add(this.label11);
			this.AudioTab.Controls.Add(this.TestAudioSettings);
			this.AudioTab.Controls.Add(this.AudioChannelCount);
			this.AudioTab.Controls.Add(this.AudioDeviceProps);
			this.AudioTab.Controls.Add(this.AudioDeviceMenu);
			this.AudioTab.Location = new System.Drawing.Point(4, 22);
			this.AudioTab.Name = "AudioTab";
			this.AudioTab.Padding = new System.Windows.Forms.Padding(3);
			this.AudioTab.Size = new System.Drawing.Size(569, 334);
			this.AudioTab.TabIndex = 1;
			this.AudioTab.Text = "Audio";
			this.AudioTab.UseVisualStyleBackColor = true;
			// 
			// BitratesTab
			// 
			this.BitratesTab.Controls.Add(this.label19);
			this.BitratesTab.Controls.Add(this.MBRChecklist);
			this.BitratesTab.Location = new System.Drawing.Point(4, 22);
			this.BitratesTab.Name = "BitratesTab";
			this.BitratesTab.Size = new System.Drawing.Size(569, 334);
			this.BitratesTab.TabIndex = 2;
			this.BitratesTab.Text = "MBR";
			this.BitratesTab.UseVisualStyleBackColor = true;
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(11, 12);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(398, 78);
			this.label19.TabIndex = 1;
			this.label19.Text = resources.GetString("label19.Text");
			// 
			// MBRChecklist
			// 
			this.MBRChecklist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MBRChecklist.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.MBRChecklist.CheckOnClick = true;
			this.MBRChecklist.FormattingEnabled = true;
			this.MBRChecklist.Items.AddRange(new object[] {
            "80%",
            "75%",
            "70%",
            "60%",
            "50%",
            "40%",
            "35%",
            "30%",
            "25%"});
			this.MBRChecklist.Location = new System.Drawing.Point(14, 109);
			this.MBRChecklist.Name = "MBRChecklist";
			this.MBRChecklist.ScrollAlwaysVisible = true;
			this.MBRChecklist.Size = new System.Drawing.Size(540, 210);
			this.MBRChecklist.TabIndex = 0;
			// 
			// OutputTab
			// 
			this.OutputTab.Controls.Add(this.label18);
			this.OutputTab.Controls.Add(this.UriHelper);
			this.OutputTab.Controls.Add(this.LocalFilesystemOutputFolder);
			this.OutputTab.Controls.Add(this.label17);
			this.OutputTab.Controls.Add(this.OutputHandlerMenu);
			this.OutputTab.Controls.Add(this.VideoFTPRoot);
			this.OutputTab.Controls.Add(this.VideoDestinationLabel);
			this.OutputTab.Controls.Add(this.IndexFTPRoot);
			this.OutputTab.Controls.Add(this.IndexFTPRootLabel);
			this.OutputTab.Controls.Add(this.label22);
			this.OutputTab.Controls.Add(this.ServerLookupRoot);
			this.OutputTab.Controls.Add(this.IndexName);
			this.OutputTab.Controls.Add(this.label20);
			this.OutputTab.Controls.Add(this.label21);
			this.OutputTab.Controls.Add(this.FilePrefix);
			this.OutputTab.Location = new System.Drawing.Point(4, 22);
			this.OutputTab.Name = "OutputTab";
			this.OutputTab.Size = new System.Drawing.Size(569, 334);
			this.OutputTab.TabIndex = 3;
			this.OutputTab.Text = "Output";
			this.OutputTab.UseVisualStyleBackColor = true;
			// 
			// LoadedFileLabel
			// 
			this.LoadedFileLabel.AutoSize = true;
			this.LoadedFileLabel.Location = new System.Drawing.Point(95, 18);
			this.LoadedFileLabel.Name = "LoadedFileLabel";
			this.LoadedFileLabel.Size = new System.Drawing.Size(72, 13);
			this.LoadedFileLabel.TabIndex = 53;
			this.LoadedFileLabel.Text = "No file loaded";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(601, 471);
			this.Controls.Add(this.LoadedFileLabel);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.SaveSettingsButton);
			this.Controls.Add(this.LoadSettingsButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.MaximumSize = new System.Drawing.Size(2000, 736);
			this.MinimumSize = new System.Drawing.Size(617, 507);
			this.Name = "MainForm";
			this.Text = "HTTP Streaming Configuration Tool";
			this.Load += new System.EventHandler(this.MainForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.CaptureWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CaptureHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FrameRate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OutputHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OutputWidth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AudioCaptureRate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AudioChannelCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VideoBitrate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FragmentSize)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.VideoTab.ResumeLayout(false);
			this.VideoTab.PerformLayout();
			this.AudioTab.ResumeLayout(false);
			this.AudioTab.PerformLayout();
			this.BitratesTab.ResumeLayout(false);
			this.BitratesTab.PerformLayout();
			this.OutputTab.ResumeLayout(false);
			this.OutputTab.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox VideoDeviceMenu;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button VideoDeviceProps;
		private System.Windows.Forms.Button AudioDeviceProps;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox AudioDeviceMenu;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown CaptureWidth;
		private System.Windows.Forms.NumericUpDown CaptureHeight;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown FrameRate;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown OutputHeight;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.NumericUpDown OutputWidth;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown AudioCaptureRate;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.NumericUpDown AudioChannelCount;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.NumericUpDown VideoBitrate;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.NumericUpDown FragmentSize;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.TextBox LocalFilesystemOutputFolder;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label VideoDestinationLabel;
		private System.Windows.Forms.TextBox VideoFTPRoot;
		private System.Windows.Forms.Label IndexFTPRootLabel;
		private System.Windows.Forms.TextBox IndexFTPRoot;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.TextBox ServerLookupRoot;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.TextBox FilePrefix;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.TextBox IndexName;
		private System.Windows.Forms.Button LoadSettingsButton;
		private System.Windows.Forms.Button SaveSettingsButton;
		private System.Windows.Forms.OpenFileDialog LoadConfigDialog;
		private System.Windows.Forms.SaveFileDialog SaveConfigDialog;
		private System.Windows.Forms.Button TestVideoSettings;
		private System.Windows.Forms.Button TestAudioSettings;
		private System.Windows.Forms.Button VideoDevicePreview;
		private System.Windows.Forms.Button AudioPreviewButton;
		private System.Windows.Forms.Button BalanceAspectButton;
		private System.Windows.Forms.ComboBox OutputHandlerMenu;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Button UriHelper;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage VideoTab;
		private System.Windows.Forms.TabPage AudioTab;
		private System.Windows.Forms.TabPage BitratesTab;
		private System.Windows.Forms.TabPage OutputTab;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.CheckedListBox MBRChecklist;
		private System.Windows.Forms.Label LoadedFileLabel;
	}
}

