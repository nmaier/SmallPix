using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace NMaier.SmallPix
{
    class WatcherThread : BaseThread
    {
        private List<string> queue;

        public WatcherThread(List<string> aQueue, string aPath, uint aTargetDims, bool aDryRun, bool aLowPriority)
            : base(aPath, aTargetDims, aDryRun, aLowPriority)
        {
            queue = aQueue;
            ThreadPool.QueueUserWorkItem(new WaitCallback(threadProc), this);
        }

        override public void Run()
        {
            bool newFile = analyze(path);
            lock (queue)
            {
                queue.Remove(path);
            }
            if (!newFile)
            {
                return;
            }
            try
            {
                addToDupes(path);
            }
            catch (IOException)
            {

            }
        }
    }
}
