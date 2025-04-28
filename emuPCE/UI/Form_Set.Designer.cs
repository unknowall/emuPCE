namespace emuPCE.UI
{
    partial class Form_Set
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnsave = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            cbscalemode = new System.Windows.Forms.ComboBox();
            label7 = new System.Windows.Forms.Label();
            tbframeidle = new System.Windows.Forms.TextBox();
            label8 = new System.Windows.Forms.Label();
            tbaudiobuffer = new System.Windows.Forms.TextBox();
            label6 = new System.Windows.Forms.Label();
            tbframeskip = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            cbmsaa = new System.Windows.Forms.ComboBox();
            label4 = new System.Windows.Forms.Label();
            cbconsole = new System.Windows.Forms.CheckBox();
            btndel = new System.Windows.Forms.Button();
            label11 = new System.Windows.Forms.Label();
            cbbios = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            chkadpcm = new System.Windows.Forms.CheckBox();
            chkfade = new System.Windows.Forms.CheckBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // btnsave
            // 
            btnsave.Location = new System.Drawing.Point(343, 263);
            btnsave.Name = "btnsave";
            btnsave.Size = new System.Drawing.Size(75, 27);
            btnsave.TabIndex = 0;
            btnsave.Text = emuPCE.Properties.Resources.Form_Set_InitializeComponent_save;
            btnsave.UseVisualStyleBackColor = true;
            btnsave.Click += btnsave_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(chkfade);
            groupBox1.Controls.Add(chkadpcm);
            groupBox1.Controls.Add(cbbios);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(cbscalemode);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(tbframeidle);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(tbaudiobuffer);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(tbframeskip);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(cbmsaa);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(cbconsole);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(411, 233);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            // 
            // cbscalemode
            // 
            cbscalemode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbscalemode.FormattingEnabled = true;
            cbscalemode.Items.AddRange(new object[] { "Neighbor", "JINC", "xBR" });
            cbscalemode.Location = new System.Drawing.Point(114, 47);
            cbscalemode.Name = "cbscalemode";
            cbscalemode.Size = new System.Drawing.Size(283, 25);
            cbscalemode.TabIndex = 20;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(16, 47);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(54, 17);
            label7.TabIndex = 19;
            label7.Text = emuPCE.Properties.Resources.Form_Set_InitializeComponent_内部分辨率放大;
            // 
            // tbframeidle
            // 
            tbframeidle.Location = new System.Drawing.Point(114, 82);
            tbframeidle.Name = "tbframeidle";
            tbframeidle.Size = new System.Drawing.Size(98, 23);
            tbframeidle.TabIndex = 13;
            tbframeidle.KeyPress += edtxt_KeyPress;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(12, 86);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(72, 17);
            label8.TabIndex = 12;
            label8.Text = emuPCE.Properties.Resources.Form_Set_InitializeComponent_limit;
            // 
            // tbaudiobuffer
            // 
            tbaudiobuffer.Location = new System.Drawing.Point(116, 115);
            tbaudiobuffer.Name = "tbaudiobuffer";
            tbaudiobuffer.Size = new System.Drawing.Size(96, 23);
            tbaudiobuffer.TabIndex = 10;
            tbaudiobuffer.KeyPress += edtxt_KeyPress;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(12, 121);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(102, 17);
            label6.TabIndex = 9;
            label6.Text = emuPCE.Properties.Resources.Form_Set_InitializeComponent_audio;
            // 
            // tbframeskip
            // 
            tbframeskip.Location = new System.Drawing.Point(331, 83);
            tbframeskip.Name = "tbframeskip";
            tbframeskip.Size = new System.Drawing.Size(66, 23);
            tbframeskip.TabIndex = 8;
            tbframeskip.KeyPress += edtxt_KeyPress;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(229, 86);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(72, 17);
            label5.TabIndex = 7;
            label5.Text = emuPCE.Properties.Resources.Form_Set_InitializeComponent_fsk;
            // 
            // cbmsaa
            // 
            cbmsaa.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbmsaa.FormattingEnabled = true;
            cbmsaa.Items.AddRange(new object[] { "None MSAA", "4xMSAA", "6xMSAA", "8xMSAA", "16xMSAA" });
            cbmsaa.Location = new System.Drawing.Point(114, 150);
            cbmsaa.Name = "cbmsaa";
            cbmsaa.Size = new System.Drawing.Size(283, 25);
            cbmsaa.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 153);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(94, 17);
            label4.TabIndex = 5;
            label4.Text = "OpenGL MSAA";
            // 
            // cbconsole
            // 
            cbconsole.AutoSize = true;
            cbconsole.Location = new System.Drawing.Point(16, 18);
            cbconsole.Name = "cbconsole";
            cbconsole.Size = new System.Drawing.Size(115, 21);
            cbconsole.TabIndex = 0;
            cbconsole.Text = emuPCE.Properties.Resources.Form_Set_InitializeComponent_con;
            cbconsole.UseVisualStyleBackColor = true;
            // 
            // btndel
            // 
            btndel.Location = new System.Drawing.Point(236, 262);
            btndel.Name = "btndel";
            btndel.Size = new System.Drawing.Size(101, 27);
            btndel.TabIndex = 5;
            btndel.Text = emuPCE.Properties.Resources.Form_Set_InitializeComponent_gbs;
            btndel.UseVisualStyleBackColor = true;
            btndel.Click += btndel_Click;
            // 
            // label11
            // 
            label11.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
            label11.Location = new System.Drawing.Point(13, 263);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(211, 23);
            label11.TabIndex = 8;
            label11.Text = emuPCE.Properties.Resources.Form_Set_InitializeComponent_不清楚作用的设置尽量不要修改;
            // 
            // cbbios
            // 
            cbbios.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbbios.FormattingEnabled = true;
            cbbios.Location = new System.Drawing.Point(114, 188);
            cbbios.Name = "cbbios";
            cbbios.Size = new System.Drawing.Size(283, 25);
            cbbios.TabIndex = 22;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 191);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(58, 17);
            label1.TabIndex = 21;
            label1.Text = "CD BIOS";
            // 
            // chkadpcm
            // 
            chkadpcm.AutoSize = true;
            chkadpcm.Location = new System.Drawing.Point(229, 117);
            chkadpcm.Name = "chkadpcm";
            chkadpcm.Size = new System.Drawing.Size(71, 21);
            chkadpcm.TabIndex = 23;
            chkadpcm.Text = "ADPCM";
            chkadpcm.UseVisualStyleBackColor = true;
            // 
            // chkfade
            // 
            chkfade.AutoSize = true;
            chkfade.Location = new System.Drawing.Point(306, 117);
            chkfade.Name = "chkfade";
            chkfade.Size = new System.Drawing.Size(57, 21);
            chkfade.TabIndex = 24;
            chkfade.Text = "FADE";
            chkfade.UseVisualStyleBackColor = true;
            // 
            // Form_Set
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(435, 303);
            Controls.Add(label11);
            Controls.Add(btndel);
            Controls.Add(groupBox1);
            Controls.Add(btnsave);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form_Set";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = emuPCE.Properties.Resources.Form_Set_InitializeComponent_设置;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnsave;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbconsole;
        private System.Windows.Forms.ComboBox cbmsaa;
        private System.Windows.Forms.TextBox tbframeskip;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbaudiobuffer;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbframeidle;
        private System.Windows.Forms.Button btndel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cbscalemode;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkfade;
        private System.Windows.Forms.CheckBox chkadpcm;
        private System.Windows.Forms.ComboBox cbbios;
        private System.Windows.Forms.Label label1;
    }
}
