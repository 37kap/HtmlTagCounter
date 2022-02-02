using System.Collections.Generic;
using System.Threading.Tasks;

namespace HtmlTagCounter.Core.Interfaces
{
    /// <summary>
    /// Specifies the source of urls
    /// </summary>
    public interface IUrlSource
    {
        /// <summary>
        /// Gets list of urls from source
        /// </summary>
        /// <returns>List of urls</returns>
        Task<IList<string>> GetUrlsAsync();
    }
}
