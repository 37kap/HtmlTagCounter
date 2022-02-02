using HtmlTagCounter.Core.Interfaces;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HtmlTagCounter.Core
{
    /// <summary>
    /// Responsible for interacting with html pages
    /// </summary>
    public class HtmlPageReader : IHtmlPageReader
    {
        /// <summary>
        /// Reads content from html page
        /// </summary>
        /// <param name="url">Page url</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Page content</returns>
        public async Task<string> ReadContentAsync(string url, CancellationToken ct)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    using (HttpResponseMessage response = await client.GetAsync(url, ct))
                    {
                        using (HttpContent content = response.Content)
                        {
                            return await content.ReadAsStringAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Checks if the url is available
        /// </summary>
        /// <param name="url">Page url</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A pair of values: is it available, if not, then the reason</returns>
        public async Task<(bool, string)> CheckUrlAvailabilityAsync(string url, CancellationToken ct)
        {
            WebRequest webRequest = WebRequest.Create(url);
            WebResponse webResponse;

            using (WebResponse response = await webRequest.GetResponseAsync().WithCancellation(ct, webRequest.Abort))
            {
                try
                {
                    webResponse = await webRequest.GetResponseAsync();
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }

                return (true, null);
            }
        }
    }
}
