using ChaseLabs.CLUpdate;
using ChaseLabs.Echo.Video_Converter.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            string remote_version = Values.Singleton.RemoteVersionURL;
            string local_version = Values.Singleton.VersionPath;
            string url_key = "LAUNCHER_URL";
            string exe_key = "LAUNCHER_EXE";
            string app_key = Values.Singleton.LauncherVersionKey;
            UpdateManager.Singleton.Init(remote_version, local_version);
            string DownloadURL = UpdateManager.Singleton.GetArchiveURL(url_key);
            string LaunchExe = UpdateManager.Singleton.GetExecutableName(exe_key);
            if (UpdateManager.Singleton.CheckForUpdate(app_key, local_version, remote_version))
            {
                string path = Path.Combine(Environment.GetEnvironmentVariable("TMP"), "CLUpdater", "EchoVideoConverter", "LauncherUpdater.exe");
                using (var client = new System.Net.WebClient())
                {
                    if (File.Exists(path)) File.Delete(path);
                    if (!Directory.Exists(path)) Directory.CreateDirectory(Directory.GetParent(path).FullName);
                    client.DownloadFile("https://www.dropbox.com/s/mjhgowibs1vcd3y/LauncherUpdater.exe?dl=1", path);
                    client.Dispose();
                }
                Console.WriteLine($"cmd /C \"{path} \"{Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName}\" \"{DownloadURL}\" \"{LaunchExe}\"\"");
                new Process() { StartInfo = new ProcessStartInfo() { FileName = "cmd", Arguments = $"/C \"{path} \"{Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName}\" \"{DownloadURL}\" \"{LaunchExe}\"\"", CreateNoWindow = false } }.Start();

                UpdateManager.Singleton.UpdateVersionFile(app_key);
                UpdateManager.Singleton.UpdateVersionFile(exe_key);
                Environment.Exit(0);
            }
        }

        public static void UpdateApplication(System.Windows.Controls.Label StatusLabel = null, bool IsForced = false, Dispatcher CurrentDispatcher = null)
        {
            string remote_version = Values.Singleton.RemoteVersionURL;
            string RootDirectory = Values.Singleton.RootLocation;
            string ConfigFolder = Values.Singleton.ConfigLocation;
            string ApplicationDirectory = Values.Singleton.ApplicationDirectory;
            string local_version = Values.Singleton.VersionPath;
            string url_key = "URL";
            string exe_key = "EXE";
            string app_key = Values.Singleton.ApplicationVersionKey;
            UpdateManager.Singleton.Init(remote_version, local_version);
            string DownloadURL = UpdateManager.Singleton.GetArchiveURL(url_key);
            string LaunchExe = UpdateManager.Singleton.GetExecutableName(exe_key);
            if (StatusLabel != null)
                StatusLabel.Content = "Checking For Updates...";
            Console.WriteLine(Path.Combine(RootDirectory, "Update"));
            Task.Run(() =>
            {
                Updater update = Updater.Init(DownloadURL, Path.Combine(RootDirectory, "Update"), ApplicationDirectory, Path.Combine(ApplicationDirectory, LaunchExe), true);
                if (UpdateManager.Singleton.CheckForUpdate(app_key, local_version, remote_version) || IsForced)
                {
                    if (CurrentDispatcher != null)
                    {

                        dis.Invoke(new Action(() =>
                        {
                            if (StatusLabel != null)
                                StatusLabel.Content = "Update Found...";
                        }), DispatcherPriority.ContextIdle);

                        dis.Invoke(new Action(() =>
                    {
                        if (StatusLabel != null)
                            StatusLabel.Content = "Dowloading Update...";
                    }), DispatcherPriority.ContextIdle);
                    }

                    update.Download();
                    if (StatusLabel != null)
                    {
                        update.DownloadClient.DownloadProgressChanged += ((object sender, System.Net.DownloadProgressChangedEventArgs e) =>
                        {
                            dis.Invoke(new Action(() =>
                            {
                                StatusLabel.Content = $"Downloading Update: {e.ProgressPercentage}%";
                            }), DispatcherPriority.ContextIdle);
                        });
                    }
                    update.DownloadClient.DownloadFileCompleted += ((object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                    {
                        if (CurrentDispatcher != null)
                            dis.Invoke(new Action(() =>
                        {
                            if (StatusLabel != null)
                                StatusLabel.Content = "Installing Update...";
                        }), DispatcherPriority.ContextIdle);

                        update.Unzip();

                        if (CurrentDispatcher != null)
                            dis.Invoke(new Action(() =>
                        {
                            if (StatusLabel != null)
                                StatusLabel.Content = "Finishing Update...";
                        }), DispatcherPriority.ContextIdle);

                        update.CleanUp();
                        UpdateManager.Singleton.UpdateVersionFile(app_key);
                        UpdateManager.Singleton.UpdateVersionFile(exe_key);
                        dis.Invoke(new Action(() =>
                        {
                            if (StatusLabel != null)
                                StatusLabel.Content = "Launching Application...";
                        }), DispatcherPriority.ContextIdle);
                        System.Threading.Thread.Sleep(2000);
                        update.LaunchExecutable();
                    });

                }
                else
                {
                    dis.Invoke(new Action(() =>
                    {
                        if (StatusLabel != null)
                            StatusLabel.Content = "Application Up to Date...";
                    }), DispatcherPriority.ContextIdle);
                    System.Threading.Thread.Sleep(2000);
                    dis.Invoke(new Action(() =>
                    {
                        if (StatusLabel != null)
                            StatusLabel.Content = "Launching Application...";
                    }), DispatcherPriority.ContextIdle);
                    System.Threading.Thread.Sleep(2000);
                    update.LaunchExecutable();
                }

            });
        }

    }
}
