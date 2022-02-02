using System;
using System.ComponentModel;

namespace ProgressDialog.Core
{
    /// <summary>
    /// Progress dialog context.
    /// </summary>
    public class ProgressDialogContext
    {
        /// <summary>
        /// BackgroundWorker.
        /// </summary>
        public BackgroundWorker Worker { get; private set; }

        /// <summary>
        /// DoWork event arguments.
        /// </summary>
        public DoWorkEventArgs Arguments { get; private set; }

        /// <summary>
        /// Progress dialog context.
        /// </summary>
        /// <param name="worker">BackgroundWorker.</param>
        /// <param name="arguments">DoWork event arguments.</param>
        public ProgressDialogContext(BackgroundWorker worker, DoWorkEventArgs arguments)
        {
            if (worker == null)
                throw new ArgumentNullException("worker");
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            Worker = worker;
            Arguments = arguments;
        }

        /// <summary>
        /// Cancel button click event.
        /// </summary>
        public event Action CancelledByUser;

        /// <summary>
        /// On user cancelled.
        /// </summary>
        internal void OnUserCancelled()
        {
            CancelledByUser?.Invoke();
        }

        /// <summary>
        /// Check cancellation pending.
        /// </summary>
        public bool CheckCancellationPending()
        {
            if (Worker.WorkerSupportsCancellation && Worker.CancellationPending)
                Arguments.Cancel = true;

            return Arguments.Cancel;
        }
        /// <summary>
        /// Throw if cancellation pending.
        /// </summary>
        public void ThrowIfCancellationPending()
        {
            if (CheckCancellationPending())
                throw new ProgressDialogCancellationException();
        }

        private string lastMessage;
        /// <summary>
        /// Last status message.
        /// </summary>
        public string LastMessage
        {
            get { return lastMessage; }
            private set { lastMessage = value; }
        }

        /// <summary>
        /// Notify progress dialog of current process status.
        /// </summary>
        /// <param name="percentProgress">Per cent of progress.</param>
        /// <param name="message">Message for user.</param>
        public void Report(int percentProgress, string message)
        {
            if (Worker.WorkerReportsProgress)
            {
                if (message != null)
                    lastMessage = message;
                Worker.ReportProgress(percentProgress, message);
            }
        }
    }
}
