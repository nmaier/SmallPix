using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using NMaier.SmallPix.ImgFiles;
using System.Threading;
using NMaier.Interop;

namespace NMaier.SmallPix
{
    abstract internal class BaseThread
    {
        protected enum Small { Yes, No, Bad, NotProcessed };

        protected List<BaseThread> children = new List<BaseThread>();

        private bool aborted = false;
        public bool Aborted
        {
            get { return aborted; }
            set
            {
                aborted = value;
                foreach (BaseThread child in children)
                {
                    child.Aborted = aborted;
                }
            }
        }


        private static Manager manager = new Manager();
        protected static Small isSmallImage(string file, uint min)
        {
            try
            {
                using (IImgInfo img = manager.getFile(file))
                {
                    lock (manager)
                    {
                        Console.WriteLine(
                            "File: {0} - {1}, Width: {2}, Height: {3}",
                            Path.GetFileName(file),
                            img.getType(),
                            img.getWidth(),
                            img.getHeight()
                        );
                    }
                    return img.getWidth() < min && img.getHeight() < min ? Small.Yes : Small.No;
                }
            }
            catch (ImgBadException)
            {
                return Small.Bad;
            }
            catch (ImgDenyException)
            {
                return Small.NotProcessed;
            }
        }

        protected string path;
        protected uint targetDims;
        protected bool dryRun = false;
        private bool lowPriority = false;
        protected bool LowPriority { get { return lowPriority; } }
        public BaseThread(string aPath, uint aTargetDims, bool aDryRun, bool aLowPriority)
        {
            dryRun = aDryRun;
            path = aPath;
            targetDims = aTargetDims;
            lowPriority = aLowPriority;
        }
        protected static void threadProc(Object info)
        {
            try
            {
                BaseThread bt = (info as BaseThread);
                if (bt.lowPriority)
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                    using (new LowPriority())
                    {
                        bt.Run();
                    }
                }
                else
                {
                    bt.Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        abstract public void Run();

        protected void hardDelete(String aFile, String aReason)
        {
            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    if (!dryRun)
                    {
                        File.Delete(aFile);
                    }
                    lock (manager)
                    {

                        Console.WriteLine("Delete ({0}): {1}", aReason, aFile);
                    }
                    return;
                }
                catch (Exception)
                {
                    Thread.Sleep(200);
                }
            }
        }
        protected bool analyze(string aPath)
        {
            for (int i = 0; i < 4 && !aborted; ++i)
            {
                try
                {
                    Small r = isSmallImage(aPath, targetDims);
                    switch (r)
                    {
                        case Small.Bad:
                            hardDelete(aPath, "corrupt");
                            return false;
                        case Small.Yes:
                            hardDelete(aPath, "small");
                            return false;
                        case Small.NotProcessed:
                            return false;
                        default:
                            return true;
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
            Console.WriteLine("{0}: {1}", Path.GetFileName(aPath), "was not accessible!");
            return false;
        }
        protected void addToDupes(string aPath)
        {
            if (aborted)
            {
                return;
            }
            ComparatorInfo o = new ComparatorInfo(aPath);
            lock (Watcher.hashes)
            {
                if (!Watcher.hashes.ContainsKey(o))
                {
                    Watcher.hashes.Add(o, true);
                }
                else
                {
                    hardDelete(aPath, "dupe");
                }
            }
        }
    }
}

