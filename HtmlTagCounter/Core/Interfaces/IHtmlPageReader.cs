using System.Threading;
using System.Threading.Tasks;

namespace HtmlTagCounter.Core.Interfaces
{
    /// <summary>
    /// Responsible for interacting with html pages
    /// </summary>
    public interface IHtmlPageReader
    {
        /// <summary>
        /// Reads content from html page
        /// </summary>
        /// <param name="url">Page url</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Page content</returns>
        Task<string> ReadContentAsync(string url, CancellationToken ct);

        /// <summary>
        /// Checks if the url is available
        /// </summary>
        /// <param name="url">Page url</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A pair of values: is it available, if not, then the reason</returns>
        Task<(bool, string)> CheckUrlAvailabilityAsync(string url, CancellationToken ct);
    }
}
