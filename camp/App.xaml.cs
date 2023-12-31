using camp.lib;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Windows;

namespace camp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static void OpenEdiText(string filename)
        {
            try
            {
                string? editor = Setting.Get(SettingHelper.EDITOR);

                if (!string.IsNullOrEmpty(editor))
                {
                    editor = editor.Trim();
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = editor,
                        Arguments = filename,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Open the default web browser with the specified URL
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "notepad",
                        Arguments = filename,
                        UseShellExecute = true,
                    });
                }
            } catch (Exception ex){
                Log.WriteLine($"Error: {ex.Message}", Code.Danger);
            }
        }

        public static void OpenBrowser(string url) {

            try
            {
                string? browser = Setting.Get(SettingHelper.BROWSER);

                if (!string.IsNullOrEmpty(browser))
                {
                    browser = browser.Trim();
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = browser,
                        Arguments = url,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Open the default web browser with the specified URL
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = url,
                        UseShellExecute = true,
                    });
                }
            } catch (Exception e)
            {
                Log.WriteLine($"Error: {e.Message}", Code.Danger);
            }
        }


        public static void OpenExplorer(string path)
        {
            try
            {
                if(!Directory.Exists(path))
                {
                    throw new Exception($"Cant open explorer, path not exist {path}");
                }
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = path,
                    UseShellExecute = true
                });

            } catch (Exception e)
            {
                Log.WriteLine($"Error: {e.Message}", Code.Danger);

            }
        }
    }




}
