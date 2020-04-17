using ChaseLabs.CLUpdate;
using ChaseLabs.Echo.Video_Converter.Resources;
using ChaseLabs.Echo.Video_Converter.Utilities;
using System;
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

        private void StartUp()
        {
            VersionControlUtilities.UpdateApplication(StatusLabel: status_lbl, CurrentDispatcher: dis);
            MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => DragMove());
        }
    }
}
