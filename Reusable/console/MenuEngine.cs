using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;

namespace reusable.console
{
    /// <summary>
    /// This relatively simple object facilitates communication between the internals of
    /// an implimented menu and the caller.  This allows changing of one menu to another without
    /// having to impliment that within the menu implimentation.
    /// 
    /// This object is thread-safe.
    /// </summary>
    public class MenuEngine
    {
        private BaseMenu _currentmenu;
        private Mutex mutexSetMenu = new Mutex(), mutexShow = new Mutex();

        public BaseMenu currentMenu { get => _currentmenu; }

        public MenuEngine()
        {
            _currentmenu = null;
        }

        public void show()
        {
            if(mutexShow.WaitOne(1000))
            {
                if(_currentmenu == null) throw new ArgumentNullException("The current menu is null!");
                do
                {
                    _currentmenu?.DisplayMenu();
                } while (_currentmenu?.ExecuteInput() == false); //if it's null or true the loop exits.
                mutexShow.ReleaseMutex();
            }
        }

        /// <summary>
        /// Sets the current menu to the specified menu in the type arguments.
        /// </summary>
        /// <typeparam name="T">A menu Type T that inherits from BaseMenu</typeparam>
        public void SetMenu<T>() where T : BaseMenu
        {
            if(mutexSetMenu.WaitOne(1000))
            {
                object o = Activator.CreateInstance(typeof(T), new MenuContext(this));

                if(o is BaseMenu mymenu)
                {
                    _currentmenu = mymenu;
                }
                mutexSetMenu.ReleaseMutex();
            }
        }

        /// <summary>
        /// This context is passed to a menu to provide an api to it.
        /// </summary>
        public class MenuContext
        {
            private MenuEngine _engine;

            public MenuContext(in MenuEngine engine)
            {
                _engine = engine;
            }

            public void SetMenu<T>() where T : BaseMenu
            {
                _engine.SetMenu<T>();
            }


        }


    }
}
