using camp.lib;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace camp.ui
{

    public class ModUi : IModWatcher
    {
        public Image ModuleIcon { get; }
        public Label NameLabel { get; }
        public Label PidsLabel { get; }
        public Label PortsLabel { get; }
        public ModUiControl ModUiControl { get; }
        private ModUiTray? _ModUiTray { get; }
        public Module Module { get; }

        private ModUi(Module module, IModContainer imc)
        {

            Application.Current.MainWindow.Closing += Window_Closing;

            module.SetModWacher(this);


            ModuleIcon = new Image()
            {
                Width = 20,
                Height = 20,
                Source = module.GetIcon()
            };
            NameLabel = CreateLabel(module.GetName(), HorizontalAlignment.Left);
            PidsLabel = CreateLabel("", HorizontalAlignment.Left);
            PortsLabel = CreateLabel("", HorizontalAlignment.Right);
            ModUiControl = new ModUiControl();


            List<UIElement> uIElements = [ModuleIcon, NameLabel, PidsLabel, PortsLabel, ModUiControl];
            imc.GetParentHolder().RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < uIElements.Count; i++)
            {
                var el = uIElements[i] as FrameworkElement;
                el.Margin = new(0, 0, 0, 5);
                imc.GetParentHolder().Children.Add(el);
                Grid.SetColumn(el, i);
                Grid.SetRow(el, imc.GetParentHolder().RowDefinitions.Count - 1);
            }


            if (imc.GetTrayHolder() != null)
            {
                _ModUiTray = new ModUiTray(module, imc.GetTrayHolder());
            }


            ModUiControl.ToggleBtn.IsEnabled = false;
            ModUiControl.AdminBtn.IsEnabled = false;

            ModUiControl.ConfigBth.Click += (s, e) => ShowContextMenu(module);
            ModUiControl.AdminBtn.Click += (s, e) => module.AdminNavigate();
            ModUiControl.ToggleBtn.Click += (s, e) =>
            {

                module.Toggle();
            };

            if (module.IsComponentOkay())
            {
                Log.WriteLine("ModUi", $"Module {module.GetName()} loaded successful, version {module.GetVersion()}");
                ModUiControl.ToggleBtn.IsEnabled = true;

                if (_ModUiTray != null) {
                    _ModUiTray.ActionBtn.IsEnabled = true;
                }

            }
            else
            {

                Log.WriteLine($"Module {module.GetName()} cannot load!, Some components of the module may be corrupt.", Code.Warning);
                ModUiControl.ToggleBtn.IsEnabled = false;
                if(_ModUiTray != null) {
                    _ModUiTray.ActionBtn.IsEnabled = false;
                }

            }

            this.Module = module;
        }


        public static ModUi NewInstance(Module module, IModContainer imc)
        {
            return new ModUi(module, imc);
        }


        private void ShowContextMenu(Module module)
        {
            Button btn = ModUiControl.ConfigBth;

            // Check if the button already has a context menu
            if (btn.ContextMenu == null)
            {
                var contextMenu = new ContextMenu();
                var menuOpts = module.GetMenuOpts();

                if (menuOpts != null)
                {

                    for (int i = 0; i < menuOpts.Count; i++)
                    {
                        var opt = menuOpts[i];
                        MenuItem menuItem = new()
                        {
                            Header = opt.Name
                        };
                        menuItem.Click += (s, a) => module.OnMenuItemClicked(opt.Code, i);
                        contextMenu.Items.Add(menuItem);
                    }
                }


                var more = new MenuItem()
                {
                    Header = "advanced option",
                    Icon = new Image
                    {
                        Source = (BitmapImage)App.Current.FindResource("ic_menu"),
                        Width = 16,
                        Height = 10,
                        Margin = new(1, 1, 1, 1)
                    }
                };

                var reinstall = new MenuItem() { Header = "Reinstall" };
                more.Items.Add(reinstall);
                contextMenu.Items.Add(more);
                reinstall.Click += (s, e) =>
                {
                    new Thread(async (t) =>
                    {

                        Thread.CurrentThread.IsBackground = true;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Mouse.OverrideCursor = Cursors.Wait;
                            ModUiControl.ToggleBtn.IsLoading = true;
                            ModUiControl.ToggleBtn.IsEnabled = false;
                            ModUiControl.AdminBtn.IsEnabled = false;
                        });

                        await Module.Install();

                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            Mouse.OverrideCursor = null;
                            ModUiControl.ToggleBtn.IsLoading = false;
                            ModUiControl.ToggleBtn.IsEnabled = true;
                            ModUiControl.AdminBtn.IsEnabled = true;
                        });

                    }).Start();
                };

                // Set the context menu for the button
                btn.ContextMenu = contextMenu;
            }

            // Open the context menu
            btn.ContextMenu.IsOpen = true;
        }


        private void Window_Closing(object sender, EventArgs args)
        {

        }


        public static Label CreateLabel(string text, HorizontalAlignment horizontalAlignment)
        {
            return new()
            {
                Content = text,
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new(5, 2, 5, 2)
            };
        }




        public void OnToggle()
        {
            if (_ModUiTray != null)
            {
                _ModUiTray.ActionBtn.IsEnabled = false;
                _ModUiTray.ActionBtn.IsLoading = true;
            }

            this.ModUiControl.ToggleBtn.IsEnabled = false;
            this.ModUiControl.ToggleBtn.IsLoading = true;

            Debug.WriteLine("Toggle is click");
        }

        public void OnStartTrigged()
        {
            Debug.WriteLine("Toggle is action start");
        }

        public void OnStopTrigged()
        {
            Debug.WriteLine("Toggle is action stop");
        }

        public void OnStartPerfomed(RunInfo runInfo)
        {

            Debug.WriteLine($"Process Run with pid {runInfo.pid} and port {runInfo.port}");
            PidsLabel.Content = runInfo.pid;
            PortsLabel.Content = runInfo.port;
            ModUiControl.ToggleBtn.Style = (Style)Application.Current.FindResource("DangerButton");
            ModUiControl.ToggleBtn.Text = "Stop";

            ModUiControl.ToggleBtn.IsLoading = false;
            ModUiControl.ToggleBtn.IsEnabled = true;
            ModUiControl.AdminBtn.IsEnabled = true;

            if (_ModUiTray != null)
            {
                _ModUiTray.AdminBtn.IsEnabled = true;
                _ModUiTray.ActionBtn.IsEnabled = true;
                _ModUiTray.ActionBtn.IsLoading = false;
                _ModUiTray.ActionBtn.Text = "Stop";
                _ModUiTray.ActionBtn.Style = (Style)Application.Current.FindResource("DangerButton");
            }

        }

        public void OnStopPerfomed()
        {

            Debug.WriteLine($"Process has been stoped");
            PidsLabel.Content = null;
            PortsLabel.Content = null;
            ModUiControl.ToggleBtn.Text = "Start";
            ModUiControl.ToggleBtn.Style = Application.Current.FindResource(typeof(Button)) as Style;

            ModUiControl.ToggleBtn.IsLoading = false;
            ModUiControl.ToggleBtn.IsEnabled = true;
            ModUiControl.AdminBtn.IsEnabled = false;

            if (_ModUiTray != null)
            {
                _ModUiTray.AdminBtn.IsEnabled = false;
                _ModUiTray.ActionBtn.IsEnabled = true;
                _ModUiTray.ActionBtn.IsLoading = false;
                _ModUiTray.ActionBtn.Text = "Start";
                _ModUiTray.ActionBtn.Style = Application.Current.FindResource(typeof(Button)) as Style;
            }

        }

        public void OnReloadState(RunInfo runInfo)
        {
            Debug.WriteLine($"Process Reload with pid {runInfo.pid} and port {runInfo.port}");
            PidsLabel.Content = runInfo.pid;
            PortsLabel.Content = runInfo.port;
            ModUiControl.ToggleBtn.Style = (Style)Application.Current.FindResource("DangerButton");
            ModUiControl.ToggleBtn.Text = "Stop";

            ModUiControl.ToggleBtn.IsLoading = false;
            ModUiControl.ToggleBtn.IsEnabled = true;
            ModUiControl.AdminBtn.IsEnabled = true;

            if (_ModUiTray != null)
            {
                _ModUiTray.AdminBtn.IsEnabled = true;
                _ModUiTray.ActionBtn.IsEnabled = true;
                _ModUiTray.ActionBtn.IsLoading = false;
                _ModUiTray.ActionBtn.Text = "Stop";
                _ModUiTray.ActionBtn.Style = (Style)Application.Current.FindResource("DangerButton");
            }

        }

        public void OnErrorPerfomed()
        {

            PidsLabel.Content = null;
            PortsLabel.Content = null;
            ModUiControl.ToggleBtn.Text = "Start";
            ModUiControl.ToggleBtn.Style = Application.Current.FindResource(typeof(Button)) as Style;

            ModUiControl.ToggleBtn.IsLoading = false;
            ModUiControl.ToggleBtn.IsEnabled = true;
            ModUiControl.AdminBtn.IsEnabled = false;

            if (_ModUiTray != null)
            {
                _ModUiTray.AdminBtn.IsEnabled = false;
                _ModUiTray.ActionBtn.IsEnabled = true;
                _ModUiTray.ActionBtn.IsLoading = false;
                _ModUiTray.ActionBtn.Text = "Start";
                _ModUiTray.ActionBtn.Style = Application.Current.FindResource(typeof(Button)) as Style;
            }
        }
    }


    public class ModUiControl : Grid
    {
        public LoadingButton ToggleBtn { get; }

        public Button AdminBtn { get; }

        public Button ConfigBth { get; }

        public Button LogsBtn { get; }

        private List<UIElement> uIElements;

        public ModUiControl()
        {

            ToggleBtn = new LoadingButton() { Text = "Start", MinHeight= 25 }; //{ Content = "Start" };
            ConfigBth = new Button() { Content = "Config", MinHeight = 25 };
            AdminBtn = new Button() { Content = "Admin", MinHeight = 25 };
            LogsBtn = new Button() { Content = "Log", MinHeight = 25 };

            uIElements = [ToggleBtn, AdminBtn, ConfigBth];


            RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < uIElements.Count; i++)
            {

                var el = uIElements[i] as FrameworkElement;
                el.Margin = new(2.5, 0, 2.5, 0);

                ColumnDefinitions.Add(new());
                Children.Add(el);

                SetColumn(el, i);
                SetRow(el, 0);
            }
        }
    }

    public class LoadingButton : Button
    {
        private Grid contentGrid;
        private TextBlock textBlock;
        private Ellipse loadingIndicator;


        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(LoadingButton), new PropertyMetadata(false, OnIsLoadingChanged));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(LoadingButton), new PropertyMetadata("", OnTextChanged));


        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingButton loadingBtn)
            {

                Debug.WriteLine("Text Changed : " + e.NewValue.ToString());

                loadingBtn.textBlock.Text = e.NewValue.ToString();
            }
        }

        private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingButton loadingButton)
            {

                if (loadingButton.IsLoading)
                {
                    loadingButton.StartLoadingAnimation();
                }
                else
                {
                    loadingButton.StopLoadingAnimation();
                }
            }
        }

        public LoadingButton()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {

            this.Style = Application.Current.FindResource(typeof(Button)) as Style;
            // Create a grid to hold the original text and loading indicator
            contentGrid = new Grid();
            contentGrid.HorizontalAlignment = HorizontalAlignment.Center;
            contentGrid.VerticalAlignment = VerticalAlignment.Center;
            contentGrid.ColumnDefinitions.Add(new());
            contentGrid.ColumnDefinitions.Add(new());

            // Create the original TextBlock
            textBlock = new();
            textBlock.Text = this.Text;
            contentGrid.Children.Add(textBlock);

            // Create the circular loading indicator (hidden by default)
            loadingIndicator = new Ellipse();
            loadingIndicator.Width = 9;
            loadingIndicator.Height = 9;

            // Create a LinearGradientBrush for the stroke
            LinearGradientBrush gradientBrush = new LinearGradientBrush();
            gradientBrush.StartPoint = new Point(0, 0);
            gradientBrush.EndPoint = new Point(1, 1);

            // Add gradient stops
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Black, 1.0));


            loadingIndicator.Stroke = gradientBrush;
            loadingIndicator.StrokeThickness = 2;
            loadingIndicator.Visibility = Visibility.Hidden;
            contentGrid.Children.Add(loadingIndicator);

            Grid.SetColumn(textBlock, 0);
            Grid.SetColumnSpan(textBlock, 2);

            this.Content = contentGrid;
        }

        private void StartLoadingAnimation()
        {
            Grid.SetColumn(textBlock, 1);
            Grid.SetColumnSpan(textBlock, 1);

            // Update the text to indicate loading
            textBlock.Text = "Loading...";
            textBlock.FontSize = 7;
            // Show the loading indicator
            loadingIndicator.Visibility = Visibility.Visible;

            // Set the center of the loadingIndicator as the rotation center
            loadingIndicator.RenderTransformOrigin = new Point(0.5, 0.5);

            // Start a rotation animation on the loading indicator
            RotateTransform rotateTransform = new RotateTransform();
            loadingIndicator.RenderTransform = rotateTransform;

            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            };

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }


        private void StopLoadingAnimation()
        {
            // Hide the loading indicator
            loadingIndicator.Visibility = Visibility.Hidden;


            // Update the text to revert to the original content
            textBlock.Text = this.Text;
            textBlock.FontSize = this.FontSize;

            Grid.SetColumn(textBlock, 0);
            Grid.SetColumnSpan(textBlock, 2);
        }
    }



    public class ModUiTray
    {

        public Label NameLabel { get; }
        public LoadingButton ActionBtn { get; }
        public Button AdminBtn { get; }

        public ModUiTray(Module module, Grid container)
        {

            NameLabel = ModUi.CreateLabel(module.GetName(), HorizontalAlignment.Left);
            ActionBtn = new() { Text = "Start" };
            AdminBtn = new() { Content = "Admin" };

            UIElement[] elms = [NameLabel, ActionBtn, AdminBtn];

            for (int i = 0; i < elms.Length; i++)
            {

                container.Children.Add(elms[i]);
                Grid.SetColumn(elms[i], i);
                Grid.SetRow(elms[i], container.RowDefinitions.Count);

            }
            container.RowDefinitions.Add(new());

            AdminBtn.IsEnabled = false;
            AdminBtn.Click += (s, e) => module.AdminNavigate();
            ActionBtn.Click += (s, e) => module.Toggle();

        }
    }
}
