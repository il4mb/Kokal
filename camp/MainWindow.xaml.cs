using camp.lib;
using camp.ui;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace camp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IModContainer
    {

        private ModContainer ModContainer;
        public string Camp_Title { get; set; } = "Camp Control Panel";
        public string Camp_Version { get; set; } = "Version 1.0";


        public MainWindow()
        {

            InitializeComponent();
            this.DataContext = this;

            Log.Current.SetLogView(new LogView(this.LogContainer));
            Log.WriteLine($"Camp Control Panel {Camp_Version}");

            this.ModContainer = new(this);
            this.ModContainer.InitialModule();

            Loaded += MainWindow_Loaded;

        }



        public Grid GetParentHolder()
        {
            return this.ModulesGrid;
        }


        public Grid? GetTrayHolder()
        {
            return this.TrayModuleLayout;
        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Visible;
        }

        private void ShowWindow(object sender, RoutedEventArgs e)
        {
            // Show or activate your main window
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            // Cleanup and exit the application
            NotifyIcon.Visibility = Visibility.Collapsed;
            Application.Current.Shutdown();
        }


        // Override the OnStateChanged method to minimize to system tray when the window is minimized
        protected override void OnStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            base.OnStateChanged(e);
        }

        // Override OnClosing event to prevent the main window from being closed
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
            this.Hide(); // Hide instead of closing
        }


        private void Button_Setting_Click(object sender, RoutedEventArgs e)
        {

            Window settingWindow = new SettingWindow();
            settingWindow.Owner = this;
            settingWindow.DataContext = this.DataContext;
            settingWindow.ShowDialog();

        }

        private void Button_Netstat_Click(object sender, RoutedEventArgs e)
        {

            Window settingWindow = new NetstatWindow();
            settingWindow.Owner = this;
            settingWindow.DataContext = this.DataContext;
            settingWindow.ShowDialog();

        }

        private void Button_Explorer_Click(object sender, RoutedEventArgs e)
        {

            App.OpenExplorer(Directory.GetCurrentDirectory());

        }

        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {

            Window window = new HelpWindow();
            window.Owner = this;
            window.DataContext = this.DataContext;
            window.ShowDialog();
        }

        private void Button_Quit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}