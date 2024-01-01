using camp.lib;
using camp.module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace camp.ui
{
    public delegate Task EventExit();

    internal class ModContainer(IModContainer imc)
    {

        public void InitialModule()
        {
            lib.Module[] modules = [
                new Apache(),
                new Mysql()
            ];

            foreach(lib.Module mod in modules)
            {
                ModUi.NewInstance(mod, imc);
                imc.Exited += mod.KillAsync;
            }

        }
    }

    public interface IModContainer
    {
        public Grid GetParentHolder();

        public Grid? GetTrayHolder();

        public event EventExit Exited;

    }
}
