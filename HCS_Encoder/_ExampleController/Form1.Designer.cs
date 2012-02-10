namespace _ExampleController {
	partial class Form1 {
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.LoadConfig = new System.Windows.Forms.Button();
			this.ToggleEncoder = new System.Windows.Forms.Button();
			this.StatusTimer = new System.Windows.Forms.Timer(this.components);
			this.RunningStatus = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.ManagedMem = new System.Windows.Forms.Label();
			this.SysMem = new System.Windows.Forms.Label();
			this.EncFPS = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.tV = new System.Windows.Forms.Label();
			this.tA = new System.Windows.Forms.Label();
			this.qV = new System.Windows.Forms.Label();
			this.qA = new System.Windows.Forms.Label();
			this.OpenConfigDialog = new System.Windows.Forms.OpenFileDialog();
			this.pbox = new System.Windows.Forms.PictureBox();
			this.label8 = new System.Windows.Forms.Label();
			this.TonePct = new System.Windows.Forms.Label();
			this.tbox = new System.Windows.Forms.PictureBox();
			this.previewCheck = new System.Windows.Forms.CheckBox();
			this.infoField = new System.Windows.Forms.Label();
			this.usePlugsCheck = new System.Windows.Forms.CheckBox();
			this.PauseOutputBtn = new System.Windows.Forms.Button();
			this.ResumeOutputBtn = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pbox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbox)).BeginInit();
			this.SuspendLayout();
			// 
			// LoadConfig
			// 
			this.LoadConfig.Location = new System.Drawing.Point(12, 12);
			this.LoadConfig.Name = "LoadConfig";
			this.LoadConfig.Size = new System.Drawing.Size(190, 23);
			this.LoadConfig.TabIndex = 0;
			this.LoadConfig.Text = "Load Configuration";
			this.LoadConfig.UseVisualStyleBackColor = true;
			this.LoadConfig.Click += new System.EventHandler(this.LoadConfig_Click);
			// 
			// ToggleEncoder
			// 
			this.ToggleEncoder.Enabled = false;
			this.ToggleEncoder.Location = new System.Drawing.Point(290, 389);
			this.ToggleEncoder.Name = "ToggleEncoder";
			this.ToggleEncoder.Size = new System.Drawing.Size(190, 23);
			this.ToggleEncoder.TabIndex = 1;
			this.ToggleEncoder.Text = "Start Encoding";
			this.ToggleEncoder.UseVisualStyleBackColor = true;
			this.ToggleEncoder.Click += new System.EventHandler(this.ToggleEncoder_Click);
			// 
			// StatusTimer
			// 
			this.StatusTimer.Enabled = true;
			this.StatusTimer.Interval = 500;
			this.StatusTimer.Tick += new System.EventHandler(this.StatusTimer_Tick);
			// 
			// RunningStatus
			// 
			this.RunningStatus.AutoSize = true;
			this.RunningStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RunningStatus.Location = new System.Drawing.Point(12, 47);
			this.RunningStatus.Name = "RunningStatus";
			this.RunningStatus.Size = new System.Drawing.Size(57, 18);
			this.RunningStatus.TabIndex = 2;
			this.RunningStatus.Text = "Waiting";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(15, 109);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(95, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Managed Memory:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(15, 132);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(84, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "System Memory:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(15, 154);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(73, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Encoder FPS:";
			// 
			// ManagedMem
			// 
			this.ManagedMem.AutoSize = true;
			this.ManagedMem.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ManagedMem.Location = new System.Drawing.Point(116, 109);
			this.ManagedMem.Name = "ManagedMem";
			this.ManagedMem.Size = new System.Drawing.Size(35, 15);
			this.ManagedMem.TabIndex = 6;
			this.ManagedMem.Text = "0 MB";
			// 
			// SysMem
			// 
			this.SysMem.AutoSize = true;
			this.SysMem.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SysMem.Location = new System.Drawing.Point(116, 132);
			this.SysMem.Name = "SysMem";
			this.SysMem.Size = new System.Drawing.Size(35, 15);
			this.SysMem.TabIndex = 7;
			this.SysMem.Text = "0 MB";
			// 
			// EncFPS
			// 
			this.EncFPS.AutoSize = true;
			this.EncFPS.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EncFPS.Location = new System.Drawing.Point(116, 154);
			this.EncFPS.Name = "EncFPS";
			this.EncFPS.Size = new System.Drawing.Size(14, 15);
			this.EncFPS.TabIndex = 8;
			this.EncFPS.Text = "0";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(117, 290);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(34, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Video";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(273, 290);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(34, 13);
			this.label5.TabIndex = 10;
			this.label5.Text = "Audio";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(15, 316);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(71, 13);
			this.label6.TabIndex = 11;
			this.label6.Text = "Record Head";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(15, 341);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(75, 13);
			this.label7.TabIndex = 12;
			this.label7.Text = "Queue Length";
			// 
			// tV
			// 
			this.tV.AutoSize = true;
			this.tV.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tV.Location = new System.Drawing.Point(117, 315);
			this.tV.Name = "tV";
			this.tV.Size = new System.Drawing.Size(105, 15);
			this.tV.TabIndex = 13;
			this.tV.Text = "00:00:00.00000";
			// 
			// tA
			// 
			this.tA.AutoSize = true;
			this.tA.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tA.Location = new System.Drawing.Point(273, 315);
			this.tA.Name = "tA";
			this.tA.Size = new System.Drawing.Size(105, 15);
			this.tA.TabIndex = 14;
			this.tA.Text = "00:00:00.00000";
			// 
			// qV
			// 
			this.qV.AutoSize = true;
			this.qV.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.qV.Location = new System.Drawing.Point(117, 339);
			this.qV.Name = "qV";
			this.qV.Size = new System.Drawing.Size(14, 15);
			this.qV.TabIndex = 15;
			this.qV.Text = "0";
			// 
			// qA
			// 
			this.qA.AutoSize = true;
			this.qA.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.qA.Location = new System.Drawing.Point(273, 339);
			this.qA.Name = "qA";
			this.qA.Size = new System.Drawing.Size(14, 15);
			this.qA.TabIndex = 16;
			this.qA.Text = "0";
			// 
			// OpenConfigDialog
			// 
			this.OpenConfigDialog.AddExtension = false;
			this.OpenConfigDialog.FileName = "default.xml";
			this.OpenConfigDialog.Filter = "Config Files|*.xml|All Files|*.*";
			this.OpenConfigDialog.InitialDirectory = "C:\\temp\\";
			this.OpenConfigDialog.Title = "Select Configuration";
			this.OpenConfigDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenConfigDialog_FileOk);
			// 
			// pbox
			// 
			this.pbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pbox.Location = new System.Drawing.Point(224, 12);
			this.pbox.Name = "pbox";
			this.pbox.Size = new System.Drawing.Size(256, 192);
			this.pbox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pbox.TabIndex = 17;
			this.pbox.TabStop = false;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(227, 245);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(92, 13);
			this.label8.TabIndex = 18;
			this.label8.Text = "Tone Confidence:";
			// 
			// TonePct
			// 
			this.TonePct.AutoSize = true;
			this.TonePct.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TonePct.Location = new System.Drawing.Point(329, 244);
			this.TonePct.Name = "TonePct";
			this.TonePct.Size = new System.Drawing.Size(49, 15);
			this.TonePct.TabIndex = 19;
			this.TonePct.Text = "0.00 %";
			// 
			// tbox
			// 
			this.tbox.Location = new System.Drawing.Point(224, 210);
			this.tbox.Name = "tbox";
			this.tbox.Size = new System.Drawing.Size(256, 32);
			this.tbox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.tbox.TabIndex = 20;
			this.tbox.TabStop = false;
			// 
			// previewCheck
			// 
			this.previewCheck.AutoSize = true;
			this.previewCheck.Location = new System.Drawing.Point(12, 223);
			this.previewCheck.Name = "previewCheck";
			this.previewCheck.Size = new System.Drawing.Size(176, 17);
			this.previewCheck.TabIndex = 22;
			this.previewCheck.Text = "Show Preview (low sample rate)";
			this.previewCheck.UseVisualStyleBackColor = true;
			// 
			// infoField
			// 
			this.infoField.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
			this.infoField.Location = new System.Drawing.Point(16, 366);
			this.infoField.Name = "infoField";
			this.infoField.Size = new System.Drawing.Size(464, 20);
			this.infoField.TabIndex = 23;
			// 
			// usePlugsCheck
			// 
			this.usePlugsCheck.AutoSize = true;
			this.usePlugsCheck.Checked = true;
			this.usePlugsCheck.CheckState = System.Windows.Forms.CheckState.Checked;
			this.usePlugsCheck.Location = new System.Drawing.Point(12, 205);
			this.usePlugsCheck.Name = "usePlugsCheck";
			this.usePlugsCheck.Size = new System.Drawing.Size(85, 17);
			this.usePlugsCheck.TabIndex = 24;
			this.usePlugsCheck.Text = "Use Plug-ins";
			this.usePlugsCheck.UseVisualStyleBackColor = true;
			// 
			// PauseOutputBtn
			// 
			this.PauseOutputBtn.Location = new System.Drawing.Point(15, 389);
			this.PauseOutputBtn.Name = "PauseOutputBtn";
			this.PauseOutputBtn.Size = new System.Drawing.Size(95, 23);
			this.PauseOutputBtn.TabIndex = 25;
			this.PauseOutputBtn.Text = "Pause Output";
			this.PauseOutputBtn.UseVisualStyleBackColor = true;
			this.PauseOutputBtn.Click += new System.EventHandler(this.PauseOutputBtn_Click);
			// 
			// ResumeOutputBtn
			// 
			this.ResumeOutputBtn.Location = new System.Drawing.Point(116, 389);
			this.ResumeOutputBtn.Name = "ResumeOutputBtn";
			this.ResumeOutputBtn.Size = new System.Drawing.Size(95, 23);
			this.ResumeOutputBtn.TabIndex = 26;
			this.ResumeOutputBtn.Text = "Resume Output";
			this.ResumeOutputBtn.UseVisualStyleBackColor = true;
			this.ResumeOutputBtn.Click += new System.EventHandler(this.ResumeOutputBtn_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(492, 424);
			this.Controls.Add(this.ResumeOutputBtn);
			this.Controls.Add(this.PauseOutputBtn);
			this.Controls.Add(this.usePlugsCheck);
			this.Controls.Add(this.infoField);
			this.Controls.Add(this.previewCheck);
			this.Controls.Add(this.tbox);
			this.Controls.Add(this.TonePct);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.pbox);
			this.Controls.Add(this.qA);
			this.Controls.Add(this.qV);
			this.Controls.Add(this.tA);
			this.Controls.Add(this.tV);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.EncFPS);
			this.Controls.Add(this.SysMem);
			this.Controls.Add(this.ManagedMem);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.RunningStatus);
			this.Controls.Add(this.ToggleEncoder);
			this.Controls.Add(this.LoadConfig);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(508, 460);
			this.MinimumSize = new System.Drawing.Size(508, 460);
			this.Name = "Form1";
			this.Text = "HCS Encoder Demo";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.pbox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button LoadConfig;
		private System.Windows.Forms.Button ToggleEncoder;
		private System.Windows.Forms.Timer StatusTimer;
		private System.Windows.Forms.Label RunningStatus;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label ManagedMem;
		private System.Windows.Forms.Label SysMem;
		private System.Windows.Forms.Label EncFPS;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label tV;
		private System.Windows.Forms.Label tA;
		private System.Windows.Forms.Label qV;
		private System.Windows.Forms.Label qA;
		private System.Windows.Forms.OpenFileDialog OpenConfigDialog;
		private System.Windows.Forms.PictureBox pbox;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label TonePct;
		private System.Windows.Forms.PictureBox tbox;
		private System.Windows.Forms.CheckBox previewCheck;
		private System.Windows.Forms.Label infoField;
		private System.Windows.Forms.CheckBox usePlugsCheck;
		private System.Windows.Forms.Button PauseOutputBtn;
		private System.Windows.Forms.Button ResumeOutputBtn;
	}
}

