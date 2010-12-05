using System;
using System.Collections.Generic;
using System.Text;

namespace NMaier.SmallPix.ImgFiles
{
    public interface IImgProcessor : IDisposable
    {
        IImgInfo getFile(string file);
        string[] getSupportedExtensions();
    }
    public interface IImgInfo : IDisposable
    {
        string getType();
        uint getWidth();
        uint getHeight();
    }
}
