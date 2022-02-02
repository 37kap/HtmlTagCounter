using System;

namespace ProgressDialog.Core
{
    /// <summary>
    /// Progress dialog cancellation exception.
    /// Throwed when the cancel button is clicked.
    /// </summary>
    public class ProgressDialogCancellationException : Exception
    {
        /// <summary>
        /// Progress dialog cancellation exception.
        /// Throwed when the cancel button is clicked.
        /// </summary>
        public ProgressDialogCancellationException()
            : base()
        {
        }

        /// <summary>
        /// Progress dialog cancellation exception.
        /// Throwed when the cancel button is clicked.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public ProgressDialogCancellationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Progress dialog cancellation exception.
        /// Throwed when the cancel button is clicked.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public ProgressDialogCancellationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
