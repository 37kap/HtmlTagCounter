using HtmlAgilityPack;
using HtmlTagCounter.Core.Interfaces;
using HtmlTagCounter.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace HtmlTagCounter.Core.Analyzers
{
    /// <summary>
    /// Analyzes tags using Agility
    /// </summary>
    public class AgilityTagAnalyzer : ITagAnalyzer<TagCounterInfo>
    {
        /// <summary>
        /// Analyzes tags using Agility
        /// </summary>
        public AgilityTagAnalyzer() { }

        /// <summary>
        /// Analyzes the content of the page for the presence of tag
        /// </summary>
        /// <param name="pageContent">Html page content</param>
        /// <param name="tagInfo">Tag information</param>
        /// <returns>Complete information about the tag written to the source object</returns>
        public TagCounterInfo Analysis(string pageContent, TagCounterInfo tagInfo)
        {
            string tag = tagInfo.Tag;
            GetCleanTagText(ref tag);

            var doc = new HtmlDocument();
            doc.LoadHtml(pageContent);
            tagInfo.Count = doc.DocumentNode.Descendants(tag).Count();
            return tagInfo;
        }

        private void GetCleanTagText(ref string tag)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9!-]");
            tag = rgx.Replace(tag, "");
        }

    }
}
