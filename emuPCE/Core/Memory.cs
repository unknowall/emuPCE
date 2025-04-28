using System;
using System.IO;

namespace ePceCD
{
    public class MemoryBank
    {
        protected int m_MemoryPage;

        public void SetMemoryPage(int page)
        {
            m_MemoryPage = page;
        }

        virtual public byte ReadAt(int address)
        {
            Console.WriteLine("Unknown memory access at address {0:x}-{1:x}", m_MemoryPage, address);
            return 0xFF;
        }

        virtual public void WriteAt(int address, byte data)
        {
            Console.WriteLine("Unknown memory access at address {0:x}-{1:x} -> {2:x}", m_MemoryPage, address, data);
        }
    }

    public class RamBank : MemoryBank
    {
        public byte[] m_Ram;

        public RamBank()
        {
            m_Ram = new byte[0x2000];
        }

        public override byte ReadAt(int address)
        {
            return m_Ram[address];
        }

        public override void WriteAt(int address, byte data)
        {
            m_Ram[address] = data;
        }
    }

    public class RomBank : MemoryBank
    {
        private byte[] m_Rom;

        public RomBank(byte[] page)
        {
            m_Rom = new byte[0x2000];
            page.CopyTo(m_Rom, 0);
        }

        public override byte ReadAt(int address)
        {
            return m_Rom[address];
        }

        public override void WriteAt(int address, byte data)
        {
            Console.WriteLine("Unknown rom access at address {0:x}-{1:x} -> {2:x}", m_MemoryPage, address, data);
        }
    }

    class ExtendedRomBank : MemoryBank
    {
        private byte[][] m_Rom;
        private static int m_Bank;

        public ExtendedRomBank(byte[][] page)
        {
            m_Rom = new byte[page.Length][];

            for (int i = 0; i < page.Length; i++)
            {
                m_Rom[i] = new byte[0x2000];
                page[i].CopyTo(m_Rom[i], 0);
            }

            m_Bank = 0;
        }

        public override byte ReadAt(int address)
        {
            return m_Rom[m_Bank][address];
        }

        public override void WriteAt(int address, byte data)
        {
            if ((address & 0x1FFC) == 0x1FF0)
                m_Bank = address & 3;
        }
    }

    public class SaveMemoryBank : MemoryBank, IDisposable
    {
        private byte[] m_Ram;
        private bool m_WriteProtect;
        private string savefile;

        public SaveMemoryBank(string filename)
        {
            m_Ram = new byte[0x800];
            m_Ram[0] = 0x48;
            m_Ram[1] = 0x55;
            m_Ram[2] = 0x42;
            m_Ram[3] = 0x4D;
            m_Ram[4] = 0x00;
            m_Ram[5] = 0xA0;
            m_Ram[6] = 0x10;
            m_Ram[7] = 0x80;

            if (filename == "") filename = "DefaultSave";
            savefile = filename + ".dat";

            try
            {
                FileStream file = new FileStream(savefile, FileMode.Open, FileAccess.Read);
                file.Read(m_Ram, 0, m_Ram.Length);
                file.Close();
            }
            catch //(IOException e)
            {
                //Console.WriteLine("No BRAM available to load: ", e.Message);
            }

            m_WriteProtect = false;
        }

        public void Dispose()
        {
            FileStream file = new FileStream(savefile, FileMode.OpenOrCreate, FileAccess.Write);
            file.Write(m_Ram, 0, m_Ram.Length);
            file.Close();
        }

        public void WriteProtect(bool protect)
        {
            m_WriteProtect = protect;
        }

        public override void WriteAt(int address, byte data)
        {
            if (address < 0x800 && !m_WriteProtect)
                m_Ram[address] = data;
        }

        public override byte ReadAt(int address)
        {
            if (address < 0x800 && !m_WriteProtect)
                return m_Ram[address];

            return 0xFF;
        }
    }

}
