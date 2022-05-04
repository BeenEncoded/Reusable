using System;
using System.IO;

namespace reusable.UI.output
{
    public class StdOutput : IStdOutput, IDisposable
    {
        private StreamWriter _wclog_o = new StreamWriter("clog.log", true);

        public StdOutput()
        {
        }

        #region STD_Interface
        public virtual void wcerr(in string message)
        {
            Console.Error.WriteLine(message);
        }

        public virtual void wclog(in string message)
        {
            _wclog_o?.WriteLine("[" + DateTime.Now.ToShortDateString() + " " + 
                DateTime.Now.ToShortTimeString() + "]: " + message);
        }

        public virtual void wcout(in string message)
        {
            Console.Out.WriteLine(message);
        }

        public virtual void status(in string message)
        {
            throw new NotImplementedException();
        }

        public virtual void progress(in uint percent)
        {
            throw new NotImplementedException();
        }
        
        public void flush()
        {
            Console.Out.Flush();
            Console.Error.Flush();
            _wclog_o.Flush();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.
                _wclog_o?.Close();
                _wclog_o?.Dispose();
                _wclog_o = null;

                disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~StdOutput()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        
        #endregion
    }
}
