using System;
using System.IO;

namespace ePceCD
{
    public class BUS : MemoryBank, IDisposable
    {
        private MemoryBank[] m_BankList;
        private MemoryBank nullMemory;
        public RamBank[] memory;
        public SaveMemoryBank BRAM;

        public HuC6280 CPU;
        public PPU PPU;
        public Controller JoyPort;
        public APU APU;
        public CDRom CDRom;

        private bool m_EnableTIMER;
        private bool m_EnableIRQ1;
        private bool m_EnableIRQ2;
        private bool m_FiredTIMER;

        private int m_TimerValue;
        private int m_TimerOverflow;
        private bool m_TimerCounting;

        private byte m_BusCap;

        private int m_OverFlowCycles;
        private int m_DeadClocks;

        public string RomName = "";
        public string CDfile = "";
        public string GameID = "";

        public BUS(IRenderHandler render, IAudioHandler audio)
        {
            nullMemory = new MemoryBank();

            memory = new RamBank[33];
            for (int i = 0; i < memory.Length; i++)
                memory[i] = new RamBank();

            CDRom = new CDRom(this);

            CPU = new HuC6280(this);
            PPU = new PPU(render);
            JoyPort = new Controller();
            APU = new APU(audio, CDRom);

            m_BankList = new MemoryBank[0x100];

            for (int i = 0; i < 0x100; i++)
                m_BankList[i] = nullMemory;

            m_BankList[0xF8] = memory[0];
            m_BankList[0xF9] = memory[0];
            m_BankList[0xFA] = memory[0];
            m_BankList[0xFB] = memory[0];

            // CD-ROM BRAM
            m_BankList[0xF7] = nullMemory;

            // CD-ROM RAM
            for (int i = 0; i < 8; i++)
                m_BankList[0x80+i] = memory[i + 1];

            m_BankList[0xFF] = this;

            m_TimerOverflow = 0x10000 << 10;
            m_OverFlowCycles = 0;
        }

        public void Dispose()
        {

        }

        public void Reset()
        {
            m_FiredTIMER = false;
            m_TimerCounting = false;

            PPU.Reset();
            m_DeadClocks = 0;

            CPU.Reset();
        }

        public int tick()
        {
            int cycles;

            if (m_OverFlowCycles > 0)
                cycles = m_OverFlowCycles;
            else
                cycles = PPU.CYCLES_PER_LINE / (int)DotClock.MHZ_7;

            if (m_TimerCounting)
            {
                if (cycles >= m_TimerValue)
                {
                    m_OverFlowCycles = cycles - m_TimerValue;
                    m_TimerValue = m_TimerOverflow;
                    m_FiredTIMER = true;
                }
                else
                {
                    PPU.tick();
                    m_OverFlowCycles = 0;
                    m_TimerValue -= cycles;
                }
            }
            else
            {
                PPU.tick();
                m_OverFlowCycles = 0;
            }

            if (m_DeadClocks > 0)
            {
                if (m_DeadClocks > cycles)
                {
                    m_DeadClocks -= cycles;
                    return 0;
                }
                else
                {
                    cycles -= m_DeadClocks;
                    m_DeadClocks = 0;
                }
            }

            return cycles;
        }

        public bool TimerWaiting()
        {
            // DESTICKY IRQS
            bool sticky = m_FiredTIMER && m_EnableTIMER;
            m_FiredTIMER = false;
            return sticky;
        }

        public bool IRQ1Waiting()
        {
            return PPU.IRQPending() && m_EnableIRQ1;
        }

        public bool IRQ2Waiting()
        {
            return CDRom.IRQWaiting() && m_EnableIRQ2;
        }

        private void WriteTimer(int address, byte data)
        {
            switch (address)
            {
                case 0: // TIMER CODE
                    data &= 0x7F;
                    m_TimerOverflow = (data << 10) | 0x3FF;
                    m_TimerValue = m_TimerOverflow;
                    break;
                case 1:
                    m_TimerCounting = (data & 1) != 0;

                    if (m_TimerCounting)
                    {
                        m_TimerValue = m_TimerOverflow; // ???
                    }
                    else
                    {
                        m_FiredTIMER = false; // Auto clear the timer if it is disabled
                    }
                    break;
            }
        }

        private byte ReadIRQCtrl(int address)
        {
            switch (address)
            {
                case 2: // Enables
                    return (byte)(
                        (m_BusCap & 0xF8) |
                        (m_EnableIRQ2 ? 0 : 0x01) |
                        (m_EnableIRQ1 ? 0 : 0x02) |
                        (m_EnableTIMER ? 0 : 0x04));
                case 3: // Pendings
                    return (byte)(
                        (m_BusCap & 0xF8) |
                        // (false ? 0x01 : 0) |         CD-ROM UNIMPLEMENTED
                        (PPU.IRQPending() ? 0x02 : 0) |
                        (m_FiredTIMER ? 0x04 : 0));
                default:
                    return m_BusCap;
            }
        }

        private void WriteIRQCtrl(int address, byte data)
        {
            switch (address)
            {
                case 2: // Enables
                    m_EnableIRQ2 = (data & 1) == 0;
                    m_EnableIRQ1 = (data & 2) == 0;
                    m_EnableTIMER = (data & 4) == 0;

                    break;
                case 3: // Pendings (ack timer)
                    m_FiredTIMER = false;
                    break;
            }
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
            CPU.m_Clock -= cycles;
        }

        public void LoadCue(string file)
        {
            CDRom.LoadCue(file);
            CDfile = Path.GetFileNameWithoutExtension(file);
            m_BankList[0xF7] = BRAM;
        }

        public void LoadRom(string fileName, bool swap)
        {
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[][] page = new byte[(file.Length - file.Length % 0x400) / 0x2000][];
            int i;
            RomName = Path.GetFileNameWithoutExtension(fileName);

            //Console.WriteLine("Loading rom {0}...", fileName);

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
                    m_BankList[i + 0x68] = memory[i + 9];
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

        public MemoryBank GetBank(byte bank)
        {
            return m_BankList[bank];
        }

        public override byte ReadAt(int address)
        {
            if (address <= 0x03FF)      // VDC
            {
                LoseCycles(1);
                return PPU.ReadVDC(address & 0x3);
            }
            else if (address <= 0x07FF) // VCE
            {
                LoseCycles(1);
                return PPU.ReadVCE(address & 0x7);
            }
            else if (address <= 0x0BFF) // PSG
                return m_BusCap;
            else if (address <= 0x0FFF) // TIMER
                return m_BusCap = (byte)((m_TimerValue >> 10) & 0x7F);    // TIMER CODE
            else if (address <= 0x13FF) // I/O Port
                return m_BusCap = JoyPort.Read();
            else if (address <= 0x17FF) // INTERRUPT CONTROL
                return m_BusCap = ReadIRQCtrl(address & 3);
            else if (address <= 0x1BFF) // CDROM
                return CDRom.ReadAt(address);

            return 0xFF;
        }

        public override void WriteAt(int address, byte data)
        {
            if (address <= 0x03FF)      // VDC
            {
                LoseCycles(1);
                PPU.WriteVDC(address & 0x3, data);
            }
            else if (address <= 0x07FF) // VCE
            {
                LoseCycles(1);
                PPU.WriteVCE(address & 0x7, data);
            }
            else if (address <= 0x0BFF) // PSG
                APU.Write(address, m_BusCap = data);
            else if (address <= 0x0FFF) // TIMER
                WriteTimer(address & 1, m_BusCap = data);
            else if (address <= 0x13FF) // I/O Port
                JoyPort.Write(m_BusCap = data);
            else if (address <= 0x17FF) // INTERRUPT CONTROL
                WriteIRQCtrl(address & 3, m_BusCap = data);
            else if (address <= 0x1BFF) // CD-ROM ACCESS
                CDRom.WriteAt(address, data);
        }
    }
}
