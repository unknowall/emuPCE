namespace emuPCE
{
    public interface IAudioHandler
    {
        void PlaySamples(byte[] samples);
    }

    public interface IRenderHandler
    {
        void RenderFrame(int[] pixels, int width, int height);
    }
}
