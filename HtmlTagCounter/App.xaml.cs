using HtmlTagCounter.Core;
using HtmlTagCounter.Core.Analyzers;
using HtmlTagCounter.Core.Interfaces;
using HtmlTagCounter.Models;
using HtmlTagCounter.ViewModels;
using HtmlTagCounter.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using resources = HtmlTagCounter.Properties.Resources;

namespace HtmlTagCounter
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddTransient<IHtmlPageReader, HtmlPageReader>();
            services.AddTransient<ITagAnalyzer<TagCounterInfo>, AgilityTagAnalyzer>();
            services.AddTransient<TagCounterViewModel>();
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (!(e.Exception is OperationCanceledException))
            {
                MessageBox.Show($"{resources.UnhandledExceptionOccurred}{Environment.NewLine}{e.Exception.Message}", resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
    }
}
