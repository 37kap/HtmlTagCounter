using System;
using System.ComponentModel;

namespace ProgressDialog.Core
{
    /// <summary>
    /// Result of progress dialog
    /// </summary>
    public class ProgressDialogResult
    {
        /// <summary>
        /// The result of the action
        /// </summary>
        public object Result { get; internal set; }

        /// <summary>
        /// Action canceled
        /// </summary>
        public bool Cancelled { get; private set; }

        /// <summary>
        /// Action execution error
        /// </summary>
        public Exception Error { get; internal set; }

        /// <summary>
        /// Sign of an unsuccessful action
        /// </summary>
        public bool OperationFailed
        {
            get { return Error != null; }
        }

        /// <summary>
        /// Result of progress dialog
        /// </summary>
        /// <param name="e">Event arguments</param>
        public ProgressDialogResult(RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                Cancelled = true;
            else if (e.Error != null)
                Error = e.Error;
            else
                Result = e.Result;
        }
    }
}
