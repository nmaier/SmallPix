using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NMaier.SmallPix.ImgFiles
{
    class GifFile : IImgProcessor, IImgInfo
    {
        static string[] exts = new string[] { ".gif" };


        uint width = 0, height = 0;

        public void Dispose() { }
        public string getType() { return "GIF"; }
        public IImgInfo getFile(string file)
        {
            return new GifFile(file);
        }
        public string[] getSupportedExtensions()
        {
            return exts;
        }

        public uint getWidth() { return width; }
        public uint getHeight() { return height; }

        public GifFile() { }


        private uint readShort(Stream s)
        {
            byte[] block = new byte[2];
            s.Read(block, 0, 2);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(block);
            }
            return BitConverter.ToUInt16(block, 0);
        }

        public GifFile(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    InternalScan(fs);
                }
                catch (Exception ex)
                {
                    fs.Dispose();
                    throw ex;
                }
                fs.Dispose();
            }

        }

        private void InternalScan(FileStream fs)
        {
            byte[] block = new byte[5];
            try
            {
                fs.Read(block, 0, 5);
            }
            catch (Exception)
            {
                throw new ImgBadException("corrupt/small");
            }
            string header = ASCIIEncoding.ASCII.GetString(block);
            if (header.CompareTo("GIF87a") == 0 && header.CompareTo("GIF89a") == 0)
            {
                throw new ImgDenyException("not a gif");
            }

            try
            {
                fs.Seek(6, SeekOrigin.Begin);
            }
            catch (Exception)
            {
                throw new ImgBadException("corrupt small");
            }
            try
            {
                width = readShort(fs);
                height = readShort(fs);
            }
            catch (Exception)
            {
                throw new ImgBadException("corrupt/small");
            }
        }

    }
}
