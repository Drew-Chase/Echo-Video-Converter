using ChaseLabs.CLUpdate;
using System;
using System.IO;
using System.Threading;

namespace LauncherUpdater
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string path = args[0];
            string url = args[1];
            string exe_name = args[2];
            //Console.ReadLine();
            Thread.Sleep(2 * 1000);
            Download(path, url, exe_name, 0);
        }

        private static bool Download(string path, string url, string exe_name, int attempt)
        {
            long current = DateTime.Now.Ticks;
            long wanted = DateTime.Now.AddSeconds(2).Ticks;
            Console.WriteLine($"Working on Attempt #{attempt}");

            try
            {
                foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    Console.WriteLine($"Cleaning {file}");
                    File.Delete(file);
                }
                var update = Updater.Init(url, Path.Combine(path, "Update"), path, exe_name, false);
                update.Download();
                Thread.Sleep(2 * 1000);
                update.Unzip();
                Thread.Sleep(1000);
                update.CleanUp();
                Thread.Sleep(1000);
                update.LaunchExecutable();
                Console.ReadLine();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);
                while (current < wanted)
                {
                    current = DateTime.Now.Ticks;
                }
                Console.ReadLine();
                //return Download(path, url, exe_name, attempt + 1);
            }
            return true;
        }
    }
}
