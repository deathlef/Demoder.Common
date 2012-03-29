/*
Demoder.Common
Copyright (c) 2010-2012 Demoder <demoder@demoder.me>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.ComponentModel;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace Demoder.Common
{
    /// <summary>
    /// This wraps a backgroundworker, providing a work input queue.
    /// This class should not be used directly, but rather be inherited by another class.
    /// </summary>
    public abstract class BackgroundWorkerInputQueue
    {
        #region members
        public BackgroundWorker BackgroundWorker = new BackgroundWorker();
        private ManualResetEvent bgwMRE = new ManualResetEvent(false);
        private Queue<object> queue = new Queue<object>(16);

        /// <summary>
        /// How many items are in queue?
        /// </summary>
        protected int queueCount { get { return this.queue.Count; } }
        #endregion

        #region Events
        public event RunWorkerCompletedEventHandler WorkComplete;
        /// <summary>
        /// Signaled by the worker thread when the queue is empty
        /// </summary>
        public event EventHandler QueueEmpty;
        #endregion

        #region constructors
        public BackgroundWorkerInputQueue(bool reportProgress, bool supportsCancellation)
        {
            this.BackgroundWorker.WorkerReportsProgress = reportProgress;
            this.BackgroundWorker.WorkerSupportsCancellation = supportsCancellation;
            this.BackgroundWorker.DoWork += new DoWorkEventHandler(this.worker_PullQueue);
            this.BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_WorkCompleted);
        }
        /// <summary>
        /// Start processing the queue.
        /// </summary>
        public void StartWorker()
        {
            if (!this.BackgroundWorker.IsBusy)
                this.BackgroundWorker.RunWorkerAsync();
        }
        #endregion

        #region Queue management
        /// <summary>
        /// Add an item to the worker queue
        /// </summary>
        /// <param name="obj"></param>
        protected void enqueue(object obj)
        {
            lock (this.queue)
                this.queue.Enqueue(obj);
            this.bgwMRE.Set();
            if (!this.BackgroundWorker.IsBusy)
                this.BackgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// This is the method assigned to the BackgroundWorkers DoWork method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_PullQueue(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                if (this.queue.Count == 0)
                {
                    this.bgwMRE.Reset();
                    //Signal that the queue is empty.
                    EventHandler eh = this.QueueEmpty;
                    if (eh != null)
                        lock (eh)
                            eh(this, new EventArgs());

                    this.bgwMRE.WaitOne(); //Wait till we get signaled.
                    continue;
                }
                else
                {
                    Object worktask = null;
                    lock (this.queue)
                        worktask = this.queue.Dequeue();
                    //Submit work task to the DoWork event. WORK TASK IS STORED AS SENDER!
                    this.myWorker(this, e, worktask);
                    return;
                }
            }
        }

        /// <summary>
        /// This will start the worker again when it's complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunWorkerCompletedEventHandler rwceh = this.WorkComplete;
            if (rwceh != null)
                lock (rwceh)
                    rwceh(sender, e);
            //Start the worker again.
            if (!e.Cancelled)
                this.BackgroundWorker.RunWorkerAsync();
        }
        #endregion

        #region Methods that the inheriting class should implement
        /// <summary>
        /// This method is called by the queue manager when there is work to be done.
        /// </summary>
        /// <param name="sender">Object performing this action</param>
        /// <param name="e">Original DoWorkEventArgs provided by the background worker</param>
        /// <param name="QueueItem">Work item provided by the queue manager</param>
        protected abstract void myWorker(object sender, DoWorkEventArgs e, object queueItem);
        #endregion
    }
}