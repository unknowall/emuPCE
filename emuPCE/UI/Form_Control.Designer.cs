namespace emuPCE.UI
{
    partial class FrmInput
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
            U = new System.Windows.Forms.Button();
            L = new System.Windows.Forms.Button();
            R = new System.Windows.Forms.Button();
            D = new System.Windows.Forms.Button();
            SELE = new System.Windows.Forms.Button();
            START = new System.Windows.Forms.Button();
            A = new System.Windows.Forms.Button();
            B = new System.Windows.Forms.Button();
            BtnSave = new System.Windows.Forms.Button();
            plwait = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            cbcon = new System.Windows.Forms.ComboBox();
            cbmode = new System.Windows.Forms.ComboBox();
            plwait.SuspendLayout();
            SuspendLayout();
            // 
            // U
            // 
            U.Location = new System.Drawing.Point(43, 103);
            U.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            U.Name = "U";
            U.Size = new System.Drawing.Size(54, 25);
            U.TabIndex = 6;
            U.Text = "U:  W";
            U.UseVisualStyleBackColor = true;
            U.Click += U_Click;
            // 
            // L
            // 
            L.Location = new System.Drawing.Point(18, 133);
            L.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            L.Name = "L";
            L.Size = new System.Drawing.Size(54, 25);
            L.TabIndex = 7;
            L.Text = "L:  A";
            L.UseVisualStyleBackColor = true;
            L.Click += L_Click;
            // 
            // R
            // 
            R.Location = new System.Drawing.Point(76, 133);
            R.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            R.Name = "R";
            R.Size = new System.Drawing.Size(54, 25);
            R.TabIndex = 8;
            R.Text = "R: D";
            R.UseVisualStyleBackColor = true;
            R.Click += R_Click;
            // 
            // D
            // 
            D.Location = new System.Drawing.Point(43, 162);
            D.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            D.Name = "D";
            D.Size = new System.Drawing.Size(54, 25);
            D.TabIndex = 9;
            D.Text = "D: S";
            D.UseVisualStyleBackColor = true;
            D.Click += D_Click;
            // 
            // SELE
            // 
            SELE.Location = new System.Drawing.Point(163, 77);
            SELE.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            SELE.Name = "SELE";
            SELE.Size = new System.Drawing.Size(54, 25);
            SELE.TabIndex = 10;
            SELE.Text = "Sel:e 2";
            SELE.UseVisualStyleBackColor = true;
            SELE.Click += SELE_Click;
            // 
            // START
            // 
            START.Location = new System.Drawing.Point(105, 77);
            START.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            START.Name = "START";
            START.Size = new System.Drawing.Size(54, 25);
            START.TabIndex = 11;
            START.Text = "Start: 1";
            START.UseVisualStyleBackColor = true;
            START.Click += START_Click;
            // 
            // A
            // 
            A.Location = new System.Drawing.Point(199, 133);
            A.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            A.Name = "A";
            A.Size = new System.Drawing.Size(64, 25);
            A.TabIndex = 12;
            A.Text = "A: J";
            A.UseVisualStyleBackColor = true;
            A.Click += A_Click;
            // 
            // B
            // 
            B.Location = new System.Drawing.Point(268, 133);
            B.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            B.Name = "B";
            B.Size = new System.Drawing.Size(61, 25);
            B.TabIndex = 14;
            B.Text = "B: I";
            B.UseVisualStyleBackColor = true;
            B.Click += B_Click;
            // 
            // BtnSave
            // 
            BtnSave.Location = new System.Drawing.Point(135, 197);
            BtnSave.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            BtnSave.Name = "BtnSave";
            BtnSave.Size = new System.Drawing.Size(73, 25);
            BtnSave.TabIndex = 16;
            BtnSave.Text = Properties.Resources.FrmInput_InitializeComponent_保存设置;
            BtnSave.UseVisualStyleBackColor = true;
            BtnSave.Click += BtnSave_Click;
            // 
            // plwait
            // 
            plwait.Controls.Add(label1);
            plwait.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            plwait.Location = new System.Drawing.Point(81, 65);
            plwait.Name = "plwait";
            plwait.Size = new System.Drawing.Size(171, 85);
            plwait.TabIndex = 17;
            plwait.Visible = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(1, 32);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(236, 22);
            label1.TabIndex = 0;
            label1.Text = "Press any key, ESC to cancel";
            // 
            // cbcon
            // 
            cbcon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbcon.FormattingEnabled = true;
            cbcon.Items.AddRange(new object[] { "Controller 1", "Controller 2" });
            cbcon.Location = new System.Drawing.Point(18, 6);
            cbcon.Name = "cbcon";
            cbcon.Size = new System.Drawing.Size(139, 25);
            cbcon.TabIndex = 18;
            cbcon.SelectedIndexChanged += cbcon_SelectedIndexChanged;
            // 
            // cbmode
            // 
            cbmode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbmode.FormattingEnabled = true;
            cbmode.Items.AddRange(new object[] { "KeyBoard" });
            cbmode.Location = new System.Drawing.Point(184, 6);
            cbmode.Name = "cbmode";
            cbmode.Size = new System.Drawing.Size(136, 25);
            cbmode.TabIndex = 19;
            // 
            // FrmInput
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(338, 232);
            Controls.Add(cbmode);
            Controls.Add(cbcon);
            Controls.Add(plwait);
            Controls.Add(BtnSave);
            Controls.Add(B);
            Controls.Add(A);
            Controls.Add(START);
            Controls.Add(SELE);
            Controls.Add(D);
            Controls.Add(R);
            Controls.Add(L);
            Controls.Add(U);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            MaximizeBox = false;
            Name = "FrmInput";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Keyboard Config";
            Shown += FrmInput_Shown;
            KeyUp += FrmInput_KeyUp;
            plwait.ResumeLayout(false);
            plwait.PerformLayout();
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button U;
        private System.Windows.Forms.Button L;
        private System.Windows.Forms.Button R;
        private System.Windows.Forms.Button D;
        private System.Windows.Forms.Button SELE;
        private System.Windows.Forms.Button START;
        private System.Windows.Forms.Button A;
        private System.Windows.Forms.Button B;
        private System.Windows.Forms.Button BtnSave;
        private System.Windows.Forms.Panel plwait;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbcon;
        private System.Windows.Forms.ComboBox cbmode;
    }
}
