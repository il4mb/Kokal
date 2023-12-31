using camp.lib;
using System.Diagnostics;
using System.IO;

namespace camp.module
{
    public class Apache : lib.Module
    {

        private string access_log, error_log, httpd_conf, php_ini;
        public Apache()
        {
            access_log = Path.Combine(GetPath(), "logs", "access.log");
            error_log = Path.Combine(GetPath(), "logs", "error.log");
            httpd_conf = Path.Combine(GetPath(), "conf", "httpd.conf");
            php_ini = Path.Combine(Directory.GetCurrentDirectory(), "php", "php.ini");
        }

        public override void AdminNavigate()
        {
            App.OpenBrowser("http://localhost");

        }

        public override List<MenuOption>? GetMenuOpts()
        {

            string[] paths = [access_log, error_log, httpd_conf, php_ini];

            List<MenuOption> menuOptions = new List<MenuOption>();

            int code = 111;
            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    menuOptions.Add(new()
                    {
                        Name = Path.GetFileName(path),
                        Code = code
                    });

                }
                code++;
            }

            return menuOptions;
        }

        public override string GetName()
        {
            return "Apache";
        }

        public override string GetVersion()
        {
            using Process proc = new Process();
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = $"/c {GetPath()}/bin/httpd.exe -v";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;

            proc.Start();
            string result = proc.StandardOutput.ReadToEnd(); // Read the output
            proc.WaitForExit();
            return result.Trim();
        }

        public override bool IsComponentOkay()
        {

            ModulePaths ModPaths = GetModulePaths();

            string[] paths = [
                Path.Combine(GetPath(), "bin", "httpd.exe"),
                Path.Combine(GetPath(), "conf", "httpd.conf"),
                Path.Combine(GetPath(), "conf", "extra", "httpd-camp.conf"),
            ];

            foreach (string path in paths)
            {
                if (!File.Exists(path))
                {
                    Log.WriteLine(GetName(), $"Unable to find {path}", Code.Danger);
                    return false;
                }
            }

            if (!File.Exists(ModPaths.Controller))
            {
                Log.WriteLine(GetName(), $"Unable to find controller.exe at path {ModPaths.Controller}", Code.Danger);
                return false;
            }


            if (!File.Exists(ModPaths.Controller))
            {
                Log.WriteLine(GetName(), $"Unable to find controller.exe at path {ModPaths.Controller}", Code.Danger);
                return false;
            }

            if (!File.Exists(ModPaths.Info))
            {
                Log.WriteLine(GetName(), $"Unable to find info.exe at path {ModPaths.Info}", Code.Danger);
                return false;
            }


            if (!File.Exists(ModPaths.Install))
            {
                Log.WriteLine(GetName(), $"Unable to find install.exe at path {ModPaths.Install}", Code.Danger);
                return false;
            }

            return true;
        }

        public override void OnMenuItemClicked(int code, int position)
        {
            switch (code)
            {
                case 111:
                    App.OpenEdiText(Path.Combine(GetPath(), "logs", "access.log"));
                    break;
                case 112:
                    App.OpenEdiText(Path.Combine(GetPath(), "logs", "error.log"));
                    break;
                case 113:
                    App.OpenEdiText(Path.Combine(GetPath(), "conf", "httpd.conf"));
                    break;
                case 114:
                    App.OpenEdiText(Path.Combine(Directory.GetCurrentDirectory(), "php", "php.ini"));
                    break;
            }
        }

    }

}
