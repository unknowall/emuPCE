using System.Windows.Forms;

namespace ePceCD.UI
{
    public partial class FrmAbout : Form
    {
        public FrmAbout()
        {
            InitializeComponent();

            labver.Text = FrmMain.version;
            label3.Text = $"{ePceCD.Properties.Resources.FrmAbout_InitializeComponent_read}\r\n\r\n{ePceCD.Properties.Resources.FrmAbout_InitializeComponent_read2}\r\n";
        }

        private void Link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = linkLabel1.Text,
                    UseShellExecute = true
                });
            }
            catch
            {

            }
        }

    }
}
