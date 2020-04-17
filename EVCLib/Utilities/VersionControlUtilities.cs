using ChaseLabs.CLUpdate;
using ChaseLabs.Echo.Video_Converter.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ChaseLabs.Echo.Video_Converter.Utilities
{
    /// <summary>
    /// <para>
    /// Author: Drew Chase
    /// </para>
    /// <para>
    /// Company: Chase Labs
    /// </para>
    /// </summary>
    public static class VersionControlUtilities
    {
        static Dispatcher dis = Dispatcher.CurrentDispatcher;

        public static bool IsLauncherUpToDate => !UpdateManager.Singleton.CheckForUpdate(Values.Singleton.LauncherVersionKey, Values.Singleton.VersionPath, Values.Singleton.RemoteVersionURL);
        public static void UpdateLauncher()
        {
            Task.Run(() =>
            {
                dis.Invoke(new Action(() =>
                {
                    Console.WriteLine(System.IO.Path.Combine(Values.Singleton.RootLocation, "Update"));
                }), DispatcherPriority.ContextIdle);
                if (UpdateManager.Singleton.CheckForUpdate(Values.Singleton.LauncherVersionKey, Values.Singleton.VersionPath, Values.Singleton.RemoteVersionURL))
                {
                    using (System.Net.WebClient client = new System.Net.WebClient())
                    {
                        client.DownloadFile("https://dl.drewchaseproject.com/echo-video-converter-setup", Path.Combine(Environment.GetEnvironmentVariable("TMP"), "UpdateLauncher.exe"));

                        client.Dispose();
                    }
                    if (File.Exists(Path.Combine(Environment.GetEnvironmentVariable("TMP"), "UpdateLauncher.exe")))
                    {
                        new Process() { StartInfo = new ProcessStartInfo() { FileName = Path.Combine(Environment.GetEnvironmentVariable("TMP"), "UpdateLauncher.exe"), UseShellExecute = true, CreateNoWindow = true } }.Start();
                        Environment.Exit(0);
                    }
                }
            });
        }

        public static void UpdateApplication(System.Windows.Controls.Label StatusLabel = null, bool IsForced = false, Dispatcher CurrentDispatcher = null)
        {
            Task.Run(() =>
            {
                if (CurrentDispatcher != null)
                    CurrentDispatcher.Invoke(new Action(() =>
                    {
                        if (StatusLabel != null)
                            StatusLabel.Content = "Checking For Updates...";
                        Console.WriteLine(System.IO.Path.Combine(Values.Singleton.RootLocation, "Update"));
                    }), DispatcherPriority.ContextIdle);
                ChaseLabs.CLUpdate.Interfaces.IUpdater update = Updater.Init(Values.Singleton.ApplicationURL, System.IO.Path.Combine(Values.Singleton.RootLocation, "Update"), Values.Singleton.ApplicationDirectory, System.IO.Path.Combine(Values.Singleton.ApplicationDirectory, "application.exe"), true);
                Thread.Sleep(500);
                if (UpdateManager.Singleton.CheckForUpdate(Values.Singleton.ApplicationVersionKey, Values.Singleton.VersionPath, Values.Singleton.RemoteVersionURL) || IsForced || !Directory.Exists(Values.Singleton.ApplicationDirectory) || !File.Exists(System.IO.Path.Combine(Values.Singleton.ApplicationDirectory, "application.exe")))
                {
                    if (CurrentDispatcher != null)
                        CurrentDispatcher.Invoke(new Action(() =>
                    {
                        if (StatusLabel != null)
                            StatusLabel.Content = "Update Found...";
                    }), DispatcherPriority.ContextIdle);

                    if (CurrentDispatcher != null)
                        CurrentDispatcher.Invoke(new Action(() =>
                    {
                        if (StatusLabel != null)
                            StatusLabel.Content = "Downloading Update...";
                    }), DispatcherPriority.ContextIdle);

                    update.Download();

                    if (CurrentDispatcher != null)
                        CurrentDispatcher.Invoke(new Action(() =>
                    {
                        if (StatusLabel != null)
                            StatusLabel.Content = "Unziping Update...";
                    }), DispatcherPriority.ContextIdle);

                    update.Unzip();

                    if (CurrentDispatcher != null)
                        CurrentDispatcher.Invoke(new Action(() =>
                    {
                        if (StatusLabel != null)
                            StatusLabel.Content = "Unziping Update...";
                    }), DispatcherPriority.ContextIdle);

                    update.CleanUp();

                    UpdateManager.Singleton.UpdateVersionFile(Values.Singleton.ApplicationVersionKey);

                }
                if (CurrentDispatcher != null)
                    CurrentDispatcher.Invoke(new Action(() =>
                {
                    StatusLabel.Content = "Application Up to Date...";
                }), DispatcherPriority.ContextIdle);
                System.Threading.Thread.Sleep(1000);
                if (CurrentDispatcher != null)
                    CurrentDispatcher.Invoke(new Action(() =>
                {
                    StatusLabel.Content = "Launching Application...";
                }), DispatcherPriority.ContextIdle);
                System.Threading.Thread.Sleep(1000);
                update.LaunchExecutable();
            });
        }

    }
}
