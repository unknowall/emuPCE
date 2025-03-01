using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static SDL2.SDL;

namespace emuPCE
{
    class Program
    {
        private PCESystem pce;

        private IntPtr m_Window;
        private IntPtr m_Renderer;
        private IntPtr m_Texture;
        private SDL_Rect srcRect, dstRect;

        private uint deviceid;
        private GCHandle gcHandle;
        private static SDL_AudioCallback audioCallbackDelegate = AudioCallbackWrapper;

        [STAThread]
        public static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
        }

        public Program()
        {
            SDL_Init(SDL_INIT_VIDEO);
            SDL_Init(SDL_INIT_AUDIO);

            m_Window = SDL_CreateWindow("PC Engine", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 1024, 768, SDL_WindowFlags.SDL_WINDOW_SHOWN);
            m_Renderer = SDL_CreateRenderer(m_Window, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            m_Texture = SDL_CreateTexture(m_Renderer, SDL_PIXELFORMAT_ARGB8888, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, 1024, 768);
            SDL_RenderClear(m_Renderer);
            SDL_RenderPresent(m_Renderer);
            srcRect = new SDL_Rect
            {
                x = 0,
                y = 0,
                w = 256,
                h = 240
            };
            dstRect = new SDL_Rect
            {
                x = 0,
                y = 0,
                w = 1024,
                h = 768
            };

            gcHandle = GCHandle.Alloc(this);
            SDL_AudioSpec desired = new SDL_AudioSpec
            {
                channels = 2,
                format = AUDIO_S16SYS,
                freq = 44100,
                samples = 1024,
                callback = audioCallbackDelegate,
                userdata = GCHandle.ToIntPtr(gcHandle)
            };
            SDL_AudioSpec obtained = new SDL_AudioSpec();
            deviceid = SDL_OpenAudioDevice(null, 0, ref desired, out obtained, 0);

            pce = new PCESystem();
            pce.FrameRender += FrameRender;

            Mute(false);
        }

        ~Program()
        {
            if (m_Texture != IntPtr.Zero) SDL_DestroyTexture(m_Texture);
            if (m_Renderer != IntPtr.Zero) SDL_DestroyRenderer(m_Renderer);
            if (m_Window != IntPtr.Zero) SDL_DestroyWindow(m_Window);
            if (deviceid != 0) SDL_CloseAudioDevice(deviceid);
            SDL_Quit();
            if (gcHandle.IsAllocated) gcHandle.Free();
        }

        private static void AudioCallbackWrapper(IntPtr userdata, IntPtr stream, int len)
        {
            GCHandle handle = GCHandle.FromIntPtr(userdata);
            Program instance = (Program)handle.Target;
            instance.AudioCallback(stream, len);
        }

        public void AudioCallback(IntPtr stream, int len)
        {
            pce.GetAudioSamples(stream, len);
        }

        public void Mute(bool mute)
        {
            if (deviceid != 0) SDL_PauseAudioDevice(deviceid, mute ? 1 : 0);
        }

        public void FrameRender(IntPtr pixels)
        {
            if (pixels == IntPtr.Zero) return;

            SDL_UpdateTexture(m_Texture, IntPtr.Zero, pixels, 256 * sizeof(int));
            SDL_RenderClear(m_Renderer);
            SDL_RenderCopy(m_Renderer, m_Texture, ref srcRect, ref dstRect);
            SDL_RenderPresent(m_Renderer);

            if (pce.CDfile != "")
            {
                SDL_SetWindowTitle(m_Window, "PC Engine [" + pce.CDfile + "]  -  " + pce.FPS.ToString() + " FPS");
            }
            else
            {
                SDL_SetWindowTitle(m_Window, "PC Engine [" + pce.RomName + "]  -  " + pce.FPS.ToString() + " FPS");
            }
        }

        private void GameKey(int key, short keyup)
        {
            switch (key)
            {
                case (int)SDL_Keycode.SDLK_UP:
                    pce.KeyState(PCEKEY.UP, keyup);
                    break;
                case (int)SDL_Keycode.SDLK_DOWN:
                    pce.KeyState(PCEKEY.DOWN, keyup);
                    break;
                case (int)SDL_Keycode.SDLK_RIGHT:
                    pce.KeyState(PCEKEY.RIGHT, keyup);
                    break;
                case (int)SDL_Keycode.SDLK_LEFT:
                    pce.KeyState(PCEKEY.LEFT, keyup);
                    break;
                case (int)SDL_Keycode.SDLK_x:
                    pce.KeyState(PCEKEY.B, keyup);
                    break;
                case (int)SDL_Keycode.SDLK_z:
                    pce.KeyState(PCEKEY.A, keyup);
                    break;
                case (int)SDL_Keycode.SDLK_RETURN:
                    pce.KeyState(PCEKEY.START, keyup);
                    break;
                case (int)SDL_Keycode.SDLK_TAB:
                    pce.KeyState(PCEKEY.SELECT, keyup);
                    break;
            }
        }

        public void Run()
        {
            bool running = true;
            bool pauseing = false;
            bool mute = false;

            SDL_Event e;

            OpenFileDialog ofn = new OpenFileDialog();
            ofn.Filter = "PCE Roms (*.pce,*.cue)|*.pce;*.cue";
            ofn.Title = "Open PCE Rom";

        LOADROM:
            if (ofn.ShowDialog() == DialogResult.Cancel) return;

            if (Path.GetExtension(ofn.FileName) == ".pce")
            {
                pce.LoadRom(ofn.FileName, false);
                pce.Reset();
            }
            else
            {
                if (!File.Exists("BIOS.pce"))
                {
                    Console.WriteLine("CDROM BIOS Not Found");
                    goto LOADROM;
                }
                pce.LoadCue(ofn.FileName);
                pce.LoadRom("BIOS.pce",false);
                pce.Reset();
            }
            while (running)
            {
                while (SDL_PollEvent(out e) != 0)
                {
                    if (e.type == SDL_EventType.SDL_QUIT)
                    {
                        running = false;
                    }
                    else if (e.type == SDL_EventType.SDL_KEYUP)
                    {
                        GameKey((int)e.key.keysym.sym, 1);
                    }
                    else if (e.type == SDL_EventType.SDL_KEYDOWN)
                    {
                        switch (e.key.keysym.sym)
                        {
                            case SDL_Keycode.SDLK_F5:
                                Mute(mute = !mute);
                                break;
                            case SDL_Keycode.SDLK_ESCAPE:
                                running = false;
                                break;
                            case SDL_Keycode.SDLK_SPACE:
                                pauseing = (!pauseing);
                                break;
                            case SDL_Keycode.SDLK_F1:
                                Mute(true);
                                ofn.Filter = "PC-Engine Roms (*.pce)|*.pce";
                                ofn.Title = "Open PCE Rom";
                                if (ofn.ShowDialog() != DialogResult.Cancel)
                                {
                                    pce.LoadRom(ofn.FileName, false);
                                    pce.Reset();
                                }
                                Mute(mute);
                                break;
                            case SDL_Keycode.SDLK_F2:
                                Mute(true);
                                ofn.Filter = "PC-Engine Bitswapped Roms (*.pce)|*.pce";
                                ofn.Title = "Open PCE Rom";
                                if (ofn.ShowDialog() != DialogResult.Cancel)
                                {
                                    pce.LoadRom(ofn.FileName, true);
                                    pce.Reset();
                                }
                                Mute(mute);
                                break;
                            case SDL_Keycode.SDLK_F3:
                                Mute(true);
                                ofn.Filter = "PC-Engine CD (*.cue)|*.cue";
                                ofn.Title = "Open PC Engine CD Image";
                                if (ofn.ShowDialog() != DialogResult.Cancel)
                                {
                                    pce.LoadCue(ofn.FileName);
                                }
                                Mute(mute);
                                break;
                            case SDL_Keycode.SDLK_F12:
                                pce.Reset();
                                break;
                            default:
                                GameKey((int)e.key.keysym.sym, 0);
                                break;
                        }
                    }

                }
                if (pauseing) continue;
                pce.Update();
            }
        }

    }
}
