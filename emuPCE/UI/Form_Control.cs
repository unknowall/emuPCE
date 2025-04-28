using System;
using System.Collections.Generic;
using System.Windows.Forms;

using static ePceCD.Controller;
using static SDL2.SDL;

namespace ePceCD.UI
{
    public partial class FrmInput : Form
    {
        public static KeyMappingManager KMM1 = new KeyMappingManager();
        public static KeyMappingManager KMM2 = new KeyMappingManager();

        public static Dictionary<SDL_GameControllerButton, PCEKEY> AnalogMap;

        private PCEKEY SetKey;
        private Button Btn;

        public FrmInput()
        {
            InitializeComponent();

            InitKeyMap();

            InitControllerMap();

            if (SDL_NumJoysticks() > 0)
            {
                cbmode.Items.Add(SDL_JoystickNameForIndex(0));
            }

            cbcon.SelectedIndex = 0;
            cbmode.SelectedIndex = 0;

            //cbcon.Enabled = false;
            cbmode.Enabled = false;

            this.KeyUp += FrmInput_KeyUp;
        }

        public static void InitKeyMap()
        {
            try
            {
                KMM1._keyMapping = FrmMain.ini.ReadDictionary<Keys, PCEKEY>("Player1Key");
                KMM2._keyMapping = FrmMain.ini.ReadDictionary<Keys, PCEKEY>("Player2Key");
            } catch { }

            if (KMM1._keyMapping.Count == 0)
            {
                KMM1.SetKeyMapping(Keys.D2, PCEKEY.Select);
                KMM1.SetKeyMapping(Keys.D1, PCEKEY.Start);
                KMM1.SetKeyMapping(Keys.W, PCEKEY.DPadUp);
                KMM1.SetKeyMapping(Keys.D, PCEKEY.DPadRight);
                KMM1.SetKeyMapping(Keys.S, PCEKEY.DPadDown);
                KMM1.SetKeyMapping(Keys.A, PCEKEY.DPadLeft);
                KMM1.SetKeyMapping(Keys.I, PCEKEY.B);
                KMM1.SetKeyMapping(Keys.U, PCEKEY.A);

            }

            if (KMM2._keyMapping.Count == 0)
            {
                KMM1.SetKeyMapping(Keys.D2, PCEKEY.Select);
                KMM1.SetKeyMapping(Keys.D1, PCEKEY.Start);
                KMM1.SetKeyMapping(Keys.W, PCEKEY.DPadUp);
                KMM1.SetKeyMapping(Keys.D, PCEKEY.DPadRight);
                KMM1.SetKeyMapping(Keys.S, PCEKEY.DPadDown);
                KMM1.SetKeyMapping(Keys.A, PCEKEY.DPadLeft);
                KMM1.SetKeyMapping(Keys.I, PCEKEY.B);
                KMM1.SetKeyMapping(Keys.U, PCEKEY.A);
            }

            FrmMain.ini.WriteDictionary<Keys, PCEKEY>("Player1Key", KMM1._keyMapping);
            FrmMain.ini.WriteDictionary<Keys, PCEKEY>("Player2Key", KMM2._keyMapping);
        }

        public static void InitControllerMap()
        {
            AnalogMap = new()
            {
            { SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A, PCEKEY.A },
            { SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B, PCEKEY.B },
            //{ SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X, PCEKEY.A },
            //{ SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y, PCEKEY.B },
            { SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK, PCEKEY.Select },
            { SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START, PCEKEY.Start },
            { SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP, PCEKEY.DPadUp },
            { SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN, PCEKEY.DPadDown },
            { SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT, PCEKEY.DPadLeft },
            { SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT, PCEKEY.DPadRight }
            };
        }

        private void FrmInput_Shown(object sender, EventArgs e)
        {

        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            FrmMain.ini.WriteDictionary<Keys, PCEKEY>("Player1Key", KMM1._keyMapping);
            FrmMain.ini.WriteDictionary<Keys, PCEKEY>("Player2Key", KMM2._keyMapping);

            FrmMain.ini.WriteDictionary<SDL_GameControllerButton, PCEKEY>("JoyKeyMap", AnalogMap);
        }

        private void FrmInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                plwait.Visible = false;
                if (cbmode.SelectedIndex == 1)
                {
                    //SdlQuit = true;
                }
                return;
            }

            if (!plwait.Visible || cbmode.SelectedIndex == 1)
                return;

            Btn.Text = e.KeyCode.ToString();

            if (cbcon.SelectedIndex == 0)
                KMM1.SetKeyMapping(e.KeyCode, SetKey);
            else
                KMM2.SetKeyMapping(e.KeyCode, SetKey);

            plwait.Visible = false;
        }

        private void ReadyGetKey(object sender, PCEKEY val)
        {
            plwait.Visible = true;
            Btn = (Button)sender;
            SetKey = val;
            if (cbmode.SelectedIndex == 1)
            {
                // todo
            }
        }

        private void U_Click(object sender, EventArgs e)
        {
            ReadyGetKey(sender, PCEKEY.DPadUp);
        }

        private void L_Click(object sender, EventArgs e)
        {
            ReadyGetKey(sender, PCEKEY.DPadLeft);
        }

        private void D_Click(object sender, EventArgs e)
        {
            ReadyGetKey(sender, PCEKEY.DPadDown);
        }

        private void R_Click(object sender, EventArgs e)
        {
            ReadyGetKey(sender, PCEKEY.DPadRight);
        }

        private void A_Click(object sender, EventArgs e)
        {
            ReadyGetKey(sender, PCEKEY.A);
        }

        private void B_Click(object sender, EventArgs e)
        {
            ReadyGetKey(sender, PCEKEY.B);
        }

        private void SELE_Click(object sender, EventArgs e)
        {
            ReadyGetKey(sender, PCEKEY.Select);
        }

        private void START_Click(object sender, EventArgs e)
        {
            ReadyGetKey(sender, PCEKEY.Start);
        }

        private void cbcon_SelectedIndexChanged(object sender, EventArgs e)
        {
            KeyMappingManager kmm;

            if (cbcon.SelectedIndex == 0)
            {
                kmm = KMM1;
            } else
            {
                kmm = KMM2;
            }

            foreach (var mapping in kmm._keyMapping)
            {
                if (mapping.Value == PCEKEY.Start)
                    START.Text = kmm.GetKeyCode(mapping.Value).ToString().ToUpper();

                if (mapping.Value == PCEKEY.Select)
                    SELE.Text = kmm.GetKeyCode(mapping.Value).ToString().ToUpper();

                if (mapping.Value == PCEKEY.DPadUp)
                    U.Text = kmm.GetKeyCode(mapping.Value).ToString().ToUpper();

                if (mapping.Value == PCEKEY.DPadDown)
                    D.Text = kmm.GetKeyCode(mapping.Value).ToString().ToUpper();

                if (mapping.Value == PCEKEY.DPadLeft)
                    L.Text = kmm.GetKeyCode(mapping.Value).ToString().ToUpper();

                if (mapping.Value == PCEKEY.DPadRight)
                    R.Text = kmm.GetKeyCode(mapping.Value).ToString().ToUpper();

                if (mapping.Value == PCEKEY.B)
                    B.Text = kmm.GetKeyCode(mapping.Value).ToString().ToUpper();

                if (mapping.Value == PCEKEY.A)
                    A.Text = kmm.GetKeyCode(mapping.Value).ToString().ToUpper();
            }

        }

    }

    public class KeyMappingManager
    {
        public Dictionary<Keys, PCEKEY> _keyMapping = new();

        public void SetKeyMapping(Keys key, PCEKEY button)
        {
            _keyMapping.Remove(GetKeyCode(button));
            _keyMapping.Remove(key);

            _keyMapping[key] = button;
        }

        public PCEKEY GetKeyButton(Keys key)
        {
            if (_keyMapping.TryGetValue(key, out var button))
            {
                return button;
            }
            return (PCEKEY)0xFF;
            //throw new KeyNotFoundException($"No mapping found for key: {key}");
        }

        public string GetKeyName(Keys key)
        {
            if (_keyMapping.TryGetValue(key, out var button))
            {
                return button.ToString();
            }
            //throw new KeyNotFoundException($"No mapping found for key: {key}");
            return "";
        }

        public Keys GetKeyCode(PCEKEY button)
        {
            foreach (var mapping in _keyMapping)
            {
                if (mapping.Value == button)
                {
                    return mapping.Key;
                }
            }
            return Keys.None;
        }

        public void ClearKeyMapping(Keys key)
        {
            _keyMapping.Remove(key);
        }

        public void ClearAllMappings()
        {
            _keyMapping.Clear();
        }

        public void PrintMappings()
        {
            Console.WriteLine("Current Key Mappings:");
            foreach (var mapping in _keyMapping)
            {
                Console.WriteLine($"{mapping.Key} -> {mapping.Value}");
            }
        }

    }

}
