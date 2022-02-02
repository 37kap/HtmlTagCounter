using ProgressDialog.Interfaces;
using ProgressDialog.Core;
using ProgressDialog.ViewModels;
using System;
using System.Threading;
using System.Windows;

namespace ProgressDialog.Views
{
    /// <summary>
    /// Логика взаимодействия для ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialogWindow : Window, IProgressDialogView
    {
        public ProgressDialogViewModel ViewModel
        {
            get => DataContext as ProgressDialogViewModel;
            set => DataContext = value;
        }
        private ProgressDialogWindow()
        {
            InitializeComponent();
            ViewModel = new ProgressDialogViewModel(this);
        }

        public static ProgressDialogResult Execute(string label, Func<ProgressDialogContext, CancellationTokenSource, object> operationWithResult)
        {
            ProgressDialogWindow dialog = new ProgressDialogWindow();

            if (!string.IsNullOrEmpty(label))
                dialog.ViewModel.Label = label;

            dialog.ViewModel.ShowCancelButton = true;

            return dialog.ViewModel.Execute(operationWithResult);
        }
    }
}
