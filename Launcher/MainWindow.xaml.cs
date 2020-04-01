using ChaseLabs.CLUpdate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dispatcher dis = Dispatcher.CurrentDispatcher;
        public MainWindow()
        {
            InitializeComponent();
            status_lbl.Content = "Checking For Updates...";
            Task.Run(() =>
            {
                var update = Updater.Init("https://www.dropbox.com/s/5p5qbhl97emxn4k/application.zip?dl=1", "update", "bin", "application.exe", true);
                if (UpdateManager.CheckForUpdate("application version: ", "version", "https://www.dropbox.com/s/nsxijbp9hkrbr1p/version?dl=1"))
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

                    using (System.Net.WebClient client = new System.Net.WebClient())
                    {
                        if (System.IO.File.Exists("version"))
                        {
                            System.IO.File.Delete("version");
                        }

                        client.DownloadFile("https://www.dropbox.com/s/nsxijbp9hkrbr1p/version?dl=1", "version");
                        client.Dispose();
                    }
                }
                else
                {
                    dis.Invoke(new Action(() =>
                    {
                        status_lbl.Content = "Application Up to Date...";
                    }), DispatcherPriority.ContextIdle);
                    System.Threading.Thread.Sleep(2000);
                }
                dis.Invoke(new Action(() =>
                {
                    status_lbl.Content = "Launching Application...";
                }), DispatcherPriority.ContextIdle);
                System.Threading.Thread.Sleep(2000);
                update.LaunchExecutable();

            });

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
