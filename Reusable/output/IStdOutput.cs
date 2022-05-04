namespace reusable.UI.output
{
    public interface IStdOutput
    {
        /**
         * print out, print err, and print log
         * They are wide-character because C# string and char types are 
         * Unicode by default, unlike in C++ where a char is implimentation-defined
         * or system-defined(hooray for fixed-width types!).
         * This interface is more appropriate to use for terminal applications.
         * Look for IDisplayOutput for GUI programs.
         */

        void wcout(in string message); //std::out
        void wcerr(in string message); //std::err
        void wclog(in string message); //std::log
        void status(in string message); //status indicator, if any
        void progress(in uint percent); //progress indicator, if any
        void flush(); //allows the user to manually flush output.
    }
}
