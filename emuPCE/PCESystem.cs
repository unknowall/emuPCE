using System;
using System.IO;

namespace emuPCE
{
    public class PCESystem : HuC6280
    {
        private MemoryBank[] m_BankList;
        private BUS m_BUS;
        private CDRom m_CDRom;

        private PPU ppu;
        private Controller JoyPort;
        private APU apu;
        private CDRom cdrom;

        public string RomName = "";
        public string CDfile = "";
        public int FPS;
        public delegate void EventFrameRender(IntPtr pixels);
        public event EventFrameRender FrameRender;

        public PCESystem()
        {
            MemoryBank nullMemory = new MemoryBank();
            RamBank memory = new RamBank();
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

            // CD-ROM ram sub system
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

            if (page.Length > 128)
            {
                Console.WriteLine("LOADING EXPERIMENTAL SF2 MAPPER");

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
    }
}
