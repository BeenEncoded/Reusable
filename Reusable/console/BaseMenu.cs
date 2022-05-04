using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using reusable.utility;

namespace reusable.console
{
    public abstract class BaseMenu
    {
        protected readonly MenuEngine.MenuContext context;

        public BaseMenu(in MenuEngine.MenuContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// This should print the menu to the console.
        /// Optionally you can add a refresh mechanism and impliment
        /// RefreshDisplay so that the menu is not printed constantly for no reason.
        /// </summary>
        public abstract void DisplayMenu();

        /// <summary>
        /// Gets input.  For this to work properly with the MenuEngine it needs to be non-blocking.
        /// </summary>
        /// <returns>True if the user is finished with the program.  False otherwise.</returns>
        public abstract bool ExecuteInput();
        public abstract void RefreshDisplay();


    }

}
