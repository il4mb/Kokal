using camp.lib;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace camp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            RenderModule(this.ModulesGrid);
            Log.SetTextBox(this.LogTextBox);

            Log.WriteLine("Hallo world");
        }


        private void RenderModule(Grid grid)
        {
            List<Mod> modules = [
                new Mod("Apache", "E:\\Apache24\\bin\\"),
                new Mod("MySQL", "")
            ];

            Debug.WriteLine("=========== RENDER BEGIN ==========");

            foreach (Mod module in modules)
            {
                _ = new ModUi(module, grid);
                
            }
        }
    }


    public class ModuleContainer
    {
        private Module module;
        public ModuleContainer(Module mod)
        {
            module = mod;
        }

        private Label GetService()
        {
            return new()
            {
                Content = "Ok",
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new(10, 0, 10, 0)
            };
        }
        private Label GetModule()
        {
            return new()
            {
                Content = module.GetName(),
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new(10, 0, 10, 0)
            };
        }
        private Label GetPids()
        {
            return new()
            {
                Content = "123",
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new(10, 0, 10, 0)
            };
        }

        private Label GetPorts()
        {
            return new()
            {
                Content = ":80",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new(10, 0, 10, 0)

            };
        }

        private Grid GetControl()
        {

            Button toggleBtn = new() { Content = "Start" };
            Button adminBtn = new() { Content = "Admin" };
            Button configBtn = new() { Content = "Config" };
            Button logsBtn = new() { Content = "Logs" };

            toggleBtn.Click += (sender, e) =>
            {
                Debug.WriteLine(toggleBtn.Content);
                toggleBtn.Content = "Stop";
               // ExecuteCommand(module.GetPath() + "httpd");
            };


            List<Button> orders = [
                toggleBtn,
                adminBtn,
                configBtn,
                logsBtn
            ];

            Grid grid = new();
            grid.RowDefinitions.Add(new());

            for (int i = 0; i < orders.Count; i++)
            {
                var el = orders[i];
                el.Margin = new(2.5, 0, 2.5, 0);
                grid.ColumnDefinitions.Add(new());
                grid.Children.Add(el);
                Grid.SetColumn(el, i);
                Grid.SetRow(el, grid.RowDefinitions.Count);
            }

            return grid;
        }

        public void Render(Grid grid)
        {
            int rowCount = grid.RowDefinitions.Count;

            List<UIElement> orders = [GetService(), GetModule(), GetPids(), GetPorts(), GetControl()];

            for (int i = 0; i < orders.Count; i++)
            {
                if (0 == i % orders.Count)
                {
                    grid.RowDefinitions.Add(new() { Height = new(2, GridUnitType.Star) });
                }

                var el = orders[i] as FrameworkElement;

                el.Margin = new(0, 0, 0, 5);
                grid.Children.Add(el);
                Grid.SetRow(el, rowCount);
                Grid.SetColumn(el, i);
            }
        }








        private void ExecuteCommand(string command)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                

                // Run the command
                process.StandardInput.WriteLine(command);
                process.StandardInput.WriteLine("exit");

                // Read the output
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Optionally, you can handle the output and error as needed
                Debug.WriteLine("OUTPUT : " + output);
                Debug.WriteLine(error);


                process.WaitForExit();
                // process.Close();
            }
            catch (Exception ex)
            {
                // Handle exceptions, e.g., command not found, etc.
                MessageBox.Show($"Error executing command: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }


    public class Module
    {
        private string _name;
        private string _path;
        public Module(string name, string path)
        {
            SetName(name);
            SetPath(path);
        }

        public void SetName(string name) { _name = name; }

        public void SetPath(string path) { _path = path; }

        public string GetName() { return _name; }

        public string GetPath() { return _path; }
    }
}