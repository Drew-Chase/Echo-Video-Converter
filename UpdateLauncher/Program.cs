using ChaseLabs.CLUpdate;
using ChaseLabs.Echo.Video_Converter.Resources;
using System;

namespace UpdateLauncher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Values.Singleton.RootLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            try
            {
                Update();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                //Update();
            }
            Console.ReadKey();
        }

        static void Update()
        {
            ChaseLabs.CLUpdate.Interfaces.IUpdater update = Updater.Init("https://www.dropbox.com/s/kqvc6nw10nht0m1/launcher.zip?dl=1", System.IO.Path.Combine(Values.Singleton.RootLocation, "Update"), System.IO.Path.Combine(Environment.CurrentDirectory), System.IO.Path.Combine(Environment.CurrentDirectory, "Echo Video Converter.exe"), true);

            update.Download();
            Console.WriteLine("Downloaded");
            update.Unzip();

            update.CleanUp();

            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                if (System.IO.File.Exists("version"))
                {
                    System.IO.File.Delete("version");
                }

                client.DownloadFile("https://www.dropbox.com/s/nsxijbp9hkrbr1p/version?dl=1", System.IO.Path.Combine(Values.Singleton.RootLocation, "version"));
                client.Dispose();
            }
            update.LaunchExecutable();
        }
    }
}