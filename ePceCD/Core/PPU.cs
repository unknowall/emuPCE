using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ePceCD
{
    public enum DotClock : int
    {
        MHZ_10 = 2,
        MHZ_5 = 4,
        MHZ_7 = 3
    }

    [Serializable]
    public class PPU : IDisposable // HuC6270A
    {
        [Serializable]
        private struct SpriteAttribute
        {
            public int m_X;
            public int m_Y;
            public int m_Pattern;
            //TODO: add CG support, ex: YS I & II
            public bool m_CGPage;

            public bool m_VerticalFlip;
            public bool m_HorizontalFlip;

            public int m_Width;
            public int m_Height;

            public bool m_Priority;
            public int m_Palette;
        }
        private SpriteAttribute[] m_SAT;
        private int m_RenderLine;
        public int CYCLES_PER_LINE = 1368;
        public int SCREEN_WIDTH = 256;
        private ushort[] m_VRAM;
        private int[] PALETTE = new int[512];
        private DotClock m_VCE_DotClock;

        [NonSerialized]
        public IntPtr _screenBufPtr;

        public int[] _screenBuf;
        public bool FrameReady;

        #region REGISTER Vars

        private bool m_DoSAT_DMA;
        private bool m_WaitingIRQ;

        private bool m_VDC_BSY;
        private bool m_VDC_VD;
        private bool m_VDC_DV;
        private bool m_VDC_DS;
        private bool m_VDC_RR;
        private bool m_VDC_OR;
        private bool m_VDC_CR;

        private int m_VDC_Reg;
        private int m_VDC_MAWR;
        private int m_VDC_MARR;
        private int m_VDC_RCR;
        private int m_VDC_BXR;
        private int m_VDC_BYR;
        private int m_VDC_BYR_Offset;
        private ushort m_VDC_DSR;
        private ushort m_VDC_DESR;
        private ushort m_VDC_LENR;
        private ushort m_VDC_VSAR;

        public int m_VDC_HDR;
        public int m_VDC_VDW;

        private int m_VDC_BAT_Width;
        private int m_VDC_BAT_Height;

        private bool m_VDC_DMA_Enable;
        private bool m_VDC_SATBDMA_IRQ;
        private bool m_VDC_VRAMDMA_IRQ;
        private bool m_VDC_SRCDECR;
        private bool m_VDC_DSTDECR;
        private bool m_VDC_SATB_ENA;

        private bool m_VDC_EnableBackground;
        private bool m_VDC_EnableSprites;
        private bool m_VDC_VBKIRQ;
        private bool m_VDC_RCRIRQ;
        private bool m_VDC_SprOvIRQ;
        private bool m_VDC_Spr0Col;
        private int m_VDC_Increment;

        // VCE REGISTERS
        //private bool m_VCE_BW;
        //private bool m_VCE_Blur;
        private ushort[] m_VCE;
        private int m_VCE_Index;
        //private int ScanlineCount;
        //private bool Grayscale;

        #endregion

        [NonSerialized]
        public IRenderHandler host;

        public PPU(IRenderHandler render)
        {
            host = render;

            m_VRAM = new ushort[0x10000];
            m_SAT = new SpriteAttribute[0x40];
            m_VCE = new ushort[0x200];
            m_VCE_Index = 0;

            _screenBuf = new int[1024 * 1024];
            _screenBufPtr = Marshal.AllocHGlobal(1024 * 1024 * sizeof(int));


            for (int i = 0; i < 512; i++)
            {
                int b = ((i) & 0x7) * 0x49 >> 1;
                int r = ((i >> 3) & 0x7) * 0x49 >> 1;
                int g = ((i >> 6) & 0x7) * 0x49 >> 1;
                //PALETTE[i] = (r << 16) | (g << 8) | b;
                PALETTE[i] = (0xFF << 24) | (r << 16) | (g << 8) | b; //ARGB888
            }

            for (int i = 0; i < 0x40; i++)
                m_SAT[i] = new SpriteAttribute();

            m_RenderLine = 0;
            m_DoSAT_DMA = false;
            m_WaitingIRQ = false;

            m_VCE_DotClock = DotClock.MHZ_5;
            m_VDC_Increment = 1;

            m_VDC_BSY = false;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_screenBufPtr);
        }

        public unsafe void Reset()
        {
            Array.Clear(m_VRAM);

            for (var i = 0; i < 64; i++)
            {
                m_SAT[i].m_X = 0;
                m_SAT[i].m_Y = 0;
            }

            m_WaitingIRQ = false;
        }

        public unsafe void tick()
        {
            if (m_RenderLine + 1 > m_VDC_VDW)
            {
                HandleDMA();
            }
            else if (m_RenderLine >= 14 || m_RenderLine <= 256)
            {
                DrawScanLine();
            }

            m_RenderLine++;

            if (m_RenderLine + 1 == m_VDC_VDW)
            {
                m_DoSAT_DMA = m_DoSAT_DMA | m_VDC_SATB_ENA;
                if (m_VDC_VBKIRQ)
                {
                    m_VDC_VD = true;
                    m_WaitingIRQ = true;
                }
            }
            else if (m_RenderLine + 0x3F == m_VDC_RCR)
            {
                if (m_VDC_RCRIRQ)
                {
                    m_VDC_RR = true;
                    m_WaitingIRQ = true;
                }
            }

            if (m_RenderLine >= 262)
            {
                m_RenderLine = 0;
                //ConvertColor();
                Marshal.Copy(_screenBufPtr, _screenBuf, 0, _screenBuf.Length);
                FrameReady = true;
                host.RenderFrame(_screenBuf, SCREEN_WIDTH, m_VDC_VDW);
            }
        }

        private unsafe void HandleDMA()
        {
            int DmaCycles = CYCLES_PER_LINE / (int)m_VCE_DotClock;
            if (m_DoSAT_DMA)
            {
                DmaCycles -= 256;

                Parallel.For(0, 64, i =>
                {
                    int g = m_VDC_VSAR + i * 4;
                    m_SAT[i].m_Y = m_VRAM[g++] - 64;
                    m_SAT[i].m_X = m_VRAM[g++] - 32;
                    m_SAT[i].m_Pattern = (m_VRAM[g] & 0x07FE) << 5;
                    m_SAT[i].m_CGPage = (m_VRAM[g++] & 0x0001) != 0;
                    m_SAT[i].m_Palette = ((m_VRAM[g] & 0xF) << 4) | ((i == 0) ? 0x4100 : 0x2100);
                    m_SAT[i].m_Priority = (m_VRAM[g] & 0x80) != 0;
                    m_SAT[i].m_Width = ((m_VRAM[g] & 0x100) != 0) ? 2 : 1;
                    m_SAT[i].m_Height = (((m_VRAM[g] & 0x3000) >> 12) + 1) << 4;
                    m_SAT[i].m_HorizontalFlip = (m_VRAM[g] & 0x0800) != 0;
                    m_SAT[i].m_VerticalFlip = (m_VRAM[g++] & 0x8000) != 0;
                });

                if (m_VDC_SATBDMA_IRQ)
                {
                    m_VDC_DS = true;
                    m_WaitingIRQ = true;
                }
                m_DoSAT_DMA = false;
            }

            if (m_VDC_DMA_Enable)
            {
                while (DmaCycles >= 2)
                {
                    m_VRAM[m_VDC_DSTDECR ? m_VDC_DESR-- : m_VDC_DESR++] =
                        m_VRAM[m_VDC_SRCDECR ? m_VDC_DSR-- : m_VDC_DSR++];
                    DmaCycles -= 2;
                    if (--m_VDC_LENR == 0)
                    {
                        m_VDC_DMA_Enable = false;
                        if (m_VDC_VRAMDMA_IRQ)
                        {
                            m_VDC_DV = true;
                            m_WaitingIRQ = true;
                        }
                    }
                }
            }
        }

        private unsafe void DrawScanLine()
        {
            int i;
            int* ScanLinePtr = (int*)_screenBufPtr.ToPointer() + SCREEN_WIDTH * m_RenderLine;

            for (i = 0; i < SCREEN_WIDTH; i++) ScanLinePtr[i] = 0;

            if (m_VDC_EnableSprites)
            {
                int BufferIndexes = 0;
                int BufferUsage;
                SpriteAttribute[] SprBuffer = new SpriteAttribute[17];
                for (i = 0, BufferUsage = 0; i < 64 && BufferUsage < 17; i++)
                {
                    int y = m_SAT[i].m_Y;
                    if (m_RenderLine < y || m_RenderLine >= y + m_SAT[i].m_Height) continue;
                    BufferUsage += m_SAT[i].m_Width;
                    SprBuffer[BufferIndexes++] = m_SAT[i];
                }
                if (BufferUsage > 16)
                {
                    if (m_VDC_SprOvIRQ)
                    {
                        m_VDC_OR = true;
                        m_WaitingIRQ = true;
                    }
                    BufferUsage = 16;
                }
                for (i = BufferIndexes - 1; i >= 0; i--)
                {
                    int SprOffY;
                    if (SprBuffer[i].m_VerticalFlip)
                        SprOffY = SprBuffer[i].m_Height - 1 - m_RenderLine + SprBuffer[i].m_Y;
                    else
                        SprOffY = m_RenderLine - SprBuffer[i].m_Y;
                    int tile = SprBuffer[i].m_Pattern + ((SprOffY & 0xFFF0) << 3) + (SprOffY & 0xF);
                    int x = SprBuffer[i].m_X;
                    int* spx = ScanLinePtr;
                    spx += x;
                    if (x >= (m_VDC_HDR + 1) << 3) continue;
                    if (x > -32)
                    {
                        switch (SprBuffer[i].m_Width)
                        {
                            case 1:
                                DrawSPRTile(ScanLinePtr, ref spx, SprBuffer[i].m_Palette, tile, SprBuffer[i].m_Priority, SprBuffer[i].m_HorizontalFlip);
                                break;
                            case 2:
                                if (SprBuffer[i].m_HorizontalFlip)
                                {
                                    DrawSPRTile(ScanLinePtr, ref spx, SprBuffer[i].m_Palette, tile + 64, SprBuffer[i].m_Priority, true);
                                    DrawSPRTile(ScanLinePtr, ref spx, SprBuffer[i].m_Palette, tile, SprBuffer[i].m_Priority, true);
                                }
                                else
                                {
                                    DrawSPRTile(ScanLinePtr, ref spx, SprBuffer[i].m_Palette, tile, SprBuffer[i].m_Priority, false);
                                    DrawSPRTile(ScanLinePtr, ref spx, SprBuffer[i].m_Palette, tile + 64, SprBuffer[i].m_Priority, false);
                                }
                                break;
                        }
                    }
                }
            }

            if (m_VDC_EnableBackground)
            {
                int RealBYR = (m_VDC_BYR - m_VDC_BYR_Offset) & 0x3FF;
                int BATMask = (m_VDC_BAT_Width - 1);
                int BATLine = (((RealBYR + m_RenderLine) >> 3) & (m_VDC_BAT_Height - 1)) * m_VDC_BAT_Width;
                int BATAddress = (m_VDC_BXR >> 3) & BATMask;
                int YOverFlow = (RealBYR + m_RenderLine) & 0x7;
                int* tileMap = ScanLinePtr;
                tileMap -= m_VDC_BXR & 7;
                for (i = 0; i < m_VDC_HDR + 2; i++)
                {
                    int tile = m_VRAM[BATAddress | BATLine];
                    DrawBGTile(ScanLinePtr, ref tileMap, (tile & 0xF000) >> 8, (tile & 0xFFF) << 4 | YOverFlow);
                    BATAddress = (BATAddress + 1) & BATMask;
                }
            }

            //colorindex to ARGB8888
            //ushort grayscaleBit = (ushort)(Grayscale ? 0x200 : 0);
            int color = 0;
            int* LineWritePtr = ScanLinePtr;
            for (i = 0; i < SCREEN_WIDTH; i++, ScanLinePtr++)
            {
                if ((*ScanLinePtr & 0x6000) == 0x6000) m_VDC_CR = m_VDC_Spr0Col;
                //color = PALETTE[m_VCE[*ScanLinePtr & 0x1FF] | grayscaleBit];
                color = PALETTE[m_VCE[*ScanLinePtr & 0x1FF]];
                *(LineWritePtr++) = color;
            }
        }

        public unsafe void ConvertColor()
        {
            int color = 0;
            for (int y = 0; y < m_VDC_VDW; y++)
            {
                int* LineWritePtr = (int*)_screenBufPtr.ToPointer() + SCREEN_WIDTH * y;
                for (int i = 0; i < SCREEN_WIDTH; i++, LineWritePtr++)
                {
                    color = PALETTE[m_VCE[*LineWritePtr & 0x1FF]];
                    *(LineWritePtr) = color;
                }
            }
        }

        public unsafe void DrawSPRTile(int* ScanLinePtr, ref int* px, int palette, int tile, bool priority, bool flip)
        {
            int p1 = m_VRAM[tile];
            int p2 = m_VRAM[tile + 16] << 1;
            int p3 = m_VRAM[tile + 32] << 2;
            int p4 = m_VRAM[tile + 48] << 3;
            int color = 0;

            if (priority) palette |= 0x1000;

            if (flip)
                for (int x = 0; x < 16; x++, px++)
                {
                    if (px < ScanLinePtr) continue;
                    color = ((p1 >> x) & 1) | ((p2 >> x) & 2) | ((p3 >> x) & 4) | ((p4 >> x) & 8);
                    if (color == 0) continue;
                    *px = palette | color;
                }
            else
                for (int x = 15; x >= 0; x--, px++)
                {
                    if (px < ScanLinePtr) continue;
                    color = ((p1 >> x) & 1) | ((p2 >> x) & 2) | ((p3 >> x) & 4) | ((p4 >> x) & 8);
                    if (color == 0) continue;
                    *px = palette | color;
                }
        }

        public unsafe void DrawBGTile(int* ScanLinePtr, ref int* px, int palette, int tile)
        {
            int p1 = m_VRAM[tile];
            int p2 = p1 >> 7;
            int p3 = m_VRAM[tile + 8] << 2;
            int p4 = p3 >> 7;
            int color = 0;

            for (int x = 7; x >= 0; x--, px++)
            {
                if (px < ScanLinePtr) continue;
                if ((*px & 0x1000) != 0) continue;
                color = ((p1 >> x) & 1) | ((p2 >> x) & 2) | ((p3 >> x) & 4) | ((p4 >> x) & 8);
                if (color != 0) *px = palette | color;
            }
        }

        public void WriteVDC(int address, byte data)
        {
            if (address == 0)
            {
                m_VDC_Reg = data & 0x1F;
                return;
            }
            switch (address)
            {
                case 2: // 写入寄存器低字节
                    WriteVDCRegisterLSB(m_VDC_Reg, data);
                    break;

                case 3: // 写入寄存器高字节
                    WriteVDCRegisterMSB(m_VDC_Reg, data);
                    break;
            }
        }

        private void WriteVDCRegisterLSB(int reg, byte data)
        {
            switch (reg)
            {
                case 0x00: m_VDC_MAWR = (m_VDC_MAWR & 0xFF00) | data; break;
                case 0x01: m_VDC_MARR = (m_VDC_MARR & 0xFF00) | data; break;
                case 0x02: m_VRAM[m_VDC_MAWR] = (ushort)((m_VRAM[m_VDC_MAWR] & 0xFF00) | data); break;
                case 0x05:
                    m_VDC_EnableBackground = (data & 0x80) != 0;
                    m_VDC_EnableSprites = (data & 0x40) != 0;
                    m_VDC_VBKIRQ = (data & 0x08) != 0;
                    m_VDC_RCRIRQ = (data & 0x04) != 0;
                    m_VDC_SprOvIRQ = (data & 0x02) != 0;
                    m_VDC_Spr0Col = (data & 0x01) != 0;
                    break;
                case 0x06: m_VDC_RCR = (m_VDC_RCR & 0x0300) | data; break;
                case 0x07: m_VDC_BXR = (m_VDC_BXR & 0x0300) | data; break;
                case 0x08:
                    m_VDC_BYR_Offset = (m_RenderLine + 1 >= m_VDC_VDW || !m_VDC_EnableBackground) ? 0 : (m_RenderLine - 1);
                    m_VDC_BYR = (m_VDC_BYR & 0x0100) | data;
                    break;
                case 0x09:
                    switch (data & 0x30)
                    {
                        case 0x00:
                            m_VDC_BAT_Width = 32;
                            break;
                        case 0x10:
                            m_VDC_BAT_Width = 64;
                            break;
                        default:
                            m_VDC_BAT_Width = 128;
                            break;
                    }
                    //VramAccessMode = data & 0x03;
                    //SpriteAccessMode = (data >> 2) & 0x03;
                    m_VDC_BAT_Height = ((data & 0x40) == 0) ? 32 : 64;
                    //m_VDC_CgMode = (data & 0x80) != 0;
                    break;
                case 0x0A:
                    //m_VDC_HSW = data & 0x1F;
                    break;
                case 0x0B:
                    m_VDC_HDR = data & 0x7F;
                    SCREEN_WIDTH = (m_VDC_HDR + 1) * 8;
                    if (SCREEN_WIDTH == 336) SCREEN_WIDTH = 352;
                    break;
                case 0x0D: m_VDC_VDW = (m_VDC_VDW & 0x100) | data; break;
                case 0x0F:
                    m_VDC_SATBDMA_IRQ = (data & 0x01) != 0;
                    m_VDC_VRAMDMA_IRQ = (data & 0x02) != 0;
                    m_VDC_SRCDECR = (data & 0x04) != 0;
                    m_VDC_DSTDECR = (data & 0x08) != 0;
                    m_VDC_SATB_ENA = (data & 0x10) != 0;
                    break;
                case 0x10: m_VDC_DSR = (ushort)((m_VDC_DSR & 0xFF00) | data); break;
                case 0x11: m_VDC_DESR = (ushort)((m_VDC_DESR & 0xFF00) | data); break;
                case 0x12: m_VDC_LENR = (ushort)((m_VDC_LENR & 0xFF00) | data); break;
                case 0x13: m_VDC_VSAR = (ushort)((m_VDC_VSAR & 0xFF00) | data); m_DoSAT_DMA = true; break;
            }
        }

        private void WriteVDCRegisterMSB(int reg, byte data)
        {
            switch (reg)
            {
                case 0x00: m_VDC_MAWR = (m_VDC_MAWR & 0xFF) | (data << 8); break;
                case 0x01: m_VDC_MARR = (m_VDC_MARR & 0xFF) | (data << 8); break;
                case 0x02:
                    m_VRAM[m_VDC_MAWR] = (ushort)((m_VRAM[m_VDC_MAWR] & 0x00FF) | (data << 8));
                    m_VDC_MAWR = (m_VDC_MAWR + m_VDC_Increment) & 0x7FFF;
                    break;
                case 0x05:
                    switch (data & 0x18)
                    {
                        case 0x00:
                            m_VDC_Increment = 1;
                            break;
                        case 0x08:
                            m_VDC_Increment = 32;
                            break;
                        case 0x10:
                            m_VDC_Increment = 64;
                            break;
                        default:
                            m_VDC_Increment = 128;
                            break;
                    }
                    break;
                case 0x06: m_VDC_RCR = (m_VDC_RCR & 0xFF) | ((data << 8) & 0x0300); break;
                case 0x07: m_VDC_BXR = (m_VDC_BXR & 0xFF) | ((data << 8) & 0x0300); break;
                case 0x08:
                    m_VDC_BYR_Offset = (m_RenderLine + 1 >= m_VDC_VDW || !m_VDC_EnableBackground) ? 0 : (m_RenderLine - 1);
                    m_VDC_BYR = (m_VDC_BYR & 0xFF) | ((data << 8) & 0x0100);
                    break;
                case 0x0A:
                    //m_VDC_HDS = data & 0x7F;
                    break;
                case 0x0B:
                    //m_VDC_HDE = (data & 0x7F);
                    break;
                case 0x0D: m_VDC_VDW = ((data << 8) & 0x100) | (m_VDC_VDW & 0xFF); break;
                case 0x10: m_VDC_DSR = (ushort)((m_VDC_DSR & 0xFF) | (data << 8)); break;
                case 0x11: m_VDC_DESR = (ushort)((m_VDC_DESR & 0xFF) | (data << 8)); break;
                case 0x12:
                    m_VDC_LENR = (ushort)((m_VDC_LENR & 0xFF) | (data << 8));
                    m_VDC_DMA_Enable = true;
                    break;
                case 0x13: m_VDC_VSAR = (ushort)((m_VDC_VSAR & 0xFF) | (data << 8)); m_DoSAT_DMA = true; break;
            }
        }

        public byte ReadVDC(int address)
        {
            switch (address)
            {
                case 0:
                    byte status = (byte)(
                        (m_VDC_BSY ? 0x40 : 0) |
                        (m_VDC_VD ? 0x20 : 0) |
                        (m_VDC_DV ? 0x10 : 0) |
                        (m_VDC_DS ? 0x08 : 0) |
                        (m_VDC_RR ? 0x04 : 0) |
                        (m_VDC_OR ? 0x02 : 0) |
                        (m_VDC_CR ? 0x01 : 0));
                    m_VDC_VD = false;
                    m_VDC_DV = false;
                    m_VDC_DS = false;
                    m_VDC_RR = false;
                    m_VDC_OR = false;
                    m_VDC_CR = false;
                    m_WaitingIRQ = false;
                    return status;

                case 2: return (byte)m_VRAM[m_VDC_MARR];
                case 3:
                    byte data = (byte)(m_VRAM[m_VDC_MARR] >> 8);
                    if (m_VDC_Reg == 2)
                        m_VDC_MARR = (m_VDC_MARR + m_VDC_Increment) & 0x7FFF;
                    return data;
            }
            return 0;
        }

        public void WriteVCE(int address, byte data)
        {
            switch (address)
            {
                case 0:
                    //ScanlineCount = (data & 0x04) != 0 ? 263 : 262;
                    //Grayscale = (data & 0x80) != 0;
                    // 设置时钟频率
                    switch (data & 3)
                    {
                        case 0:
                            m_VCE_DotClock = DotClock.MHZ_5;
                            break;
                        case 1:
                            m_VCE_DotClock = DotClock.MHZ_7;
                            break;
                        default:
                            m_VCE_DotClock = DotClock.MHZ_10;
                            break;
                    }
                    break;

                case 2:
                    // 设置调色板索引低字节
                    m_VCE_Index = (m_VCE_Index & 0x100) | (data & 0xFF);
                    break;

                case 3:
                    // 设置调色板索引高字节
                    m_VCE_Index = (m_VCE_Index & 0xFF) | ((data & 0x01) << 8);
                    break;

                case 4:
                    // 写入调色板数据低字节
                    m_VCE[m_VCE_Index] = (ushort)((m_VCE[m_VCE_Index] & 0xFF00) | data);
                    break;

                case 5:
                    // 写入调色板数据高字节
                    m_VCE[m_VCE_Index] = (ushort)((m_VCE[m_VCE_Index] & 0xFF) | ((data << 8) & 0x100));
                    m_VCE_Index = (m_VCE_Index + 1) & 0x1FF;
                    break;
            }
        }

        public byte ReadVCE(int address)
        {
            switch (address)
            {
                case 4:
                    return (byte)m_VCE[m_VCE_Index];
                case 5:
                    {
                        byte data = (byte)((m_VCE[m_VCE_Index] >> 8) | 0xFE);
                        m_VCE_Index = (m_VCE_Index + 1) & 0x1FF;
                        return data;
                    }
                default:
                    return 0xFF;
            }
        }

        public bool IRQPending()
        {
            bool wait = m_WaitingIRQ;
            m_WaitingIRQ = false;
            return wait;
        }
    }
}
