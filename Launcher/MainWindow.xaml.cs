using ChaseLabs.Echo.Video_Converter.Resources;
using ChaseLabs.Echo.Video_Converter.Utilities;
using System;
using System.IO;
using System.Reflection;
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
        private Dispatcher dis;
        public MainWindow()
        {
            InitializeComponent();
            RegisterEvents();
            Values.Singleton.LauncherDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            UpdateLauncher();
            Update();
        }

        private void RegisterEvents()
        {
            dis = Dispatcher.CurrentDispatcher;
            MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => DragMove());
            ExitBtn.Click += ((object sender, RoutedEventArgs e) => Environment.Exit(0));
        }

        private void UpdateLauncher()
        {
            VersionControlUtilities.UpdateLauncher();
        }

        private void Update()
        {
            VersionControlUtilities.UpdateApplication(StatusLabel: status_lbl, IsForced: false, CurrentDispatcher: dis);
        }
    }
}
