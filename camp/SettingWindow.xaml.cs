using camp.lib;
using Microsoft.Win32;
using System.Windows;

namespace camp
{




    public partial class SettingWindow : Window
    {


        string set_editor = "";
        string set_browser = "";


        public SettingWindow()
        {

            InitializeComponent();
            LoadLastSetting();
            InitialViewComponent();

        }


        private void SaveSetting()
        {

            Setting.Set(SettingHelper.EDITOR, set_editor);
            Setting.Set(SettingHelper.BROWSER, set_browser);

        }

        private void LoadLastSetting()
        {
            set_editor = Setting.Get(SettingHelper.EDITOR) ?? "";
            set_browser = Setting.Get(SettingHelper.BROWSER) ?? "";

            if (string.IsNullOrEmpty(set_editor))
            {
                set_editor = "notepad.exe";
                Setting.Set(SettingHelper.EDITOR, "notepad.exe");
            }

            RenderViewValue();
        }



        private void InitialViewComponent()
        {
            this.Button_Cancel.Click += (s, e) => {

                string oldTextEdt = Setting.Get(SettingHelper.EDITOR) ?? "";
                string oldBrowser = Setting.Get(SettingHelper.BROWSER) ?? "";

                if(oldTextEdt.Equals(set_editor) && oldBrowser.Equals(set_browser)) {
                    this.Close();
                    return;                }
                LoadLastSetting();
            };
            this.Button_Save.Click += (s, e) => {
                SaveSetting();
            };

            this.Button_TextEditor.Click += (s, e) => Select_Executeable(p => {
                set_editor = p;
                RenderViewValue();
            });
            this.Button_Browser.Click += (s, e) => Select_Executeable(p => {
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


    public delegate void EventPickFile(string path);
}
