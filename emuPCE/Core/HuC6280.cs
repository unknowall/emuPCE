namespace ePceCD
{

    public class HuC6280
    {
        public enum InstructionOpcode : byte
        {
            INS_BRK,            // 00
            INS_ORA_INX,        // 01
            INS_SXY,            // 02
            INS_ST0,            // 03
            INS_TSB_ZP,         // 04
            INS_ORA_ZP,         // 05
            INS_ASL_ZP,         // 06
            INS_RMB0_ZP,        // 07
            INS_PHP,            // 08
            INS_ORA_IMM,        // 09
            INS_ASL_A,          // 0A
            INS_UNDEF_0B,       // 0B
            INS_TSB_ABS,        // 0C
            INS_ORA_ABS,        // 0D
            INS_ASL_ABS,        // 0E
            INS_BBR0_ZP_REL,    // 0F
            INS_BPL_REL,        // 10
            INS_ORA_INY,        // 11
            INS_ORA_IND,        // 12
            INS_ST1,            // 13
            INS_TRB_ZP,         // 14
            INS_ORA_ZPX,        // 15
            INS_ASL_ZPX,        // 16
            INS_RMB1_ZP,        // 17
            INS_CLC,            // 18
            INS_ORA_ABSY,       // 19
            INS_INC_A,          // 1A
            INS_UNDEF_1B,       // 1B
            INS_TRB_ABS,        // 1C
            INS_ORA_ABSX,       // 1D
            INS_ASL_ABSX,       // 1E
            INS_BBR1_ZP_REL,    // 1F
            INS_JSR_ABS,        // 20
            INS_AND_INX,        // 21
            INS_SAX,            // 22
            INS_ST2,            // 23
            INS_BIT_ZP,         // 24
            INS_AND_ZP,         // 25
            INS_ROL_ZP,         // 26
            INS_RMB2_ZP,        // 27
            INS_PLP,            // 28
            INS_AND_IMM,        // 29
            INS_ROL_A,          // 2A
            INS_UNDEF_2B,       // 2B
            INS_BIT_ABS,        // 2C
            INS_AND_ABS,        // 2D
            INS_ROL_ABS,        // 2E
            INS_BBR2_ZP_REL,    // 2F
            INS_BMI_REL,        // 30
            INS_AND_INY,        // 31
            INS_AND_IND,        // 32
            INS_UNDEF_33,       // 33
            INS_BIT_ZPX,        // 34
            INS_AND_ZPX,        // 35
            INS_ROL_ZPX,        // 36
            INS_RMB3_ZP,        // 37
            INS_SEC,            // 38
            INS_AND_ABSY,       // 39
            INS_DEC_A,          // 3A
            INS_UNDEF_3B,       // 3B
            INS_BIT_ABSX,       // 3C
            INS_AND_ABSX,       // 3D
            INS_ROL_ABSX,       // 3E
            INS_BRR3_ZP_REL,    // 3F
            INS_RTI,            // 40 
            INS_EOR_INX,        // 41
            INS_SAY,            // 42
            INS_TMA,            // 43
            INS_BSR_REL,        // 44
            INS_EOR_ZP,         // 45
            INS_LSR_ZP,         // 46
            INS_RMB4_ZP,        // 47
            INS_PHA,            // 48
            INS_EOR_IMM,        // 49 
            INS_LSR_A,          // 4A
            INS_UNDEF_4B,       // 4B
            INS_JMP_ABS,        // 4C
            INS_EOR_ABS,        // 4D
            INS_LSR_ABS,        // 4E
            INS_BBR4_ZP_REL,    // 4F 
            INS_BVC_REL,        // 50
            INS_EOR_INY,        // 51
            INS_EOR_IND,        // 52
            INS_TAM,            // 53
            INS_CSL,            // 54
            INS_EOR_ZPX,        // 55
            INS_LSR_ZPX,        // 56
            INS_RMB5_ZP,        // 57
            INS_CLI,            // 58
            INS_EOR_ABSY,       // 59 
            INS_PHY,            // 5A
            INS_UNDEF_5B,       // 5B
            INS_UNDEF_5C,       // 5C
            INS_EOR_ABSX,       // 5D
            INS_LSR_ABSX,       // 5E
            INS_BBR5_ZP_REL,    // 5F
            INS_RTS,            // 60 
            INS_ADC_INX,        // 61
            INS_CLA,            // 62
            INS_UNDEF_63,       // 63
            INS_STZ_ZP,         // 64
            INS_ADC_ZP,         // 65
            INS_ROR_ZP,         // 66
            INS_RMB6_ZP,        // 67
            INS_PLA,            // 68
            INS_ADC_IMM,        // 69
            INS_ROR_A,          // 6A
            INS_UNDEF_6B,       // 6B
            INS_JMP_INA,        // 6C
            INS_ADC_ABS,        // 6D
            INS_ROR_ABS,        // 6E
            INS_BBR6_ZP_REL,    // 6F 
            INS_BVS_REL,        // 70
            INS_ADC_INY,        // 71
            INS_ADC_IND,        // 72
            INS_TII,            // 73
            INS_STZ_ZPX,        // 74
            INS_ADC_ZPX,        // 75
            INS_ROR_ZPX,        // 76
            INS_RMB7_ZP,        // 77
            INS_SEI,            // 78
            INS_ADC_ABSY,       // 79
            INS_PLY,            // 7A
            INS_UNDEF_7B,       // 7B
            INS_JMP_INAX,       // 7C
            INS_ADC_ABSX,       // 7D
            INS_ROR_ABSX,       // 7E
            INS_BBR7_ZP_REL,    // 7F
            INS_BRA_REL,        // 80
            INS_STA_INX,        // 81
            INS_CLX,            // 82
            INS_TST_ZP,         // 83
            INS_STY_ZP,         // 84
            INS_STA_ZP,         // 85
            INS_STX_ZP,         // 86
            INS_SMB0_ZP,        // 87
            INS_DEY,            // 88
            INS_BIT_IMM,        // 89
            INS_TXA,            // 8A
            INS_UNDEF_8B,       // 8B
            INS_STY_ABS,        // 8C
            INS_STA_ABS,        // 8D
            INS_STX_ABS,        // 8E
            INS_BBS0_ZP_REL,    // 8F
            INS_BCC_REL,        // 90
            INS_STA_INY,        // 91
            INS_STA_IND,        // 92
            INS_TST_ABS,        // 93
            INS_STY_ZPX,        // 94
            INS_STA_ZPX,        // 95
            INS_STX_ZPY,        // 96
            INS_SMB1_ZP,        // 97
            INS_TYA,            // 98
            INS_STA_ABSY,       // 99
            INS_TXS,            // 9A
            INS_UNDEF_9B,       // 9B
            INS_STZ_ABS,        // 9C
            INS_STA_ABSX,       // 9D
            INS_STZ_ABSX,       // 9E
            INS_BBS1_ZP_REL,    // 9F
            INS_LDY_IMM,        // A0
            INS_LDA_INX,        // A1
            INS_LDX_IMM,        // A2
            INS_TST_ZPX,        // A3
            INS_LDY_ZP,         // A4
            INS_LDA_ZP,         // A5
            INS_LDX_ZP,         // A6
            INS_SMB2_ZP,        // A7
            INS_TAY,            // A8
            INS_LDA_IMM,        // A9
            INS_TAX,            // AA
            INS_UNDEF_AB,       // AB
            INS_LDY_ABS,        // AC
            INS_LDA_ABS,        // AD
            INS_LDX_ABS,        // AE
            INS_BBS2_ZP_REL,    // AF
            INS_BCS_REL,        // B0
            INS_LDA_INY,        // B1
            INS_LDA_IND,        // B2
            INS_TST_ABSX,       // B3
            INS_LDY_ZPX,        // B4
            INS_LDA_ZPX,        // B5
            INS_LDX_ZPY,        // B6
            INS_SMB3_ZP,        // B7
            INS_CLV,            // B8
            INS_LDA_ABSY,       // B9
            INS_TSX,            // BA
            INS_UNDEF_BB,       // BB
            INS_LDY_ABSX,       // BC
            INS_LDA_ABSX,       // BD
            INS_LDX_ABSY,       // BE
            INS_BBS3_ZP_REL,    // BF
            INS_CPY_IMM,        // C0
            INS_CMP_INX,        // C1
            INS_CLY,            // C2
            INS_TDD,            // C3
            INS_CPY_ZP,         // C4
            INS_CMP_ZP,         // C5
            INS_DEC_ZP,         // C6
            INS_SMB4_ZP,        // C7
            INS_INY,            // C8
            INS_CMP_IMM,        // C9
            INS_DEX,            // CA
            INS_UNDEF_CB,       // CB
            INS_CPY_ABS,        // CC
            INS_CMP_ABS,        // CD
            INS_DEC_ABS,        // CE
            INS_BBS4_ZP_REL,    // CF
            INS_BNE_REL,        // D0
            INS_CMP_INY,        // D1
            INS_CMP_IND,        // D2
            INS_TIN,            // D3
            INS_CSH,            // D4
            INS_CMP_ZPX,        // D5
            INS_DEC_ZPX,        // D6
            INS_SMB5_ZP,        // D7
            INS_CLD,            // D8
            INS_CMP_ABSY,       // D9
            INS_PHX,            // DA
            INS_UNDEF_DB,       // DB
            INS_UNDEF_DC,       // DC
            INS_CMP_ABSX,       // DD
            INS_DEC_ABSX,       // DE
            INS_BBS5_ZP_REL,    // DF
            INS_CPX_IMM,        // E0
            INS_SBC_INX,        // E1
            INS_UNDEF_E2,       // E2
            INS_TIA,            // E3
            INS_CPX_ZP,         // E4
            INS_SBC_ZP,         // E5
            INS_INC_ZP,         // E6
            INS_SMB6_ZP,        // E7
            INS_INX,            // E8
            INS_SBC_IMM,        // E9
            INS_NOP,            // EA
            INS_UNDEF_EB,       // EB
            INS_CPX_ABS,        // EC
            INS_SBC_ABS,        // ED
            INS_INC_ABS,        // EE
            INS_BBS6_ZP_REL,    // EF
            INS_BEQ_REL,        // F0
            INS_SBC_INY,        // F1
            INS_SBC_IND,        // F2
            INS_TAI,            // F3
            INS_SET,            // F4
            INS_SBC_ZPX,        // F5
            INS_INC_ZPX,        // F6
            INS_SMB7_ZP,        // F7
            INS_SED,            // F8
            INS_SBC_ABSY,       // F9
            INS_PLX,            // FA
            INS_UNDEF_FB,       // FB
            INS_UNDEF_FC,       // FC
            INS_SBC_ABSX,       // FD
            INS_INC_ABSX,       // FE
            INS_BBS7_ZP_REL     // FF
        }

        public enum IRQVector : ushort
        {
            VECTOR_BRK = 0xFFF6,
            VECTOR_IRQ2 = 0xFFF6,
            VECTOR_IRQ1 = 0xFFF8,
            VECTOR_TIMER = 0xFFFA,
            VECTOR_NMI = 0xFFFC,
            VECTOR_RESET = 0xFFFE
        }

        public enum BlockTransferMode
        {
            NoTransferActive,
            IncrementIncrement,
            IncrementAlternate_Up,
            IncrementAlternate_Down,
            AlternateIncrement_Up,
            AlternateIncrement_Down,
            DecrementDecrement,
            IncrementConstant
        }

        private static int[] InstructionTiming = new int[] {
            8,  7,  3,  4,  6,  4,  6,  7,  3,  2,  2,  2,  7,  5,  7,  6,
            2,  7,  7,  4,  6,  4,  6,  7,  2,  5,  2,  2,  7,  5,  7,  6,
            7,  7,  3,  4,  4,  4,  6,  7,  3,  2,  2,  2,  5,  5,  7,  6,
            2,  7,  7,  2,  4,  4,  6,  7,  2,  5,  2,  2,  5,  5,  7,  6,
            7,  7,  3,  4,  9,  4,  6,  7,  3,  2,  2,  2,  4,  5,  7,  6,
            2,  7,  7,  5,  3,  4,  6,  7,  2,  5,  3,  2,  2,  5,  7,  6,
            7,  7,  2,  2,  4,  4,  6,  7,  3,  2,  2,  2,  7,  5,  7,  6,
            2,  7,  7, 17,  4,  4,  6,  7,  2,  5,  3,  2,  7,  5,  7,  6,
            2,  7,  2,  7,  4,  4,  4,  7,  2,  2,  2,  2,  5,  5,  5,  6,
            2,  7,  7,  8,  4,  4,  4,  7,  2,  5,  2,  2,  5,  5,  5,  6,
            2,  7,  2,  7,  4,  4,  4,  7,  2,  2,  2,  2,  5,  5,  5,  6,
            2,  7,  7,  8,  4,  4,  4,  7,  2,  5,  2,  2,  5,  5,  5,  6,
            2,  7,  2, 17,  4,  4,  6,  7,  2,  2,  2,  2,  5,  5,  7,  6,
            2,  7,  7, 17,  3,  4,  6,  7,  2,  5,  3,  2,  2,  5,  7,  6,
            2,  7,  2, 17,  4,  4,  6,  7,  2,  2,  2,  2,  5,  5,  7,  6,
            2,  7,  7, 17,  2,  4,  6,  7,  2,  5,  3,  2,  2,  5,  7,  6
        };

        // 6502 Registers
        private byte m_A;
        private byte m_X;
        private byte m_Y;
        private byte m_S;
        private ushort m_PC;
        public static ushort CurrentPC;

        // Block Transfer Registers
        private BlockTransferMode m_TransferMode;
        private ushort m_TransferSrc;
        private ushort m_TransferDest;
        private ushort m_TransferCount;

        // Status Flags
        private bool m_NFlag;
        private bool m_VFlag;
        private bool m_TFlag;
        private bool m_DFlag;
        private bool m_IFlag;
        private bool m_ZFlag;
        private bool m_CFlag;

        // HuC6580 Specific Functions
        private bool m_HiSpeed;
        public int m_Clock;
        private int m_AdvanceClock;
        private byte[] m_MPR = new byte[8];
        private MemoryBank[] m_Bank = new MemoryBank[8];
        private MemoryBank m_IOPage;
        private MemoryBank m_ZeroPage;

        private BUS BUS;

        public HuC6280(BUS bus)
        {
            BUS = bus;
        }

        public void Reset()
        {
            m_TransferMode = BlockTransferMode.NoTransferActive;
            m_HiSpeed = false;

            m_NFlag = false;
            m_VFlag = false;
            m_TFlag = false;
            m_DFlag = false;
            m_IFlag = true;
            m_ZFlag = false;
            m_CFlag = false;

            m_A = 0;
            m_X = 0;
            m_Y = 0;
            m_S = 0;

            m_Clock = 0;
            m_AdvanceClock = 0;

            m_MPR = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            m_Bank = new MemoryBank[8] { GetBank(0), GetBank(0), GetBank(0), GetBank(0), GetBank(0), GetBank(0), GetBank(0), GetBank(0) };

            m_IOPage = GetBank(0xFF);
            m_ZeroPage = GetBank(0xF8);

            m_PC = Read16((ushort)IRQVector.VECTOR_RESET);
        }

        public bool IRQ2Waiting()
        {
            return BUS.IRQ2Waiting();
        }

        public bool IRQ1Waiting()
        {
            return BUS.IRQ1Waiting();
        }

        public bool TimerWaiting()
        {
            return BUS.TimerWaiting();
        }

        // BUS based Reads / Writes
        public MemoryBank GetBank(byte bank)
        {
            return BUS.GetBank(bank);
        }

        private void DoIRQ(IRQVector v)
        {
            Push16(m_PC);
            Push8(GetP(false));

            m_IFlag = true;
            m_TFlag = false;
            m_DFlag = false;

            m_PC = Read16((ushort)v);

            m_AdvanceClock += 7;
        }

        public void cycle()
        {
            while (m_Clock > 0)
            {
                m_AdvanceClock = 0;
                if (m_TransferMode != BlockTransferMode.NoTransferActive)
                {
                    switch (m_TransferMode)
                    {
                        case BlockTransferMode.DecrementDecrement:
                            Write8(m_TransferDest--, Read8(m_TransferSrc--));
                            break;
                        case BlockTransferMode.IncrementConstant:
                            Write8(m_TransferDest, Read8(m_TransferSrc++));
                            break;
                        case BlockTransferMode.IncrementIncrement:
                            Write8(m_TransferDest++, Read8(m_TransferSrc++));
                            break;
                        case BlockTransferMode.IncrementAlternate_Up:
                            Write8(m_TransferDest++, Read8(m_TransferSrc++));
                            m_TransferMode = BlockTransferMode.IncrementAlternate_Down;
                            break;
                        case BlockTransferMode.IncrementAlternate_Down:
                            Write8(m_TransferDest--, Read8(m_TransferSrc++));
                            m_TransferMode = BlockTransferMode.IncrementAlternate_Up;
                            break;
                        case BlockTransferMode.AlternateIncrement_Up:
                            Write8(m_TransferDest++, Read8(m_TransferSrc++));
                            m_TransferMode = BlockTransferMode.AlternateIncrement_Down;
                            break;
                        case BlockTransferMode.AlternateIncrement_Down:
                            Write8(m_TransferDest++, Read8(m_TransferSrc--));
                            m_TransferMode = BlockTransferMode.AlternateIncrement_Up;
                            break;
                    }

                    m_AdvanceClock += 6;

                    if (--m_TransferCount == 0)
                    {
                        m_TransferMode = BlockTransferMode.NoTransferActive;
                    }
                }
                else
                {
                    if (m_IFlag || !TestIRQs())
                        OpExecute();
                }

                // DECREMENT THE CLOCK CATCH UP
                m_Clock -= m_HiSpeed ? m_AdvanceClock : (m_AdvanceClock << 1);
            }
        }

        private bool TestIRQs()
        {
            if (IRQ2Waiting())
                DoIRQ(IRQVector.VECTOR_IRQ2);
            else if (IRQ1Waiting())
                DoIRQ(IRQVector.VECTOR_IRQ1);
            else if (TimerWaiting())
                DoIRQ(IRQVector.VECTOR_TIMER);
            else
                return false;

            return true;
        }

        private byte GetP(bool BFlag)
        {
            return (byte)((m_NFlag ? 0x80 : 0) |
                            (m_VFlag ? 0x40 : 0) |
                            (m_TFlag ? 0x20 : 0) |
                            (BFlag ? 0x10 : 0) |
                            (m_DFlag ? 0x08 : 0) |
                            (m_IFlag ? 0x04 : 0) |
                            (m_ZFlag ? 0x02 : 0) |
                            (m_CFlag ? 0x01 : 0));
        }

        private void SetP(byte value)
        {
            m_NFlag = (value & 0x80) != 0;
            m_VFlag = (value & 0x40) != 0;
            m_TFlag = (value & 0x20) != 0;
            m_DFlag = (value & 0x08) != 0;
            m_IFlag = (value & 0x04) != 0;
            m_ZFlag = (value & 0x02) != 0;
            m_CFlag = (value & 0x01) != 0;
        }

        private void Write8(ushort address, byte value)
        {
            m_Bank[address >> 13].WriteAt(address & 0x1FFF, value);
        }

        private byte Read8(ushort address)
        {
            return m_Bank[address >> 13].ReadAt(address & 0x1FFF);
        }

        private ushort Read16(ushort address)
        {
            return (ushort)(Read8(address) | (Read8((ushort)(address + 1)) << 8));
        }

        private void Push8(byte value)
        {
            m_ZeroPage.WriteAt((ushort)(0x100 | (m_S--)), value);
        }

        private byte Pop8()
        {
            return m_ZeroPage.ReadAt((ushort)(0x100 | (++m_S)));
        }

        private void Push16(ushort value)
        {
            Push8((byte)(value >> 8));
            Push8((byte)value);
        }

        private ushort Pop16()
        {
            ushort value = Pop8();
            value |= (ushort)(Pop8() << 8);

            return value;
        }

        private byte ReadImmediate8()
        {
            return Read8(m_PC++);
        }

        private ushort ReadImmediate16()
        {
            ushort lo = Read8(m_PC++);

            return (ushort)((Read8(m_PC++) << 8) | lo);
        }

        // Zero Page Specific Functions (locked to ram)
        private byte Peek_ZP8(byte address)
        {
            return m_ZeroPage.ReadAt(address);
        }

        private void Poke_ZP8(byte address, byte data)
        {
            m_ZeroPage.WriteAt(address, data);
        }

        private ushort Peek_ZP16(byte address)
        {
            ushort addr = m_ZeroPage.ReadAt(address++);
            addr |= (ushort)(m_ZeroPage.ReadAt(address) << 8);

            return addr;
        }

        // --- EFFECTIVE ADDRESSES

        private ushort Eff_ABSX(ushort address)
        {
            return (ushort)(address + m_X);
        }
        private ushort Eff_ABSY(ushort address)
        {
            return (ushort)(address + m_Y);
        }
        private ushort Eff_IND(byte index)
        {
            return Peek_ZP16(index);
        }
        private ushort Eff_INX(byte index)
        {
            return Peek_ZP16((byte)(index + m_X));
        }
        private ushort Eff_INY(byte index)
        {
            return (ushort)(Peek_ZP16(index) + m_Y);
        }

        // Easy read instructions

        private byte Read_ZP()
        {
            return Peek_ZP8(ReadImmediate8());
        }
        private byte Read_ZPX()
        {
            return Peek_ZP8((byte)(ReadImmediate8() + m_X));
        }
        private byte Read_ZPY()
        {
            return Peek_ZP8((byte)(ReadImmediate8() + m_Y));
        }
        private byte Read_IND()
        {
            return Read8(Eff_IND(ReadImmediate8()));
        }
        private byte Read_INX()
        {
            return Read8(Eff_INX(ReadImmediate8()));
        }
        private byte Read_INY()
        {
            return Read8(Eff_INY(ReadImmediate8()));
        }
        private byte Read_ABS()
        {
            return Read8(ReadImmediate16());
        }
        private byte Read_ABSX()
        {
            return Read8(Eff_ABSX(ReadImmediate16()));
        }
        private byte Read_ABSY()
        {
            return Read8(Eff_ABSY(ReadImmediate16()));
        }

        // Easy write instructions

        private void Write_ZP(byte data)
        {
            Poke_ZP8(ReadImmediate8(), data);
        }
        private void Write_ZPX(byte data)
        {
            Poke_ZP8((byte)(ReadImmediate8() + m_X), data);
        }
        private void Write_ZPY(byte data)
        {
            Poke_ZP8((byte)(ReadImmediate8() + m_Y), data);
        }
        private void Write_IND(byte data)
        {
            Write8(Eff_IND(ReadImmediate8()), data);
        }
        private void Write_INX(byte data)
        {
            Write8(Eff_INX(ReadImmediate8()), data);
        }
        private void Write_INY(byte data)
        {
            Write8(Eff_INY(ReadImmediate8()), data);
        }
        private void Write_ABS(byte data)
        {
            Write8(ReadImmediate16(), data);
        }
        private void Write_ABSX(byte data)
        {
            Write8(Eff_ABSX(ReadImmediate16()), data);
        }
        private void Write_ABSY(byte data)
        {
            Write8(Eff_ABSY(ReadImmediate16()), data);
        }

        // Memory manager instructions
        private void TAM(byte op)
        {
            MemoryBank bank = GetBank(m_A);

            if ((op & 0x80) != 0) { m_MPR[7] = m_A; m_Bank[7] = bank; }
            if ((op & 0x40) != 0) { m_MPR[6] = m_A; m_Bank[6] = bank; }
            if ((op & 0x20) != 0) { m_MPR[5] = m_A; m_Bank[5] = bank; }
            if ((op & 0x10) != 0) { m_MPR[4] = m_A; m_Bank[4] = bank; }
            if ((op & 0x08) != 0) { m_MPR[3] = m_A; m_Bank[3] = bank; }
            if ((op & 0x04) != 0) { m_MPR[2] = m_A; m_Bank[2] = bank; }
            if ((op & 0x02) != 0) { m_MPR[1] = m_A; m_Bank[1] = bank; }
            if ((op & 0x01) != 0) { m_MPR[0] = m_A; m_Bank[0] = bank; }
        }
        private void TMA(byte op)
        {
            if ((op & 0x80) != 0) m_A = m_MPR[7];
            else if ((op & 0x40) != 0) m_A = m_MPR[6];
            else if ((op & 0x20) != 0) m_A = m_MPR[5];
            else if ((op & 0x10) != 0) m_A = m_MPR[4];
            else if ((op & 0x08) != 0) m_A = m_MPR[3];
            else if ((op & 0x04) != 0) m_A = m_MPR[2];
            else if ((op & 0x02) != 0) m_A = m_MPR[1];
            else if ((op & 0x01) != 0) m_A = m_MPR[0];
        }

        // Branch template
        private void BRA(bool condition)
        {
            if (condition)
            {
                sbyte offset = (sbyte)ReadImmediate8();
                m_PC += (ushort)offset;
                m_AdvanceClock += 2;
            }
            else
            {
                m_PC++;
            }
        }

        // Compare template
        private void CMP(int data)
        {
            m_NFlag = (data & 0x80) != 0;
            m_ZFlag = (data & 0xFF) == 0;
            m_CFlag = data >= 0;
        }

        // TFlag Helpers
        private byte TInput()
        {
            return m_TFlag ? Peek_ZP8(m_X) : m_A;
        }
        private void TOutput(byte data)
        {
            if (m_TFlag)
            {
                Poke_ZP8(m_X, data);
                m_AdvanceClock += 2;
            }
            else
            {
                m_A = data;
            }
        }

        // Instruction base code
        private void ADC(byte right)
        {
            byte left = TInput();

            if (m_DFlag)
            {
                int lo = left + right + (m_CFlag ? 1 : 0);

                if ((lo & 0x0F) > 0x09)
                    lo += 0x06;
                if ((lo & 0xF0) > 0x90)
                    lo += 0x60;

                // SET OVERFLOW HERE?
                left = (byte)lo;
                m_CFlag = lo >= 0x100;
                FlagNZ(m_A);
            }
            else
            {
                int data = left + right + (m_CFlag ? 1 : 0);

                m_VFlag = ((left ^ ~right) & (left ^ data) & 0x80) != 0;
                m_CFlag = data >= 0x100;
                left = (byte)data;
                FlagNZ(left);
            }

            TOutput(left);
        }
        private void SBC(byte right)
        {
            if (m_DFlag)
            {
                int lo = m_A - right - (m_CFlag ? 0 : 1);

                if ((lo & 0x0F) > 0x09)
                    lo -= 0x06;
                if ((lo & 0xF0) > 0x90)
                    lo -= 0x60;

                // SET OVERFLOW HERE?
                m_A = (byte)lo;
                m_CFlag = lo > 0;
                FlagNZ(m_A);
            }
            else
            {
                int data = m_A - right - (m_CFlag ? 0 : 1);

                m_VFlag = ((m_A ^ right) & (m_A ^ data) & 0x80) != 0;
                m_CFlag = data >= 0;
                m_A = (byte)data;
                FlagNZ(m_A);
            }
        }
        private byte ASL(byte data)
        {
            m_CFlag = (data & 0x80) != 0;
            data <<= 1;
            FlagNZ(data);
            return data;
        }
        private byte LSR(byte data)
        {
            m_CFlag = (data & 0x01) != 0;
            data >>= 1;
            FlagNZ(data);
            return data;
        }
        private byte ROL(byte data)
        {
            bool newC = (data & 0x80) != 0;
            data = (byte)((data << 1) | (m_CFlag ? 1 : 0));
            m_CFlag = newC;
            FlagNZ(data);
            return data;
        }
        private byte ROR(byte data)
        {
            bool newC = (data & 0x01) != 0;
            data = (byte)((data >> 1) | (m_CFlag ? 0x80 : 0));
            m_CFlag = newC;
            FlagNZ(data);
            return data;
        }
        private void ORA(byte data)
        {
            data |= TInput();
            FlagNZ(data);
            TOutput(data);
        }
        private void EOR(byte data)
        {
            data ^= TInput();
            FlagNZ(data);
            TOutput(data);
        }
        private void AND(byte data)
        {
            data &= TInput();
            FlagNZ(data);
            TOutput(data);
        }
        private void BIT(byte mask, byte val)
        {
            m_NFlag = (val & 0x80) != 0;
            m_VFlag = (val & 0x40) != 0;
            val &= mask;
            m_ZFlag = val == 0;
        }
        private byte TSB(byte data)
        {
            m_NFlag = (data & 0x80) != 0;
            m_VFlag = (data & 0x40) != 0;
            data |= m_A;
            m_ZFlag = data == 0;
            return data;
        }
        private byte TRB(byte data)
        {
            m_NFlag = (data & 0x80) != 0;
            m_VFlag = (data & 0x40) != 0;
            data &= (byte)~m_A;
            m_ZFlag = data == 0;
            return data;
        }

        private void FlagNZ(byte data)
        {
            m_NFlag = (data & 0x80) != 0;
            m_ZFlag = data == 0;
        }
        private void StartBlock(BlockTransferMode mode)
        {
            m_TransferMode = mode;
            m_TransferSrc = ReadImmediate16();
            m_TransferDest = ReadImmediate16();
            m_TransferCount = ReadImmediate16();
        }

        private void OpExecute()
        {
            CurrentPC = m_PC;
            byte op = ReadImmediate8();

            m_AdvanceClock += InstructionTiming[op];

            switch ((InstructionOpcode)op)
            {
                // --- ADD WITH CARRY INSTRUCTIONS ---
                case InstructionOpcode.INS_ADC_IMM:
                    ADC(ReadImmediate8());
                    break;
                case InstructionOpcode.INS_ADC_ZP:
                    ADC(Read_ZP());
                    break;
                case InstructionOpcode.INS_ADC_ZPX:
                    ADC(Read_ZPX());
                    break;
                case InstructionOpcode.INS_ADC_IND:
                    ADC(Read_IND());
                    break;
                case InstructionOpcode.INS_ADC_INX:
                    ADC(Read_INX());
                    break;
                case InstructionOpcode.INS_ADC_INY:
                    ADC(Read_INY());
                    break;
                case InstructionOpcode.INS_ADC_ABS:
                    ADC(Read_ABS());
                    break;
                case InstructionOpcode.INS_ADC_ABSX:
                    ADC(Read_ABSX());
                    break;
                case InstructionOpcode.INS_ADC_ABSY:
                    ADC(Read_ABSY());
                    break;

                // --- SUBTRACT WITH CARRY INSTRUCTIONS ---
                case InstructionOpcode.INS_SBC_IMM:
                    SBC(ReadImmediate8());
                    break;
                case InstructionOpcode.INS_SBC_ZP:
                    SBC(Read_ZP());
                    break;
                case InstructionOpcode.INS_SBC_ZPX:
                    SBC(Read_ZPX());
                    break;
                case InstructionOpcode.INS_SBC_IND:
                    SBC(Read_IND());
                    break;
                case InstructionOpcode.INS_SBC_INX:
                    SBC(Read_INX());
                    break;
                case InstructionOpcode.INS_SBC_INY:
                    SBC(Read_INY());
                    break;
                case InstructionOpcode.INS_SBC_ABS:
                    SBC(Read_ABS());
                    break;
                case InstructionOpcode.INS_SBC_ABSX:
                    SBC(Read_ABSX());
                    break;
                case InstructionOpcode.INS_SBC_ABSY:
                    SBC(Read_ABSY());
                    break;

                // --- BLOCK TRANSFER OPERATIONS ---
                case InstructionOpcode.INS_TII:
                    StartBlock(BlockTransferMode.IncrementIncrement);
                    break;
                case InstructionOpcode.INS_TIA:
                    StartBlock(BlockTransferMode.IncrementAlternate_Up);
                    break;
                case InstructionOpcode.INS_TAI:
                    StartBlock(BlockTransferMode.AlternateIncrement_Up);
                    break;
                case InstructionOpcode.INS_TDD:
                    StartBlock(BlockTransferMode.DecrementDecrement);
                    break;
                case InstructionOpcode.INS_TIN:
                    StartBlock(BlockTransferMode.IncrementConstant);
                    break;

                // --- COMPARISON INSTRUCTIONS ---
                case InstructionOpcode.INS_CMP_IMM:
                    CMP(m_A - ReadImmediate8());
                    break;
                case InstructionOpcode.INS_CMP_ZP:
                    CMP(m_A - Read_ZP());
                    break;
                case InstructionOpcode.INS_CMP_ZPX:
                    CMP(m_A - Read_ZPX());
                    break;
                case InstructionOpcode.INS_CMP_ABS:
                    CMP(m_A - Read_ABS());
                    break;
                case InstructionOpcode.INS_CMP_ABSX:
                    CMP(m_A - Read_ABSX());
                    break;
                case InstructionOpcode.INS_CMP_ABSY:
                    CMP(m_A - Read_ABSY());
                    break;
                case InstructionOpcode.INS_CMP_IND:
                    CMP(m_A - Read_IND());
                    break;
                case InstructionOpcode.INS_CMP_INX:
                    CMP(m_A - Read_INX());
                    break;
                case InstructionOpcode.INS_CMP_INY:
                    CMP(m_A - Read_INY());
                    break;

                case InstructionOpcode.INS_CPY_IMM:
                    CMP(m_Y - ReadImmediate8());
                    break;
                case InstructionOpcode.INS_CPY_ZP:
                    CMP(m_Y - Read_ZP());
                    break;
                case InstructionOpcode.INS_CPY_ABS:
                    CMP(m_Y - Read_ABS());
                    break;

                case InstructionOpcode.INS_CPX_IMM:
                    CMP(m_X - ReadImmediate8());
                    break;
                case InstructionOpcode.INS_CPX_ZP:
                    CMP(m_X - Read_ZP());
                    break;
                case InstructionOpcode.INS_CPX_ABS:
                    CMP(m_X - Read_ABS());
                    break;

                // --- BINARY SHIFT INSTRUCTIONS
                case InstructionOpcode.INS_ASL_A:
                    m_A = ASL(m_A);
                    break;
                case InstructionOpcode.INS_ASL_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, ASL(Peek_ZP8(address)));
                    }
                    break;
                case InstructionOpcode.INS_ASL_ZPX:
                    {
                        byte address = (byte)(m_X + ReadImmediate8());
                        Poke_ZP8(address, ASL(Peek_ZP8(address)));
                    }
                    break;
                case InstructionOpcode.INS_ASL_ABS:
                    {
                        ushort address = ReadImmediate16();
                        Write8(address, ASL(Read8(address)));
                    }
                    break;
                case InstructionOpcode.INS_ASL_ABSX:
                    {
                        ushort address = Eff_ABSX(ReadImmediate16());
                        Write8(address, ASL(Read8(address)));
                    }
                    break;
                case InstructionOpcode.INS_LSR_A:
                    m_A = LSR(m_A);
                    break;
                case InstructionOpcode.INS_LSR_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, LSR(Peek_ZP8(address)));
                    }
                    break;
                case InstructionOpcode.INS_LSR_ZPX:
                    {
                        byte address = (byte)(m_X + ReadImmediate8());
                        Poke_ZP8(address, LSR(Peek_ZP8(address)));
                    }
                    break;
                case InstructionOpcode.INS_LSR_ABSX:
                    {
                        ushort address = Eff_ABSX(ReadImmediate16());
                        Write8(address, LSR(Read8(address)));
                    }
                    break;
                case InstructionOpcode.INS_LSR_ABS:
                    {
                        ushort address = ReadImmediate16();
                        Write8(address, LSR(Read8(address)));
                    }
                    break;

                // --- BINARY ROTATION INSTRUCTIONS ---
                case InstructionOpcode.INS_ROL_A:
                    m_A = ROL(m_A);
                    break;
                case InstructionOpcode.INS_ROL_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, ROL(Peek_ZP8(address)));
                    }
                    break;
                case InstructionOpcode.INS_ROL_ZPX:
                    {
                        byte address = (byte)(m_X + ReadImmediate8());
                        Poke_ZP8(address, ROL(Peek_ZP8(address)));
                    }
                    break;
                case InstructionOpcode.INS_ROL_ABS:
                    {
                        ushort address = ReadImmediate16();
                        Write8(address, ROL(Read8(address)));
                    }
                    break;
                case InstructionOpcode.INS_ROL_ABSX:
                    {
                        ushort address = Eff_ABSX(ReadImmediate16());
                        Write8(address, ROL(Read8(address)));
                    }
                    break;
                case InstructionOpcode.INS_ROR_A:
                    m_A = ROR(m_A);
                    break;
                case InstructionOpcode.INS_ROR_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, ROR(Peek_ZP8(address)));
                    }
                    break;
                case InstructionOpcode.INS_ROR_ZPX:
                    {
                        byte address = (byte)(m_X + ReadImmediate8());
                        Poke_ZP8(address, ROR(Peek_ZP8(address)));
                    }
                    break;
                case InstructionOpcode.INS_ROR_ABS:
                    {
                        ushort address = ReadImmediate16();
                        Write8(address, ROR(Read8(address)));
                    }
                    break;
                case InstructionOpcode.INS_ROR_ABSX:
                    {
                        ushort address = Eff_ABSX(ReadImmediate16());
                        Write8(address, ROR(Read8(address)));
                    }
                    break;

                // --- DECREMENTING INSTRUCTIONS ---
                case InstructionOpcode.INS_DEC_A:
                    FlagNZ(--m_A);
                    break;
                case InstructionOpcode.INS_DEY:
                    FlagNZ(--m_Y);
                    break;
                case InstructionOpcode.INS_DEX:
                    FlagNZ(--m_X);
                    break;
                case InstructionOpcode.INS_DEC_ZP:
                    {
                        byte address = ReadImmediate8();
                        byte data = Peek_ZP8(address);
                        FlagNZ(--data);
                        Poke_ZP8(address, data);
                    }
                    break;
                case InstructionOpcode.INS_DEC_ABS:
                    {
                        ushort address = ReadImmediate16();
                        byte data = Read8(address);
                        FlagNZ(--data);
                        Write8(address, data);
                    }
                    break;
                case InstructionOpcode.INS_DEC_ZPX:
                    {
                        byte address = (byte)(m_X + ReadImmediate8());
                        byte data = Peek_ZP8(address);
                        FlagNZ(--data);
                        Poke_ZP8(address, data);
                    }
                    break;
                case InstructionOpcode.INS_DEC_ABSX:
                    {
                        ushort address = Eff_ABSX(ReadImmediate16());
                        byte data = Read8(address);
                        FlagNZ(--data);
                        Write8(address, data);
                    }
                    break;

                // --- INCREMENTING INSTRUCTIONS ---
                case InstructionOpcode.INS_INC_A:
                    FlagNZ(++m_A);
                    break;
                case InstructionOpcode.INS_INY:
                    FlagNZ(++m_Y);
                    break;
                case InstructionOpcode.INS_INX:
                    FlagNZ(++m_X);
                    break;
                case InstructionOpcode.INS_INC_ZP:
                    {
                        byte address = ReadImmediate8();
                        byte data = Peek_ZP8(address);
                        FlagNZ(++data);
                        Poke_ZP8(address, data);
                    }
                    break;
                case InstructionOpcode.INS_INC_ABS:
                    {
                        ushort address = ReadImmediate16();
                        byte data = Read8(address);
                        FlagNZ(++data);
                        Write8(address, data);
                    }
                    break;
                case InstructionOpcode.INS_INC_ZPX:
                    {
                        byte address = (byte)(m_X + ReadImmediate8());
                        byte data = Peek_ZP8(address);
                        FlagNZ(++data);
                        Poke_ZP8(address, data);
                    }
                    break;
                case InstructionOpcode.INS_INC_ABSX:
                    {
                        ushort address = Eff_ABSX(ReadImmediate16());
                        byte data = Read8(address);
                        FlagNZ(++data);
                        Write8(address, data);
                    }
                    break;

                // --- BINARY OR OPERATIONS ---
                case InstructionOpcode.INS_ORA_IMM:
                    ORA(ReadImmediate8());
                    break;
                case InstructionOpcode.INS_ORA_ABS:
                    ORA(Read_ABS());
                    break;
                case InstructionOpcode.INS_ORA_ABSY:
                    ORA(Read_ABSY());
                    break;
                case InstructionOpcode.INS_ORA_ABSX:
                    ORA(Read_ABSX());
                    break;
                case InstructionOpcode.INS_ORA_ZP:
                    ORA(Read_ZP());
                    break;
                case InstructionOpcode.INS_ORA_ZPX:
                    ORA(Read_ZPX());
                    break;
                case InstructionOpcode.INS_ORA_IND:
                    ORA(Read_IND());
                    break;
                case InstructionOpcode.INS_ORA_INX:
                    ORA(Read_INX());
                    break;
                case InstructionOpcode.INS_ORA_INY:
                    ORA(Read_INY());
                    break;

                // --- BINARY EXCLUSIVE OR OPERATIONS ---
                case InstructionOpcode.INS_EOR_IMM:
                    EOR(ReadImmediate8());
                    break;
                case InstructionOpcode.INS_EOR_ABS:
                    EOR(Read_ABS());
                    break;
                case InstructionOpcode.INS_EOR_ABSY:
                    EOR(Read_ABSY());
                    break;
                case InstructionOpcode.INS_EOR_ABSX:
                    EOR(Read_ABSX());
                    break;
                case InstructionOpcode.INS_EOR_ZP:
                    EOR(Read_ZP());
                    break;
                case InstructionOpcode.INS_EOR_ZPX:
                    EOR(Read_ZPX());
                    break;
                case InstructionOpcode.INS_EOR_IND:
                    EOR(Read_IND());
                    break;
                case InstructionOpcode.INS_EOR_INX:
                    EOR(Read_INX());
                    break;
                case InstructionOpcode.INS_EOR_INY:
                    EOR(Read_INY());
                    break;

                // --- BINARY AND OPERATIONS ---
                case InstructionOpcode.INS_AND_IMM:
                    AND(ReadImmediate8());
                    break;
                case InstructionOpcode.INS_AND_ABS:
                    AND(Read_ABS());
                    break;
                case InstructionOpcode.INS_AND_ABSY:
                    AND(Read_ABSY());
                    break;
                case InstructionOpcode.INS_AND_ABSX:
                    AND(Read_ABSX());
                    break;
                case InstructionOpcode.INS_AND_ZP:
                    AND(Read_ZP());
                    break;
                case InstructionOpcode.INS_AND_ZPX:
                    AND(Read_ZPX());
                    break;
                case InstructionOpcode.INS_AND_IND:
                    AND(Read_IND());
                    break;
                case InstructionOpcode.INS_AND_INX:
                    AND(Read_INX());
                    break;
                case InstructionOpcode.INS_AND_INY:
                    AND(Read_INY());
                    break;

                // --- STORE OPERATIONS ---
                case InstructionOpcode.INS_STX_ABS:
                    Write_ABS(m_X);
                    break;
                case InstructionOpcode.INS_STX_ZP:
                    Write_ZP(m_X);
                    break;
                case InstructionOpcode.INS_STX_ZPY:
                    Write_ZPY(m_X);
                    break;

                case InstructionOpcode.INS_STY_ABS:
                    Write_ABS(m_Y);
                    break;
                case InstructionOpcode.INS_STY_ZP:
                    Write_ZP(m_Y);
                    break;
                case InstructionOpcode.INS_STY_ZPX:
                    Write_ZPX(m_Y);
                    break;

                case InstructionOpcode.INS_STA_ABS:
                    Write_ABS(m_A);
                    break;
                case InstructionOpcode.INS_STA_ABSY:
                    Write_ABSY(m_A);
                    break;
                case InstructionOpcode.INS_STA_ABSX:
                    Write_ABSX(m_A);
                    break;
                case InstructionOpcode.INS_STA_ZP:
                    Write_ZP(m_A);
                    break;
                case InstructionOpcode.INS_STA_ZPX:
                    Write_ZPX(m_A);
                    break;
                case InstructionOpcode.INS_STA_IND:
                    Write_IND(m_A);
                    break;
                case InstructionOpcode.INS_STA_INX:
                    Write_INX(m_A);
                    break;
                case InstructionOpcode.INS_STA_INY:
                    Write_INY(m_A);
                    break;

                case InstructionOpcode.INS_STZ_ZP:
                    Write_ZP(0);
                    break;
                case InstructionOpcode.INS_STZ_ZPX:
                    Write_ZPX(0);
                    break;
                case InstructionOpcode.INS_STZ_ABS:
                    Write_ABS(0);
                    break;
                case InstructionOpcode.INS_STZ_ABSX:
                    Write_ABSX(0);
                    break;

                // --- LOAD OPERATIONS ---
                case InstructionOpcode.INS_LDX_IMM:
                    FlagNZ(m_X = ReadImmediate8());
                    break;
                case InstructionOpcode.INS_LDX_ZP:
                    FlagNZ(m_X = Read_ZP());
                    break;
                case InstructionOpcode.INS_LDX_ZPY:
                    FlagNZ(m_X = Read_ZPY());
                    break;
                case InstructionOpcode.INS_LDX_ABS:
                    FlagNZ(m_X = Read_ABS());
                    break;
                case InstructionOpcode.INS_LDX_ABSY:
                    FlagNZ(m_X = Read_ABSY());
                    break;

                case InstructionOpcode.INS_LDY_IMM:
                    FlagNZ(m_Y = ReadImmediate8());
                    break;
                case InstructionOpcode.INS_LDY_ZP:
                    FlagNZ(m_Y = Read_ZP());
                    break;
                case InstructionOpcode.INS_LDY_ZPX:
                    FlagNZ(m_Y = Read_ZPX());
                    break;
                case InstructionOpcode.INS_LDY_ABS:
                    FlagNZ(m_Y = Read_ABS());
                    break;
                case InstructionOpcode.INS_LDY_ABSX:
                    FlagNZ(m_Y = Read_ABSX());
                    break;

                case InstructionOpcode.INS_LDA_IMM:
                    FlagNZ(m_A = ReadImmediate8());
                    break;
                case InstructionOpcode.INS_LDA_ZP:
                    FlagNZ(m_A = Read_ZP());
                    break;
                case InstructionOpcode.INS_LDA_ZPX:
                    FlagNZ(m_A = Read_ZPX());
                    break;
                case InstructionOpcode.INS_LDA_ABS:
                    FlagNZ(m_A = Read_ABS());
                    break;
                case InstructionOpcode.INS_LDA_ABSX:
                    FlagNZ(m_A = Read_ABSX());
                    break;
                case InstructionOpcode.INS_LDA_ABSY:
                    FlagNZ(m_A = Read_ABSY());
                    break;
                case InstructionOpcode.INS_LDA_IND:
                    FlagNZ(m_A = Read_IND());
                    break;
                case InstructionOpcode.INS_LDA_INX:
                    FlagNZ(m_A = Read_INX());
                    break;
                case InstructionOpcode.INS_LDA_INY:
                    FlagNZ(m_A = Read_INY());
                    break;

                // --- BIT TESTING INSTRUCTIONS ---
                case InstructionOpcode.INS_TST_ZP:
                    BIT(ReadImmediate8(), Read_ZP());
                    break;
                case InstructionOpcode.INS_TST_ABS:
                    BIT(ReadImmediate8(), Read_ABS());
                    break;
                case InstructionOpcode.INS_TST_ZPX:
                    BIT(ReadImmediate8(), Read_ZPX());
                    break;
                case InstructionOpcode.INS_TST_ABSX:
                    BIT(ReadImmediate8(), Read_ABSX());
                    break;
                case InstructionOpcode.INS_BIT_IMM:
                    BIT(m_A, ReadImmediate8());
                    break;
                case InstructionOpcode.INS_BIT_ZP:
                    BIT(m_A, Read_ZP());
                    break;
                case InstructionOpcode.INS_BIT_ABS:
                    BIT(m_A, Read_ABS());
                    break;
                case InstructionOpcode.INS_BIT_ZPX:
                    BIT(m_A, Read_ZPX());
                    break;
                case InstructionOpcode.INS_BIT_ABSX:
                    BIT(m_A, Read_ABSX());
                    break;

                // --- BIT SET/RESET TRANSFER INSTRUCTIONS ---
                case InstructionOpcode.INS_TRB_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, TRB(Peek_ZP8(address)));
                    }
                    break;
                case InstructionOpcode.INS_TRB_ABS:
                    {
                        ushort address = ReadImmediate16();
                        Write8(address, TRB(Read8(address)));
                    }
                    break;
                case InstructionOpcode.INS_TSB_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, TSB(Peek_ZP8(address)));
                    }
                    break;
                case InstructionOpcode.INS_TSB_ABS:
                    {
                        ushort address = ReadImmediate16();
                        Write8(address, TSB(Read8(address)));
                    }
                    break;

                // --- JUMP INSTRUCTIONS ---
                case InstructionOpcode.INS_BRK:
                    Push16(++m_PC);

                    Push8(GetP(true));

                    m_IFlag = true;
                    m_DFlag = false;
                    m_PC = Read16((ushort)IRQVector.VECTOR_BRK);
                    break;
                case InstructionOpcode.INS_RTI:
                    SetP(Pop8());
                    m_PC = Pop16();
                    break;
                case InstructionOpcode.INS_RTS:
                    m_PC = Pop16();
                    m_PC++;
                    break;
                case InstructionOpcode.INS_JMP_ABS:
                    m_PC = ReadImmediate16();
                    break;
                case InstructionOpcode.INS_JMP_INA:
                    m_PC = Read16(ReadImmediate16());
                    break;
                case InstructionOpcode.INS_JMP_INAX:
                    m_PC = Read16((ushort)(ReadImmediate16() + m_X));
                    break;
                case InstructionOpcode.INS_JSR_ABS:
                    {
                        ushort newPC = ReadImmediate16();
                        Push16(--m_PC);
                        m_PC = newPC;
                    }
                    break;

                // --- STACK OPERATIONS ---
                case InstructionOpcode.INS_PHP:
                    m_TFlag = false;
                    Push8(GetP(true));
                    break;
                case InstructionOpcode.INS_PHA:
                    Push8(m_A);
                    break;
                case InstructionOpcode.INS_PHY:
                    Push8(m_Y);
                    break;
                case InstructionOpcode.INS_PHX:
                    Push8(m_X);
                    break;
                case InstructionOpcode.INS_PLP:
                    SetP(Pop8());
                    break;
                case InstructionOpcode.INS_PLA:
                    FlagNZ(m_A = Pop8());
                    break;
                case InstructionOpcode.INS_PLY:
                    FlagNZ(m_Y = Pop8());
                    break;
                case InstructionOpcode.INS_PLX:
                    FlagNZ(m_X = Pop8());
                    break;

                // --- RELATIVE BRANCHING INSTRUCTIONS ---
                case InstructionOpcode.INS_BRA_REL:
                    m_PC = (ushort)((sbyte)ReadImmediate8() + m_PC);
                    break;
                case InstructionOpcode.INS_BSR_REL:
                    {
                        Push16(m_PC);
                        m_PC = (ushort)((sbyte)ReadImmediate8() + m_PC);
                    }
                    break;
                case InstructionOpcode.INS_BBR0_ZP_REL:
                    BRA((0x01 & Read_ZP()) == 0);
                    break;
                case InstructionOpcode.INS_BBR1_ZP_REL:
                    BRA((0x02 & Read_ZP()) == 0);
                    break;
                case InstructionOpcode.INS_BBR2_ZP_REL:
                    BRA((0x04 & Read_ZP()) == 0);
                    break;
                case InstructionOpcode.INS_BRR3_ZP_REL:
                    BRA((0x08 & Read_ZP()) == 0);
                    break;
                case InstructionOpcode.INS_BBR4_ZP_REL:
                    BRA((0x10 & Read_ZP()) == 0);
                    break;
                case InstructionOpcode.INS_BBR5_ZP_REL:
                    BRA((0x20 & Read_ZP()) == 0);
                    break;
                case InstructionOpcode.INS_BBR6_ZP_REL:
                    BRA((0x40 & Read_ZP()) == 0);
                    break;
                case InstructionOpcode.INS_BBR7_ZP_REL:
                    BRA((0x80 & Read_ZP()) == 0);
                    break;
                case InstructionOpcode.INS_BBS0_ZP_REL:
                    BRA((0x01 & Read_ZP()) != 0);
                    break;
                case InstructionOpcode.INS_BBS1_ZP_REL:
                    BRA((0x02 & Read_ZP()) != 0);
                    break;
                case InstructionOpcode.INS_BBS2_ZP_REL:
                    BRA((0x04 & Read_ZP()) != 0);
                    break;
                case InstructionOpcode.INS_BBS3_ZP_REL:
                    BRA((0x08 & Read_ZP()) != 0);
                    break;
                case InstructionOpcode.INS_BBS4_ZP_REL:
                    BRA((0x10 & Read_ZP()) != 0);
                    break;
                case InstructionOpcode.INS_BBS5_ZP_REL:
                    BRA((0x20 & Read_ZP()) != 0);
                    break;
                case InstructionOpcode.INS_BBS6_ZP_REL:
                    BRA((0x40 & Read_ZP()) != 0);
                    break;
                case InstructionOpcode.INS_BBS7_ZP_REL:
                    BRA((0x80 & Read_ZP()) != 0);
                    break;
                case InstructionOpcode.INS_BPL_REL:
                    BRA(!m_NFlag);
                    break;
                case InstructionOpcode.INS_BMI_REL:
                    BRA(m_NFlag);
                    break;
                case InstructionOpcode.INS_BVC_REL:
                    BRA(!m_VFlag);
                    break;
                case InstructionOpcode.INS_BVS_REL:
                    BRA(m_VFlag);
                    break;
                case InstructionOpcode.INS_BCC_REL:
                    BRA(!m_CFlag);
                    break;
                case InstructionOpcode.INS_BCS_REL:
                    BRA(m_CFlag);
                    break;
                case InstructionOpcode.INS_BNE_REL:
                    BRA(!m_ZFlag);
                    break;
                case InstructionOpcode.INS_BEQ_REL:
                    BRA(m_ZFlag);
                    break;

                // --- BIT SET/RESET INSTRUCTIONS ---
                case InstructionOpcode.INS_RMB0_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) & ~0x01));
                    }
                    break;
                case InstructionOpcode.INS_RMB1_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) & ~0x02));
                    }
                    break;
                case InstructionOpcode.INS_RMB2_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) & ~0x04));
                    }
                    break;
                case InstructionOpcode.INS_RMB3_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) & ~0x08));
                    }
                    break;
                case InstructionOpcode.INS_RMB4_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) & ~0x10));
                    }
                    break;
                case InstructionOpcode.INS_RMB5_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) & ~0x20));
                    }
                    break;
                case InstructionOpcode.INS_RMB6_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) & ~0x40));
                    }
                    break;
                case InstructionOpcode.INS_RMB7_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) & ~0x80));
                    }
                    break;
                case InstructionOpcode.INS_SMB0_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) | 0x01));
                    }
                    break;
                case InstructionOpcode.INS_SMB1_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) | 0x02));
                    }
                    break;
                case InstructionOpcode.INS_SMB2_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) | 0x04));
                    }
                    break;
                case InstructionOpcode.INS_SMB3_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) | 0x08));
                    }
                    break;
                case InstructionOpcode.INS_SMB4_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) | 0x10));
                    }
                    break;
                case InstructionOpcode.INS_SMB5_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) | 0x20));
                    }
                    break;
                case InstructionOpcode.INS_SMB6_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) | 0x40));
                    }
                    break;
                case InstructionOpcode.INS_SMB7_ZP:
                    {
                        byte address = ReadImmediate8();
                        Poke_ZP8(address, (byte)(Peek_ZP8(address) | 0x80));
                    }
                    break;

                // --- SPECIALIZED STORE OPERATIONS ---
                case InstructionOpcode.INS_ST0:
                    m_IOPage.WriteAt(0x0000, ReadImmediate8());
                    break;
                case InstructionOpcode.INS_ST1:
                    m_IOPage.WriteAt(0x0002, ReadImmediate8());
                    break;
                case InstructionOpcode.INS_ST2:
                    m_IOPage.WriteAt(0x0003, ReadImmediate8());
                    break;

                // --- REGISTER SWAP OPERATIONS ---
                case InstructionOpcode.INS_SXY:
                    {
                        byte t = m_X;
                        m_X = m_Y;
                        m_Y = t;
                    }
                    break;
                case InstructionOpcode.INS_SAX:
                    {
                        byte t = m_A;
                        m_A = m_X;
                        m_X = t;
                    }
                    break;
                case InstructionOpcode.INS_SAY:
                    {
                        byte t = m_A;
                        m_A = m_Y;
                        m_Y = t;
                    }
                    break;

                // --- REGISTER TRANSFER INSTRUCTIONS ---
                case InstructionOpcode.INS_TXA:
                    FlagNZ(m_A = m_X);
                    break;
                case InstructionOpcode.INS_TYA:
                    FlagNZ(m_A = m_Y);
                    break;
                case InstructionOpcode.INS_TXS:
                    m_S = m_X;
                    break;
                case InstructionOpcode.INS_TAY:
                    FlagNZ(m_Y = m_A);
                    break;
                case InstructionOpcode.INS_TAX:
                    FlagNZ(m_X = m_A);
                    break;
                case InstructionOpcode.INS_TSX:
                    FlagNZ(m_X = m_S);
                    break;

                // --- REGISTER CLEAR FUNCTIONS ---
                case InstructionOpcode.INS_CLA:
                    m_A = 0;
                    break;
                case InstructionOpcode.INS_CLX:
                    m_X = 0;
                    break;
                case InstructionOpcode.INS_CLY:
                    m_Y = 0;
                    break;

                // --- CPU SPEED CONTROL ---
                case InstructionOpcode.INS_CSL:
                    m_HiSpeed = false;
                    break;
                case InstructionOpcode.INS_CSH:
                    m_HiSpeed = true;
                    break;

                // --- MEMORY MANAGEMENT UNIT CONTROL ---
                case InstructionOpcode.INS_TMA:
                    TMA(ReadImmediate8());
                    break;
                case InstructionOpcode.INS_TAM:
                    TAM(ReadImmediate8());
                    break;

                // --- FLAG RESET/SET FUNCTIONS ---
                case InstructionOpcode.INS_CLC:
                    m_CFlag = false;
                    break;
                case InstructionOpcode.INS_CLI:
                    m_IFlag = false;
                    break;
                case InstructionOpcode.INS_CLV:
                    m_VFlag = false;
                    break;
                case InstructionOpcode.INS_CLD:
                    m_DFlag = false;
                    break;
                case InstructionOpcode.INS_SEC:
                    m_CFlag = true;
                    break;
                case InstructionOpcode.INS_SEI:
                    m_IFlag = true;
                    break;
                case InstructionOpcode.INS_SET:
                    m_TFlag = true;
                    return;
                case InstructionOpcode.INS_SED:
                    m_DFlag = true;
                    break;

                default:
                    break;
            }

            m_TFlag = false;
        }
    }
}
