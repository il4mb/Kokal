using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace camp.lib
{

    internal class ModUi
    {
        public Label ServiceLabel { get; }
        public Label NameLabel { get; }
        public Label PidsLabel { get; }
        public Label PortsLabel { get; }
        public ModUiControl ModUiControl { get; }
        public Mod Mod {  get; }

        public ModUi(Mod mod, Grid container)
        {

            Application.Current.MainWindow.Closing += Window_Closing;

            Mod = mod;
            ServiceLabel = CreateLabel("Ok", HorizontalAlignment.Left);
            NameLabel = CreateLabel(mod.Name, HorizontalAlignment.Left);
            PidsLabel = CreateLabel("None", HorizontalAlignment.Left);
            PortsLabel = CreateLabel(":80", HorizontalAlignment.Right);
            ModUiControl = new ModUiControl();


            List<UIElement> uIElements = [ServiceLabel, NameLabel, PidsLabel, PortsLabel, ModUiControl];


            container.RowDefinitions.Add(new RowDefinition());

            for (int i = 0; i < uIElements.Count; i++)
            {

                var el = uIElements[i] as FrameworkElement;
                el.Margin = new(0, 0, 0, 5);

                container.Children.Add(el);
                Grid.SetColumn(el, i);
                Grid.SetRow(el, container.RowDefinitions.Count-1);

            }


            this.Mod.OnStart += (Process process) =>
            {
               PidsLabel.Content = ""+process.Id;
            };

            ModUiControl.StartBtn.Click += ActionStart;
        }

        private void Window_Closing(Object sender, EventArgs args)
        {

        }


       
        private void ActionStart(object sender, RoutedEventArgs e)
        {
            this.Mod.Run();
            
        }

    

        private Label CreateLabel(string text, HorizontalAlignment horizontalAlignment)
        {
            return new()
            {
                Content = text,
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new(5, 2, 5, 2)
            };
        }

    }


    public class ModUiControl : Grid
    {
        public Button StartBtn { get; }

        public Button AdminBtn { get; }

        public Button ConfigBth { get; }

        public Button LogsBtn { get; }

        private List<UIElement> uIElements;
        public ModUiControl()
        {

            StartBtn = new Button() { Content = "Start" };
            ConfigBth = new Button() { Content = "Config" };
            AdminBtn = new Button() { Content = "Admin" };
            LogsBtn = new Button() { Content = "Log" };
            uIElements = [StartBtn, AdminBtn, ConfigBth, LogsBtn];


            this.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < uIElements.Count; i++)
            {

                var el = uIElements[i] as FrameworkElement;
                el.Margin = new(2.5, 0, 2.5, 0);

                this.ColumnDefinitions.Add(new());
                this.Children.Add(el);

                Grid.SetColumn(el, i);
                Grid.SetRow(el, 0);
            }
        }
    }
}
