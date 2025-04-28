using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace emuPCE
{
    public class PCECore : HuC6280, IDisposable
    {
        private MemoryBank[] m_BankList;
        private MemoryBank nullMemory;
        private RamBank memory;

        private BUS m_BUS;
        private CDRom m_CDRom;

        private PPU ppu;
        private Controller JoyPort;
        private APU apu;
        private CDRom cdrom;

        public struct AddrItem
        {
            public UInt32 Address;
            public UInt32 Value;
            public Byte Width;
        }
        public struct CheatCode
        {
            public string Name;
            public List<AddrItem> Item;
            public bool Active;
        }
        public List<CheatCode> cheatCodes = new List<CheatCode> { };

        public string RomName = "";
        public string CDfile = "";
        public string GameID = "";
        public int FPS;

        public bool Pauseing, Pauseed, Running, Boost;

        public delegate void EventFrameRender(IntPtr pixels);
        public event EventFrameRender FrameRender;

        public PCECore()
        {
            nullMemory = new MemoryBank();
            memory = new RamBank();
            m_CDRom = new CDRom();
            m_BUS = new BUS(this, m_CDRom);

            ppu = m_BUS.m_PPU;
            JoyPort = m_BUS.m_JoyPort;
            apu = m_BUS.m_APU;
            cdrom = m_BUS.m_CDRom;

            ppu.FrameReady += FrameReady;

            m_BankList = new MemoryBank[0x100];

            for (int i = 0; i < 0x100; i++)
                m_BankList[i] = nullMemory;

            m_BankList[0xF8] = memory;
            m_BankList[0xF9] = memory;
            m_BankList[0xFA] = memory;
            m_BankList[0xFB] = memory;

            // CD-ROM BRAM
            m_BankList[0xF7] = nullMemory;

            // CD-ROM RAM
            m_BankList[0x80] = m_CDRom.GetRam(0);
            m_BankList[0x81] = m_CDRom.GetRam(1);
            m_BankList[0x82] = m_CDRom.GetRam(2);
            m_BankList[0x83] = m_CDRom.GetRam(3);
            m_BankList[0x84] = m_CDRom.GetRam(4);
            m_BankList[0x85] = m_CDRom.GetRam(5);
            m_BankList[0x86] = m_CDRom.GetRam(6);
            m_BankList[0x87] = m_CDRom.GetRam(7);

            m_BankList[0xFF] = m_BUS;
        }

        public void Dispose()
        {

        }

        public void Stop()
        {

        }

        public void Pause()
        {

        }

        public void WaitPaused()
        {

        }

        public void Start()
        {

        }

        public void LoadState(string Fix = "")
        {
            if (!Running)
                return;

            string fn = "./SaveState/" + GameID + "_Save" + Fix + ".dat";
            if (!File.Exists(fn))
                return;
        }

        public void SaveState(string Fix = "")
        {
            if (!Running)
                return;
        }

        public void LoadCheats()
        {
            cheatCodes.Clear();
            string fn = "./Cheats/" + GameID + ".txt";
            if (!File.Exists(fn))
                return;
            cheatCodes = ParseTextToCheatCodeList(fn);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[CHEAT] {cheatCodes.Count} Codes Loaded");
            foreach (var code in cheatCodes)
            {
                if (code.Active)
                    Console.WriteLine($"    {code.Name} [Active]");
                else
                    Console.WriteLine($"    {code.Name} [Non Active]");
            }
            Console.ResetColor();
        }

        public void tick()
        {
            m_Clock = m_BUS.tick();
            FPS = ppu.FramePerSec;
            base.cycle();
        }

        public void FrameReady(IntPtr pixels)
        {
            FrameRender?.Invoke(pixels);
        }

        public void GetAudioSamples(IntPtr stream, int len)
        {
            apu.GetSamples(stream, len);
        }

        public void KeyState(PCEKEY key, short keyup)
        {
            JoyPort.KeyState(key, keyup);
        }

        public void Button(PCEKEY key, bool isDown, int ConIdx = 0)
        {
            JoyPort.KeyState(key, (short)(isDown ? 1 : 0));
        }

        public void KeyDown(PCEKEY key)
        {
            JoyPort.KeyState(key, 0);
        }

        public void KeyUp(PCEKEY key)
        {
            JoyPort.KeyState(key, 1);
        }

        protected override bool IRQ2Waiting()
        {
            return m_BUS.IRQ2Waiting();
        }

        protected override bool IRQ1Waiting()
        {
            return m_BUS.IRQ1Waiting();
        }

        protected override bool TimerWaiting()
        {
            return m_BUS.TimerWaiting();
        }

        private void BitSwap(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(
                    ((buffer[i] & 0x80) >> 7) |
                    ((buffer[i] & 0x40) >> 5) |
                    ((buffer[i] & 0x20) >> 3) |
                    ((buffer[i] & 0x10) >> 1) |
                    ((buffer[i] & 0x08) << 1) |
                    ((buffer[i] & 0x04) << 3) |
                    ((buffer[i] & 0x02) << 5) |
                    ((buffer[i] & 0x01) << 7));

            }
        }

        public void LoseCycles(int cycles)
        {
            m_Clock -= cycles;
        }

        public void LoadCue(string file)
        {
            m_CDRom.LoadCue(file);
            CDfile = Path.GetFileNameWithoutExtension(file);
            m_BankList[0xF7] = m_CDRom.GetSaveMemory();
        }

        public void LoadRom(string fileName, bool swap)
        {
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[][] page = new byte[(file.Length - file.Length % 0x400) / 0x2000][];
            int i;
            RomName = Path.GetFileNameWithoutExtension(fileName);

            Console.WriteLine("Loading rom {0}...", fileName);

            file.Seek(file.Length % 0x400, SeekOrigin.Begin);
            for (i = 0; i < page.Length; i++)
            {
                page[i] = new byte[0x2000];
                file.Read(page[i], 0, 0x2000);
            }

            // Bit swap the rom if it boots in a page other than MPR7
            if (swap)//page[0][0x1FFF] < 0xE0)
                for (i = 0; i < page.Length; i++)
                    BitSwap(page[i]);

            file.Close();

            // Super System Card ram only active when there is enough space
            if (page.Length <= 0x68)
            {
                for (i = 0; i < 24; i++)
                    m_BankList[i + 0x68] = m_CDRom.GetRam(i + 8);
            }

            //SF2 MAPPER
            if (page.Length > 128)
            {
                for (i = 0; i < 64; i++)
                {
                    byte[][] p = new byte[4][] {
                        page[i],
                        page[i],
                        page[i],
                        page[i]
                        };

                    m_BankList[i] = new ExtendedRomBank(p);
                }

                for (i = 0; i < 64; i++)
                {
                    byte[][] p = new byte[4][] {
                        page[i+0x40],
                        page[i+0x80],
                        page[i+0xC0],
                        page[i+0x100]
                        };

                    m_BankList[i + 0x40] = new ExtendedRomBank(p);
                }
            }
            else if (page.Length == 48)
            {
                // 384kB games (requires some mirroring
                int b = 0;

                for (i = 0; i < 32; i++)
                    m_BankList[b++] = new RomBank(page[i]);
                for (i = 0; i < 48; i++)
                    m_BankList[b++] = new RomBank(page[i]);
                for (i = 0; i < 48; i++)
                    m_BankList[b++] = new RomBank(page[i]);
            }
            else
            {
                for (i = 0; i < page.Length; i++)
                    m_BankList[i] = new RomBank(page[i]);
            }

            for (i = 0; i < 0x100; i++)
                GetBank((byte)i).SetMemoryPage(i);
        }

        public override void Reset()
        {
            m_BUS.Reset();
            base.Reset();
        }

        protected override MemoryBank GetBank(byte bank)
        {
            return m_BankList[bank];
        }

        public static List<CheatCode> ParseTextToCheatCodeList(string fn, bool isfile = true)
        {
            string input;

            if (isfile)
                input = File.ReadAllText(fn);
            else
                input = fn;

            List<CheatCode> result = new List<CheatCode>();
            string currentSection = null;
            bool isActive = false;
            List<AddrItem> currentItems = null;

            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (currentSection != null)
                    {
                        result.Add(new CheatCode { Name = currentSection, Item = currentItems, Active = isActive });
                    }
                    currentSection = line.Substring(1, line.Length - 2).Trim();
                    isActive = true;
                    currentItems = new List<AddrItem>();
                }
                else if (line.StartsWith("Active", StringComparison.OrdinalIgnoreCase))
                {
                    var match = Regex.Match(line, @"Active\s*=\s*(true|false|0|1)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        if (match.Groups[1].Value == "true" || match.Groups[1].Value == "1")
                            isActive = true;
                        else
                            isActive = false;
                    }
                }
                else
                {
                    var match = Regex.Match(line, @"^([0-9A-F]{8})\s+([0-9A-F]{1,8})$", RegexOptions.IgnoreCase);
                    if (match.Success && currentSection != null)
                    {
                        UInt32 address = Convert.ToUInt32(match.Groups[1].Value, 16);
                        UInt32 value = Convert.ToUInt32(match.Groups[2].Value, 16);
                        //byte width = value <= 0xFF ? (byte)1 : value <= 0xFFFF ? (byte)2 : (byte)4;
                        byte width = match.Groups[2].Value.Length <= 2 ? (byte)1 : match.Groups[2].Value.Length <= 4 ? (byte)2 : (byte)4;

                        currentItems.Add(new AddrItem { Address = address, Value = value, Width = width });
                    }
                }
            }
            if (currentSection != null)
            {
                result.Add(new CheatCode { Name = currentSection, Item = currentItems, Active = isActive });
            }
            return result;
        }
    }
}
