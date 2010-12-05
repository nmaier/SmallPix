using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace NMaier.Glob
{
    public class Glob
    {
        internal static Regex magic = new Regex("[*?]");

        private bool recursive = false;
        private string pattern = string.Empty;
        public string Pattern
        {
            get { return pattern; }
        }
        public Glob(string aPattern) : this(aPattern, false) { }
        public Glob(string aPattern, bool aRecursive)
        {
            pattern = aPattern;
            recursive = aRecursive;
        }

        public IEnumerable<string> GetItems()
        {
            return GetItems(pattern);
        }
        internal IEnumerable<string> GetItems(string aPattern)
        {
            if (!magic.IsMatch(aPattern))
            {
                if (Directory.Exists(aPattern))
                {
                    yield return new DirectoryInfo(aPattern).FullName;
                    if (recursive)
                    {
                        foreach (string i in GetItems(aPattern + Path.DirectorySeparatorChar + "*"))
                        {
                            yield return i;
                        }
                    }
                }
                else if (File.Exists(aPattern))
                {
                    yield return new FileInfo(aPattern).FullName;
                }
                yield break;
            }

            string dir = "";
            string fn = aPattern;
            int idx = aPattern.LastIndexOfAny(new char[] { '/', '\\' });
            if (idx >= 0)
            {
                dir = aPattern.Substring(0, idx);
                fn = aPattern.Substring(idx + 1);
            }
            if (String.IsNullOrEmpty(dir))
            {
                foreach (string d in Directory.GetDirectories(Directory.GetCurrentDirectory(), fn))
                {
                    yield return new DirectoryInfo(d).FullName;
                    if (recursive)
                    {
                        foreach (string i in GetItems(d + Path.DirectorySeparatorChar + "*"))
                        {
                            yield return i;
                        }
                    }
                }
                foreach (string f in Directory.GetFiles(Directory.GetCurrentDirectory(), fn))
                {
                    yield return new FileInfo(f).FullName;
                }
                yield break;
            }
            if (magic.IsMatch(dir))
            {
                foreach (string s in GetItems(dir))
                {
                    if (!Directory.Exists(s))
                    {
                        continue;
                    }
                    foreach (string i in GetItems(s + Path.DirectorySeparatorChar + fn))
                    {
                        yield return i;
                    }
                }
                yield break;
            }
            foreach (string i in Directory.GetDirectories(dir, fn))
            {
                yield return new DirectoryInfo(i).FullName;
                if (recursive)
                {
                    foreach (string di in GetItems(i + Path.DirectorySeparatorChar + "*"))
                    {
                        yield return di;
                    }
                }
            }
            foreach (string i in Directory.GetFiles(dir, fn))
            {
                yield return new FileInfo(i).FullName;
            }
        }
        public IEnumerable<FileInfo> GetFiles()
        {
            if (!magic.IsMatch(pattern))
            {
                if (Directory.Exists(pattern))
                {
                    if (recursive)
                    {
                        foreach (string i in GetItems(pattern + Path.DirectorySeparatorChar + "*"))
                        {
                            if (File.Exists(i))
                            {
                                yield return new FileInfo(i);
                            }
                        }
                    }
                }
                else if (File.Exists(pattern))
                {
                    yield return new FileInfo(pattern);
                }
                yield break;
            }

            string dir = "";
            string fn = pattern;
            int idx = pattern.LastIndexOfAny(new char[] { '/', '\\' });
            if (idx >= 0)
            {
                dir = pattern.Substring(0, idx);
                fn = pattern.Substring(idx + 1);
            }
            if (String.IsNullOrEmpty(dir))
            {
                foreach (string d in Directory.GetDirectories(Directory.GetCurrentDirectory(), fn))
                {
                    if (recursive)
                    {
                        foreach (string i in GetItems(d + Path.DirectorySeparatorChar + "*"))
                        {
                            if (File.Exists(i))
                            {
                                yield return new FileInfo(i);
                            }
                        }
                    }
                }
                foreach (string f in Directory.GetFiles(Directory.GetCurrentDirectory(), fn))
                {
                    yield return new FileInfo(f);
                }
                yield break;
            }
            if (magic.IsMatch(dir))
            {
                foreach (string s in GetItems(dir))
                {
                    if (!Directory.Exists(s))
                    {
                        continue;
                    }
                    foreach (string i in GetItems(s + Path.DirectorySeparatorChar + fn))
                    {
                        if (File.Exists(i))
                        {
                            yield return new FileInfo(i);
                        }
                    }
                }
                yield break;
            }
            foreach (string i in Directory.GetDirectories(dir, fn))
            {
                if (recursive)
                {
                    foreach (string di in GetItems(i + Path.DirectorySeparatorChar + "*"))
                    {
                        if (File.Exists(di))
                        {
                            yield return new FileInfo(di);
                        }
                    }
                }
            }
            foreach (string i in Directory.GetFiles(dir, fn))
            {
                yield return new FileInfo(i);
            }
        }
        public IEnumerable<DirectoryInfo> GetDirectories()
        {
            return GetDirectories(pattern);
        }
        public IEnumerable<DirectoryInfo> GetDirectories(string aPattern)
        {
            if (!magic.IsMatch(aPattern))
            {
                if (Directory.Exists(aPattern))
                {
                    yield return new DirectoryInfo(aPattern);
                    if (recursive)
                    {
                        foreach (DirectoryInfo i in GetDirectories(aPattern + Path.DirectorySeparatorChar + "*"))
                        {
                            yield return i;
                        }
                    }
                }
                yield break;
            }

            string dir = "";
            string fn = aPattern;
            int idx = aPattern.LastIndexOfAny(new char[] { '/', '\\' });
            if (idx >= 0)
            {
                dir = aPattern.Substring(0, idx);
                fn = aPattern.Substring(idx + 1);
            }
            if (String.IsNullOrEmpty(dir))
            {
                foreach (string d in Directory.GetDirectories(Directory.GetCurrentDirectory(), fn))
                {
                    yield return new DirectoryInfo(d);
                    if (recursive)
                    {
                        foreach (DirectoryInfo i in GetDirectories(d + Path.DirectorySeparatorChar + "*"))
                        {
                            yield return i;
                        }
                    }
                }
                yield break;
            }
            if (magic.IsMatch(dir))
            {
                foreach (string s in GetItems(dir))
                {
                    if (!Directory.Exists(s))
                    {
                        continue;
                    }
                    foreach (DirectoryInfo i in GetDirectories(s + Path.DirectorySeparatorChar + fn))
                    {
                        yield return i;
                    }
                }
                yield break;
            }
            foreach (string i in Directory.GetDirectories(dir, fn))
            {
                yield return new DirectoryInfo(i);
                if (recursive)
                {
                    foreach (DirectoryInfo di in GetDirectories(i + Path.DirectorySeparatorChar + "*"))
                    {
                        yield return di;
                    }
                }
            }
        }
    }
}
