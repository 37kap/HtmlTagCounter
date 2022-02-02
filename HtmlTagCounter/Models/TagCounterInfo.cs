using HtmlTagCounter.Core;
using System;

namespace HtmlTagCounter.Models
{
    /// <summary>
    /// Describes tag count information on an html page.
    /// </summary>
    public class TagCounterInfo : BindableBase
    {
        private string _url;
        private int _count;
        private string _tag;
        private string _comment;
        private bool _urlIsValid;

        /// <summary>
        /// Url to html page
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                SetProperty(ref _url, value);
                ValidateUrl();
            }
        }

        /// <summary>
        /// Determines if the url is valid
        /// </summary>
        public bool UrlIsValid
        {
            get => _urlIsValid;
            set => SetProperty(ref _urlIsValid, value);
        }

        /// <summary>
        /// Tag to be used for analysis
        /// </summary>
        public string Tag
        {
            get => _tag;
            set => SetProperty(ref _tag, value);
        }

        /// <summary>
        /// Count of tags found
        /// </summary>
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        /// <summary>
        /// Additional Information
        /// </summary>
        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        private void ValidateUrl()
        {
            UrlIsValid = Uri.TryCreate(Url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
