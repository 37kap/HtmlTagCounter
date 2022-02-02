using ProgressDialog.Interfaces;
using ProgressDialog.Core;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using ProgressDialog.Commands;

namespace ProgressDialog.ViewModels
{
    public class ProgressDialogViewModel : INotifyPropertyChanged
    {
        #region Commands
        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(OnCancel, CanExecuteCancel));
        #endregion

        #region Fields
        private IProgressDialogView _progressDialogView;
        private ICommand _cancelCommand;
        private string _label;
        private string _sublabel;
        private bool _showCancelButton;
        private double _progressValue;
        BackgroundWorker _worker;
        #endregion

        #region Properties
        public ProgressDialogContext Current { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string Label
        {
            get { return _label; }
            set
            {
                if (_label != value)
                {
                    _label = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string SubLabel
        {
            get { return _sublabel; }
            set
            {
                if (_sublabel != value)
                {
                    _sublabel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ShowCancelButton
        {
            get { return _showCancelButton; }
            set
            {
                if (_showCancelButton != value)
                {
                    _showCancelButton = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                if (_progressValue != value)
                {
                    _progressValue = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        public ProgressDialogViewModel(IProgressDialogView progressDialogView) 
        {
            _progressDialogView = progressDialogView;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public ProgressDialogResult Execute(Func<ProgressDialogContext, CancellationTokenSource, object> operationWithResult)
        {
            if (operationWithResult == null)
                throw new ArgumentNullException(ProgressDialog.Properties.Resources.Operation);

            ProgressDialogResult result = null;
            CancellationTokenSource cts = new CancellationTokenSource();

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            _worker.DoWork +=
                (s, e) => {

                    try
                    {
                        Current = new ProgressDialogContext(s as BackgroundWorker, e as DoWorkEventArgs);
                        Current.CancelledByUser += () => cts.Cancel();

                        e.Result = operationWithResult(Current, cts);

                        Current.CheckCancellationPending();
                    }
                    catch (ProgressDialogCancellationException)
                    { }
                    catch (Exception ex)
                    {
                        if (!Current.CheckCancellationPending())
                            throw ex;
                    }
                    finally
                    {
                        Current = null;
                    }

                };

            _worker.RunWorkerCompleted +=
                (s, e) => {

                    result = new ProgressDialogResult(e);

                    _progressDialogView.Dispatcher.BeginInvoke(DispatcherPriority.Send, (SendOrPostCallback)delegate {
                        _progressDialogView.Close();
                    }, null);

                };

            _worker.ProgressChanged +=
                (s, e) => {

                    if (!_worker.CancellationPending)
                    {
                        if (e.UserState != null)
                            SubLabel = (e.UserState as string) ?? string.Empty;
                        ProgressValue = e.ProgressPercentage;
                    }

                };

            _worker.RunWorkerAsync();

            _progressDialogView.ShowDialog();

            return result;
        }


        private void OnCancel(object parameter)
        {
            if (_worker != null && _worker.WorkerSupportsCancellation)
            {
                SubLabel = ProgressDialog.Properties.Resources.Finishing;
                _worker.CancelAsync();
                Current.OnUserCancelled();
            }
        }

        private bool CanExecuteCancel(object parameter)
        {
            return !_worker.CancellationPending;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
