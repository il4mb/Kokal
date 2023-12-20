using camp.lib;
using System.Windows;
using System.Windows.Controls;

namespace camp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static readonly string VERSION = "Version 1.0";
        private bool autoScroll = true;

        public MainWindow()
        {

            InitializeComponent();

            Log.SetGridContainer(this.LogContainer);


            this.scrollViewer.MouseEnter += (s, e) =>
            {
                autoScroll = false;
            };
            this.scrollViewer.MouseLeave += (s, e) =>
            {
                autoScroll = true;
            };

            this.LogContainer.LayoutUpdated += (s, e) =>
            {
                if(autoScroll) this.scrollViewer.ScrollToEnd();
            };

            Log.WriteLine($"Camp Control {VERSION}");

            RenderModule(this.ModulesGrid);
        }


        private void RenderModule(Grid grid)
        {

            _ = new ModuleScanner((module) =>
            {
                _ = new ModuleUi(module, grid);

            });
        }
    }
}