using kokal.module;
using System.Windows.Controls;

namespace kokal.ui
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
