using System;

namespace ePceCD
{
    [Serializable]
    public class ADPCM
    {
        private const uint RAM_SIZE = 0x10000; // 64KB ADPCM RAM
        private byte[] _ram = new byte[RAM_SIZE];
        private CDRom _cdRom;

        // 寄存器状态
        private ushort _addressPort;      // 地址端口（0x08/0x09）
        private byte _dmaControl;         // DMA控制（0x0B）
        private byte _control;            // 控制寄存器（0x0D）
        private byte _playbackRate;        // 播放速率（0x0E）

        // 内部状态
        private uint _readAddress;         // 当前读取地址
        private uint _writeAddress;        // 当前写入地址
        private uint _adpcmLength;         // 剩余播放长度
        private bool _isPlaying;          // 是否正在播放
        private bool _endReached;         // 播放结束标志
        private bool _halfReached;        // 半缓冲区标志
        private double _clocksPerSample;  // 每个样本的时钟周期
        private int _currentPredictor;    // ADPCM解码预测值
        private int _currentStepIndex;    // ADPCM步长索引
        private byte _currentAdpcmByte;   // 当前处理的ADPCM字节
        private bool _isHighNibble;       // 当前处理高4位标志

        // 中断标志位掩码
        private const byte STATUS_END_FLAG = 0x01;
        private const byte STATUS_PLAYING_FLAG = 0x08;

        public ADPCM(CDRom cdRom)
        {
            _cdRom = cdRom;
            ResetDecoderState();
        }

        private void ResetDecoderState()
        {
            _currentPredictor = 0;
            _currentStepIndex = 0;
            _currentAdpcmByte = 0;
            _isHighNibble = true;
        }

        public byte ReadData(int addr)
        {
            switch (addr & 0x0F)
            {
                case 0x0A: // 读取数据端口
                    return ReadBuffer();

                case 0x0B: // DMA控制
                    return _dmaControl;

                case 0x0C: // 状态寄存器
                    byte status = 0;
                    status |= (byte)(_endReached ? STATUS_END_FLAG : 0);
                    status |= (byte)(_isPlaying ? STATUS_PLAYING_FLAG : 0);
                    return status;

                case 0x0D: // 控制寄存器
                    return _control;

                case 0x0E: // 播放速率
                    return _playbackRate;

                default:
                    Console.WriteLine($"ADPCM Read Unknown Register: 0x{addr:X2}");
                    return 0;
            }
        }

        public void WriteData(int addr, byte value)
        {
            //Console.WriteLine($"ADPCM Write Register: 0x{addr:X2}");
            switch (addr & 0x0F)
            {
                case 0x08: // 地址端口低8位
                    _addressPort = (ushort)((_addressPort & 0xFF00) | value);
                    break;

                case 0x09: // 地址端口高8位
                    _addressPort = (ushort)((_addressPort & 0x00FF) | (value << 8));
                    break;

                case 0x0A: // 数据写入端口
                    WriteBuffer(value);
                    break;

                case 0x0B: // DMA控制
                    _dmaControl = value;
                    if (_dmaControl != 0)
                        ProcessDmaRequest();
                    break;

                case 0x0D: // 控制寄存器
                    UpdateControlState(value);
                    break;

                case 0x0E: // 播放速率
                    _playbackRate = value;
                    UpdatePlaybackRate();
                    break;

                default:
                    Console.WriteLine($"ADPCM Write Unknown Register: 0x{addr:X2}");
                    break;
            }
        }

        // 更新播放速率计算
        private void UpdatePlaybackRate()
        {
            int rateCode = _playbackRate & 0x0F;
            double freq = 32000.0 / (16 - rateCode); // 实际采样率
            _clocksPerSample = 21477270.0 / freq;    // 系统时钟21.47727 MHz
        }

        // 处理控制寄存器写入
        private void UpdateControlState(byte value)
        {
            //Console.WriteLine($"ADPCM DMA UpdateControlState {_control}");

            if ((value & 0x02) != 0 && (_control & 0x02) == 0)
            {
                if (_addressPort > 0)
                    _writeAddress = (uint)(_addressPort - ((value & 0x01) != 0 ? 0 : 1));
                else
                    _writeAddress = 0;
                //Console.WriteLine("ADPCM Update WRITE addr");
            }
            if ((value & 0x08) != 0 && (_control & 0x08) == 0)
            {
                if (_addressPort > 0)
                    _readAddress = (uint)(_addressPort - ((value & 0x04) != 0 ? 0 : 1));
                else
                    _readAddress = 0;
                //Console.WriteLine("ADPCM Update READ addr");
            }

            _control = value;

            if (IsLengthLatched())
            {
                _adpcmLength = _addressPort;
                SetEndReached(false);
            }

            // 启动播放（Bit 7）
            if ((_control & 0x80) != 0 && !_isPlaying)
                StartPlayback();

            // 停止播放（Bit 6）
            if ((_control & 0x40) != 0 && _isPlaying)
                StopPlayback();

            if ((_control & 0x20) != 0)
            {
            }

            // 长度锁存（Bit 4）
            if ((_control & 0x10) != 0)
                LatchAdpcmLength();
        }

        // 启动播放
        private void StartPlayback()
        {
            _isPlaying = true;
            _endReached = false;
            _halfReached = false;
            _readAddress = _addressPort;
            ResetDecoderState();
            //Console.WriteLine("ADPCM Playback Started");
        }

        // 停止播放
        private void StopPlayback()
        {
            _isPlaying = false;
            //Console.WriteLine("ADPCM Playback Stopped");
        }

        // 锁存播放长度
        private void LatchAdpcmLength()
        {
            _adpcmLength = _addressPort;
            //Console.WriteLine($"ADPCM Length Latched: 0x{_adpcmLength:X4}");
        }

        // 处理DMA请求
        private void ProcessDmaRequest()
        {
            Console.WriteLine("ADPCM DMA Transfer Started");

            if (_cdRom.dataBuffer == null && _cdRom.dataBuffer.Length == 0)
            {
                _dmaControl &= (byte)(~0x01 & 0xFF);
                return;
            }

            int dmasize = 0;
            while (dmasize < _cdRom.dataBuffer.Length)
            {
                byte data = _cdRom.ReadDataPort();
                WriteBuffer(data);
                dmasize++;
            }

            //while (_cdRom.ResponseBufferIndex < _cdRom.ResponseBufferSize)
            //{
            //    byte data = _cdRom.ReadDataPort();
            //    WriteBuffer(data);
            //}

            Console.WriteLine($"ADPCM DMA Transfer {dmasize} Bytes");
        }

        // 从缓冲区读取数据（更新地址和长度）
        private byte ReadBuffer()
        {

            if (_readAddress >= RAM_SIZE) _readAddress = 0;

            byte data = _ram[_readAddress];
            _readAddress = (_readAddress + 1) % RAM_SIZE;

            if (!IsLengthLatched())
            {
                if (_adpcmLength > 0)
                {
                    _adpcmLength--;
                    SetHalfReached(_adpcmLength < 0x8000);
                }
                else
                {
                    SetEndReached(true);
                    SetHalfReached(false);
                }
            }

            return data;
        }

        // 写入数据到缓冲区
        private void WriteBuffer(byte data)
        {
            if (_writeAddress >= RAM_SIZE) _writeAddress = 0;

            _ram[_writeAddress] = data;
            _writeAddress = (_writeAddress + 1) % RAM_SIZE;

            if (_adpcmLength == 0)
            {
                SetEndReached(_endReached);
            }
            else SetHalfReached(_adpcmLength <= 0x8000);

            if (!IsLengthLatched())
                _adpcmLength = (_adpcmLength + 1) % RAM_SIZE;
        }

        // 检查是否启用长度锁存
        private bool IsLengthLatched() => (_control & 0x10) != 0;

        private void SetEndReached(bool value)
        {
            if (_endReached != value)
            {
                _endReached = value;
                if (value)
                {
                    _cdRom.ActiveIrqs |= (byte)CDRom.CdRomIrqSource.Stop;
                }
                else
                {
                    _cdRom.ActiveIrqs &= (byte)CDRom.CdRomIrqSource.Stop;
                }
            }
        }

        private void SetHalfReached(bool value)
        {
            if (_halfReached != value)
            {
                _halfReached = value;

                if (value)
                {
                    _cdRom.ActiveIrqs |= (byte)CDRom.CdRomIrqSource.Adpcm;
                }
                else
                {
                    _cdRom.ActiveIrqs &= (byte)CDRom.CdRomIrqSource.Adpcm;
                }
            }
        }

        // 生成音频样本
        public short GetSample()
        {
            if (IsLengthLatched())
            {
                _adpcmLength = _addressPort;
                SetEndReached(false);
            }
            if ((_control & 0x80) != 0)
            {
                _isPlaying = (_control & 0x20) != 0;
                return 0;
            }
            if ((_control & 0x40) != 0 && _adpcmLength == 0)
            {
                _control &= 0x20;
            }
            if ((_control & 0x20) == 0)
            {
                _isPlaying = false;
                return 0;
            }
            if (!_isPlaying)
            {
                _isPlaying = true;
            }

            byte nibble;
            if (_isHighNibble)
            {
                _currentAdpcmByte = ReadBuffer();
                nibble = (byte)((_currentAdpcmByte >> 4) & 0x0F);
                _isHighNibble = false;
            }
            else
            {
                nibble = (byte)(_currentAdpcmByte & 0x0F);
                _isHighNibble = true;
            }

            SetHalfReached(_adpcmLength <= 0x8000);
            if (_adpcmLength == 0)
            {
                SetEndReached(true);
            }

            return DecodeSample(nibble);
        }

        private int[] _stepSize =
        {
        0x0002, 0x0006, 0x000A, 0x000E, 0x0012, 0x0016, 0x001A, 0x001E,
        0x0002, 0x0006, 0x000A, 0x000E, 0x0013, 0x0017, 0x001B, 0x001F,
        0x0002, 0x0006, 0x000B, 0x000F, 0x0015, 0x0019, 0x001E, 0x0022,
        0x0002, 0x0007, 0x000C, 0x0011, 0x0017, 0x001C, 0x0021, 0x0026,
        0x0002, 0x0007, 0x000D, 0x0012, 0x0019, 0x001E, 0x0024, 0x0029,
        0x0003, 0x0009, 0x000F, 0x0015, 0x001C, 0x0022, 0x0028, 0x002E,
        0x0003, 0x000A, 0x0011, 0x0018, 0x001F, 0x0026, 0x002D, 0x0034,
        0x0003, 0x000A, 0x0012, 0x0019, 0x0022, 0x0029, 0x0031, 0x0038,
        0x0004, 0x000C, 0x0015, 0x001D, 0x0026, 0x002E, 0x0037, 0x003F,
        0x0004, 0x000D, 0x0016, 0x001F, 0x0029, 0x0032, 0x003B, 0x0044,
        0x0005, 0x000F, 0x0019, 0x0023, 0x002E, 0x0038, 0x0042, 0x004C,
        0x0005, 0x0010, 0x001B, 0x0026, 0x0032, 0x003D, 0x0048, 0x0053,
        0x0006, 0x0012, 0x001F, 0x002B, 0x0038, 0x0044, 0x0051, 0x005D,
        0x0006, 0x0013, 0x0021, 0x002E, 0x003D, 0x004A, 0x0058, 0x0065,
        0x0007, 0x0016, 0x0025, 0x0034, 0x0043, 0x0052, 0x0061, 0x0070,
        0x0008, 0x0018, 0x0029, 0x0039, 0x004A, 0x005A, 0x006B, 0x007B,
        0x0009, 0x001B, 0x002D, 0x003F, 0x0052, 0x0064, 0x0076, 0x0088,
        0x000A, 0x001E, 0x0032, 0x0046, 0x005A, 0x006E, 0x0082, 0x0096,
        0x000B, 0x0021, 0x0037, 0x004D, 0x0063, 0x0079, 0x008F, 0x00A5,
        0x000C, 0x0024, 0x003C, 0x0054, 0x006D, 0x0085, 0x009D, 0x00B5,
        0x000D, 0x0027, 0x0042, 0x005C, 0x0078, 0x0092, 0x00AD, 0x00C7,
        0x000E, 0x002B, 0x0049, 0x0066, 0x0084, 0x00A1, 0x00BF, 0x00DC,
        0x0010, 0x0030, 0x0051, 0x0071, 0x0092, 0x00B2, 0x00D3, 0x00F3,
        0x0011, 0x0034, 0x0058, 0x007B, 0x00A0, 0x00C3, 0x00E7, 0x010A,
        0x0013, 0x003A, 0x0061, 0x0088, 0x00B0, 0x00D7, 0x00FE, 0x0125,
        0x0015, 0x0040, 0x006B, 0x0096, 0x00C2, 0x00ED, 0x0118, 0x0143,
        0x0017, 0x0046, 0x0076, 0x00A5, 0x00D5, 0x0104, 0x0134, 0x0163,
        0x001A, 0x004E, 0x0082, 0x00B6, 0x00EB, 0x011F, 0x0153, 0x0187,
        0x001C, 0x0055, 0x008F, 0x00C8, 0x0102, 0x013B, 0x0175, 0x01AE,
        0x001F, 0x005E, 0x009D, 0x00DC, 0x011C, 0x015B, 0x019A, 0x01D9,
        0x0022, 0x0067, 0x00AD, 0x00F2, 0x0139, 0x017E, 0x01C4, 0x0209,
        0x0026, 0x0072, 0x00BF, 0x010B, 0x0159, 0x01A5, 0x01F2, 0x023E,
        0x002A, 0x007E, 0x00D2, 0x0126, 0x017B, 0x01CF, 0x0223, 0x0277,
        0x002E, 0x008A, 0x00E7, 0x0143, 0x01A1, 0x01FD, 0x025A, 0x02B6,
        0x0033, 0x0099, 0x00FF, 0x0165, 0x01CB, 0x0231, 0x0297, 0x02FD,
        0x0038, 0x00A8, 0x0118, 0x0188, 0x01F9, 0x0269, 0x02D9, 0x0349,
        0x003D, 0x00B8, 0x0134, 0x01AF, 0x022B, 0x02A6, 0x0322, 0x039D,
        0x0044, 0x00CC, 0x0154, 0x01DC, 0x0264, 0x02EC, 0x0374, 0x03FC,
        0x004A, 0x00DF, 0x0175, 0x020A, 0x02A0, 0x0335, 0x03CB, 0x0460,
        0x0052, 0x00F6, 0x019B, 0x023F, 0x02E4, 0x0388, 0x042D, 0x04D1,
        0x005A, 0x010F, 0x01C4, 0x0279, 0x032E, 0x03E3, 0x0498, 0x054D,
        0x0063, 0x012A, 0x01F1, 0x02B8, 0x037F, 0x0446, 0x050D, 0x05D4,
        0x006D, 0x0148, 0x0223, 0x02FE, 0x03D9, 0x04B4, 0x058F, 0x066A,
        0x0078, 0x0168, 0x0259, 0x0349, 0x043B, 0x052B, 0x061C, 0x070C,
        0x0084, 0x018D, 0x0296, 0x039F, 0x04A8, 0x05B1, 0x06BA, 0x07C3,
        0x0091, 0x01B4, 0x02D8, 0x03FB, 0x051F, 0x0642, 0x0766, 0x0889,
        0x00A0, 0x01E0, 0x0321, 0x0461, 0x05A2, 0x06E2, 0x0823, 0x0963,
        0x00B0, 0x0210, 0x0371, 0x04D1, 0x0633, 0x0793, 0x08F4, 0x0A54,
        0x00C2, 0x0246, 0x03CA, 0x054E, 0x06D2, 0x0856, 0x09DA, 0x0B5E
        };

        private int[] _stepFactor = { -1, -1, -1, -1, 2, 4, 6, 8 };

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        public short DecodeSample(byte nibble)
        {
            nibble &= 0x0F; // 确保4位数据
            int sign = nibble & 0x08;
            int magnitude = nibble & 0x07;

            // 计算步长和delta
            int step = _stepSize[_currentStepIndex];
            int delta = (step * magnitude) >> 2; // 等价于除以4

            if (sign != 0)
                delta = -delta;

            // 更新预测值
            _currentPredictor += delta;
            _currentPredictor = Clamp(_currentPredictor, -32768, 32767);

            // 更新步长索引
            _currentStepIndex += _stepFactor[magnitude];
            _currentStepIndex = Clamp(_currentStepIndex, 0, _stepSize.Length - 1);

            return (short)_currentPredictor;
        }
    }

    [Serializable]
    public class AUDIOFADE
    {
        public byte fade;

        public byte ReadData(int addr)
        {
            //Console.WriteLine($"AUDIOFADE ReadData: 0x{addr:X4}");
            return fade;
        }

        public void WriteData(int addr, byte value)
        {
            fade = value;
            //Console.WriteLine($"AUDIOFADE WriteData: 0x{addr:X4} 0x{value:X4}");
        }

    }
}