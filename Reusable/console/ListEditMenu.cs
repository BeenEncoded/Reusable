using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using reusable.data;
using reusable.utility;

namespace reusable.console
{
    /// <summary>
    /// A List editing menu for console programs.
    /// Inherit from this class and impliment just a couple functions and you're off rolling.
    /// </summary>
    /// <typeparam name="T">Type of the list.</typeparam>
    public abstract class ListEditMenu<T> : BaseMenu
    {
        #region member variables
        protected List<T> list;
        protected string title; // the title of the list
        protected int selected = -1; // the selected element in the list [1, infinity)

        private bool refresh = true;
        private ConsoleKey?[] keybuffer;
        private ArrayExtensions.to_string_func<ConsoleKey?> keytostr;
        #endregion

        public ListEditMenu(in MenuEngine.MenuContext context) : base(context)
        {
            list = new List<T>();
            this.title = "NO TITLE";
            keybuffer = new ConsoleKey?[2];
            for (int x = 0; x < keybuffer.Length; ++x) keybuffer[x] = null;
            keytostr = (ConsoleKey? key) => {
                if (key == null) return "";
                string rep = key.ToString();
                rep = rep.Replace("D", "");
                return rep;
            };
        }

        public ListEditMenu(
            in MenuEngine.MenuContext context, 
               List<T>                l, 
            in string                 title) : base(context)
        {
            list = l;
            this.title = title;
            keybuffer = new ConsoleKey?[2];
            for(int x = 0; x < keybuffer.Length; ++x) keybuffer[x] = null;
            keytostr = (ConsoleKey? key) => {
                if (key == null) return "";
                string rep = key.ToString();
                rep = rep.Replace("D", "");
                return rep;
            };
        }

        ~ListEditMenu()
        {
            list = null;
            title = null;
            for(int x = 0; x < keybuffer.Length; ++x) keybuffer[x] = null;
            title = string.Empty;
        }

        #region IConsoleMenu
        public override void DisplayMenu()
        {
            if(refresh)
            {
                Console.Clear();
                Console.CursorLeft = ((Console.BufferWidth / 2) - (title.Length / 2));
                Console.WriteLine(title + Environment.NewLine);

                for(int x = 0; x < list.Count; ++x)
                {
                    if(selected == x) Console.Write("** ");
                    else Console.Write("   ");
                    Console.Write($"{(x + 1).ToString("00")}: {itemString(list[x])}");
                    if (selected == x) Console.Write(" **");
                    else Console.Write("   ");
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine("A - add new item         D - Delete selected item");
                Console.WriteLine("E - Edit Item            C - Back");
                Console.WriteLine("N -  New Item");
                refresh = false;
            }
        }

        public override bool ExecuteInput()
        {
            if(!Console.KeyAvailable) return false;
            keybuffer.Append(Console.ReadKey().Key);

            bool isnumber = true;
            for(int x = 0; x < keybuffer.Length; ++x)
            {
                if(keybuffer[x] == null) break;
                isnumber &= keyIsNumber(keybuffer[x]);
            }

            if(!isnumber)
            {
                processCommand(in keybuffer[0]);
                keybuffer.ShiftLeft(2);
                input.flush();
                RefreshDisplay();
            }
            else if(keybuffer.IsFull())
            {
                if(int.TryParse(keybuffer.StringRep(keytostr), out int number))
                {
                    Debug.WriteLine($"Parsed number: {number}");
                    if(number < (list.Count + 1) && number > 0)
                    {
                        selected = number - 1;
                        OnSelectionMade(selected);
                    }
                }
                keybuffer.ShiftLeft(2);
                input.flush();
                RefreshDisplay();
            }
            return false;
        }

        public override void RefreshDisplay()
        {
            refresh = true;
        }
        
        /// <summary>
        /// Gets if an item is selected.
        /// </summary>
        /// <returns>True if selected is within the range of list</returns>
        protected bool ItemSelected()
        {
            return (selected > -1 && selected < list.Count);
        }
        
        
        #endregion

        #region menu backend
        private bool keyIsNumber(in ConsoleKey? key)
        {
            return (key >= ConsoleKey.D0) && (key <= ConsoleKey.D9);
        }

        private void processCommand(in ConsoleKey? key)
        {
            switch(key)
            {
                case ConsoleKey.C:
                    Debug.WriteLine("Cancel pressed.");
                    cancelMenu();
                    break;
                case ConsoleKey.A:
                    Debug.WriteLine("Add button pressed");
                    newItem();
                    break;
                case ConsoleKey.D:
                    Debug.WriteLine("Delete key pressed");
                    deleteItem();
                    break;
                case ConsoleKey.E:
                    Debug.WriteLine("Edit button pressed");
                    editItem();
                    break;
                case ConsoleKey.N:
                    Debug.WriteLine("New Button pressed");
                    newItem();
                    break;
                case ConsoleKey.Escape:
                    Debug.WriteLine("Escape pressed.");
                    for(int x = 0; x < keybuffer.Length; ++x) keybuffer[x] = null;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region abstract functions to impliment
        /// <summary>
        /// returns a string representation of the item in the menu.
        /// </summary>
        /// <param name="item">The item to create a string representation for.</param>
        /// <returns>a string representing the item as displayed within the menu.</returns>
        public abstract string itemString(in T item);

        //commands:

        /// <summary>
        /// called when the user cancels out of the menu.
        /// </summary>
        public abstract void cancelMenu();

        /// <summary>
        /// Delete an item from the list
        /// </summary>
        public abstract void deleteItem();

        /// <summary>
        /// Edit an item in the list.
        /// </summary>
        public abstract void editItem();
        
        /// <summary>
        /// Add an item to the list.
        /// </summary>
        public abstract void newItem();
        #endregion

        #region optional functions to impliment
        /// <summary>
        /// Called when a selection is made by the user.  Can be used
        /// to add additional actions when an item is selected in the list.
        /// </summary>
        /// <param name="choice">The choice made.  [0, </param>
        protected virtual void OnSelectionMade(in int choice)
        {} // does nothing by default.  Override this in an inheriting class for functionality.

        #endregion
    }

    public abstract class StringListEditMenu : ListEditMenu<string>
    {
        public StringListEditMenu(in MenuEngine.MenuContext context) : base(context)
        {
        }

        public override void newItem()
        {
            bool inputcomplete = false;
            string? userinput = null;
            do
            {
                inputcomplete = input.get_string(out userinput, $"Enter a {itemTypeName()}:  ");
                if(inputcomplete) inputcomplete &= validate(userinput ?? string.Empty);
                else break; // input timeout
            }while(!inputcomplete);
            if(userinput != null)
            {
                list.Add(userinput);
            }
            RefreshDisplay();
        }

        public override void deleteItem()
        {
            if (selected >= 0 && (selected < list.Count))
            {
                bool? answer = input.yesno_question($"Are you sure you want to delete \"{list[selected]}\"?");
                if(answer == true)
                {
                    list.RemoveAt(selected);
                    if(selected >= list.Count) selected = 0;
                }
                RefreshDisplay();
            }
        }

        public override void editItem()
        {
            if(selected > -1)
            {
                string? uinput = null;
                bool success = false;
                do
                {
                    success = input.get_string(out uinput, 
                        $"Currently Selected: ({selected+1}) \"{list[selected]}\"" +
                        $"{Environment.NewLine}{Environment.NewLine}Enter a new {itemTypeName()}:  ");
                    if(success) success &= validate(uinput ?? string.Empty);
                    else break;
                } while(!success);
                if(success)
                {
                    list[selected] = uinput;
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Unable to edit: nothing is selected!");
                input.wait_for_keypress();
            }
            RefreshDisplay();
        }

        /// <summary>
        /// Impliment this to add validation.  This will be called when edits or additions
        /// are made.
        /// </summary>
        /// <param name="value">The string to validate.</param>
        /// <returns>true if the string is valid, false otherwise.</returns>
        public abstract bool validate(string value);

        /// <summary>
        /// Used in entry prompts to display the type of value the user
        /// is expected to enter.
        /// </summary>
        /// <returns>The name of the type of values the list contains.  This should also
        /// imply what kind of validation is performed: ie. are the strings directories, addresses, etc?</returns>
        public abstract string itemTypeName();
    }
}
