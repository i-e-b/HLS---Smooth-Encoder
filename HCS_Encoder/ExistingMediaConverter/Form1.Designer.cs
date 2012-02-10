namespace ExistingMediaConverter {
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
			this.SourceFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.SourceLabel = new System.Windows.Forms.Label();
			this.ChooseSourceButton = new System.Windows.Forms.Button();
			this.ChooseDestinationButton = new System.Windows.Forms.Button();
			this.ConvertButton = new System.Windows.Forms.Button();
			this.ConfigFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// SourceFileDialog
			// 
			this.SourceFileDialog.FileName = "source_file.avi";
			this.SourceFileDialog.Filter = "All files|*.*";
			this.SourceFileDialog.Title = "Select source media file";
			this.SourceFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.SourceFileDialog_FileOk);
			// 
			// SourceLabel
			// 
			this.SourceLabel.AutoEllipsis = true;
			this.SourceLabel.Location = new System.Drawing.Point(13, 13);
			this.SourceLabel.Name = "SourceLabel";
			this.SourceLabel.Size = new System.Drawing.Size(325, 64);
			this.SourceLabel.TabIndex = 0;
			this.SourceLabel.Text = "Media file converter.\r\n1) Choose a source media file\r\n2) Save to an output destin" +
				"ation\r\n3) Click \"Convert Now\"";
			// 
			// ChooseSourceButton
			// 
			this.ChooseSourceButton.Location = new System.Drawing.Point(16, 80);
			this.ChooseSourceButton.Name = "ChooseSourceButton";
			this.ChooseSourceButton.Size = new System.Drawing.Size(155, 23);
			this.ChooseSourceButton.TabIndex = 1;
			this.ChooseSourceButton.Text = "Choose Source";
			this.ChooseSourceButton.UseVisualStyleBackColor = true;
			this.ChooseSourceButton.Click += new System.EventHandler(this.ChooseSourceButton_Click);
			// 
			// ChooseDestinationButton
			// 
			this.ChooseDestinationButton.Enabled = false;
			this.ChooseDestinationButton.Location = new System.Drawing.Point(183, 80);
			this.ChooseDestinationButton.Name = "ChooseDestinationButton";
			this.ChooseDestinationButton.Size = new System.Drawing.Size(155, 23);
			this.ChooseDestinationButton.TabIndex = 2;
			this.ChooseDestinationButton.Text = "Choose Configuration";
			this.ChooseDestinationButton.UseVisualStyleBackColor = true;
			this.ChooseDestinationButton.Click += new System.EventHandler(this.ChooseDestinationButton_Click);
			// 
			// ConvertButton
			// 
			this.ConvertButton.Enabled = false;
			this.ConvertButton.Location = new System.Drawing.Point(16, 109);
			this.ConvertButton.Name = "ConvertButton";
			this.ConvertButton.Size = new System.Drawing.Size(322, 23);
			this.ConvertButton.TabIndex = 3;
			this.ConvertButton.Text = "Convert Now";
			this.ConvertButton.UseVisualStyleBackColor = true;
			this.ConvertButton.Click += new System.EventHandler(this.ConvertButton_Click);
			// 
			// ConfigFileDialog
			// 
			this.ConfigFileDialog.FileName = "Choose configuration";
			this.ConfigFileDialog.Filter = "Config Files|*.xml";
			this.ConfigFileDialog.Title = "Choose configuration";
			this.ConfigFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.DestFileDialog_FileOk);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(350, 144);
			this.Controls.Add(this.ConvertButton);
			this.Controls.Add(this.ChooseDestinationButton);
			this.Controls.Add(this.ChooseSourceButton);
			this.Controls.Add(this.SourceLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "Form1";
			this.Text = "Media File to HCS Converter";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog SourceFileDialog;
		private System.Windows.Forms.Label SourceLabel;
		private System.Windows.Forms.Button ChooseSourceButton;
		private System.Windows.Forms.Button ChooseDestinationButton;
		private System.Windows.Forms.Button ConvertButton;
		private System.Windows.Forms.OpenFileDialog ConfigFileDialog;
	}
}

