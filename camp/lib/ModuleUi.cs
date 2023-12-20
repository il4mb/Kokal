using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace camp.lib
{

    internal class ModuleUi
    {
        public Label ServiceLabel { get; }
        public Label NameLabel { get; }
        public Label PidsLabel { get; }
        public Label PortsLabel { get; }
        public ModUiControl ModUiControl { get; }
        public Module Mod { get; }
        public Dictionary<string, string>? Menus { get; set; }

        public ModuleUi(Module module, Grid container)
        {

            Application.Current.MainWindow.Closing += Window_Closing;
            Mod = module;


            ServiceLabel = CreateLabel("Ok", HorizontalAlignment.Left);
            NameLabel = CreateLabel(Mod.Name, HorizontalAlignment.Left);
            PidsLabel = CreateLabel("None", HorizontalAlignment.Left);
            PortsLabel = CreateLabel(":80", HorizontalAlignment.Right);
            ModUiControl = new ModUiControl();

            ModUiControl.ConfigBth.Click += ShowContextMenu;


            List<UIElement> uIElements = [ServiceLabel, NameLabel, PidsLabel, PortsLabel, ModUiControl];
            container.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < uIElements.Count; i++)
            {

                var el = uIElements[i] as FrameworkElement;
                el.Margin = new(0, 0, 0, 5);

                container.Children.Add(el);
                Grid.SetColumn(el, i);
                Grid.SetRow(el, container.RowDefinitions.Count - 1);

            }



            Mod.OnStart += async () =>
            {
                string pids = await Mod.GetPidAsync();
                if (!string.IsNullOrEmpty(pids))
                {
                    PidsLabel.Content = $"{pids}";
                    ModUiControl.StartBtn.Content = "Stop";
                }
            };

            Mod.OnToggle += async () =>
            {

                string pids = await Mod.GetPidAsync();
                if (!string.IsNullOrEmpty(pids))
                {
                    PidsLabel.Content = $"{pids}";
                    ModUiControl.StartBtn.Content = "Stop";
                } 
                else 
                {
                    PidsLabel.Content = "";
                    ModUiControl.StartBtn.Content = "Start";
                }
            };

            ModUiControl.StartBtn.Click += (s, a) => {

                Mod.Toggle();
            };
        }

        private void ShowContextMenu(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            // Check if the button already has a context menu
            if (btn.ContextMenu == null)
            {
                var contextMenu = new ContextMenu();
                contextMenu.Items.Add(new MenuItem()
                {
                    Header = "my.ini",
                    Icon = new Image
                    {
                        Source = new BitmapImage(new Uri("C:\\Users\\duria\\source\\repos\\camp\\camp\\icon\\file.png", UriKind.RelativeOrAbsolute)),
                        Width = 16,
                        Height = 16,
                        Margin = new(1, 1, 1, 1)
                    }
                });
                contextMenu.Items.Add(new MenuItem() { Header = "log access" });
                contextMenu.Items.Add(new MenuItem() { Header = "log error" });

                var more = new MenuItem()
                {
                    Header = "advanced option",
                    Icon = new Image
                    {
                        Source = new BitmapImage(new Uri("C:\\Users\\duria\\source\\repos\\camp\\camp\\icon\\menu.png", UriKind.RelativeOrAbsolute)),
                        Width = 16,
                        Height = 10,
                        Margin = new(1, 1, 1, 1)
                    }
                };

                more.Items.Add(new MenuItem() { Header = "Reinstall" });
                more.Items.Add(new MenuItem() { Header = "Delete" });
                contextMenu.Items.Add(more);

                // Set the context menu for the button
                btn.ContextMenu = contextMenu;
            }

            // Open the context menu
            btn.ContextMenu.IsOpen = true;
        }


        private void Window_Closing(Object sender, EventArgs args)
        {

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
