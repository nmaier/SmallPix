using System;

namespace NMaier.SmallPix.ImgFiles
{
    abstract class ImgException : Exception
    {
        public ImgException(string Message)
            : base(Message)
        { }
    };
    class ImgDenyException : ImgException
    {
        public ImgDenyException(string Message)
            : base(Message)
        { }
    };
    class ImgBadException : ImgException
    {
        public ImgBadException(string Message)
            : base(Message)
        { }
    };
}