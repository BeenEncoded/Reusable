using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using reusable.utility;

namespace reusable.console
{
    /// <summary>
    /// Miscellaneous input functions for use with the console.
    /// Uses stdin/stdout
    /// </summary>
    public static partial class input
    {
        public static void flush()
        {
            while(Console.KeyAvailable) Console.ReadKey(true);
        }

        /// <summary>
        /// Asks the user a yes/no question.  The default is 'no' if the
        /// user does not answer within the timeout period (in seconds).
        /// </summary>
        /// <param name="question">The question to ask.</param>
        /// <returns>True if the user answered yes, false if no, 
        /// NULL if no answer was given.</returns>
        public static bool? yesno_question(in string question, int timeout = 30)
        {
            Console.Clear();
            Console.WriteLine(question);
            Console.WriteLine();
            Console.WriteLine("Y/N");
            if(common.wait_until(() => Console.KeyAvailable, (uint)(timeout * 1000), 20))
            {
                return (Console.ReadKey(true).Key == ConsoleKey.Y);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("No answer given, returning \"no\".");
            }
            return null;
        }

        /// <summary>
        /// Gets a string while displaying a question.
        /// If it returns true, s is garunteed not to be false.
        /// </summary>
        /// <param name="s">The input is stored here.</param>
        /// <param name="question">The prompt for the user.  Displayed directly before the string as such:
        /// $"{question}: "</param>
        /// <returns>True if input was provided, false otherwise.</returns>
        public static bool get_string(out string? s, in string question)
        {
            input.flush();
            Console.Clear();
            Console.WriteLine();
            Console.Write($"{question}");
            s = null;
            if(common.wait_until(() => Console.KeyAvailable, 20000, 20))
            {
                s = Console.ReadLine();
            }
            else
            {
                return false;
            }
            return s != null;
        }

        /// <summary>
        /// Clears the line.  Useful for when you use getline and it prints to the screen when you don't want it to.
        /// </summary>
        public static void clear_line()
        {
            var pos = Console.GetCursorPosition();
            Console.CursorLeft = 0;
            for(int x = 0; x < Console.WindowWidth; ++x)
            {
                Console.Write(" ");
            }
            Console.SetCursorPosition(0, pos.Top);
        }

        public static void wait_for_keypress()
        {
            common.wait_until(() => Console.KeyAvailable, 10000, 20);
            if (Console.KeyAvailable) Console.ReadKey(true);
        }
    }
}
