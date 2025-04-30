using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable SYSLIB0011

namespace ePceCD
{
    public class PCECore : IDisposable
    {
        public double FRAME_LIMIT = 60.0;

        private Task MainTask;

        public BUS Bus;

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
        public string CdBios = "";
        public string CDfile = "";
        public string GameID = "";

        public bool Pauseing, Pauseed, Running, Boost;

        public IRenderHandler hostrender;
        public IAudioHandler hostaudio;

        public PCECore(IRenderHandler render, IAudioHandler audio, string rom, string bios, string gameid)
        {
            hostrender = render;
            hostaudio = audio;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ePceCD Booting...");
            Console.ResetColor();

            RomName = rom;
            CdBios = bios;
            GameID = gameid;

            Bus = new BUS(render, audio);

            if (!File.Exists(RomName))
            {
                Console.WriteLine("Rom Not Found");
                return;
            }

            if (!File.Exists(CdBios))
            {
                Console.WriteLine("CD BIOS Not Found");
                return;
            }

            if (Path.GetExtension(RomName) == ".pce")
            {
                Bus.LoadRom(RomName, false);
                if (GameID == "")
                {
                    GameID = $"{PCECore.CalcCRC32(RomName):X8}";
                }

            }
            else if (Path.GetExtension(RomName) == ".cue")
            {
                CDfile = RomName;
                Bus.LoadCue(RomName);
                Bus.LoadRom(CdBios, false);
                if (GameID == "")
                {
                    GameID = $"{PCECore.CalcCRC32(Bus.CDRom.tracks[0].File.Name):X8}";
                }
            }
            else
            {
                return;
            }

            Bus.Reset();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ePceCD Running...");
            Console.ResetColor();
        }

        public void Dispose()
        {

        }

        public void Stop()
        {
            if (Running)
            {
                Running = false;
                Pauseing = false;
                MainTask.Wait();
            }
        }

        public void Pause()
        {
            Pauseing = !Pauseing;
        }

        public void WaitPaused()
        {
            Pauseing = true;
            while (!Pauseed)
            {
                Thread.Sleep(20);
                Pauseing = true;
            }
        }

        public void Start()
        {
            if (GameID == "")
                return;

            if (MainTask == null && !Running && Bus != null)
            {
                Running = true;
                Pauseing = false;
                MainTask = Task.Factory.StartNew(PCE_EXECUTE, TaskCreationOptions.LongRunning);
            }
        }

        private void PCE_EXECUTE()
        {
            double TargetFrameTime = 1000 / FRAME_LIMIT; // 60 FPS
            var stopwatch = new Stopwatch();
            double accumulatedError = 0;

            while (Running)
            {

                stopwatch.Restart();

                if (!Pauseing)
                {
                    Pauseed = false;

                    while (!Bus.PPU.FrameReady)
                    {
                        Bus.CPU.m_Clock = Bus.tick();
                        Bus.CPU.cycle();
                    }

                    Bus.PPU.FrameReady = false;
                    ApplyCheats();
                }
                else
                    Pauseed = true;

                if (Boost)
                    continue;

                // 精确帧时间控制
                double elapsed = stopwatch.Elapsed.TotalMilliseconds;
                double targetDelay = TargetFrameTime - elapsed + accumulatedError;

                if (targetDelay > 1)
                {
                    int sleepTime = (int)(targetDelay - 0.1); // 预留0.1ms给SpinWait
                    Thread.Sleep(sleepTime);

                    // 亚毫秒级补偿
                    var spin = new SpinWait();
                    while (stopwatch.Elapsed.TotalMilliseconds < TargetFrameTime)
                    {
                        spin.SpinOnce();
                    }
                }
                else
                {
                    Thread.Yield();
                }

                // 累计时间误差用于补偿
                accumulatedError += TargetFrameTime - stopwatch.Elapsed.TotalMilliseconds;
                accumulatedError = Math.Max(-TargetFrameTime, Math.Min(accumulatedError, TargetFrameTime));
            }
        }

        public void LoadState(string Fix = "")
        {
            if (!Running)
                return;

            string fn = "./SaveState/" + GameID + "_Save" + Fix + ".dat";
            if (!File.Exists(fn))
                return;

            Pauseing = true;
            while (!Pauseed)
            {
                Thread.Sleep(20);
                Pauseing = true;
            }

            Bus.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Bus = StateFromFile<BUS>(fn);
            Bus.DeSerializable(hostrender, hostaudio);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("State LOADED.");
            Console.ResetColor();

            Pauseing = false;
        }

        public void SaveState(string Fix = "", bool isBak = false)
        {
            if (!Running)
                return;

            Pauseing = true;
            while (!Pauseed)
            {
                Thread.Sleep(20);
                Pauseing = true;
            }

            string fn = "./SaveState/" + GameID + "_Save" + Fix + ".dat";

            Bus.ReadySerializable();
            StateToFile(Bus, fn);

            if (!isBak)
            {
                fn = "./Icons/" + GameID + ".png";
                SaveToPng(fn, Bus.PPU._screenBuf, Bus.PPU.SCREEN_WIDTH, Bus.PPU.m_VDC_VDW);
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("State SAVEED.");
            Console.ResetColor();

            Pauseing = false;
        }

        public void SaveToPng(string filePath, int[] buffer, int Width, int Height)
        {
            Bitmap bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);

            Marshal.Copy(buffer, 0, bmpData.Scan0, Width * Height);
            bitmap.UnlockBits(bmpData);

            bitmap.Save(filePath, ImageFormat.Png);
        }

        private BUS StateFromFile<BUS>(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                gzipStream.CopyTo(memoryStream);
                memoryStream.Position = 0;
                BinaryFormatter formatter = new BinaryFormatter();
                return (BUS)formatter.Deserialize(memoryStream);
            }
        }

        private void StateToFile<BUS>(BUS obj, string filePath)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, obj);
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
                {
                    memoryStream.Position = 0;
                    memoryStream.CopyTo(gzipStream);
                }
            }
        }

        public void ApplyCheats()
        {
            foreach (var code in cheatCodes)
            {
                if (!code.Active)
                    continue;

                foreach (var item in code.Item)
                {
                    switch (item.Width)
                    {
                        case 1:
                            Bus.memory[0].m_Ram[item.Address] = (byte)item.Value;
                            break;
                        case 2:
                            Bus.memory[0].m_Ram[item.Address + 1] = (byte)(item.Value >> 8);
                            Bus.memory[0].m_Ram[item.Address] = (byte)(item.Value & 0xFF);
                            break;
                        case 4:
                            break;
                    }
                }
            }
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

        public void GetAudioSamples(IntPtr stream, int len)
        {
            Bus.APU.GetSamples(stream, len);
        }

        public void Button(PCEKEY key, bool isDown, int ConIdx = 0)
        {
            Bus.JoyPort.KeyState(key, (short)(isDown ? 0 : 1));
        }

        public void KeyState(PCEKEY key, short keyup)
        {
            Bus.JoyPort.KeyState(key, keyup);
        }

        public void KeyDown(PCEKEY key)
        {
            Bus.JoyPort.KeyState(key, 0);
        }

        public void KeyUp(PCEKEY key)
        {
            Bus.JoyPort.KeyState(key, 1);
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

        public static uint CalcCRC32(string filename)
        {
            uint[] crc32Table = new uint[256];
            const uint polynomial = 0xEDB88320;

            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ polynomial;
                    else
                        crc >>= 1;
                }
                crc32Table[i] = crc;
            }

            uint crcValue = 0xFFFFFFFF;
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        byte index = (byte)((crcValue ^ buffer[i]) & 0xFF);
                        crcValue = (crcValue >> 8) ^ crc32Table[index];
                    }
                }
            }

            return crcValue ^ 0xFFFFFFFF;
        }
    }
}
