using System;
using System.Linq;

namespace emuPCE
{
    public class APU //PSG , HuC6260
    {
        public int m_SampleRate = 44100;
        public short m_BaseLine = 0;
        private class PSG_Channel
        {
            public int m_Frequency;
            public float m_RealFrequency;
            public bool m_Enabled, m_DDA, m_Noise;
            public int m_Left_Volume, m_Right_Volume, m_Volume, m_DDA_Output, m_NoiseFrequency;
            public float m_RealNoiseFrequency, m_NoiseIndex, m_LeftOutput, m_RightOutput;
            public int[] m_Buffer = new int[32];
            public int m_BufferIndex;
            public float m_OutputIndex;
        }
        private int m_Left_Volume, m_Right_Volume;
        private float m_RealLFOFrequency;
        private int m_LFO_Frequency;
        private bool m_LFO_Enabled, m_LFO_Active;
        private int m_LFO_Shift;
        private PSG_Channel[] m_Channels = new PSG_Channel[8];
        private PSG_Channel m_Selected;
        private static int[] m_NoiseBuffer = new int[0x8000];
        private static float[] m_VolumeTable = new float[92];
        private CDRom m_CDRom;

        static APU()
        {
            // 初始化噪声缓冲区
            int noiseRegister = 0x100;
            for (int i = 0; i < m_NoiseBuffer.Length; i++)
            {
                int bit0 = noiseRegister & 0x01;
                int bit1 = (noiseRegister & 0x02) >> 1;
                noiseRegister = (noiseRegister >> 1) | ((bit0 ^ bit1) << 14);
                m_NoiseBuffer[i] = (bit0 == 1) ? -12 : 12;
            }
            // 初始化音量表
            for (int i = 0; i < 92; i++)
            {
                m_VolumeTable[i] = 1024.0f * (float)Math.Pow(10.0, (91 - i) * -0.075);
            }
        }

        public APU(CDRom cdrom)
        {
            for (int i = 0; i < 8; i++)
            {
                m_Channels[i] = new PSG_Channel();
            }
            m_Selected = m_Channels[0];
            m_CDRom = cdrom;
        }

        public unsafe void GetSamples(IntPtr stream, int len)
        {
            if (stream == IntPtr.Zero || len == 0) return;

            short* buffer = (short*)stream.ToPointer();
            int samples = len / 4; // 每个样本包含左右声道
            for (int i = 0; i < samples; i++)
            {
                float left = 0, right = 0;
                if (m_LFO_Enabled && m_LFO_Active)
                {
                    var channel = m_Channels[1];
                    int lfoFreq = channel.m_Buffer[(int)channel.m_OutputIndex];
                    channel.m_OutputIndex += m_RealLFOFrequency;
                    channel.m_OutputIndex %= 32;
                    // 符号扩展频率
                    if ((lfoFreq & 0x10) != 0) lfoFreq |= -16;
                    channel = m_Channels[0];
                    channel.m_RealFrequency = 3584160.0f / m_SampleRate / (channel.m_Frequency + (lfoFreq << m_LFO_Shift) + 1);
                }
                // 遍历所有通道并生成音频样本
                foreach (var channel in m_Channels.Take(6))
                {
                    if (!channel.m_Enabled || (m_LFO_Enabled && channel == m_Channels[1])) continue;
                    int channelSample = GetChannelSample(channel);
                    left += channelSample * channel.m_LeftOutput;
                    right += channelSample * channel.m_RightOutput;
                }
                // 添加ADPCM音频混合
                short adpcmSample = m_CDRom._ADPCM.GetSample();
                left += adpcmSample;
                right += adpcmSample;
                // 写入最终的音频样本
                buffer[i * 2] = (short)(right + m_BaseLine);
                buffer[i * 2 + 1] = (short)(left + m_BaseLine);
            }

            m_CDRom.MixCD((short*)stream.ToPointer(), len / 2);
        }

        private int GetChannelSample(PSG_Channel channel)
        {
            int sample;
            if (channel.m_DDA)
            {
                sample = channel.m_DDA_Output;
            }
            else if (channel.m_Noise)
            {
                sample = m_NoiseBuffer[(int)channel.m_NoiseIndex];
                channel.m_NoiseIndex += channel.m_RealFrequency;
                channel.m_NoiseIndex %= 0x8000;
            }
            else
            {
                sample = channel.m_Buffer[(int)channel.m_OutputIndex];
                channel.m_OutputIndex += channel.m_RealFrequency;
                channel.m_OutputIndex %= 32;
            }
            return sample;
        }

        public void Write(int address, byte data)
        {
            switch (address)
            {
                case 0x800:
                    m_Selected = m_Channels[data & 0x07];
                    break;
                case 0x801:
                    m_Left_Volume = (data >> 4);
                    m_Right_Volume = (data & 0x0F);
                    break;
                case 0x808:
                    m_LFO_Frequency = data;
                    break;
                case 0x809:
                    m_LFO_Enabled = (data & 0x80) != 0;
                    switch (data & 0x3)
                    {
                        case 0: m_LFO_Active = false; break;
                        case 1: m_LFO_Active = true; m_LFO_Shift = 0; break;
                        case 2: m_LFO_Active = true; m_LFO_Shift = 4; break;
                        case 3: m_LFO_Active = true; m_LFO_Shift = 8; break;
                    }
                    if (m_LFO_Enabled && m_LFO_Active)
                        Console.WriteLine("LFO MODE HAS BEEN ACTIVATED");
                    break;
                case 0x802:
                    m_Selected.m_Frequency = (m_Selected.m_Frequency & 0x0F00) | data;
                    break;
                case 0x803:
                    m_Selected.m_Frequency = (m_Selected.m_Frequency & 0x00FF) | ((data << 8) & 0x0F00);
                    break;
                case 0x804:
                    m_Selected.m_Enabled = (data & 0x80) != 0;
                    m_Selected.m_DDA = (data & 0x40) != 0;
                    m_Selected.m_Volume = data & 0x0F;
                    break;
                case 0x805:
                    m_Selected.m_Left_Volume = (data >> 4);
                    m_Selected.m_Right_Volume = (data & 0x0F);
                    break;
                case 0x806:
                    m_Selected.m_DDA_Output = data & 0x1F;
                    m_Selected.m_Buffer[m_Selected.m_BufferIndex] = (data & 0x1F) - 0x10;
                    m_Selected.m_BufferIndex = (m_Selected.m_BufferIndex + 1) & 0x1F;
                    break;
                case 0x807:
                    m_Selected.m_Noise = (data & 0x80) != 0;
                    m_Selected.m_NoiseFrequency = (data & 0x1F ^ 0x1F);
                    m_Selected.m_RealNoiseFrequency = 112005.0f / m_SampleRate / (m_Selected.m_NoiseFrequency + 1);
                    break;
                default:
                    Console.WriteLine($"PSG Access at {address:X} -> {data:X}");
                    break;
            }

            UpdateChannelParameters();
        }

        private void UpdateChannelParameters()
        {
            foreach (var channel in m_Channels.Take(6))
            {
                channel.m_RealFrequency = 3584160.0f / m_SampleRate / (channel.m_Frequency + 1);
                channel.m_LeftOutput = m_VolumeTable[(channel.m_Left_Volume + m_Left_Volume) * 2 + channel.m_Volume];
                channel.m_RightOutput = m_VolumeTable[(channel.m_Right_Volume + m_Right_Volume) * 2 + channel.m_Volume];
                if (Array.IndexOf(m_Channels, channel) >= 4) channel.m_Noise = false;
            }

            m_RealLFOFrequency = 3584160.0f / m_SampleRate / ((m_Channels[1].m_Frequency + 1) * m_LFO_Frequency);
        }
    }
}