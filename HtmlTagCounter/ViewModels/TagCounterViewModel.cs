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

namespace HtmlTagCounter.ViewModels
{
    /// <summary>
    /// View model for analysing a list of urls from a file
    /// </summary>
    public class TagCounterViewModel : BindableBase
    {
        #region Commands
        /// <summary>
        /// Analysis start command
        /// </summary>
        public ICommand StartAnalysisCommand => _startAnalysisCommand ?? (_startAnalysisCommand = new RelayCommand(OnStartAnalize, CanExecuteStartAnalize));

        /// <summary>
        /// UrlSource (File) selection command
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
        #endregion

        #region Properties

        /// <summary>
        /// Path to the file
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        /// <summary>
        /// Maximum number of tags found
        /// </summary>
        public int MaxTagCount
        {
            get => _maxTagCount;
            set => SetProperty(ref _maxTagCount, value);
        }

        /// <summary>
        /// Last or current operation state
        /// </summary>
        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        /// <summary>
        /// Url source
        /// </summary>
        public IUrlSource UrlSource { get; private set; }

        /// <summary>
        /// The tag to be used when analysing pages
        /// </summary>
        public string SearchedTag
        {
            get => _searchedTag;
            set => SetProperty(ref _searchedTag, value);
        }

        /// <summary>
        /// Observable collection of tag count information
        /// </summary>
        public ObservableCollection<TagCounterInfo> TagInfos { get; } = new ObservableCollection<TagCounterInfo>();

        /// <summary>
        /// Instance of analyzer
        /// </summary>
        public ITagAnalyzer<TagCounterInfo> Analyzer { get; set; }

        /// <summary>
        /// Instance of page content reader
        /// </summary>
        public IHtmlPageReader PageContentReader { get; set; }
        #endregion

        /// <summary>
        /// View model for analysing a list of urls from a file
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

                var urlsFromFile = await UrlSource.GetUrlsAsync();
                PrepareForAnalysis(urlsFromFile);
            }
        }
        
        private void PrepareForAnalysis(IList<string> urls)
        {
            TagInfos.Clear();

            var result = ProgressDialogWindow.Execute(Properties.Resources.CheckUrlsFromFile,
                new Func<ProgressDialogContext, CancellationTokenSource, object>((context, cts) =>
                {
                    int index = 1;
                    int pagesCount = urls.Count;

                    urls
                    .AsParallel()
                    .AsOrdered()
                    .WithCancellation(cts.Token)
                    .Select(x => new TagCounterInfo() { Url = x })
                    .ForAll(tagInfo =>
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        Application.Current.Dispatcher.Invoke(() => TagInfos.Add(tagInfo));

                        context.Report(0, Properties.Resources.CheckUrlsFromFile);


                        if (!tagInfo.UrlIsValid)
                        {
                            tagInfo.Comment = Properties.Resources.WrongUrl;
                        }
                        else
                        {
                            CancellationTokenSource urlCheckCts = new CancellationTokenSource();
                            var timeOutForOneUrlCheck = 10000;
                            urlCheckCts.CancelAfter(timeOutForOneUrlCheck);

                            try
                            {
                                var (urlIsAvailable, checkUrlAvailabilityResult) = PageContentReader.CheckUrlAvailabilityAsync(tagInfo.Url, urlCheckCts.Token).Result;
                                if (!urlIsAvailable)
                                {
                                    tagInfo.UrlIsValid = false;
                                    tagInfo.Comment = checkUrlAvailabilityResult;
                                }
                            }
                            catch
                            {
                                tagInfo.UrlIsValid = false;
                                tagInfo.Comment = Properties.Resources.TimeoutExpired;
                            }

                            var progress = index * 100 / pagesCount;
                            var message = string.Format(Properties.Resources.CheckUrlXofN, index++, pagesCount);
                            context.Report(progress, message);
                        }

                        cts.Token.ThrowIfCancellationRequested();
                    });

                    return true;
                }));

            if (result.OperationFailed)
            {
                SetOperationStatus(result.Error.InnerException?.Message ?? result.Error.Message);
                TagInfos.Clear();
            }
            if (result.Cancelled)
            {
                SetOperationStatus(Properties.Resources.CanceledByUser);
                TagInfos.Clear();
            }
            else if (result?.Result is bool)
            {
                SetOperationStatus(Properties.Resources.FileReadingCompleted);
            }
        }

        private void OnStartAnalize(object parameter)
        {
            SetOperationStatus(Properties.Resources.AnalysisStarted);

            var result = ProgressDialogWindow.Execute(Properties.Resources.HtmlPagesAnalysis,
                new Func<ProgressDialogContext, CancellationTokenSource, object>((context, cts) =>
                {
                    context.Report(0, Properties.Resources.AnalysisStarted);

                    int index = 1;
                    int pagesCount = TagInfos.Count(t => t.UrlIsValid);

                    CancellationTokenSource readPageCts = new CancellationTokenSource();
                    var timeOutForOneUrlCheck = 10000;
                    readPageCts.CancelAfter(timeOutForOneUrlCheck);

                    TagInfos
                    .AsParallel()
                    .WithCancellation(cts.Token)
                    .Where(x => x.UrlIsValid)
                    .Select(x => (tagInfo: x, content: PageContentReader.ReadContentAsync(x.Url, readPageCts.Token).Result))
                    .Where(x => x.content != null)
                    .ForAll(x =>
                    {
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
                SetOperationStatus(Properties.Resources.CanceledByUser);
            }
            else if ((bool)result.Result)
            {
                SetOperationStatus(Properties.Resources.AnalysisCompleted);
                AnalysisFinished?.Invoke();
            }
        }

        private bool CanExecuteStartAnalize(object parameter)
        {
            return TagInfos.Where(t => t.UrlIsValid).Count() > 0;
        }

        private void SetOperationStatus(string state)
        {
            State = state;
        }
    }
}

