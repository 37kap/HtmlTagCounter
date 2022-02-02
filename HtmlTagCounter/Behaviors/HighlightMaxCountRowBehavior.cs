using HtmlTagCounter.Models;
using HtmlTagCounter.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace HtmlTagCounter.Behaviors
{
    /// <summary>
    /// DataGrid behavior for highlighting row contains max count tags in column Count.
    /// </summary>
    public class HighlightMaxCountRowBehavior : Behavior<DataGrid>
    {
        /// <summary>
        /// DataGrid ViewModel getting from DataContext.
        /// </summary>
        public TagCounterViewModel ViewModel
        {
            get
            {
                return AssociatedObject.DataContext as TagCounterViewModel;
            }
        }

        /// <summary>
        /// DataGrid behavior for highlighting row contains max count tags in column Count.
        /// </summary>
        public HighlightMaxCountRowBehavior() { }

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
        }

        private void AssociatedObject_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var lastTagCounterVM = e.OldValue as TagCounterViewModel;
                lastTagCounterVM.AnalysisFinished -= TagCounterVM_AnalysisFinished;
            }

            if (ViewModel != null)
            {
                ViewModel.AnalysisFinished += TagCounterVM_AnalysisFinished;
            }
        }

        private void TagCounterVM_AnalysisFinished()
        {
            if (ViewModel != null)
            {
                int maxTagCount = ViewModel.TagInfos.Max(ti => ti.Count);
                foreach (TagCounterInfo item in ViewModel.TagInfos)
                {
                    DataGridRow row = (DataGridRow)AssociatedObject.ItemContainerGenerator.ContainerFromItem(item);
                    if (item.Count == maxTagCount)
                    {
                        row.Background = Brushes.LightGreen;
                        row.ToolTip = Properties.Resources.MaxCountTags;
                    }
                    else if(item.UrlIsValid)
                    {
                        row.Background = Brushes.Transparent;
                        row.ToolTip = null;
                    }
                }
            }
        }
    }
}
