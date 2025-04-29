﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ePceCD.UI
{
    public partial class Form_Cheat : Form
    {
        public List<PCECore.CheatCode> cheatCodes = new List<PCECore.CheatCode> { };

        private string DiskID;

        public Form_Cheat(string id)
        {
            InitializeComponent();

            ctb.LostFocus += updatecodes;

            if (id == "")
            {
                btnsave.Enabled = false;
                btnload.Enabled = false;
                btnapply.Enabled = false;
            }
            DiskID = id;

            this.Text = $"  {DiskID}  {ePceCD.Properties.Resources.Form_Cheat_Form_Cheat_的金手指}";

            btnload_Click(this, null);

            if (clb.Items.Count > 0)
                clb.Items[0].Selected = true;
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            var item = clb.Items.Add(ePceCD.Properties.Resources.Form_Cheat_btnadd_Click_新建);
            item.SubItems.Add("");
        }

        private void btndel_Click(object sender, EventArgs e)
        {
            if (clb.SelectedItems.Count == 0)
                return;
            clb.Items.Remove(clb.SelectedItems[0]);
            ctb.Clear();
        }

        private void updatecodes(object sender, EventArgs e)
        {
            if (clb.SelectedItems.Count == 0)
                return;

            clb.SelectedItems[0].SubItems[1].Text = ctb.Text;
        }

        private void updateclbs()
        {
            ctb.Clear();
            clb.Items.Clear();
            foreach (var item in cheatCodes)
            {
                var clbitem = clb.Items.Add(item.Name);
                clbitem.Checked = item.Active;
                string codes = "";
                foreach (var sitem in item.Item)
                {
                    codes += $"{sitem.Address:X8} {sitem.Value:X4}\r\n";
                }
                clbitem.SubItems.Add(codes);
            }
        }

        private void btnload_Click(object sender, EventArgs e)
        {
            cheatCodes.Clear();
            string fn = "./Cheats/" + DiskID + ".txt";
            if (!File.Exists(fn))
                return;
            cheatCodes = PCECore.ParseTextToCheatCodeList(fn);

            updateclbs();
        }

        private void btnimp_Click(object sender, EventArgs e)
        {
            OpenFileDialog FD = new OpenFileDialog();

            FD.Filter = "CheatCodes|*.txt;*.cht";
            FD.ShowDialog();
            if (!File.Exists(FD.FileName) || FD.FileName == "")
                return;

            cheatCodes.Clear();
            try
            {
                cheatCodes = PCECore.ParseTextToCheatCodeList(FD.FileName);
            }
            catch
            {
                return;
            }
            updateclbs();
        }

        private string GetText()
        {
            string ret = "";
            for (int i = 0; i < clb.Items.Count; i++)
            {
                var item = clb.Items[i];
                ret += "\r\n[" + item.Text + "]\r\n";
                ret += "Active = ";
                if (item.Checked)
                    ret += "1\r\n";
                else
                    ret += "0\r\n";
                ret += item.SubItems[1].Text;
            }
            return ret;
        }

        private void btnapply_Click(object sender, EventArgs e)
        {
            if (FrmMain.Core == null)
                return;

            btnsave_Click(sender, e);

            FrmMain.Core.LoadCheats();
        }

        private void btnsave_Click(object sender, EventArgs e)
        {
            string fn = "./Cheats/" + DiskID + ".txt";
            string txt = GetText();

            File.WriteAllText(fn, txt);
        }

        private void clb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (clb.SelectedItems.Count == 0)
                return;

            ctb.Text = "";
            if (clb.SelectedItems[0].SubItems.Count >= 2)
            {
                var sub = clb.SelectedItems[0].SubItems[1];
                ctb.Text += sub.Text;
            }
        }

        private void clb_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            int width = clb.ClientSize.Width;

            e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
            e.Graphics.DrawString(e.Item.Text, clb.Font, SystemBrushes.WindowText, e.Bounds);

            e.DrawFocusRectangle();
        }

        private string ConvertToBracketedFormat(string input)
        {
            string[] lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> outputLines = new List<string>();

            foreach (string line in lines)
            {
                if (line.TrimStart().StartsWith("#"))
                {
                    string content = line.Substring(line.IndexOf('#') + 1).Trim(); // 去掉 # 并去除多余空格
                    outputLines.Add($"[{content}]");
                    outputLines.Add("Active = 0");
                }
                else
                {
                    outputLines.Add(line.Trim());
                }
            }
            return string.Join(Environment.NewLine, outputLines);
        }

    }

}
