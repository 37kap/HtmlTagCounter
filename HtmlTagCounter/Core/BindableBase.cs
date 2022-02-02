using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HtmlTagCounter.Core
{
    /// <summary>
    /// Base class that implements the INotifyPropertyChanged interface.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets property new value and notifies about it.
        /// /// </summary>
        /// <param name="backingStore">Reference to source.</param>
        /// <param name="value">New value.</param>
        /// <param name="onChanged">On changed action.</param>
        /// <param name="propertyname">Changed property name.</param>
        protected void SetProperty<T>(ref T backingStore, in T value, in System.Action onChanged = null, [CallerMemberName] in string propertyname = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;

            backingStore = value;

            onChanged?.Invoke();

            OnPropertyChanged(propertyname);
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
