using HtmlTagCounter.ViewModels;
using System.Windows;

namespace HtmlTagCounter.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Main window of Html Tag Counter.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).ServiceProvider.GetService(typeof(TagCounterViewModel));
        }
    }
}
