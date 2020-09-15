using ChaseLabs.CLUpdate;
using ChaseLabs.Echo.Video_Converter.Resources;
using System;
using System.Diagnostics;
using System.IO;
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
        public static bool IsLauncherUpToDate
        {
            get
            {
                UpdateManager.Singleton.Init(Values.Singleton.RemoteVersionURL, Values.Singleton.VersionPath);
                return !UpdateManager.Singleton.CheckForUpdate(Values.Singleton.LauncherVersionKey, Values.Singleton.VersionPath, Values.Singleton.RemoteVersionURL);
            }
        }
        public static void UpdateLauncher(string launcher_path = "")
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
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(Directory.GetParent(path).FullName);
                    }

                    client.DownloadFile("https://www.dropbox.com/s/mjhgowibs1vcd3y/LauncherUpdater.exe?dl=1", path);
                    client.Dispose();
                }
                Console.WriteLine($"cmd /C \"{path} \"{(launcher_path == "" ? Values.Singleton.LauncherDirectory : launcher_path)}\" \"{DownloadURL}\" \"{LaunchExe}\"\"");
                new Process() { StartInfo = new ProcessStartInfo() { FileName = "cmd", Arguments = $"/C \"{path} \"{(launcher_path == "" ? Values.Singleton.LauncherDirectory : launcher_path)}\" \"{DownloadURL}\" \"{LaunchExe}\"\"", CreateNoWindow = false } }.Start();

                UpdateManager.Singleton.UpdateVersionFile(app_key);
                UpdateManager.Singleton.UpdateVersionFile(exe_key);
                Environment.Exit(0);
            }
        }

        public static void UpdateApplication(System.Windows.Controls.Label StatusLabel = null, bool IsForced = false, Dispatcher CurrentDispatcher = null)
        {
            string remote_version = Values.Singleton.RemoteVersionURL;
            string local_version = Values.Singleton.VersionPath;
            UpdateManager.Singleton.Init(remote_version, local_version);
            string RootDirectory = Values.Singleton.RootLocation;
            string ConfigFolder = Values.Singleton.ConfigLocation;
            string ApplicationDirectory = Values.Singleton.ApplicationDirectory;
            string url_key = "URL";
            string exe_key = "EXE";
            string app_key = Values.Singleton.ApplicationVersionKey;
            string DownloadURL = UpdateManager.Singleton.GetArchiveURL(url_key);
            string LaunchExe = UpdateManager.Singleton.GetExecutableName(exe_key);
            if (StatusLabel != null)
            {
                StatusLabel.Content = "Checking For Updates...";
            }

            Task.Run(() =>
            {
                try
                {
                    Updater update = Updater.Init(DownloadURL, Path.Combine(RootDirectory, "Update"), ApplicationDirectory, Path.Combine(ApplicationDirectory, LaunchExe), true);
                    if (UpdateManager.Singleton.CheckForUpdate(app_key, local_version, remote_version) || IsForced)
                    {
                        CurrentDispatcher.Invoke(new Action(() =>
                        {
                            if (CurrentDispatcher != null)
                            {

                                if (StatusLabel != null)
                                {
                                    StatusLabel.Content = "Update Found...";
                                    StatusLabel.Content = "Dowloading Update...";
                                }
                            }
                        }), DispatcherPriority.Normal);
                        update.Download();
                        update.DownloadClient.DownloadProgressChanged += ((object sender, System.Net.DownloadProgressChangedEventArgs e) => { CurrentDispatcher.Invoke(new Action(() => { StatusLabel.Content = $"Downloading Update: {e.ProgressPercentage}%"; }), DispatcherPriority.ContextIdle); });
                        CurrentDispatcher.Invoke(new Action(() =>
                        {
                            if (StatusLabel != null)
                            {
                                StatusLabel.Content = "Installing Update...";
                            }
                        }), DispatcherPriority.Normal);

                        update.Unzip();

                        CurrentDispatcher.Invoke(new Action(() =>
                        {
                            if (StatusLabel != null)
                            {
                                StatusLabel.Content = "Finishing Update...";
                            }
                        }), DispatcherPriority.Normal);

                        update.CleanUp();
                        UpdateManager.Singleton.UpdateVersionFile(app_key);
                        UpdateManager.Singleton.UpdateVersionFile(exe_key);
                        if (CurrentDispatcher != null)
                        {
                            CurrentDispatcher.Invoke(new Action(() =>
                            {
                                if (StatusLabel != null)
                                {
                                    StatusLabel.Content = "Launching Application...";
                                }
                            }), DispatcherPriority.ContextIdle);
                            System.Threading.Thread.Sleep(2000);
                            update.LaunchExecutable();
                        }

                    }
                    else
                    {
                        Console.WriteLine("No Update Available");
                        CurrentDispatcher.Invoke(new Action(() =>
                        {
                            StatusLabel.Content = "Application Up to Date...";
                        }), DispatcherPriority.Normal);
                        System.Threading.Thread.Sleep(2000);
                        if (CurrentDispatcher != null)
                        {
                            CurrentDispatcher.Invoke(new Action(() =>
                            {
                                StatusLabel.Content = "Launching Application...";
                            }), DispatcherPriority.Normal);
                        }
                        System.Threading.Thread.Sleep(2000);
                        update.LaunchExecutable();
                    }

                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.StackTrace);
                    //UpdateApplication(StatusLabel, IsForced, CurrentDispatcher);
                }
            });
        }

    }
}
