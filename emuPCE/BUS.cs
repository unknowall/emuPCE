namespace emuPCE
{
    class BUS : MemoryBank
    {
        // IRQ Values (IRQ1 and IRQ2 are outsourced)
        private bool m_EnableTIMER;
        private bool m_EnableIRQ1;
        private bool m_EnableIRQ2;
        private bool m_FiredTIMER;

        // Timer values
        private int m_TimerValue;
        private int m_TimerOverflow;
        private bool m_TimerCounting;

        private byte m_BusCap;

        // Chip classes
        public PPU m_PPU;
        public Controller m_JoyPort;
        public APU m_APU;
        public CDRom m_CDRom;
        public PCESystem m_Host;

        // Timing values
        private int m_OverFlowCycles;
        private int m_DeadClocks;

        public BUS(PCESystem host, CDRom cdrom)
        {
            m_PPU = new PPU();
            m_JoyPort = new Controller();
            m_APU = new APU(cdrom);
            m_CDRom = cdrom;
            m_Host = host;

            m_TimerOverflow = 0x10000 << 10;
            m_OverFlowCycles = 0;
        }

        public void Reset()
        {
            m_FiredTIMER = false;
            m_TimerCounting = false;

            m_PPU.Reset();
            m_DeadClocks = 0;
        }

        public int Update()
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
                    m_PPU.Update();
                    m_OverFlowCycles = 0;
                    m_TimerValue -= cycles;
                }
            }
            else
            {
                m_PPU.Update();
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
            return m_PPU.IRQPending() && m_EnableIRQ1;
        }

        public bool IRQ2Waiting()
        {
            return m_CDRom.IRQWaiting() && m_EnableIRQ2;
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
                        (m_PPU.IRQPending() ? 0x02 : 0) |
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

        public override byte ReadAt(int address)
        {
            if (address <= 0x03FF)      // VDC
            {
                m_Host.LoseCycles(1);
                return m_PPU.ReadVDC(address & 0x3);
            }
            else if (address <= 0x07FF) // VCE
            {
                m_Host.LoseCycles(1);
                return m_PPU.ReadVCE(address & 0x7);
            }
            else if (address <= 0x0BFF) // PSG
                return m_BusCap;
            else if (address <= 0x0FFF) // TIMER
                return m_BusCap = (byte)((m_TimerValue >> 10) & 0x7F);    // TIMER CODE
            else if (address <= 0x13FF) // I/O Port
                return m_BusCap = m_JoyPort.Read();
            else if (address <= 0x17FF) // INTERRUPT CONTROL
                return m_BusCap = ReadIRQCtrl(address & 3);
            else if (address <= 0x1BFF) // CDROM
                return m_CDRom.ReadAt(address);

            return 0xFF;
        }

        public override void WriteAt(int address, byte data)
        {
            if (address <= 0x03FF)      // VDC
            {
                m_Host.LoseCycles(1);
                m_PPU.WriteVDC(address & 0x3, data);
            }
            else if (address <= 0x07FF) // VCE
            {
                m_Host.LoseCycles(1);
                m_PPU.WriteVCE(address & 0x7, data);
            }
            else if (address <= 0x0BFF) // PSG
                m_APU.Write(address, m_BusCap = data);
            else if (address <= 0x0FFF) // TIMER
                WriteTimer(address & 1, m_BusCap = data);
            else if (address <= 0x13FF) // I/O Port
                m_JoyPort.Write(m_BusCap = data);
            else if (address <= 0x17FF) // INTERRUPT CONTROL
                WriteIRQCtrl(address & 3, m_BusCap = data);
            else if (address <= 0x1BFF) // CD-ROM ACCESS
                m_CDRom.WriteAt(address, data);
        }
    }
}
