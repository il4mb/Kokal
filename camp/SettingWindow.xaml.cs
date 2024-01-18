using kokal.lib;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace kokal
{

    public delegate void LanguageChangedEvent();

    public partial class SettingWindow : Window
    {

        public static event LanguageChangedEvent LanguageChanged;

        string set_editor = "";
        string set_browser = "";
        string set_lang = "";

        public SettingWindow()
        {
            InitializeComponent();


            List<AppLang> languanges = [
                new()
                {
                    Name = "ID - Indonesia",
                    Icon = (BitmapImage)App.Current.FindResource("ic_flag_id"),
                    Code = "id-ID"
                },
                new()
                {
                    Name = "EN - English",
                    Icon = (BitmapImage)App.Current.FindResource("ic_flag_gb"),
                    Code = "en-GB"
                }
             ];

            CmbLanguage.ItemsSource = languanges;
            CmbLanguage.SelectedIndex = 0;
            CmbLanguage.SelectionChanged += (s, e) =>
            {
                set_lang = languanges[CmbLanguage.SelectedIndex].Code;
                
            };

            LoadLastSetting();
            InitialViewComponent();
        }


        private void SaveSetting()
        {

            Setting.Set(SettingHelper.EDITOR, set_editor);
            Setting.Set(SettingHelper.BROWSER, set_browser);
            Setting.Set(SettingHelper.LANG, set_lang);

            App.SetLanguage(set_lang);

        }

        private void LoadLastSetting()
        {
            set_editor = Setting.Get(SettingHelper.EDITOR) ?? "";
            set_browser = Setting.Get(SettingHelper.BROWSER) ?? "";
            set_lang = Setting.Get(SettingHelper.LANG) ?? "id-ID";

            if (string.IsNullOrEmpty(set_editor))
            {
                set_editor = "notepad.exe";
                Setting.Set(SettingHelper.EDITOR, "notepad.exe");
            }

            RenderViewValue();
        }



        private void InitialViewComponent()
        {
            this.Button_Reset.Click += (s, e) =>
            {

                Setting.Set(SettingHelper.EDITOR, "notepad");
                Setting.Set(SettingHelper.BROWSER, "");

                LoadLastSetting();
            };
            this.Button_Save.Click += (s, e) =>
            {
                SaveSetting();
            };

            this.Button_TextEditor.Click += (s, e) => Select_Executeable(p =>
            {
                set_editor = p;
                RenderViewValue();
            });
            this.Button_Browser.Click += (s, e) => Select_Executeable(p =>
            {
                set_browser = p;
                RenderViewValue();
            });

        }


        private void RenderViewValue()
        {

            int maxLength = 35;

            string textEditor = TruncateText(set_editor, maxLength);
            string browser = TruncateText(set_browser, maxLength);

            this.TextBox_TextEditor.Text = textEditor;
            this.TextBox_Browser.Text = browser;

            switch (set_lang)
            {
                case "id-ID":
                    this.CmbLanguage.SelectedIndex = 0; break;
                case "en-GB":
                    this.CmbLanguage.SelectedIndex = 1; break;
            }
        }

        private void Select_Executeable(EventPickFile eventPick)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Executable File",
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string executablePath = openFileDialog.FileName;

                // Check if the selected file is an executable
                if (executablePath.EndsWith(".exe", System.StringComparison.OrdinalIgnoreCase))
                {
                    // Start the process
                    eventPick.Invoke(executablePath);
                }
                else
                {
                    MessageBox.Show("Please select a valid executable file.");
                }
            }
        }


        static string TruncateText(string text, int maxWidth)
        {
            if (text.Length > maxWidth)
            {
                // Subtract 3 to make room for the ellipsis ("...")
                int startIndex = Math.Max(0, text.Length - maxWidth + 4);
                return "... " + text.Substring(startIndex);
            }
            else
            {
                return text;
            }
        }

    }


    public class AppLang
    {
        public required string Name { get; set; }
        public required BitmapImage Icon { get; set; }
        public required string Code { get; set; }
    }


    public delegate void EventPickFile(string path);
}
