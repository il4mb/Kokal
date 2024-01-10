using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace kokal
{


    /// <summary>
    /// Interaction logic for SupportWindow.xaml
    /// </summary>
    public partial class SupportWindow : Window
    {

        private const string Url = "https://il4mb.github.io/kokal/support/";
        private bool hasNavigated = false;
        public SupportWindow()
        {

            InitializeComponent();

            StartNavigation();

            webBrowser.Navigating += HandleNavigating;
            webBrowser.Navigated += HandleNavigation;
            webBrowser.LoadCompleted += (o, args) =>
            {
                ((WebBrowser)o).InvokeScript("eval", "document.oncontextmenu = function() { return false; }", "JavaScript");
            };


        }
        private void HandleNavigating(object sender, NavigatingCancelEventArgs e)
        {
            // Check if navigation has already occurred
            if (hasNavigated)
            {
                e.Cancel = true; // Cancel further navigation
            }
            App.OpenBrowser(e.Uri.ToString());
        }

        private void HandleNavigation(object sender, NavigationEventArgs e)
        {
            // Set the flag to true after the first navigation
            hasNavigated = true;

            // Your code for handling navigation completion
        }

        // ...

        // In some other part of your code, initiate the first navigation
        private void StartNavigation()
        {
            webBrowser.Navigate(Url);
        }

        private void WebBrowser_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Set e.Handled to true to prevent the default context menu
            e.Handled = true;
        }

    }


}
