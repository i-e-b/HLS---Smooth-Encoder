using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConfigurationTool {
	public partial class UriHelper : Form {
		public UriHelper () {
			InitializeComponent();
		}

		private void SrcBox_TextChanged (object sender, EventArgs e) {
			UpdateUri();
		}

		private void UsrBox_TextChanged (object sender, EventArgs e) {
			UpdateUri();
		}

		private void PassBox_TextChanged (object sender, EventArgs e) {
			UpdateUri();
		}

		private void UpdateUri () {
			try {
				Uri src = new Uri(SrcBox.Text, UriKind.Absolute);

				/*if (UsrBox.Text.IndexOfAny(new char[] { ':', '/', '\\', '@', ';' }) >= 0) throw new Exception("Illegal characters in user name");
				if (PassBox.Text.IndexOfAny(new char[] { ':', '/', '\\', '@', ';' }) >= 0) throw new Exception("Illegal characters in password");
				*/
				OutBox.Text = src.Scheme + "://" + Uri.EscapeDataString(UsrBox.Text) + ":" + Uri.EscapeDataString(PassBox.Text) + "@"
					+ src.Authority + src.PathAndQuery;

				Uri dst = new Uri(OutBox.Text, UriKind.Absolute);
				OutBox.Text = dst.AbsoluteUri;
			} catch (Exception ex) {
				OutBox.Text = ex.Message;
			}
		}

	}
}
