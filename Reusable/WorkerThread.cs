using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace reusable.async
{
    /// <summary>
    /// This is a standard worker thread.  Create a class the derives from this one and impliments the
    /// .work() method.  That is all that is needed.  Optionally, the throttle can be
    /// modified in order to speed up or slow down the threads max speed so you
    /// don't waste CPU.  :)
    /// </summary>
    public abstract class WorkerThread : IDisposable
    {
        private bool running = false;
        private Thread thread = null;
        private uint _throttle = 30; //The number of times per second the thread will cycle

        protected uint throttle
        {
            get
            {
                return _throttle;
            }
            set
            {
                _throttle = value;
            }
        }

        protected WorkerThread()
        {
        }

        public void Start()
        {
            if(!running)
            {
                running = true;
                thread = new Thread(new ThreadStart(t));
                thread.Start();
            }
        }

        public void Join(uint time = 0)
        {
            if(running)
            {
                running = false;
                if(time == 0) thread?.Join();
                else thread?.Join((int)time);
                if(!IsAlive) thread = null;
            }
        }

        public bool IsAlive
        {
            get
            {
                if(thread == null) return false;
                return thread.IsAlive;
            }
        }

        private void t()
        {
            do
            {
                if(!work()) Thread.Sleep(1000 / 10); //if inactive, only check 10 times a second
                else Thread.Sleep(1000 / (int)_throttle); //if "active" work at throttle speed
            }while(running);
        }

        /// <summary>
        /// Does work
        /// </summary>
        /// <returns> true if the thread should be considered "active".  If 
        /// false is returned, the worker will throttle down to conserve CPU.</returns>
        protected abstract bool work();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    //dispose managed state (managed objects).
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.
                Join();

                disposedValue = true;
            }
        }

        // override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~WorkerThread()
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
