namespace HtmlTagCounter.Core.Interfaces
{
    public interface ITagAnalyzer<T>
    {
        /// <summary>
        /// Analyzes the content of the page for the presence of the necessary information.
        /// </summary>
        /// <param name="pageContent">Html page content.</param>
        /// <param name="tagInfo">Tag information.</param>
        /// <returns>Complete information about the tag written to the source object.</returns>
        T Analysis(string pageContent, T tagInfo);
    }
}
