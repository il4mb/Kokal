using camp.lib;
using camp.module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace camp.ui
{
    internal class ModContainer(IModContainer imc)
    {

        public void InitialModule()
        {

            ModUi.NewInstance(new Apache(), imc);
            ModUi.NewInstance(new Mysql(), imc);

        }
    }

    public interface IModContainer
    {
        public Grid GetParentHolder();

        public Grid? GetTrayHolder();

    }
}
