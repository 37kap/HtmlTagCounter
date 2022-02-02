using HtmlTagCounter.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HtmlTagCounter.Core
{
    /// <summary>
    /// Specifies the file in which urls are stored
    /// </summary>
    public class UrlFileSource : IUrlSource
    {
        private List<string> cachedLines;

        /// <summary>
        /// Path to the source file
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Specifies the file in which urls are stored
        /// </summary>
        public UrlFileSource(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Gets list of urls from source file
        /// </summary>
        /// <returns>List of urls</returns>
        public async Task<IList<string>> GetUrlsAsync()
        {
            if (!File.Exists(FilePath))
            {
                throw new Exception(string.Format(Properties.Resources.FileDoesNotExist, Path.GetFileName(FilePath)));
            }

            if (cachedLines != null)
            {
                return cachedLines;
            }

            List<string> lines = new List<string>();

            try
            {
                StreamReader sr = new StreamReader(FilePath);
                var line = await sr.ReadLineAsync();

                while (line != null)
                {
                    lines.Add(line);
                    line = await sr.ReadLineAsync();
                }

                sr.Close();
                return lines;
            }
            catch (Exception e)
            {
                lines = null;
                throw e;
            }
            finally
            {
                if (cachedLines == null && lines != null)
                {
                    cachedLines = lines;
                }
            }
        }
    }
}