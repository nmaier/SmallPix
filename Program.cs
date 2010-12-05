using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.IO;
using NMaier.SmallPix.ImgFiles;
using NMaier.GetOptNet;
using System.Windows.Forms;


namespace NMaier.SmallPix
{
    public class Watcher
    {
        [GetOptOptions(AcceptPrefixType = ArgumentPrefixType.Dashes)]
        private class Opts : GetOpt
        {
            [Argument(Helptext = "Minimal image dimensions. If both image sides are below this the image is considered small and will be deleted", Helpvar = "SIZE")]
            [ShortArgument('d')]
            public uint Dimensions = 1020;

#if !WINAPP
            [Argument(Helptext = "Scan existing files. All files will be analysed and deleted if either small, corrupt or duplicate.")]
            [ShortArgument('s')]
            [ArgumentAlias("ScanExisting")]
            public bool Scan = false;

            [Argument(Helptext = "Watch directories. New files will be analysed and deleted if either small, corrupt or duplicate.")]
            [ShortArgument('w')]
            public bool Watch = false;
#endif

            [Argument(Helptext = "Walk the directories recursively")]
            [ShortArgument('r')]
            public bool Recursive = false;

            [Argument(Helptext = "Only files created within the last week")]
            [ShortArgument('n')]
            public bool weekly = false;


            [Argument(Helptext = "Only files created within the last X days", Helpvar = "X")]
            [ShortArgument('l')]
            public uint days = 0;

            [Argument("dry-run", Helptext = "Dry-run (no deletion)")]
            public bool dryRun = false;

            [Parameters]
            public string[] Parameters = new string[0];
        }

        private Opts opts = new Opts();

        private Watcher(string[] args)
        {
            Console.WriteLine("SmallPixWatcher - Delete SmallPix on the fly");
            Console.WriteLine("(C) 2006,2007,2009 tn123");
            Console.WriteLine();

            try
            {
                opts.Parse(args);
#if !WINAPP
                if (!opts.Watch && !opts.Scan)
                {
                    throw new InvalidValueException("Either watch or scan folders or do both.");
                }
#endif
                if (opts.Parameters.Length == 0)
                {
                    throw new InvalidValueException("Provide at least one directory");
                }
                foreach (string path in opts.Parameters)
                {
                    if (!Directory.Exists(path))
                    {
                        throw new InvalidValueException(String.Format("Path {0} does not exist or is not a directory", path));
                    }
                }
                if (opts.weekly)
                {
                    opts.days = 7;
                }
            }
            catch (GetOptException ex)
            {
#if WINAPP
                MessageBox.Show(ex.Message + "\n\n" + opts.AssembleUsage(60), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                Console.WriteLine("Error: {0}", ex.Message);
                Console.WriteLine();
                opts.PrintUsage();
#endif
                return;
            }

            Console.Title = "SmallPixWatcher...";

            Console.WriteLine("Target image dimensions: {0}", opts.Dimensions);

            queue = new List<String>();
            hashes = new Dictionary<ComparatorInfo, bool>();

            List<ScannerThread> threads = new List<ScannerThread>();
            foreach (string path in opts.Parameters)
            {
#if !WINAPP
                if (opts.Watch)
                {
                    Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs cea)
                    {
                        opts.Watch = false;
                        cea.Cancel = true;
                        foreach (ScannerThread thread in threads)
                        {
                            thread.Aborted = true;
                        }
                    };
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = path;

                    watcher.NotifyFilter = NotifyFilters.FileName;
                    watcher.Filter = "*.*";

                    // Add event handlers.
                    watcher.Created += new FileSystemEventHandler(OnChanged);
                    watcher.Renamed += new RenamedEventHandler(OnRenamed);

                    watcher.IncludeSubdirectories = opts.Recursive;

                    // Begin watching.
                    watcher.EnableRaisingEvents = true;
                    Console.WriteLine("Watching (close or CTRL-C to quit)");
                }
                if (opts.Scan)
                {
                    threads.Add(new ScannerThread(path, opts.Dimensions, opts.Recursive, opts.days, opts.dryRun));
                }
#else
                threads.Add(new ScannerThread(path, opts.Dimensions, opts.Recursive, opts.days, opts.dryRun));
#endif
            }
#if !WINAPP
            for (; opts.Watch; )
            {
                Thread.Sleep(500);
            }
#endif
        }

        public static void Main(string[] args)
        {
            new Watcher(args);
        }

        static AutoResetEvent evt = new AutoResetEvent(false);
        public static List<String> queue;
        public static Dictionary<ComparatorInfo, bool> hashes;
        private void process(String file)
        {
            lock (queue)
            {
                if (queue.Contains(file))
                {
                    return;
                }
                queue.Add(file);
            }
            WatcherThread thread = new WatcherThread(queue, file, opts.Dimensions, opts.dryRun);
        }
        private void OnChanged(Object source, FileSystemEventArgs e)
        {
            process(e.FullPath);
        }
        private void OnRenamed(Object source, RenamedEventArgs e)
        {
            if (Path.GetDirectoryName(e.OldFullPath).ToLower() == Path.GetDirectoryName(e.FullPath).ToLower())
            {
                if (Path.GetExtension(e.OldFullPath).ToLower() == Path.GetExtension(e.FullPath).ToLower())
                {
                    return;
                }
            }
            process(e.FullPath);
        }

    }
}