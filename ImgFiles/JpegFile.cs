using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NMaier.SmallPix.ImgFiles
{
    public class JpegFile : IImgProcessor, IImgInfo
    {
        private static string[] exts = new string[] { ".jpg", ".jpe", ".jpeg" };
        private uint width, height;

        private uint readShort(Stream s, int offset)
        {
            byte[] block = new byte[2];
            if (offset != 0)
            {
                s.Seek(offset, SeekOrigin.Current);
            }
            s.Read(block, 0, 2);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(block);
            }
            return BitConverter.ToUInt16(block, 0);
        }

        public void Dispose() { }
        public string getType() { return "JPEG"; }
        public IImgInfo getFile(string file)
        {
            return new JpegFile(file);
        }
        public string[] getSupportedExtensions()
        {
            return exts;
        }


        public JpegFile()
        {
            width = height = 0;
        }
        public JpegFile(string file)
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
            if (fs.ReadByte() != 0xff || fs.ReadByte() != 0xd8)
            {
                throw new ImgDenyException("not a jpeg");
            }
            for (; ; )
            {
                uint marker = (uint)fs.ReadByte();
                if (marker != 0xff)
                {
                    throw new ImgBadException("Invalid marker");
                }
                for (uint i = 0; i < 7 && marker == 0xff; ++i)
                {
                    marker = (uint)fs.ReadByte();
                }
                if (marker == 0xff)
                {
                    throw new ImgBadException("Invalid marker");
                }
                if (marker == 0xda)
                {
                    for (; ; )
                    {
                        uint d = (uint)fs.ReadByte();
                        if (d == 0xff)
                        {
                            d = (uint)fs.ReadByte();
                            if (d != 0x00 && !(d >= 0xd0 && d <= 0xd7))
                            {
                                marker = d;
                                break;
                            }
                        }
                    }
                }
                if (marker == 0xd8)
                {
                    continue;
                }
                else if (marker == 0xd9)
                {
                    throw new ImgBadException("No valid marker containing dimensions found!");
                }

                uint size = readShort(fs, 0) - 2;
                if (marker >= 0xc0 && marker < 0xc3)
                {
                    try
                    {
                        width = readShort(fs, 1);
                        height = readShort(fs, 0);
                    }
                    catch (IOException ex)
                    {
                        throw new ImgBadException(ex.Message);
                    }
                    return;
                }
                try
                {
                    fs.Seek(size, SeekOrigin.Current);
                }
                catch (Exception Ex)
                {
                    throw new ImgBadException(Ex.Message);
                }
            }
        }

        public uint getWidth() { return width; }
        public uint getHeight() { return height; }

    }
}