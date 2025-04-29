using ePceCD.Render;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static SDL2.SDL;

namespace ePceCD.UI
{

    public partial class FrmMain : Form, IAudioHandler, IRenderHandler
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        public static string version = "ePceCD Beta 0.0.2";

        private static string mypath = Application.StartupPath;
        public static IniFile ini = new IniFile(mypath + "ePceCD.ini");
        private string currbios;
        private string shaderpath;
        private int StatusDelay;
        private string gamename;

        private int StateSlot;
        private int CoreWidth, CoreHeight;

        public static PCECore Core;

        private nint controller1, controller2;
        private bool KeyFirst;
        private int concount = 0;

        private int _frameCount = 0;
        private Stopwatch _fpsStopwatch = Stopwatch.StartNew();
        private float _currentFps = 0;

        private static uint audiodeviceid;
        private SDL_AudioCallback audioCallbackDelegate;
        private CircularBuffer<byte> SamplesBuffer;

        private System.Windows.Forms.Timer timer;

        public ScaleParam scale;
        public int IRscale = 1;
        public bool bFullScreen;

        private RomList romList;

        RenderMode Rendermode = RenderMode.OpenGL;

        RendererManager Render = new RendererManager();

        public FrmMain()
        {
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            InitializeComponent();

            if (ini.ReadInt("Main", "Console") == 1)
            {
                AllocConsole();
            }

            //CheckForIllegalCrossThreadCalls = false;

            KeyDown += new KeyEventHandler(ButtonsDown);
            KeyUp += new KeyEventHandler(ButtonsUp);

            if (!Path.Exists("./Save"))
                Directory.CreateDirectory("./Save");
            if (!Path.Exists("./BIOS"))
                Directory.CreateDirectory("./BIOS");
            if (!Path.Exists("./Cheats"))
                Directory.CreateDirectory("./Cheats");
            if (!Path.Exists("./SaveState"))
                Directory.CreateDirectory("./SaveState");
            if (!Path.Exists("./Shaders"))
                Directory.CreateDirectory("./Shaders");
            if (!Path.Exists("./Icons"))
                Directory.CreateDirectory("./Icons");

            CloseRomMnu_Click(null, null);

            shaderpath = ini.Read("main", "shader");
            Rendermode = (RenderMode)ini.ReadInt("Main", "Render");

            openGLRender.Checked = Rendermode == RenderMode.OpenGL;
            directx2DRender.Checked = Rendermode == RenderMode.Directx2D;
            directx3DRender.Checked = Rendermode == RenderMode.Directx3D;
            VulkanRenderMnu.Checked = Rendermode == RenderMode.Vulkan;

            switch (ini.ReadInt("OpenGL", "MSAA"))
            {
                case 0:
                    Render.oglMSAA = 0;
                    break;
                case 1:
                    Render.oglMSAA = 4;
                    break;
                case 2:
                    Render.oglMSAA = 6;
                    break;
                case 3:
                    Render.oglMSAA = 8;
                    break;
                case 4:
                    Render.oglMSAA = 16;
                    break;
            }
            Render.frameskip = ini.ReadInt("main", "skipframe");
            Render.oglShaderPath = ("./Shaders/" + shaderpath);

            frameskipmnu.Checked = ini.ReadInt("main", "frameskip") == 1;

            KeyFirst = ini.ReadInt("main", "keyfirst") == 1;

            currbios = ini.Read("main", "bios");

            FrmInput.InitKeyMap();
            FrmInput.InitControllerMap();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Elapsed;
            timer.Enabled = true;
            timer.Start();

            SDLInit();

            InitShaderMnu();

            BackColor = Color.Black;
            StatusBar.BackColor = Color.White;
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_Closing;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string[] PosStr = ini.Read("Main", "FromPos").Split('|');
            if (PosStr.Length >= 4)
            {
                this.Location = new Point(Convert.ToInt16(PosStr[0]), Convert.ToInt16(PosStr[1]));
                this.Size = new Size(Convert.ToInt16(PosStr[2]), Convert.ToInt16(PosStr[3]));
                Rectangle screenBounds = Screen.PrimaryScreen.WorkingArea;
                if (!screenBounds.Contains(this.Bounds))
                {
                    this.Location = new Point(0, 0);
                }
            }
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            string PosStr = $"{this.Location.X}|{this.Location.Y}|{this.Size.Width}|{this.Size.Height}";
            ini.Write("Main", "FromPos", PosStr);
        }

        private void UpdateStatus(int index, string text, bool clean = false)
        {
            if (index < 0 || index >= StatusBar.Items.Count)
                return;
            if (clean)
                foreach (ToolStripItem item in StatusBar.Items)
                {
                    item.Text = "";
                }
            StatusBar.Items[index].Text = text;
        }

        private unsafe void Timer_Elapsed(object sender, EventArgs e)
        {
            CheckController();

            if (Core == null)
            {
                this.Text = version;
                return;
            }

            if (StatusDelay > 0)
            {
                StatusDelay--;
            }
            else
            {
                UpdateStatus(1, $"F3+ F4- {ePceCD.Properties.Resources.FrmMain_Timer_Elapsed_存档槽} [{StateSlot}]");
                UpdateStatus(2, $"F9[{(KeyFirst ? ePceCD.Properties.Resources.FrmMain_Timer_Elapsed_键盘优先 : ePceCD.Properties.Resources.FrmMain_Timer_Elapsed_手柄优先)}]");
                UpdateStatus(3, "");
            }

            if (Core.Pauseed)
            {
                UpdateStatus(1, ePceCD.Properties.Resources.FrmMain_Timer_Elapsed_暂停中, true);
            }

            this.Text = $"{version}  -  {gamename}";

            int scalew = CoreWidth;
            int scaleh = CoreHeight;

            string rendername = Rendermode.ToString();
            if (Rendermode == RenderMode.OpenGL)
            {
                if (Render.oglMSAA > 0)
                    rendername += $" {Render.oglMSAA}xMSAA";
            }
            if (scale.scale > 0)
            {
                scalew *= scale.scale;
                scaleh *= scale.scale;
            }
            UpdateStatus(0, Core.GameID);
            UpdateStatus(5, $"{rendername}");
            UpdateStatus(6, $"{(scale.scale > 0 ? scale.mode.ToString() : "")} {scalew}*{scaleh}");
            UpdateStatus(7, $"FPS {_currentFps:F1}");
        }

        ~FrmMain()
        {
            if (Core != null)
                Core.Stop();
            if (audiodeviceid != 0)
                SDL_CloseAudioDevice(audiodeviceid);
            if (controller1 != 0)
            {
                SDL_GameControllerClose(controller1);
                SDL_JoystickClose(controller1);
            }
            SDL_Quit();
        }

        private void SDLInit()
        {
            SDL_Init(SDL_INIT_AUDIO | SDL_INIT_GAMECONTROLLER);

            if (File.Exists("./ControllerDB.txt"))
            {
                Console.WriteLine("Load ControllerMappings...");
                SDL_GameControllerAddMappingsFromFile("./ControllerDB.txt");
            }

            audioCallbackDelegate = AudioCallbackImpl;

            SDL_AudioSpec desired = new SDL_AudioSpec
            {
                channels = 2,
                format = AUDIO_S16LSB,
                freq = 44100,
                samples = 2048,
                callback = audioCallbackDelegate,
                userdata = IntPtr.Zero

            };
            SDL_AudioSpec obtained = new SDL_AudioSpec();

            SetAudioBuffer();

            audiodeviceid = SDL_OpenAudioDevice(null, 0, ref desired, out obtained, 0);
            if (audiodeviceid != 0)
                SDL_PauseAudioDevice(audiodeviceid, 0);
        }

        private void SetAudioBuffer()
        {
            if (audiodeviceid != 0)
                SDL_PauseAudioDevice(audiodeviceid, 1);

            int bufms = ini.ReadInt("Audio", "Buffer");
            if (bufms < 50)
                bufms = 50;

            int alignedSize = ((bufms * 176 + 2048 - 1) / 2048) * 2048;

            SamplesBuffer = new CircularBuffer<byte>(alignedSize); // 300 ms = 52920

            if (audiodeviceid != 0)
                SDL_PauseAudioDevice(audiodeviceid, 0);
        }

        #region MENU
        private void InitShaderMnu()
        {
            if (shaderpath == null)
                shaderpath = "Base";

            DirectoryInfo dir2 = new DirectoryInfo(mypath + "/Shaders");
            foreach (DirectoryInfo f in dir2.GetDirectories())
            {
                ToolStripMenuItem mnu = new ToolStripMenuItem();
                mnu.Name = f.Name;
                mnu.Text = f.Name;
                mnu.Tag = 20;
                mnu.CheckOnClick = true;
                mnu.Click += Mnu_Click;
                mnu.CheckedChanged += Mnu_CheckedChanged;
                openGLRender.DropDownItems.Add(mnu);
                if (shaderpath == f.Name)
                {
                    mnu.Checked = true;
                    shaderpath = mnu.Text;
                }
            }
        }

        private void InitStateMnu()
        {
            SaveStripMenuItem.DropDownItems.Clear();
            LoadStripMenuItem.DropDownItems.Clear();

            SaveStripMenuItem.Enabled = true;
            LoadStripMenuItem.Enabled = true;
            UnLoadStripMenuItem.Enabled = true;

            StateSlot = ini.ReadInt("main", "StateSlot");

            string statefile, statename;

            for (int i = 0; i < 10; i++)
            {
                ToolStripMenuItem mnusave = new ToolStripMenuItem();
                ToolStripMenuItem mnuload = new ToolStripMenuItem();

                statefile = mypath + "/SaveState/" + Core.GameID + "_Save" + i.ToString() + ".dat";
                if (File.Exists(statefile))
                {
                    statename = i.ToString() + " - " + File.GetLastWriteTime(statefile).ToLocalTime();
                }
                else
                {
                    statename = i.ToString() + " - None";
                    mnuload.Enabled = false;
                }
                if (i == StateSlot)
                    mnusave.Checked = true;
                mnusave.Name = statefile;
                mnusave.Text = statename;
                mnusave.Tag = 30 + i;
                mnusave.CheckOnClick = true;
                mnusave.Click += Mnu_Click;
                mnusave.CheckedChanged += Mnu_CheckedChanged;
                SaveStripMenuItem.DropDownItems.Add(mnusave);

                mnuload.Name = statefile;
                mnuload.Text = statename;
                mnuload.Tag = 40 + i;
                mnuload.Click += Mnu_Click;
                LoadStripMenuItem.DropDownItems.Add(mnuload);
            }
        }

        private ToolStripMenuItem AddMenu(string name, string Text, object tag, ToolStripMenuItem Parent, bool chk = false, bool ch = true)
        {
            ToolStripMenuItem mnu = new ToolStripMenuItem();
            mnu.Text = Text;
            mnu.Name = name;
            mnu.CheckOnClick = ch;
            mnu.Checked = chk;
            mnu.Tag = tag;
            mnu.Click += Mnu_Click;
            if (ch)
            {
                mnu.CheckedChanged += Mnu_CheckedChanged;
            }
            Parent.DropDownItems.Add(mnu);

            return mnu;
        }

        private void Mnu_CheckedChanged(object sender, EventArgs e)
        {
            var mnu = (ToolStripMenuItem)sender;
            var parent = mnu.GetCurrentParent();
            if (!mnu.Checked || parent == null)
                return;
            switch (mnu.Tag)
            {
                case 20:
                    shaderpath = mnu.Text;
                    ini.Write("main", "shader", shaderpath);
                    break;
            }
            foreach (ToolStripMenuItem item in parent.Items)
            {
                if (item != null && item != mnu && item.Checked)
                {
                    item.Checked = false;
                    return;
                }
            }
        }

        private void Mnu_Click(object sender, EventArgs e)
        {
            var mnu = (ToolStripMenuItem)sender;

            if ((int)mnu.Tag >= 30 && (int)mnu.Tag < 40)
            {
                StateSlot = (int)mnu.Tag - 30;
                SaveState(StateSlot);

                Mnu_CheckedChanged(sender, e);

                ini.WriteInt("main", "StateSlot", StateSlot);
                string statefile = mypath + "/SaveState/" + Core.GameID + "_Save" + StateSlot.ToString() + ".dat";
                string statename = StateSlot.ToString() + " - " + File.GetLastWriteTime(statefile).ToLocalTime();

                mnu.Text = statename;
                foreach (ToolStripMenuItem item in LoadStripMenuItem.DropDownItems)
                {
                    if ((int)item.Tag == 40 + StateSlot)
                    {
                        item.Text = statename;
                        item.Enabled = true;
                        break;
                    }
                }
                return;
            }
            if ((int)mnu.Tag >= 40 && (int)mnu.Tag < 50)
            {
                StateSlot = (int)mnu.Tag - 40;
                LoadState(StateSlot);

                return;
            }
        }

        private void fullScreenF2_Click(object sender, EventArgs e)
        {
            if (Core == null)
            {
                return;
            }

            bFullScreen = !bFullScreen;

            if (bFullScreen)
            {
                this.MainMenu.Visible = false;
                this.StatusBar.Visible = false;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.MainMenu.Visible = true;
                this.StatusBar.Visible = true;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void SearchMnu_Click(object sender, EventArgs e)
        {
            if (Core != null)
            {
                return;
            }
            if (romList != null)
            {
                OpenFileDialog FD = new OpenFileDialog();
                FD.InitialDirectory = ini.Read("main", "LastPath");
                FD.ValidateNames = false;
                FD.CheckFileExists = false;
                FD.CheckPathExists = true;
                FD.FileName = "select";
                if (FD.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
                if (FD.FileName != "")
                    romList.SearchDir(Path.GetDirectoryName(FD.FileName));
            }
        }

        private void CloseRomMnu_Click(object sender, EventArgs e)
        {
            if (Core != null)
            {
                Core.Stop();
                Core.Dispose();
                Core = null;
            }

            romList = new RomList();
            romList.Parent = this;
            romList.Dock = DockStyle.Fill;
            Controls.Add(romList);
            romList.Enabled = true;
            romList.Visible = true;
            romList.BorderStyle = BorderStyle.FixedSingle;
            romList.DoubleClick += new EventHandler(romList_DoubleClick);
            romList.FillByini();
            romList.BringToFront();

            Render.DisposeCurrentRenderer();

            if (audiodeviceid != 0)
                SDL_PauseAudioDevice(audiodeviceid, 1);

            UpdateStatus(0, "", true);
        }

        private void SeleRender(RenderMode mode)
        {
            Rendermode = mode;
            ini.WriteInt("Main", "Render", (int)Rendermode);

            if (Core != null && Core.Running)
            {
                Core.WaitPaused();

                Render.SelectRenderer(Rendermode, this);

                Core.Pauseing = false;
            }
        }

        private void directx2DRender_Click(object sender, EventArgs e)
        {
            directx3DRender.Checked = false;
            openGLRender.Checked = false;
            VulkanRenderMnu.Checked = false;

            SeleRender(RenderMode.Directx2D);
        }

        private void directx3DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openGLRender.Checked = false;
            directx2DRender.Checked = false;
            VulkanRenderMnu.Checked = false;

            SeleRender(RenderMode.Directx3D);
        }

        private void openGLToolStripMenuItem_Click(object sender, EventArgs e)
        {

            directx2DRender.Checked = false;
            directx3DRender.Checked = false;
            VulkanRenderMnu.Checked = false;

            Render.oglShaderPath = "./Shaders/" + shaderpath;

            SeleRender(RenderMode.OpenGL);
        }

        private void VulkanRenderMnu_Click(object sender, EventArgs e)
        {
            openGLRender.Checked = false;
            directx2DRender.Checked = false;
            directx3DRender.Checked = false;

            SeleRender(RenderMode.Vulkan);
        }

        private void frameskipmnu_CheckedChanged(object sender, EventArgs e)
        {
            if (Rendermode != RenderMode.OpenGL && Render._currentRenderer != null)
            {
                if (frameskipmnu.Checked == false)
                    Render._currentRenderer.SetParam(0);
                else
                    Render._currentRenderer.SetParam(ini.ReadInt("Main", "SkipFrame"));
            }

            ini.WriteInt("main", "frameskip", Convert.ToInt16(frameskipmnu.Checked));
        }

        private void LoadDisk_Click(object sender, EventArgs e)
        {
            LoadRom();
        }

        private void romList_DoubleClick(object sender, EventArgs e)
        {
            var Game = romList.SelectedGame();
            if (Game != null)
            {
                LoadRom(Game.fullName, Game);
            }
        }

        private void xBRScaleAdd_Click(object sender, EventArgs e)
        {
            if (Core != null && Core.Running)
            {
                IRscale = IRscale < 9 ? IRscale + 1 : 9;

                if (scale.scale < 8)
                    scale.scale = scale.scale == 0 ? 2 : scale.scale * 2;
            }
        }

        private void xBRScaleDec_Click(object sender, EventArgs e)
        {
            if (Core != null && Core.Running)
            {
                IRscale = IRscale > 1 ? IRscale - 1 : 1;

                if (scale.scale > 0)
                    scale.scale /= 2;

            }
        }

        private void MnuPause_Click(object sender, EventArgs e)
        {
            if (Core != null && Core.Running)
                Core.Pause();
        }

        private void UnLoadStripMenuItem_Click(object sender, EventArgs e)
        {
            UnLoadState();
        }

        private void ShowFrom(Form Frm)
        {
            Frm.StartPosition = FormStartPosition.Manual;
            Frm.Owner = this;

            Point parentCenterClient = new Point(
                this.ClientSize.Width / 2,
                this.ClientSize.Height / 2
            );

            Point parentCenterScreen = this.PointToScreen(parentCenterClient);

            Frm.Location = new Point(
                parentCenterScreen.X - Frm.Width / 2,
                parentCenterScreen.Y - Frm.Height / 2
                );

            Frm.Show();
        }

        private void CheatCode_Click(object sender, EventArgs e)
        {
            if (Core != null && Core.Running)
            {
                ShowFrom(new Form_Cheat(Core.GameID));
            }
        }

        private void MnuDebug_Click(object sender, EventArgs e)
        {
            ShowFrom(new Form_Mem());
        }

        private void KeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowFrom(new FrmInput());
        }

        private void AboutMnu_Click(object sender, EventArgs e)
        {
            ShowFrom(new FrmAbout());
        }

        private void SysSetMnu_Click(object sender, EventArgs e)
        {
            ShowFrom(new Form_Set());
        }
        #endregion

        private void SaveState(int Slot = 0)
        {
            if (Core != null && Core.Running)
            {
                Core.SaveState(Slot.ToString());
                UpdateStatus(1, $"{ePceCD.Properties.Resources.FrmMain_SaveState_saved} [{StateSlot}]", true);
                StatusDelay = 3;
            }
        }

        private void LoadState(int Slot = 0)
        {
            if (Core != null && Core.Running)
            {
                Core.SaveState("~", true);
                Core.LoadState(Slot.ToString());
            }
        }

        private void UnLoadState()
        {
            if (Core != null && Core.Running)
            {
                Core.LoadState("~");
            }
        }

        private void ButtonsDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                if (Core != null && Core.Running)
                    Core.Pause();
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                if (bFullScreen)
                    fullScreenF2_Click(null, null);
                return;
            }
            if (e.KeyCode == Keys.F2)
            {
                fullScreenF2_Click(null, null);
                return;
            }
            if (e.KeyCode == Keys.F3)
            {
                StateSlot = StateSlot < 9 ? StateSlot + 1 : StateSlot;
                return;
            }
            if (e.KeyCode == Keys.F4)
            {
                StateSlot = StateSlot > 0 ? StateSlot - 1 : StateSlot;
                return;
            }
            if (e.KeyCode == Keys.F5)
            {
                foreach (ToolStripMenuItem item in SaveStripMenuItem.DropDownItems)
                {
                    if ((int)item.Tag == 30 + StateSlot)
                    {
                        Mnu_Click(item, null);
                        break;
                    }
                }
                return;
            }
            if (e.KeyCode == Keys.F6)
            {
                LoadState(StateSlot);
                return;
            }
            if (e.KeyCode == Keys.F7)
            {
                UnLoadState();
                return;
            }
            if (e.KeyCode == Keys.F9)
            {
                KeyFirst = !KeyFirst;
                ini.WriteInt("main", "keyfirst", KeyFirst ? 1 : 0);
                return;
            }
            if (e.KeyCode == Keys.Tab && Core != null)
            {
                Core.Boost = true;
                return;
            }
            if (e.KeyCode == Keys.F11)
            {
                xBRScaleAdd_Click(null, null);
                return;
            }
            if (e.KeyCode == Keys.F12)
            {
                xBRScaleDec_Click(null, null);
                return;
            }

            PCEKEY button = FrmInput.KMM1.GetKeyButton(e.KeyCode);
            if ((int)button != 0xFF && Core != null && Core.Running)
            {
                Core.Button(button, true, 0);
                if (!KeyFirst)
                    KeyFirst = true;
            }

            PCEKEY button1 = FrmInput.KMM2.GetKeyButton(e.KeyCode);
            if ((int)button1 != 0xFF && Core != null && Core.Running)
                Core.Button(button1, true, 1);
        }

        private void ButtonsUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab && Core != null)
            {
                Core.Boost = false;
                return;
            }

            PCEKEY button = FrmInput.KMM1.GetKeyButton(e.KeyCode);
            if ((int)button != 0xFF && Core != null && Core.Running)
                Core.Button(button, false, 0);

            PCEKEY button1 = FrmInput.KMM2.GetKeyButton(e.KeyCode);
            if ((int)button1 != 0xFF && Core != null && Core.Running)
                Core.Button(button1, false, 1);
        }

        private void LoadRom(string fn = "", RomList.Game game = null)
        {
            if (fn == "")
            {
                OpenFileDialog FD = new OpenFileDialog();
                FD.InitialDirectory = ini.Read("main", "LastPath");
                FD.Filter = "Games|*.cue;*.pce";
                if (FD.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
                if (!File.Exists(FD.FileName))
                    return;

                fn = FD.FileName;
                gamename = Path.GetFileNameWithoutExtension(fn);
            }

            if (Path.GetExtension(fn) == ".cue" && !File.Exists("./BIOS/" + currbios))
            {
                UpdateStatus(0, $"{ePceCD.Properties.Resources.FrmMain_LoadRom_nobios} (Bios Not Found)", true);
                timer.Enabled = false;
                timer.Stop();
                return;
            }

            ini.Write("main", "LastPath", Path.GetDirectoryName(fn));

            if (Core != null)
            {
                Core.Stop();
            }

            IniFile bootini = null;
            IniFile sysini = ini;
            string gameid = "";
            if (game != null)
            {
                gamename = game.Name;
                gameid = game.ID;
                string inifn = $"./Save/{game.ID}.ini";
                if (File.Exists(inifn))
                {
                    bootini = new IniFile(inifn);
                    ini = bootini;
                }
            }

            IRscale = ini.ReadInt("main", "GpuModeScale");

            currbios = ini.Read("main", "bios");

            romList.Dispose();

            Render.SelectRenderer(Rendermode, this);

            Core = new PCECore(this, this, fn, mypath + "/BIOS/" + currbios, gameid);

            if (Core == null || Core.GameID == "")
            {
                Core = null;
                return;
            }

            sysini.Write("history", Core.GameID, $"{fn}|{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

            SetAudioBuffer();

            IRscale = 1;

            scale.scale = 0;
            scale.mode = (ScaleMode)ini.ReadInt("Main", "ScaleMode");

            Core.FRAME_LIMIT = ini.ReadFloat("CPU", "FrameIdle");

            Core.Bus.APU.MixADPCM = ini.ReadInt("Main", "ADPCM") == 1;
            Core.Bus.APU.MixFADE = ini.ReadInt("Main", "FADE") == 1;

            Core.Start();

            CheatCode.Enabled = true;

            if (sysini != null)
            {
                ini = sysini;
            }

            InitStateMnu();
        }

        public void RenderFrame(int[] pixels, int width, int height)
        {
            CoreWidth = width;
            CoreHeight = height;

            QueryControllerState(1);
            QueryControllerState(2);

            Render.RenderBuffer(pixels, width, height, scale);

            _frameCount++;
            var elapsedSeconds = _fpsStopwatch.Elapsed.TotalSeconds;
            if (elapsedSeconds >= 1.0)
            {
                _currentFps = (float)(_frameCount / elapsedSeconds);
                _frameCount = 0;
                _fpsStopwatch.Restart();
            }
        }

        private unsafe void AudioCallbackImpl(IntPtr userdata, IntPtr stream, int len)
        {
            if(Core != null && Core.Running)
                Core.Bus.APU.GetSamples(stream, len);

            //byte[] tempBuffer = new byte[len];

            //int bytesRead = SamplesBuffer.Read(tempBuffer, 0, len);

            //fixed (byte* ptr = tempBuffer)
            //{
            //    Buffer.MemoryCopy(ptr, (void*)stream, len, bytesRead);
            //}

            //if (bytesRead < len)
            //{
            //    new Span<byte>((void*)(stream + bytesRead), len - bytesRead).Fill(0);
            //}
        }

        public void PlaySamples(byte[] samples)
        {
            SamplesBuffer.Write(samples);
        }

        #region SDLController

        private void CheckController()
        {
            concount = SDL_NumJoysticks();

            if (controller1 == 0 && concount >= 1)
            {
                if (SDL_IsGameController(0) == SDL_bool.SDL_TRUE)
                {
                    controller1 = SDL_GameControllerOpen(0);
                }
                else
                {
                    controller1 = SDL_JoystickOpen(0);
                }
                Console.WriteLine($"Controller Device 1 : {SDL_JoystickNameForIndex(0)} Connected");
            }

            if (controller2 == 0 && concount >= 2)
            {
                if (SDL_IsGameController(1) == SDL_bool.SDL_TRUE)
                {
                    controller2 = SDL_GameControllerOpen(1);
                }
                else
                {
                    controller2 = SDL_JoystickOpen(1);
                }
                Console.WriteLine($"Controller Device 2 : {SDL_JoystickNameForIndex(1)} Connected");
            }
        }

        private void QueryControllerState(int conidx)
        {
            nint controller;

            if (conidx == 1)
            {
                controller = controller1;
            }
            else if (conidx == 2)
            {
                controller = controller2;
            }
            else
            {
                return;
            }

            if (Core == null || controller == 0)
                return;

            conidx--;

            //Button
            bool isPadPressed = false;
            foreach (SDL_GameControllerButton button in Enum.GetValues(typeof(SDL_GameControllerButton)))
            {
                bool isPressed = SDL_GameControllerGetButton(controller, button) == 1;

                if (isPressed)
                {
                    if (isPressed && (int)button >= 11 && (int)button <= 15)
                    {
                        isPadPressed = true;
                    }
                }
                if (isPressed && KeyFirst)
                    KeyFirst = false;
                if (!KeyFirst)
                    if (FrmInput.AnalogMap.TryGetValue(button, out var gamepadInput))
                    {
                        Core.Button(gamepadInput, isPressed, conidx);
                    }
            }

            if (KeyFirst)
                return;

            //AnalogAxis
            float lx = 0.0f, ly = 0.0f, rx = 0.0f, ry = 0.0f;

            short leftX = SDL_GameControllerGetAxis(controller, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX);
            short leftY = SDL_GameControllerGetAxis(controller, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY);
            short rightX = SDL_GameControllerGetAxis(controller, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX);
            short rightY = SDL_GameControllerGetAxis(controller, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY);

            lx = NormalizeAxis(leftX);
            ly = NormalizeAxis(leftY);
            rx = NormalizeAxis(rightX);
            ry = NormalizeAxis(rightY);

            if (isPadPressed)
                return;

            //Hat
            int hatIndex = 0;
            int hatState = 0;
            IntPtr joystick = SDL_GameControllerGetJoystick(controller);
            if (joystick != IntPtr.Zero)
            {
                hatState = SDL_JoystickGetHat(joystick, hatIndex);

                Core.Button(PCEKEY.DPadUp, (hatState & SDL_HAT_UP) != 0, conidx);
                Core.Button(PCEKEY.DPadDown, (hatState & SDL_HAT_DOWN) != 0, conidx);
                Core.Button(PCEKEY.DPadLeft, (hatState & SDL_HAT_LEFT) != 0, conidx);
                Core.Button(PCEKEY.DPadRight, (hatState & SDL_HAT_RIGHT) != 0, conidx);
            }

            if (hatState == 0)
            {
                // 将左摇杆的值转换为方向键状态
                Core.Button(PCEKEY.DPadUp, ly < -0.5f, conidx);    // 上
                Core.Button(PCEKEY.DPadDown, ly > 0.5f, conidx);   // 下
                Core.Button(PCEKEY.DPadLeft, lx < -0.5f, conidx);  // 左
                Core.Button(PCEKEY.DPadRight, lx > 0.5f, conidx);  // 右
            }

        }

        private float NormalizeAxis(short value)
        {
            float ret = Math.Clamp(value / 32767.0f, -1.0f, 1.0f);
            if (Math.Abs(ret) < 0.1f)
            {
                ret = 0.0f;
            }
            return ret;
        }

        #endregion

    }

}
