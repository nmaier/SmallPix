using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace NMaier.SmallPix
{
    class ScannerThread : BaseThread
    {
        static uint threads = 0;

        bool recursive = false;
        uint level = 0;
        uint days = 0;

        public ScannerThread(string aPath, uint aTargetDims, bool aRecursive, uint aDays, bool aDryRun)
            : this(new DirectoryInfo(aPath), aTargetDims, aRecursive, aDays, 0, aDryRun)
        {
        }
        private ScannerThread(DirectoryInfo aPath, uint aTargetDims, bool aRecursive, uint aDays, uint aLevel, bool aDryRun)
            : base(aPath.FullName, aTargetDims, aDryRun)
        {
            days = aDays;
            recursive = aRecursive;
            level = aLevel;
            if (level < 2 && threads < 100)
            {
                threads += 1;
                Thread thread = new Thread(threadProc);
                thread.IsBackground = false;
                thread.Start(this);
            }
            else
            {
                Run();
            }
        }
        public override void Run()
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                processDirectory(info);
            }
            catch (IOException)
            {
            }
        }
        private void processDirectory(DirectoryInfo info)
        {
            if (recursive)
            {
                foreach (DirectoryInfo subdir in info.GetDirectories())
                {
                    if (Aborted)
                    {
                        return;
                    }
                    children.Add(new ScannerThread(subdir, targetDims, true, days, level + 1, dryRun));
                }
            }
            foreach (FileInfo file in info.GetFiles())
            {
                if (Aborted)
                {
                    return;
                }
                processFile(file);
            }
        }
        private void processFile(FileInfo info)
        {
            try
            {
                if (Aborted || (days != 0 && info.CreationTime <= DateTime.Now - TimeSpan.FromDays(days)))
                {
                    return;
                }
                if (Aborted || !analyze(info.FullName))
                {
                    return;
                }
                addToDupes(info.FullName);
            }
            catch (IOException)
            {
            }

        }
    }
}
