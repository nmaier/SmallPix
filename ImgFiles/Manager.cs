using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace NMaier.SmallPix.ImgFiles
{
    class Manager : IImgProcessor
    {
        Dictionary<List<string>, IImgProcessor> dict;

        public Manager()
        {
            dict = new Dictionary<List<string>, IImgProcessor>();

            Assembly a = Assembly.GetExecutingAssembly();
            foreach (Type t in a.GetTypes())
            {
                if (t == GetType() || t.GetInterface("IImgProcessor") == null)
                {
                    continue;
                }
                ConstructorInfo ctor = t.GetConstructor(new Type[] { });
                if (ctor == null)
                {
                    continue;
                }
                IImgProcessor p = (IImgProcessor)ctor.Invoke(new object[] { });
                if (p == null)
                {
                    continue;
                }
                List<string> l = new List<string>(p.getSupportedExtensions());
                dict.Add(l, p);
                Console.WriteLine("Loaded processor: " + t.Name);
            }
            Console.WriteLine("Available processors: {0}", dict.Count);
        }

        public IImgInfo getFile(string file)
        {
            Boolean hitDecoder = false;
            if (!File.Exists(file))
            {
                throw new ImgDenyException("file does not exist");
            }
            foreach (KeyValuePair<List<string>, IImgProcessor> kvp in dict)
            {
                string ext = Path.GetExtension(file).ToLower();
                if (!kvp.Key.Contains(ext))
                {
                    continue;
                }
                try
                {
                    return kvp.Value.getFile(file);
                }
                catch (ImgDenyException)
                {
                    hitDecoder = true;
                }
            }
            if (hitDecoder)
            {
                throw new ImgBadException("failed to decode!");
            }
            throw new ImgDenyException("no decoder found!");
        }
        public string[] getSupportedExtensions()
        {
            return new string[] { };
        }
        public void Dispose()
        {
            dict.Clear();
        }
    }
}
