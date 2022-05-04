using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using reusable.utility;

namespace reusable.console.staticpos
{
    public class FixedPositionObject
    {
        public static Mutex displaylock = new Mutex();

        protected pair<int, int> coords;

        protected FixedPositionObject(in pair<int, int> coords)
        {
            this.coords = coords;
        }

        ~FixedPositionObject()
        {
        }

        /// <summary>
        /// Gets the current console's cursor position: {X, Y}
        /// </summary>
        /// <returns>reusable.utility.pair{int, int} representing (x, y) 
        /// coordinates on a cartesian plane.</returns>
        protected pair<int, int> current_pos() => new pair<int, int> { 
            first = Console.CursorLeft, 
            second = Console.CursorTop };

        /// <summary>
        /// Sets the cursor position to (X, Y) represented by the pair.
        /// </summary>
        /// <param name="c">The coordinates to set the cursor at.</param>
        protected void SetLocation(in pair<int, int> c)
        {
            Console.SetCursorPosition(c.first, c.second);
        }


    }

    public class FixedPositionString : FixedPositionObject
    {
        protected readonly int maxlength;

        /// <summary>
        /// Create a fixed-position string.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="maxlength"></param>
        public FixedPositionString(in pair<int, int> coords, int maxlength = 0) : base(coords)
        {
            this.maxlength = maxlength;
        }

        /// <summary>
        /// Prints the string to the console.  Optionally you can set
        /// a length limit to the output so that it never prints any more than
        /// maxlength number of characters.  If you set the maxlength, it will only
        /// ever print the first maxlength characters of the string.
        /// </summary>
        /// <param name="text">The text to print.</param>
        public void print(in string text)
        {
            displaylock.WaitOne();
            pair<int, int> current_pos = this.current_pos();

            SetLocation(in coords);
            if (text.Length > maxlength)
            {
                Console.Write(text.Substring(0, maxlength));
            }
            else
            {
                Console.Write(text);
            }
            SetLocation(in current_pos);

            displaylock.ReleaseMutex();
        }


    }

    public class FixedPositionDisplay : FixedPositionString
    {
        private string display;
        private ushort lines;

        public FixedPositionDisplay(in pair<int, int> coords, in ushort lines) : 
            base(coords, lines * Console.BufferWidth)
        {
            this.lines = lines;
            display = string.Empty;
        }

        /// <summary>
        /// Writes a line to this object's buffer behaving exactly as it would if
        /// you used Console.WriteLine.
        /// </summary>
        /// <param name="s">The String to write.</param>
        public void WriteLine(in string s)
        {
            display += s;

            //pad the end with spaces so everything ends up aligned
            display += new string(' ', Console.BufferWidth - (s.Length % Console.BufferWidth));

            //trim the end if necessary
            if (display.Length > lines * Console.BufferWidth)
            {
                display = display.Substring((display.Length - (lines * Console.BufferWidth)),
                    lines * Console.BufferWidth);
            }

            //print the result
            print(display);
        }


    }


}
