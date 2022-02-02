using HtmlTagCounter.Core.Interfaces;
using HtmlTagCounter.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HtmlTagCounter.Core;
using ProgressDialog.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using ProgressDialog.Commands;
using ProgressDialog.Views;
using System.Text.RegularExpressions;

namespace HtmlTagCounter.ViewModels
{
    /// <summary>
    /// View model for analysing a list of urls from a file.
    /// </summary>
    public class TagCounterViewModel : BindableBase
    {
        #region Commands
        /// <summary>
        /// Analysis start command.
        /// </summary>
        public ICommand StartAnalysisCommand => _startAnalysisCommand ?? (_startAnalysisCommand = new RelayCommand(OnStartAnalize, CanExecuteStartAnalize));

        /// <summary>
        /// UrlSource (File) selection command.
        /// </summary>
        public ICommand SelectUrlSourceCommand => _selectUrlSourceCommand ?? (_selectUrlSourceCommand = new RelayCommand(OnSelectFile));
        #endregion

        #region Fields
        public event Action AnalysisFinished;
        private ICommand _startAnalysisCommand;
        private ICommand _selectUrlSourceCommand;
        private string _searchedTag = "<a>";
        private string _filePath;
        private string _state;
        private int _maxTagCount;
        private int _requestTimeout = 10;
        #endregion

        #region Properties

        /// <summary>
        /// Path to the file.
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        /// <summary>
        /// Maximum number of tags found.
        /// </summary>
        public int MaxTagCount
        {
            get => _maxTagCount;
            set => SetProperty(ref _maxTagCount, value);
        }

        /// <summary>
        /// Last or current operation state.
        /// </summary>
        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        /// <summary>
        /// Url source.
        /// </summary>
        public IUrlSource UrlSource { get; private set; }

        /// <summary>
        /// The tag to be used when analysing pages.
        /// </summary>
        public string SearchedTag
        {
            get => _searchedTag;
            set => SetProperty(ref _searchedTag, value);
        }
        /// <summary>
        /// Request timeout in seconds.
        /// </summary>
        public int RequestTimeout
        {
            get => _requestTimeout;
            set
            {
                if (value > 0 && value < 300)
                {
                    SetProperty(ref _requestTimeout, value);
                }
            }
        }

        /// <summary>
        /// Observable collection of tag count information.
        /// </summary>
        public ObservableCollection<TagCounterInfo> TagInfos { get; } = new ObservableCollection<TagCounterInfo>();

        /// <summary>
        /// Instance of analyzer.
        /// </summary>
        public ITagAnalyzer<TagCounterInfo> Analyzer { get; set; }

        /// <summary>
        /// Instance of page content reader.
        /// </summary>
        public IHtmlPageReader PageContentReader { get; set; }
        #endregion

        /// <summary>
        /// View model for analysing a list of urls from a file.
        /// </summary>
        public TagCounterViewModel(IHtmlPageReader pageContentReader, ITagAnalyzer<TagCounterInfo> analyzer)
        {
            PageContentReader = pageContentReader;
            Analyzer = analyzer;
        }

        private async void OnSelectFile(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SetOperationStatus($"{Properties.Resources.FileReadingStarted}: {openFileDialog.FileName}");
                FilePath = openFileDialog.FileName;
                UrlSource = new UrlFileSource(FilePath);

                try
                {
                    var urlsFromFile = await UrlSource.GetUrlsAsync();
                    PrepareForAnalysis(urlsFromFile);
                    SetOperationStatus(Properties.Resources.FileReadingCompleted);
                }
                catch (Exception ex)
                {
                    SetOperationStatus(ex.InnerException?.Message ?? ex.Message);
                }
            }
        }

        private void PrepareForAnalysis(IList<string> urls)
        {
            TagInfos.Clear();

            foreach (string url in urls)
            {
                TagInfos.Add(new TagCounterInfo() { Url = url });
            };
        }

        private bool GetUrlAvailability(TagCounterInfo tagInfo, CancellationToken urlCheckCancellationToken)
        {
            tagInfo.ValidateUrl();

            if (!tagInfo.UrlIsValid)
            {
                tagInfo.Comment = Properties.Resources.WrongUrl;
                return false;
            }
            else
            {
                try
                {
                    var (urlIsAvailable, checkUrlAvailabilityResult) = PageContentReader.CheckUrlAvailabilityAsync(tagInfo.Url, urlCheckCancellationToken).Result;
                    if (!urlIsAvailable)
                    {
                        tagInfo.UrlIsValid = false;
                        tagInfo.Comment = checkUrlAvailabilityResult;
                    }
                    return urlIsAvailable;
                }
                catch (Exception ex)
                {
                    tagInfo.Comment = urlCheckCancellationToken.IsCancellationRequested ?
                        Properties.Resources.TimeoutExpired :
                        ex.InnerException?.Message ?? ex.Message;
                }
                tagInfo.UrlIsValid = false;
                return false;
            }
        }

        private void OnStartAnalize(object parameter)
        {
            SetOperationStatus(Properties.Resources.AnalysisStarted);

            var result = ProgressDialogWindow.Execute(Properties.Resources.HtmlPagesAnalysis,
                new Func<ProgressDialogContext, CancellationTokenSource, object>((context, cts) =>
                {
                    context.Report(0, Properties.Resources.AnalysisStarted);

                    CancellationTokenSource AnalysisSCts = new CancellationTokenSource();
                    var timeOutForAnalysis = RequestTimeout * 1000;
                    AnalysisSCts.CancelAfter(timeOutForAnalysis);
                    context.CancelledByUser += () => AnalysisSCts.Cancel();

                    int index = 1;
                    int pagesCount = 0;

                    TagInfos
                    .AsParallel()
                    .WithCancellation(cts.Token)
                    .Where(x => GetUrlAvailability(x, AnalysisSCts.Token))
                    .Select(x => (tagInfo: x, content: PageContentReader.ReadContentAsync(x.Url, AnalysisSCts.Token).Result))
                    .Where(x => x.content != null)
                    .ForAll(x =>
                    {
                        if (pagesCount == 0)
                        {
                            pagesCount = TagInfos.Count(ti => ti.UrlIsValid);
                        }

                        cts.Token.ThrowIfCancellationRequested();
                        var tagInfo = x.tagInfo;
                        tagInfo.Tag = SearchedTag;

                        try
                        {
                            Analyzer.Analysis(x.content, tagInfo);

                            if (tagInfo.Count > MaxTagCount)
                            {
                                MaxTagCount = tagInfo.Count;
                            }
                        }
                        catch (Exception ex)
                        {
                            tagInfo.Comment = ex.InnerException?.Message ?? ex.Message;
                        }

                        var progress = index * 100 / pagesCount;
                        var message = string.Format(Properties.Resources.PageAnalysisXofN, index++, pagesCount);
                        context.Report(progress, message);

                        cts.Token.ThrowIfCancellationRequested();
                    });

                    return true;
                }));

            if (result.OperationFailed)
            {
                SetOperationStatus(result.Error.InnerException?.Message ?? result.Error.Message);
            }
            if (result.Cancelled)
            {
                foreach (TagCounterInfo tagInfo in TagInfos)
                {
                    tagInfo.ClearAnalysisInfo();
                }
                SetOperationStatus(Properties.Resources.CanceledByUser);
            }
            else if ((bool)result.Result)
            {
                SetOperationStatus(Properties.Resources.AnalysisCompleted);
                AnalysisFinished?.Invoke();
            }
        }

        private bool CanExecuteStartAnalize(object parameter) => TagInfos.Count() > 0;

        private void SetOperationStatus(string state)
        {
            State = state;
        }
    }
}

