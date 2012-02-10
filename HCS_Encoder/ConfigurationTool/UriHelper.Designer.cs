namespace ConfigurationTool {
	partial class UriHelper {
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
			this.SrcBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.UsrBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.PassBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.OutBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// SrcBox
			// 
			this.SrcBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SrcBox.Location = new System.Drawing.Point(16, 29);
			this.SrcBox.Name = "SrcBox";
			this.SrcBox.Size = new System.Drawing.Size(573, 20);
			this.SrcBox.TabIndex = 0;
			this.SrcBox.TextChanged += new System.EventHandler(this.SrcBox_TextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(66, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Source URL";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 72);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(58, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Username:";
			// 
			// UsrBox
			// 
			this.UsrBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.UsrBox.Location = new System.Drawing.Point(85, 69);
			this.UsrBox.Name = "UsrBox";
			this.UsrBox.Size = new System.Drawing.Size(504, 20);
			this.UsrBox.TabIndex = 2;
			this.UsrBox.TextChanged += new System.EventHandler(this.UsrBox_TextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(13, 98);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Password:";
			// 
			// PassBox
			// 
			this.PassBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.PassBox.Location = new System.Drawing.Point(85, 95);
			this.PassBox.Name = "PassBox";
			this.PassBox.Size = new System.Drawing.Size(504, 20);
			this.PassBox.TabIndex = 4;
			this.PassBox.TextChanged += new System.EventHandler(this.PassBox_TextChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(82, 118);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(276, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "Username and password may not contain any of / \\ : ; @";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(13, 155);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(62, 13);
			this.label5.TabIndex = 8;
			this.label5.Text = "Result URL";
			// 
			// OutBox
			// 
			this.OutBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OutBox.Location = new System.Drawing.Point(16, 171);
			this.OutBox.Name = "OutBox";
			this.OutBox.Size = new System.Drawing.Size(573, 20);
			this.OutBox.TabIndex = 7;
			// 
			// UriHelper
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(601, 212);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.OutBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.PassBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.UsrBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.SrcBox);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UriHelper";
			this.Text = "URL Helper";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox SrcBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox UsrBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox PassBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox OutBox;
	}
}