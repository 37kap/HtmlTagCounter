using System.Windows.Threading;

namespace ProgressDialog.Interfaces
{
    public interface IProgressDialogView
    {
        Dispatcher Dispatcher { get; }
        void Close();
        bool? ShowDialog();
    }
}
