using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace emuPCE
{
    public class CDRom
    {
        // 常量定义
        private const int SECTOR_SIZE = 2352;
        private const int DATA_SECTOR_OFFSET = 16;
        private const int MODE1_DATA_SIZE = 2048;

        // SCSI状态管理
        private byte[] CMDBuffer = new byte[16];
        private int CMDBufferIndex = 0;
        private int CMDLength;
        private bool[] Signals = new bool[9];
        private ScsiPhase _ScsiPhase;
        public MemoryStream dataBuffer;
        public int dataOffset;
        private byte messageByte;
        private int currentSector = -1;
        private CDTrack currentTrack, FileTrack;

        // CD 播放
        private int AudioSS, AudioES, AudioCS;
        private bool CdPlaying;
        private CDPLAYMODE CdPlayMode;
        private enum CDPLAYMODE { LOOP, IRQ, STOP };
        byte[] CDSBuffer = new byte[SECTOR_SIZE];

        // 光盘数据结构
        public enum TrackType { AUDIO, MODE1, MODE2, MODE1_2352 }
        public struct PosMSF
        {
            public int MSF_M;
            public int MSF_S;
            public int MSF_F;
        }
        public class CDTrack
        {
            public int Number;
            public TrackType Type;
            public FileStream File;
            public long SectorStart;
            public long SectorEnd;
            public long OffsetStart;
            public long OffsetEnd;
            public bool IsLeadIn;
            public long LeadInSectorStart;
            public PosMSF LeadIn;
            public PosMSF StartPos;
            public PosMSF EndPos;
            public byte Control;  // 子通道Q控制字段
            public byte Adr;      // 子通道Q ADR类型
        }
        public enum CdRomIrqSource
        {
            Adpcm = 0x04,
            Stop = 0x08,
            DataTransferDone = 0x20,
            DataTransferReady = 0x40
        }

        // IRQ和状态管理
        public byte EnabledIrqs, ActiveIrqs, ResetRegValue;
        private bool bramLocked;
        public SaveMemoryBank BRAM;
        public RamBank[] RAMBanks { get; } = new RamBank[32];
        private List<CDTrack> tracks = new List<CDTrack>();

        public ADPCM _ADPCM;
        public AUDIOFADE _AUDIOFADE = new AUDIOFADE();

        public CDRom()
        {
            _ADPCM = new ADPCM(this);

            for (int i = 0; i < 32; i++)
                RAMBanks[i] = new RamBank();
        }

        public SaveMemoryBank GetSaveMemory()
        {
            return BRAM;
        }

        public RamBank GetRam(int i)
        {
            return RAMBanks[i];
        }

        public bool IRQWaiting()
        {
            return ((EnabledIrqs & ActiveIrqs) != 0);
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        public unsafe void MixCD(short* outputBuffer, int len)
        {
            if (!CdPlaying) return;

            int samplesMixed = 0;
            while (samplesMixed < len)
            {
                if (AudioCS > AudioES)
                {
                    switch (CdPlayMode)
                    {
                        case CDPLAYMODE.STOP:
                            CdPlaying = false;
                            return;
                        case CDPLAYMODE.LOOP:
                            AudioCS = AudioSS;
                            break;
                        case CDPLAYMODE.IRQ:
                            ActiveIrqs |= (byte)CdRomIrqSource.Stop;
                            CdPlaying = false;
                            return;
                    }
                }
                FileTrack.File.Seek(AudioCS * SECTOR_SIZE, SeekOrigin.Begin);
                FileTrack.File.Read(CDSBuffer, 0, SECTOR_SIZE);
                for (int i = DATA_SECTOR_OFFSET; i < SECTOR_SIZE && samplesMixed < len; i += 2)
                {
                    short sample = (short)((CDSBuffer[i + 1] << 8) | CDSBuffer[i]);
                    short mixedSample = (short)Clamp(outputBuffer[samplesMixed] + sample, short.MinValue, short.MaxValue);
                    outputBuffer[samplesMixed++] = mixedSample;
                }
                AudioCS++;
            }
        }

        private int MSFToLBA(int m, int s, int f) => m * 60 * 75 + s * 75 + f;
        private byte ToBCD(int value) => (byte)(((value / 10) << 4) | (value % 10));
        private byte FromBCD(byte value) { return (byte)(((value >> 4) & 0x0F) * 10 + (value & 0x0F)); }

        #region CUE文件解析
        public void LoadCue(string cuePath)
        {
            tracks.Clear();
            string baseDir = Path.GetDirectoryName(cuePath);

            foreach (string line in File.ReadLines(cuePath))
            {
                string[] parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                switch (parts[0].ToUpper())
                {
                    case "FILE":
                        ParseFileCommand(parts, baseDir);
                        break;
                    case "TRACK":
                        ParseTrackCommand(parts);
                        break;
                    case "INDEX":
                        ParseIndexCommand(parts);
                        break;
                    case "PREGAP":
                        HandlePregap(parts[1]);
                        break;
                }
            }
            CalculateTrackMSF();
            Console.WriteLine($"Loaded {tracks.Count} tracks");
        }

        private void ParseFileCommand(string[] parts, string baseDir)
        {
            string filename = string.Join(" ", parts.Skip(1).TakeWhile(p => p != "BINARY")).Trim('"');
            string filePath = Path.Combine(baseDir, filename);
            currentTrack = new CDTrack { File = new FileStream(filePath, FileMode.Open, FileAccess.Read) };
            FileTrack = currentTrack;
            Console.WriteLine($"CDROM {filePath} BINARY LOADED");
            BRAM = new SaveMemoryBank(Path.GetFileNameWithoutExtension(filePath));
        }

        private void ParseTrackCommand(string[] parts)
        {
            var track = new CDTrack { File = currentTrack.File };
            track.Number = int.Parse(parts[1]);

            switch (parts[2])
            {
                case "AUDIO":
                    track.Type = TrackType.AUDIO;
                    track.Control = 0x40;
                    track.Adr = 0x01;
                    break;
                case "MODE1/2048":
                    track.Type = TrackType.MODE1;
                    track.Control = 0x00;
                    track.Adr = 0x01;
                    break;
                case "MODE1/2352":
                    track.Type = TrackType.MODE1_2352;
                    track.Control = 0x00;
                    track.Adr = 0x01;
                    break;
            }
            tracks.Add(track);
            currentTrack = track;
        }

        private void ParseIndexCommand(string[] parts)
        {
            var msf = parts[2].Split(':').Select(int.Parse).ToArray();
            switch (parts[1])
            {
                case "00":
                    currentTrack.LeadIn = new PosMSF { MSF_M = msf[0], MSF_S = msf[1], MSF_F = msf[2] };
                    currentTrack.LeadInSectorStart = MSFToLBA(msf[0], msf[1], msf[2]);
                    break;
                case "01":
                    currentTrack.StartPos = new PosMSF { MSF_M = msf[0], MSF_S = msf[1], MSF_F = msf[2] };
                    currentTrack.SectorStart = MSFToLBA(msf[0], msf[1], msf[2]);
                    break;
            }
        }

        private void HandlePregap(string msf)
        {
            var pregap = msf.Split(':').Select(int.Parse).ToArray();
            currentTrack.SectorEnd = MSFToLBA(pregap[0], pregap[1], pregap[2]);
        }

        private void CalculateTrackMSF()
        {
            long fileOffset = 0;
            foreach (var track in tracks)
            {
                track.SectorStart = MSFToLBA(track.StartPos.MSF_M, track.StartPos.MSF_S, track.StartPos.MSF_F);
                track.OffsetStart = fileOffset;

                var nextTrack = tracks.FirstOrDefault(t => t.Number == track.Number + 1);
                track.SectorEnd = nextTrack?.SectorStart ?? (track.File.Length / SECTOR_SIZE);

                track.OffsetEnd = track.OffsetStart + (track.SectorEnd - track.SectorStart) * SECTOR_SIZE;
                fileOffset = track.OffsetEnd;

                track.EndPos = new PosMSF
                {
                    MSF_M = (int)(track.SectorEnd / (60 * 75)),
                    MSF_S = (int)((track.SectorEnd / 75) % 60),
                    MSF_F = (int)(track.SectorEnd % 75)
                };
            }
        }
        #endregion

        #region SCSI核心逻辑
        private void SetPhase(ScsiPhase phase)
        {
            _ScsiPhase = phase;
            Array.Clear(Signals, 0, 9);
            switch (phase)
            {
                case ScsiPhase.CMD:
                    Signals[(int)ScsiSignal.Bsy] = true;
                    Signals[(int)ScsiSignal.Cd] = true;
                    Signals[(int)ScsiSignal.Msg] = false;
                    Signals[(int)ScsiSignal.Io] = false;
                    Signals[(int)ScsiSignal.Req] = true;
                    break;

                case ScsiPhase.DataIn:
                    Signals[(int)ScsiSignal.Bsy] = true;
                    Signals[(int)ScsiSignal.Io] = true;
                    ActiveIrqs |= (byte)CdRomIrqSource.DataTransferReady;
                    break;

                case ScsiPhase.Status:
                    Signals[(int)ScsiSignal.Bsy] = true;
                    Signals[(int)ScsiSignal.Io] = true;
                    Signals[(int)ScsiSignal.Cd] = true;
                    Signals[(int)ScsiSignal.Req] = true;
                    break;

                case ScsiPhase.MessageIn:
                    Signals[(int)ScsiSignal.Bsy] = true;
                    Signals[(int)ScsiSignal.Io] = true;
                    Signals[(int)ScsiSignal.Cd] = true;
                    Signals[(int)ScsiSignal.Msg] = true;
                    Signals[(int)ScsiSignal.Req] = true;
                    ActiveIrqs &= (byte)CdRomIrqSource.DataTransferReady;
                    ActiveIrqs |= (byte)CdRomIrqSource.DataTransferDone;
                    break;

                case ScsiPhase.BusFree:
                    Signals[(int)ScsiSignal.Bsy] = false;
                    Signals[(int)ScsiSignal.Req] = false;
                    ActiveIrqs &= (byte)CdRomIrqSource.DataTransferDone;
                    break;
            }
        }

        private void PrepareResponse(byte[] data)
        {
            dataBuffer?.Dispose();
            if (_ScsiPhase == ScsiPhase.Status)
            {
                dataBuffer = new MemoryStream(data, 0, 1, writable: false, publiclyVisible: true);
                dataBuffer.WriteByte(data.Length > 0 ? data[0] : (byte)0);
            }
            else
            {
                dataBuffer = new MemoryStream(data, 0, data.Length, writable: false, publiclyVisible: true);
            }
            dataOffset = 0;
        }

        private void SendStatus(byte status)
        {
            PrepareResponse(new byte[] { status });
            SetPhase(ScsiPhase.Status);

            System.Timers.Timer endTimer = new System.Timers.Timer(100);
            endTimer.Elapsed += (s, e) =>
            {
                FinishCommand();
                endTimer.Dispose();
            };
            endTimer.AutoReset = false;
            endTimer.Start();
        }

        private void FinishCommand()
        {
            if (_ScsiPhase != ScsiPhase.Status) return;

            SetPhase(ScsiPhase.MessageIn);
            messageByte = 0x00;
            PrepareResponse(new byte[] { messageByte });

            System.Timers.Timer busFreeTimer = new System.Timers.Timer(100);
            busFreeTimer.Elapsed += (s, e) =>
            {

                SetPhase(ScsiPhase.BusFree);
                busFreeTimer.Dispose();
            };
            busFreeTimer.AutoReset = false;
            busFreeTimer.Start();
        }

        public byte ReadDataPort()
        {
            if (dataBuffer == null || dataBuffer.Position >= dataBuffer.Length)
                return 0x00;

            int value = dataBuffer.ReadByte();
            if (value == -1)
                return 0x00;

            dataOffset++;
            if (dataOffset >= dataBuffer.Length)
            {
                if (_ScsiPhase == ScsiPhase.DataIn)
                {
                    ActiveIrqs &= (byte)CdRomIrqSource.DataTransferReady;
                    ActiveIrqs |= (byte)CdRomIrqSource.DataTransferDone;
                    SendStatus(0);
                }
                else
                {
                    FinishCommand();
                }
            }

            return (byte)value;
        }

        public void WriteDataPort(byte value)
        {
            if (_ScsiPhase == ScsiPhase.CMD)
            {
                CMDBuffer[CMDBufferIndex++] = value;
                if (CMDBufferIndex == 1)
                    CMDLength = ScsiCMDLength((ScsiCommand)value);

                if (CMDBufferIndex >= CMDLength)
                {
                    ProcessCommand();
                    CMDBufferIndex = 0;
                }
            }
        }

        private int ScsiCMDLength(ScsiCommand cmd)
        {
            switch (cmd)
            {
                case ScsiCommand.TestUnitReady: return 6;
                case ScsiCommand.RequestSense: return 10;
                case ScsiCommand.Read: return 6;
                case ScsiCommand.ReadToc: return 10;
                case ScsiCommand.ReadSubCodeQ: return 10;
                default: return 10;
            }
        }

        private void ProcessCommand()
        {
            var cmd = (ScsiCommand)CMDBuffer[0];
            try
            {
                switch (cmd)
                {
                    case ScsiCommand.TestUnitReady:
                        HandleTestUnitReady();
                        break;
                    case ScsiCommand.RequestSense:
                        HandleRequestSense();
                        break;
                    case ScsiCommand.Read:
                        HandleRead6();
                        break;
                    case ScsiCommand.ReadToc:
                        HandleReadToc();
                        break;
                    case ScsiCommand.ReadSubCodeQ:
                        HandleSubChannelQ();
                        break;
                    case ScsiCommand.AudioStartPos:
                        AudioStartPos();
                        break;
                    case ScsiCommand.AudioEndPos:
                        AudioEndPos();
                        break;
                    case ScsiCommand.AudioPause:
                        Console.WriteLine($"CD-ROM: AudioPause");
                        CdPlaying = false;
                        SendStatus(0);
                        break;
                    default:
                        Console.WriteLine($"CD-ROM: Unsupported SCSI command: 0x{cmd:X}");
                        SendStatus(0);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CD-ROM: SCSI Error: {ex.Message}");
                SendStatus(0x02);
            }
        }

        private void HandleTestUnitReady()
        {
            Console.WriteLine($"CD-ROM: TestUnitReady");

            SendStatus(0x00);
        }

        private void HandleRequestSense()
        {
            byte[] senseData = { 0x70, 0x00, 0x06, 0x00, 0x00, 0x00 };

            PrepareResponse(senseData);

            SetPhase(ScsiPhase.DataIn);

            Console.WriteLine($"CD-ROM: RequestSense");
        }

        private void HandleRead6()
        {
            currentSector = CMDBuffer[3] | (CMDBuffer[2] << 8) | ((CMDBuffer[1] & 0x1F) << 16);
            byte SectorsToRead = CMDBuffer[4];
            byte[] sectorBuffer = new byte[SECTOR_SIZE];

            Console.WriteLine($"CD-ROM: ReadSector {currentSector} to {SectorsToRead + currentSector - 1}");
            long fileOffset = 0;
            long datasize = 0;
            currentTrack = tracks.FirstOrDefault(t => currentSector >= t.SectorStart && currentSector + SectorsToRead <= t.SectorEnd);
            int ssize = (currentTrack.Type == TrackType.AUDIO) ? SECTOR_SIZE : MODE1_DATA_SIZE;
            byte[] data = new byte[ssize * SectorsToRead];
            do
            {
                fileOffset = currentSector * SECTOR_SIZE;
                FileTrack.File.Seek(fileOffset, SeekOrigin.Begin);
                FileTrack.File.Read(sectorBuffer, 0, SECTOR_SIZE);
                switch (currentTrack.Type)
                {
                    case TrackType.MODE1:
                    case TrackType.MODE1_2352:
                        Array.Copy(sectorBuffer, DATA_SECTOR_OFFSET, data, datasize, MODE1_DATA_SIZE);
                        datasize += MODE1_DATA_SIZE;
                        break;
                    default:
                        Array.Copy(sectorBuffer, 0, data, datasize, SECTOR_SIZE);
                        datasize += SECTOR_SIZE;
                        break;
                }
                currentSector++;
                SectorsToRead--;
            } while (SectorsToRead > 0);

            PrepareResponse(data);

            SetPhase(ScsiPhase.DataIn);
        }

        private void HandleReadToc()
        {
            byte format = CMDBuffer[1]; // 参数
            byte trackNumber = FromBCD(CMDBuffer[2]); // 曲目号
            byte[] toc = new byte[4];
            int pos = 0;
            int minutes, seconds, frames;
            long calcLBA;

            switch (format)
            {
                case 0x00:
                    toc[pos++] = 0x01;
                    toc[pos++] = ToBCD(tracks.Count);
                    toc[pos++] = 0x00;
                    toc[pos++] = 0x00;
                    Console.WriteLine($"CD-ROM: ReadTOC TrackCount {tracks.Count}");
                    break;

                case 0x01:
                    calcLBA = FileTrack.File.Length / SECTOR_SIZE;
                    minutes = (int)(calcLBA / (60 * 75));
                    seconds = (int)((calcLBA / 75) % 60);
                    frames = (int)(calcLBA % 75);
                    toc[pos++] = ToBCD(minutes); // 分钟
                    toc[pos++] = ToBCD(seconds); // 秒
                    toc[pos++] = ToBCD(frames); // 帧
                    toc[pos++] = 0x00;
                    Console.WriteLine($"CD-ROM: ReadTOC TotalTime {minutes}:{seconds}:{frames}");
                    break;

                case 0x02:
                    currentTrack = tracks.FirstOrDefault(t => t.Number == trackNumber);
                    if (currentTrack == null)
                    {
                        Console.WriteLine($"CD-ROM: Invalid track number {trackNumber} for ReadTOC");
                        currentTrack = FileTrack;
                        return;
                    }
                    calcLBA = currentTrack.SectorStart + 150;
                    minutes = (int)(calcLBA / (60 * 75));
                    seconds = (int)((calcLBA / 75) % 60);
                    frames = (int)(calcLBA % 75);
                    toc[pos++] = ToBCD(minutes); // 分钟
                    toc[pos++] = ToBCD(seconds); // 秒
                    toc[pos++] = ToBCD(frames); // 帧
                    if (trackNumber > tracks.Count() || currentTrack.Type == TrackType.AUDIO)
                        toc[pos++] = 0;
                    else
                        toc[pos++] = 4;

                    Console.WriteLine($"CD-ROM: ReadTOC Track {trackNumber} StartPos {currentTrack.SectorStart}");
                    break;

                default:
                    Console.WriteLine($"CD-ROM: Unsupported ReadTOC format {format:X}");
                    return;
            }

            PrepareResponse(toc);

            SetPhase(ScsiPhase.DataIn);
        }

        private void HandleSubChannelQ()
        {
            currentSector = AudioCS;

            var track = tracks.FirstOrDefault(t => t.SectorStart <= currentSector && t.SectorEnd > currentSector);
            if (track == null)
            {
                Console.WriteLine("SubChannelQ Invalid LBA");
                SendStatus(0x00);
                return;
            }
            Console.WriteLine($"CD-ROM: SubChannelQ Track {track.Number} Sector {currentSector}");

            byte[] qData = new byte[10];
            int relLba = (int)(currentSector - track.SectorStart);
            qData[0] = (byte)((CdPlaying) ? 0 : 3);
            // 轨道信息
            qData[1] = (byte)(track.Adr | track.Control);
            qData[2] = FromBCD((byte)track.Number); // Index
            qData[3] = 1;
            // 相对时间
            qData[4] = ToBCD(relLba / (60 * 75));
            qData[5] = ToBCD((relLba / 75) % 60);
            qData[6] = ToBCD(relLba % 75);
            // 绝对时间
            int absLba = currentSector + 150;
            qData[7] = ToBCD(absLba / (60 * 75));
            qData[8] = ToBCD((absLba / 75) % 60);
            qData[9] = ToBCD(absLba % 75);

            PrepareResponse(qData);

            SetPhase(ScsiPhase.DataIn);
        }

        private int AudioGetPos()
        {
            int audiosector = 0;
            switch (CMDBuffer[9] & 0xC0)
            {
                case 0x00:
                    audiosector = (CMDBuffer[3] << 16) | (CMDBuffer[4] << 8) | CMDBuffer[5];
                    break;
                case 0x40:
                    {
                        int Minutes = FromBCD(CMDBuffer[2]);
                        int Seconds = FromBCD(CMDBuffer[3]);
                        int Frames = FromBCD(CMDBuffer[4]);
                        audiosector = MSFToLBA(Minutes, Seconds, Frames) - 150;
                        break;
                    }
                case 0x80:
                    {
                        byte trackNumber = FromBCD(CMDBuffer[2]);
                        int sector = (int)tracks.FirstOrDefault(t => t.Number == trackNumber).SectorStart;
                        audiosector = sector >= 0 ? sector : 0;
                        break;
                    }
            }
            return audiosector;
        }

        private void AudioStartPos()
        {
            AudioSS = AudioGetPos();
            AudioCS = AudioSS;
            Console.WriteLine($"CD-ROM: AudioStartPos [{AudioSS}]");
            if (CMDBuffer[1] == 0)
            {
                CdPlaying = true;
            }
            else
            {
                CdPlaying = false;
            }
            SendStatus(0x00);
        }

        private void AudioEndPos()
        {
            AudioES = AudioGetPos();
            Console.WriteLine($"CD-ROM: AudioEndPos [{AudioES}] Mode {CMDBuffer[1]:X1}");
            switch (CMDBuffer[1])
            {
                case 0: CdPlaying = false; break;
                case 1: CdPlayMode = CDPLAYMODE.LOOP; break;
                case 2: CdPlayMode = CDPLAYMODE.IRQ; break;
                case 3: CdPlayMode = CDPLAYMODE.STOP; break;
            }
            SendStatus(0x00);
        }

        public byte ReadAt(int address)
        {
            byte ret = 0xFF;

            switch (address & 0xFF)
            {
                case 0x00:
                    ret = ScsiStatus();
                    ProcessACK();
                    break;

                case 0x01:
                    ret = ReadDataPort();
                    break;

                case 0x02:
                    ret = (byte)(EnabledIrqs | (Signals[(int)ScsiSignal.Ack] ? 0x80 : 0));
                    break;

                case 0x03:
                    bramLocked = true;
                    ret = (byte)(ActiveIrqs | 0x10 | 0x02); //ActiveIrqs | 0x10 | (ReadRightChannel ? 0 : 0x02)
                    break;

                case 0x04:
                    ret = ResetRegValue;
                    break;

                case 0x07:
                    ret = (byte)(bramLocked ? 0 : 0x80);
                    break;

                case 0x08:
                    ret = ReadDataPort();
                    break;

                case 0x09: case 0x0A: case 0x0B: case 0x0C: case 0x0D: case 0x0E:
                    ret = _ADPCM.ReadData(address);
                    break;

                case 0x0F:
                    ret = _AUDIOFADE.ReadData(address);
                    break;

                case 0xC1: case 0xC2: case 0xC3: case 0xC5: case 0xC6: case 0xC7: //Magic Signature
                    byte[] Signature = { 0x00, 0xAA, 0x55, 0x03 };
                    ret = Signature[address & 0x03];
                    break;

                default:
                    Console.WriteLine("CD-ROM READ  ACCESS [ 0x{0:X} << 0x{1:X2} ]  |  CPU <0x{2:X}>", address, ret, HuC6280.CurrentPC);
                    break;
            }
            //Console.WriteLine("CD-ROM READ  ACCESS [ 0x{0:X} << 0x{1:X2} ]  |  CPU <0x{2:X}>", address, ret, HuC6280.CurrentPC);
            return ret;
        }

        public void WriteAt(int address, byte value)
        {
            //Console.WriteLine($"CD-ROM WRITE ACCESS [ 0x{address:X} >> 0x{value:X2} ]");
            switch (address & 0xF)
            {
                case 0x00: // 状态/控制寄存器 处理硬件复位或其他控制信号
                    if ((value & 0x80) != 0 && _ScsiPhase != ScsiPhase.DataIn)
                    {
                        ResetController();
                        SetPhase(ScsiPhase.CMD);
                    }
                    break;

                case 0x01: // 数据端口
                    WriteDataPort(value);
                    break;

                case 0x02:
                    EnabledIrqs = value;
                    Signals[(int)ScsiSignal.Ack] = (value & 0x80) != 0;
                    break;

                case 0x4:
                    ResetRegValue = (byte)(value & 0xF);
                    if ((value & 0x02) != 0)
                    {
                        ActiveIrqs = 0;
                        bramLocked = false;
                        EnabledIrqs &= 0x8F;
                    }
                    break;

                case 0x07:
                    bramLocked = (value & 0x80) == 0;
                    BRAM.WriteProtect(bramLocked);
                    break;

                case 0x08: case 0x09: case 0x0A: case 0x0B: case 0x0C: case 0x0D: case 0x0E:
                    _ADPCM.WriteData(address, value);
                    break;

                case 0x0F:
                    _AUDIOFADE.WriteData(address, value);
                    break;

                default:
                    Console.WriteLine($"CD-ROM WRITE ACCESS [ 0x{address:X} >> 0x{value:X2} ]");
                    break;
            }
        }

        private void ResetController()
        {
            CMDBufferIndex = 0;
            dataBuffer?.Dispose();
            dataBuffer = null;
            SetPhase(ScsiPhase.BusFree);
            currentSector = -1;
            //Console.WriteLine("CD-ROM Controller Reset");
        }

        private void ProcessACK()
        {
            if (Signals[(int)ScsiSignal.Req] && Signals[(int)ScsiSignal.Ack])
            {
                Signals[(int)ScsiSignal.Req] = false;
                return;
            }
            if (Signals[(int)ScsiSignal.Ack]) return;

            Signals[(int)ScsiSignal.Req] = true;
        }

        private byte ScsiStatus()
        {
            byte status = 0;
            if (Signals[(int)ScsiSignal.Io]) status |= 0x08;
            if (Signals[(int)ScsiSignal.Cd]) status |= 0x10;
            if (Signals[(int)ScsiSignal.Msg]) status |= 0x20;
            if (Signals[(int)ScsiSignal.Req]) status |= 0x40;
            if (Signals[(int)ScsiSignal.Bsy]) status |= 0x80;
            return status;
        }

        private enum ScsiSignal
        {
            Ack, Atn, Bsy, Cd, Io, Msg, Req, Rst, Sel
        }

        private enum ScsiCommand
        {
            TestUnitReady = 0x00,
            RequestSense = 0x03,
            Read = 0x08,
            AudioStartPos = 0xD8,
            AudioEndPos = 0xD9,
            AudioPause = 0xDA,
            ReadSubCodeQ = 0xDD,
            ReadToc = 0xDE,
        }

        private enum ScsiPhase
        {
            CMD, DataIn, Status, MessageIn, BusFree
        }
        #endregion
    }
}