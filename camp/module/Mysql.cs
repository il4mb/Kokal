﻿using camp.lib;
using System.Diagnostics;
using System.IO;

namespace camp.module
{
    internal class Mysql : Module
    {

        private string my_ini, mysql_error;

        public Mysql()
        {
            my_ini = Path.Combine(GetPath(), "my.ini");
            mysql_error = Path.Combine(GetPath(), "data", "mysql-error.log");

        }

        public override void AdminNavigate()
        {
            App.OpenBrowser("http://localhost/phpmyadmin");
        }

        public override List<MenuOption>? GetMenuOpts()
        {
            List<MenuOption> opts = new();
            string[] paths = [my_ini, mysql_error];
            int code = 111;

            foreach (string path in paths)
            {
                if(File.Exists(path))
                {
                    opts.Add(new()
                    {
                        Name = Path.GetFileName(path),
                        Code = code
                    });
                }
                code++;
            }
            
            return opts;
        }

        public override string GetName()
        {
            return "Mysql";
        }

        public override string GetVersion()
        {
            using Process proc = new Process();
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = $"/c {GetPath()}/bin/mysql.exe --version";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;

            proc.Start();
            string result = proc.StandardOutput.ReadToEnd(); // Read the output
            proc.WaitForExit();

            return result.Replace($"{GetPath()}/bin/mysql.exe", "").Trim();
        }

        public override bool IsComponentOkay()
        {
            ModulePaths ModPaths = GetModulePaths();

            string[] paths = [
                Path.Combine(GetPath(), "bin", "mysqld.exe"),
                Path.Combine(GetPath(), "my.ini")
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
            switch(code)
            {
                case 111:
                    App.OpenEdiText(my_ini);
                    break;
                case 112:
                    App.OpenEdiText(mysql_error);
                    break;
            }
        }
    }
}