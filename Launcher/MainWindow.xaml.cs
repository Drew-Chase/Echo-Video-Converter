using ChaseLabs.CLUpdate;
using ChaseLabs.Echo.Video_Converter.Resources;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dispatcher dis = Dispatcher.CurrentDispatcher;
        public MainWindow()
        {
            InitializeComponent();
            StartUp();
        }

        void StartUp()
        {
            //Task.Run(() => UpdateLauncher()).Wait();
            Task.Run(() => UpdateApplication());
        }


        void UpdateLauncher()
        {
            dis.Invoke(new Action(() =>
            {
                status_lbl.Content = "Checking For Updates...";
                Values.Singleton.RootLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Console.WriteLine(System.IO.Path.Combine(Values.Singleton.RootLocation, "Update"));
            }), DispatcherPriority.ContextIdle);
            if (UpdateManager.Singleton.CheckForUpdate(Values.Singleton.LauncherVersionKey, Values.Singleton.VersionPath, Values.Singleton.RemoteVersionURL))
            {
                using (var client = new System.Net.WebClient())
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
        }

        void UpdateApplication()
        {
            dis.Invoke(new Action(() =>
            {
                status_lbl.Content = "Checking For Updates...";
                Values.Singleton.RootLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Console.WriteLine(System.IO.Path.Combine(Values.Singleton.RootLocation, "Update"));
            }), DispatcherPriority.ContextIdle);
            ChaseLabs.CLUpdate.Interfaces.IUpdater update = Updater.Init(Values.Singleton.ApplicationURL, System.IO.Path.Combine(Values.Singleton.RootLocation, "Update"), Values.Singleton.ApplicationDirectory, System.IO.Path.Combine(Values.Singleton.ApplicationDirectory, "application.exe"), true);
            Thread.Sleep(500);
            if (UpdateManager.Singleton.CheckForUpdate(Values.Singleton.ApplicationVersionKey, Values.Singleton.VersionPath, Values.Singleton.RemoteVersionURL))
            {
                dis.Invoke(new Action(() =>
                {
                    status_lbl.Content = "Update Found...";
                }), DispatcherPriority.ContextIdle);

                dis.Invoke(new Action(() =>
                {
                    status_lbl.Content = "Downloading Update...";
                }), DispatcherPriority.ContextIdle);

                update.Download();

                dis.Invoke(new Action(() =>
                {
                    status_lbl.Content = "Unziping Update...";
                }), DispatcherPriority.ContextIdle);

                update.Unzip();

                dis.Invoke(new Action(() =>
                {
                    status_lbl.Content = "Unziping Update...";
                }), DispatcherPriority.ContextIdle);

                update.CleanUp();

                UpdateManager.Singleton.UpdateVersionFile(Values.Singleton.ApplicationVersionKey);

            }
            dis.Invoke(new Action(() =>
            {
                status_lbl.Content = "Application Up to Date...";
            }), DispatcherPriority.ContextIdle);
            System.Threading.Thread.Sleep(1000);
            dis.Invoke(new Action(() =>
            {
                status_lbl.Content = "Launching Application...";
            }), DispatcherPriority.ContextIdle);
            System.Threading.Thread.Sleep(1000);
            update.LaunchExecutable();

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
