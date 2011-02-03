using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace NMaier.SmallPix
{
    public class ComparatorInfo : IComparable<ComparatorInfo>, IEquatable<ComparatorInfo>
    {
        FileInfo f;
        string _hash = null;
        string hash
        {
            get
            {
                if (_hash == null)
                {
                    _hash = "";
                    try
                    {
                        using (FileStream fs = f.Open(FileMode.Open, FileAccess.Read))
                        {
                            MD5 md5 = MD5.Create();
                            try
                            {
                                md5.ComputeHash(fs);
                                _hash = BitConverter.ToString(md5.Hash);
                            }
                            catch (Exception) { }
                            fs.Dispose();
                        }
                    }
                    catch (Exception) { }
                }
                return _hash;
            }
        }
        public ComparatorInfo(String aFile)
        {
            f = new FileInfo(aFile.ToLower());
        }
        public int CompareTo(ComparatorInfo rhs)
        {
            if (f == rhs.f)
            {
                return -1;
            }
            int rv = (int)(rhs.f.Length - f.Length);
            if (rv != 0)
            {
                return rv;
            }
            return hash.CompareTo(rhs.hash);
        }
        public override int GetHashCode() { return 0; }

        public bool Equals(ComparatorInfo other)
        {
            return CompareTo(other) == 0;
        }
    }
}
