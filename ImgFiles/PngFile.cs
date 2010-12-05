using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NMaier.SmallPix.ImgFiles
{
    class PngFile : IImgProcessor, IImgInfo
    {

        static string[] exts = new String[] { ".png", ".mng" };

        private UInt32 width, height;

        public void Dispose() { }
        public string getType() { return "PNG"; }
        public IImgInfo getFile(string file)
        {
            return new PngFile(file);
        }
        public string[] getSupportedExtensions()
        {
            return exts;
        }

        public PngFile()
        {
            width = height = 0;
        }

        public PngFile(string file)
        {
            width = height = 0;
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
            try
            {
                fs.Seek(12, SeekOrigin.Begin);
            }
            catch (Exception)
            {
                throw new ImgBadException("Corrupt");
            }
            byte[] block = new byte[4];
            fs.Read(block, 0, 4);
            string header = ASCIIEncoding.ASCII.GetString(block);
            if (header.CompareTo("IHDR") != 0 && header.CompareTo("MHDR") != 0)
            {
                throw new ImgDenyException("not a png");
            }
            try
            {
                width = readUint(fs);
                height = readUint(fs);
            }
            catch (Exception ex)
            {
                throw new ImgBadException(ex.Message);
            }
        }

        private uint readUint(FileStream fs)
        {
            byte[] block = new byte[4];
            fs.Read(block, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(block);

            }
            return BitConverter.ToUInt32(block, 0);
        }

        public uint getWidth() { return width; }
        public uint getHeight() { return height; }
    }
}
