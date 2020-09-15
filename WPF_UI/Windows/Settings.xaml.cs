using ChaseLabs.CLLogger;
using ChaseLabs.CLLogger.Interfaces;
using ChaseLabs.Echo.Video_Converter.Resources;
using ChaseLabs.Echo.Video_Converter.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ChaseLabs.Echo.Video_Converter.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public ILog log => LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).SetMinimumLogType(Lists.LogTypes.All);
        private List<Button> navButtons;
        private readonly Dispatcher dis = Dispatcher.CurrentDispatcher;

        public Settings()
        {
            InitializeComponent();
            RegisterEvents();
            onStartUp();
        }

        private void RegisterEvents()
        {
            //Nav Buttons
            MinimizeBtn.Click += ((object sender, RoutedEventArgs e) => { WindowState = WindowState.Minimized; });
            CloseBtn.Click += ((object sender, RoutedEventArgs e) => Hide());
            //UpdateBtn.Click += ((object sender, RoutedEventArgs e) => VersionControlUtilities.UpdateLauncher());
            Clean_Logs_Btn.Click += ((object sender, RoutedEventArgs e) => InterfaceUtilities.Singleton.CleanLogs());

            //Select File Button
            SelectFFMPEGFile.Click += ((object sender, RoutedEventArgs e) =>
            {
                string dir = FileUtilities.OpenFile(FileUtilities.FileExtensionType.Executable, Values.Singleton.FFMPEGDirectory, "Select FFMPEG Executable File");
                Values.Singleton.FFMPEGFile = dir == "" ? Values.Singleton.DefaultFFMPEGFile : dir;
                FFMPEG_TxtBx.Text = Values.Singleton.FFMPEGFile;
            });

            //Open Folder Buttons
            OpenRootLocation.Click += ((object sender, RoutedEventArgs e) => InterfaceUtilities.Singleton.OpenFolder(Values.Singleton.RootLocation));
            OpenInstallationLocation.Click += ((object sender, RoutedEventArgs e) => InterfaceUtilities.Singleton.OpenFolder(Values.Singleton.InstallationFolder));
            OpenConfigLocation.Click += ((object sender, RoutedEventArgs e) => InterfaceUtilities.Singleton.OpenFolder(Values.Singleton.ConfigLocation));
            OpenLogLocation.Click += ((object sender, RoutedEventArgs e) => InterfaceUtilities.Singleton.OpenFolder(Values.Singleton.LogLocation));
            EnclosedFolderOpenBtn.Click += ((object sender, RoutedEventArgs e) =>
            {
                string dir = string.Empty;
                if (EnclosedFolderTxtBox.Text != string.Empty || EnclosedFolderTxtBox.Text != null)
                {
                    try
                    {
                        dir = FileUtilities.OpenFolder(EnclosedFolderTxtBox.Text, "Select the temp folder for the media files.");

                    }
                    catch (Exception)
                    {
                        dir = FileUtilities.OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select the temp folder for the media files.");
                    }
                }
                else
                {
                    dir = FileUtilities.OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select the temp folder for the media files.");
                }

                if (dir != string.Empty && dir != "" && dir != " ")
                {
                    EnclosedFolderTxtBox.Text = dir;
                    log.Debug(@"File location set to " + dir);
                    Values.Singleton.TempFolderLocation = dir;
                }
                else
                {
                    EnclosedFolderTxtBox.Text = "Source ";
                    log.Debug(@"File location was not found");
                }
            });

            //Open File Buttons
            OpenConfigFile.Click += ((object sender, RoutedEventArgs e) => InterfaceUtilities.Singleton.OpenFile(System.IO.Path.Combine(Values.Singleton.ConfigLocation, Values.Singleton.ConfigFileLocation)));
            OpenLogFile.Click += ((object sender, RoutedEventArgs e) => InterfaceUtilities.Singleton.OpenFile(System.IO.Path.Combine(Values.Singleton.LogLocation, "latest.log")));

            //Check Boxes
            Overwrite_CKBX.Click += ((object sender, RoutedEventArgs e) => { Values.Singleton.OverwriteOriginal = (bool)Overwrite_CKBX.IsChecked; });
            EncoderConsole_CKBX.Click += ((object sender, RoutedEventArgs e) => { CheckBox check = (CheckBox)sender; Values.Singleton.ShowEncoderConsole = (bool)check.IsChecked; });
            UseNvidiaNVENC_CKBX.Click += ((object sender, RoutedEventArgs e) => { Values.Singleton.UseNvidiaNVENC = (bool)((CheckBox)sender).IsChecked; });
            TempEncodeFolder_CKBX.Click += ((object sender, RoutedEventArgs e) => { Values.Singleton.UseEnclosedFolder = (bool)TempEncodeFolder_CKBX.IsChecked; EnclosedFolderOpenBtn.IsEnabled = !(bool)TempEncodeFolder_CKBX.IsChecked; });
            IsNetworkPath_CKBX.Click += ((object sender, RoutedEventArgs e) => { Values.Singleton.IsNetworkPath = (bool)((CheckBox)sender).IsChecked; });
            UseHardwareEncoding_CKBX.Click += ((object sender, RoutedEventArgs e) => { Values.Singleton.UseHardwareEncoding = (bool)((CheckBox)sender).IsChecked; });

            //Window Events
            MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => DragMove());
        }

        private void onStartUp()
        {
            UpdateBtn.Visibility = Visibility.Hidden;
            LogLocation_TxtBx.Text = Values.Singleton.LogLocation;
            ConfigLocation_TxtBx.Text = Values.Singleton.ConfigLocation;
            RootLocation_TxtBx.Text = Values.Singleton.RootLocation;
            InstallationLocation_TxtBx.Text = Values.Singleton.InstallationFolder;
            build_txtblk.Text = $"{Values.Singleton.LauncherBuildVersion} | {Values.Singleton.ApplicationBuildVersion}";
            EnclosedFolderTxtBox.Text = Values.Singleton.TempFolderLocation;

            EnclosedFolderOpenBtn.IsEnabled = !Values.Singleton.UseEnclosedFolder;

            Overwrite_CKBX.IsChecked = Values.Singleton.OverwriteOriginal;
            TempEncodeFolder_CKBX.IsChecked = Values.Singleton.UseEnclosedFolder;
            UseNvidiaNVENC_CKBX.IsChecked = Values.Singleton.UseNvidiaNVENC;
            UseHardwareEncoding_CKBX.IsChecked = Values.Singleton.UseHardwareEncoding;
            EncoderConsole_CKBX.IsChecked = Values.Singleton.ShowEncoderConsole;
            IsNetworkPath_CKBX.IsChecked = Values.Singleton.IsNetworkPath;

            FFMPEG_TxtBx.Text = Values.Singleton.FFMPEGFile;

            navButtons = new List<Button>();
            addNavItems();

            foreach (object item in Enum.GetValues(typeof(SortType)))
            {
                sortType_comboBox.Items.Add(item);
            }
            sortType_comboBox.Foreground = System.Windows.Media.Brushes.Black;
            Task.Run(() =>
            {
                if (!VersionControlUtilities.IsLauncherUpToDate)
                {
                    dis.Invoke(() =>
                    {
                        UpdateBtn.IsEnabled = true;
                        UpdateBtn.Content = "Launcher Update Needed";
                        UpdateBtn.Visibility = Visibility.Visible;
                        UpdateBtn.Click += ((object sender, RoutedEventArgs e) => VersionControlUtilities.UpdateLauncher());
                    }, DispatcherPriority.Normal);
                }
                else
                {
                    dis.Invoke(() =>
                    {
                        UpdateBtn.IsEnabled = false;
                        UpdateBtn.Content = "Up-To-Date";
                        UpdateBtn.Visibility = Visibility.Hidden;
                    }, DispatcherPriority.Normal);
                }
            });


        }

        private void addNavItems()
        {
            foreach (TabItem item in contentTabController.Items)
            {
                Button btn = new Button
                {
                    Name = item.Header.ToString() + "Btn",
                    Content = item.Header.ToString(),
                    Style = Application.Current.Resources["NavButton"] as Style
                };

                btn.Click += nav_button_Clicked;

                navButtons.Add(btn);
                NavigationBar.Children.Add(btn);

                TextBlock tblk = new TextBlock
                {
                    Text = item.Header.ToString(),
                    Name = item.Header.ToString() + "TxtBx",
                    FontSize = 72,
                    Margin = new Thickness
                    {
                        Top = 48,
                        Left = 31,
                        Bottom = 0,
                        Right = 0
                    },
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 72 * item.Header.ToString().Length + 15,
                    Height = 72 + 15,
                };
                Grid grid = item.Content as Grid;
                grid.Children.Add(tblk);
                Grid.SetColumn(tblk, 1);
            }
        }

        private void nav_button_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender.GetType().Equals(typeof(Button)))
            {
                Button btn = (Button)sender;
                foreach (TabItem tab in contentTabController.Items)
                {
                    if (btn.Content.Equals(tab.Header.ToString()))
                    {
                        contentTabController.SelectedItem = tab;
                    }
                }

            }
        }

    }
}
