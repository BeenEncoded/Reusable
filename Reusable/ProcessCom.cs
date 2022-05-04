using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using reusable.UI.output;

namespace reusable.async
{
    /// <summary>
    /// Allows the user to pass a ThreadContext to an algorithm allowing
    /// for output to non-standard mediums and basic state communication.
    /// The idea is that you write your algorithm to use ThreadContext to
    /// know if you need to pause, or stop, or unpause.
    /// </summary>
    public class ThreadCommand
    {
        private ProcessCom com;

        public ThreadCommand(in IStdOutput o)
        {
            com = new ProcessCom(in o);
        }

        /// <summary>
        /// Gets a thread contaxt for a thread to use.
        /// This is how the thread will recieve commands from this 
        /// object.
        /// </summary>
        /// <returns>ThreadContext: The context the thread will use.</returns>
        public ThreadContext getContext()
        {
            return new ThreadContext(in com);
        }

        /// <summary>
        /// Tells the thread to stop.
        /// </summary>
        public void stop()
        {
            if(com.state != ProcessCom.state_type.idle)
            {
                com.pls_stop = true;
            }
        }

        /// <summary>
        /// Tells the thread to unpause.
        /// </summary>
        public void unpause()
        {
            com.is_paused = false;
        }

        /// <summary>
        /// Tells the thread to pause.
        /// </summary>
        public void pause()
        {
            com.is_paused = true;
        }

        /// <summary>
        /// Reads the state of the thread.
        /// </summary>
        /// <returns>True if the thread in not in an idle state. (it's running)</returns>
        public bool isRunning()
        {
            return ProcessCom.state_type.idle != com.state;
        }

        public class ThreadContext
        {
            private ProcessCom com;

            /// <summary>
            /// True if the thread should stop.
            /// Set to false after you stop!
            /// </summary>
            public bool pls_stop
            {
                get => com.pls_stop;
                set => com.pls_stop = value;
            }

            /// <summary>
            /// True if the thread should pause.
            /// Set to false after you pause!
            /// </summary>
            public bool pls_pause
            {
                get => com.is_paused;
            }

            /// <summary>
            /// True if the thread should unpause.
            /// Set to false after you unpause!
            /// </summary>
            public bool pls_unpause
            {
                get => !com.is_paused;
            }

            public ProcessCom.state_type state { get => com.state; set => com.state = value; }

            public ThreadContext(in ProcessCom c)
            {
                com = c;
            }

            public IStdOutput GetOutput()
            {
                return com.output;
            }

            public void PausePoint()
            {
                while(pls_pause) Thread.Sleep(1000 / 30);
            }

            /// <summary>
            /// Return from the calling thread if this is true.
            /// </summary>
            /// <returns>true if the calling thread asked this one to stop.</returns>
            public bool StopSignaled()
            {
                if(pls_stop)
                {
                    state = ProcessCom.state_type.stopping;
                    pls_stop = false;
                    return true;
                }
                return false;
            }
        }
    }

    /// <summary>
    /// Provides synchronized access to the basic inter-thread communication.
    /// </summary>
    public class ProcessCom
    {
        public enum state_type
        {
            idle = 0,
            starting,
            running,
            stopping
        }

        private IStdOutput _output;
        private state_type _state = state_type.idle;
        private bool 
            _is_paused = false,
            _pls_stop = false;

        // mutexes for each accessibly member variable.  These help ensure that
        // accessing 1 of them doesn't place a lock on all of them.
        private Mutex 
            lock1 = new Mutex(), 
            lock2 = new Mutex(),
            lock3 = new Mutex();

        public bool pls_stop
        {
            set
            {
                lock1.WaitOne();
                _pls_stop = value;
                lock1.ReleaseMutex();
            }
            get
            {
                lock1.WaitOne();
                bool tempb = _pls_stop;
                lock1.ReleaseMutex();
                return tempb;
            }
        }

        public bool is_paused
        {
            set
            {
                lock2.WaitOne();
                _is_paused = value;
                lock2.ReleaseMutex();
            }
            get
            {
                lock2.WaitOne();
                bool tempb = _is_paused;
                lock2.ReleaseMutex();
                return tempb;
            }
        }

        public state_type state
        {
            get
            {
                lock3.WaitOne();
                state_type temps = _state;
                lock3.ReleaseMutex();
                return temps;
            }
            set
            {
                lock3.WaitOne();
                _state = value;
                lock3.ReleaseMutex();
            }
        }

        public IStdOutput output
        {
            get
            {
                if(_output == null)
                {
                    throw new NotImplementedException("ProcessCom._output: " + 
                        "Nothing was passed for output to the com object, but " + 
                        "the function referencing this tried to use it for output.");
                }
                return _output;
            }
        }

        public ProcessCom(in IStdOutput o)
        {
            _output = o;
        }


    }

    

}
