﻿using System.Drawing;
using System.Windows.Forms;

namespace ePceCD.UI
{
    public partial class FrmAbout : Form
    {
        public FrmAbout()
        {
            InitializeComponent();

            labver.Text = FrmMain.version;
        }

    }
}
