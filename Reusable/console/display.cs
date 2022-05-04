using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using reusable.console.staticpos;
using reusable.utility;

namespace reusable.console
{
    public static class display
    {
        /// <summary>
        /// A lot of times, strings are just too long to display within the required width.
        /// What this function does is shorten the string making it look nice, but also keeping the
        /// ends for the sake of context.  This can be useful for paths that are too
        /// long to display.
        /// </summary>
        /// <param name="s">The string to shorten.</param>
        /// <param name="new_size">The size that you want the new string to be.</param>
        /// <returns>A string whose length is equal to <c>new_size</c>.</returns>
        public static string display_string(in string s, in uint new_size)
        {
            if(s == null) return "";
            if (new_size < s.Length)
            {
                string fhalf = "";
                for (uint x = 0; x < (new_size / 2); ++x)
                {
                    fhalf += s[(int)x];
                }
                if (new_size > 6) fhalf += "...";
                for (int x = (int)(s.Length - ((new_size / 2) - 3)); x < s.Length; ++x)
                {
                    fhalf += s[x];
                }
                return fhalf;
            }
            return s;
        }

        public static string centered_string(in string s)
        {
            int center = (int)(((double)Console.BufferWidth / 2) - ((double)s.Length / 2));
            if(center < 0) center = 0;
            return s.PadLeft(center + s.Length);
        }


    }
    
    /// <summary>
    /// Represents a fixed-position progress bar on the console output.
    /// </summary>
    public class ProgressBar
    {
        private byte width, percent;
        private FixedPositionString bar, title;

        /// <summary>
        /// A full-width progress bar that 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="y_axis"></param>
        public ProgressBar(in int width, in int y_axis)
        {
            this.width = (byte)width;
            percent = 0;
            bar = new FixedPositionString(new pair<int, int> { first = 0, second = (y_axis + 1) }, width);
            title = new FixedPositionString(new pair<int, int> { first = 0, second = y_axis }, width);
        }

        /// <summary>
        /// Prints the title.
        /// </summary>
        /// <param name="title">The title to use.</param>
        public void Title(in string title)
        {
            this.title.print(title);
        }

        /// <summary>
        /// Updates the percentage and prints the progress bar.
        /// </summary>
        /// <param name="percent">The new percent complete to represent.</param>
        public void Update(in byte percent)
        {
            this.percent = percent;
            if(this.percent > 100) this.percent = 100;
            DrawBar();
        }

        private void DrawBar()
        {
            byte prog = (byte)(((double)(width - 2) / (double)100) * (double)percent);
            StringBuilder b = new StringBuilder();
            b.Append("[");
            b.Append('#', prog);
            b.Append(' ', ((width - 2) - prog));
            b.Append("]");

            bar.print(b.ToString());
        }


    }


}
