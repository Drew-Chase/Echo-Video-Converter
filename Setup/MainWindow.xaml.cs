using ChaseLabs.CLUpdate;
using IWshRuntimeLibrary;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
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
        private string install_dir = "";
        private bool create_desktop_shortcut = true, launch_on_completion = true;
        private ChaseLabs.CLUpdate.Interfaces.IUpdater update = null;
        public MainWindow()
        {
            InitializeComponent();
            RegisterEvents();
            Startup();
        }

        private void RegisterEvents()
        {
            MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => DragMove());
            ExitBtn.Click += ((object sender, RoutedEventArgs e) => Environment.Exit(0));

            OpenFolderBtn.Click += ((object sender, RoutedEventArgs e) => OpenFolder());

            desktop_shortcut_chkbx.Click += (object sender, RoutedEventArgs e) => { create_desktop_shortcut = (bool)((System.Windows.Controls.CheckBox)sender).IsChecked; };
            launch_on_completion_chkbx.Click += (object sender, RoutedEventArgs e) => { launch_on_completion = (bool)((System.Windows.Controls.CheckBox)sender).IsChecked; };

            FinishBtn.Click += ((object sender, RoutedEventArgs e) => { if (launch_on_completion && update != null) { update.LaunchExecutable(); } });

            NextBtn.Click += ((object sender, RoutedEventArgs e) =>
            {
                if (TabedContentFrame.SelectedIndex != TabedContentFrame.Items.Count - 1 && (string)((TabItem)TabedContentFrame.Items[TabedContentFrame.SelectedIndex]).Header != "Installing")
                {
                    TabedContentFrame.SelectedIndex += 1;
                }

                TitleLbl.Content = ((TabItem)TabedContentFrame.SelectedItem).Header;

                if ((string)((TabItem)TabedContentFrame.Items[TabedContentFrame.SelectedIndex]).Header == "Installing")
                {
                    NextBtn.Visibility = Visibility.Hidden;
                    PreviousBtn.Visibility = Visibility.Hidden;
                    Install();
                }

            });

            PreviousBtn.Click += ((object sender, RoutedEventArgs e) =>
            {
                if (TabedContentFrame.SelectedIndex != 0 && (string)((TabItem)TabedContentFrame.Items[TabedContentFrame.SelectedIndex]).Header != "Installing")
                {
                    TabedContentFrame.SelectedIndex -= 1;
                }

                TitleLbl.Content = ((TabItem)TabedContentFrame.SelectedItem).Header;
            });
        }

        private void Startup()
        {
            desktop_shortcut_chkbx.IsChecked = true;
            launch_on_completion_chkbx.IsChecked = true;
            TitleLbl.Content = ((TabItem)TabedContentFrame.Items[0]).Header;
            install_dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Chase Labs", "Horror Game Launcher");
            InstallDirectoryTxb.Text = install_dir;
        }

        private void OpenFolder()
        {
            using (FolderBrowserDialog folder = new FolderBrowserDialog())
            {
                if (InstallDirectoryTxb.Text != "")
                {
                    folder.SelectedPath = InstallDirectoryTxb.Text;
                }
                else
                {
                    folder.SelectedPath = install_dir;
                }
                folder.Description = "Select the Install Directory";
                DialogResult result = folder.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    install_dir = folder.SelectedPath;
                    InstallDirectoryTxb.Text = install_dir;
                }
                else
                {
                    install_dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Chase Labs", "Horror Game Launcher");
                    InstallDirectoryTxb.Text = install_dir;
                }

            }
        }

        private void Install()
        {
            status_lbl.Content = "Preparing Installer...";
            Task.Run(() =>
            {
                update = Updater.Init("https://www.dropbox.com/s/3ipx1tk07zw8r35/Launcher.zip?dl=1", System.IO.Path.Combine(Environment.GetEnvironmentVariable("TMP"), "Update"), install_dir, System.IO.Path.Combine(install_dir, "Horror Game Launcher.exe"), true);
                dis.Invoke(new Action(() =>
                {
                    status_lbl.Content = "Installing...";
                }), DispatcherPriority.ContextIdle);

                dis.Invoke(new Action(() =>
                {
                    status_lbl.Content = "Downloading...";
                }), DispatcherPriority.ContextIdle);

                update.Download();

                dis.Invoke(new Action(() =>
                {
                    status_lbl.Content = "Unziping...";
                }), DispatcherPriority.ContextIdle);

                update.Unzip();

                dis.Invoke(new Action(() =>
                {
                    status_lbl.Content = "Finishing Up...";
                }), DispatcherPriority.ContextIdle);

                update.CleanUp();

                System.Threading.Thread.Sleep(2000);
                dis.Invoke(new Action(() =>
                {
                    status_lbl.Content = "Launching Application...";
                }), DispatcherPriority.ContextIdle);
                CreateShortcut("Horror Game", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Chase Labs"), System.IO.Path.Combine(install_dir, "Horror Game Launcher.exe"));
                if (create_desktop_shortcut)
                {
                    CreateShortcut("Horror Game", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), System.IO.Path.Combine(install_dir, "Horror Game Launcher.exe"));
                }

                dis.Invoke(new Action(() =>
                {
                    TabedContentFrame.SelectedIndex++;
                }), DispatcherPriority.ContextIdle);

            });
        }

        public static void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {
            if (!Directory.Exists(shortcutPath))
            {
                Directory.CreateDirectory(shortcutPath);
            }

            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
            shortcut.Description = "Launches the Horror Game Launcher";   // The description of the shortcut
            shortcut.IconLocation = System.IO.Path.Combine(Directory.GetParent(targetFileLocation).FullName, "icon.ico");           // The icon of the shortcut
            shortcut.TargetPath = targetFileLocation;                 // The path of the file that will launch when the shortcut is run
            shortcut.Save();                                    // Save the shortcut
        }


    }
}
