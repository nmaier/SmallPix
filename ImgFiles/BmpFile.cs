using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NMaier.SmallPix.ImgFiles
{
    class BmpFile : IImgProcessor, IImgInfo
    {
        public BmpFile() { }
        protected BmpFile(string aFile)
        {
            using (FileStream fs = new FileStream(aFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    InternalScan(fs);
                }
                finally
                {
                    fs.Dispose();
                }
            }
        }

        private void InternalScan(Stream s)
        {
            byte[] header = new byte[26];
            try
            {
                s.Read(header, 0, header.Length);
            }
            catch (Exception)
            {
                throw new ImgBadException("corrupt/small");
            }
            if (header[0] != 'B' || header[1] != 'M')
            {
                throw new ImgDenyException("Not a bitmap");
            }

            width = BitConverter.ToUInt32(header, 18);
            height = BitConverter.ToUInt32(header, 22);
        }

        static string[] exts = new String[] { ".bmp" };

        #region IImgProcessor Members

        public IImgInfo getFile(string file)
        {
            return new BmpFile(file);
        }

        public string[] getSupportedExtensions()
        {
            return exts;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IImgInfo Members

        public string getType()
        {
            return "BMP";
        }

        uint width, height;
        public uint getWidth()
        {
            return width;
        }

        public uint getHeight()
        {
            return height;
        }

        #endregion
    }
}
