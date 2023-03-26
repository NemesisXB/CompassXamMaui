using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CompassXam
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool isBusy = false;
        private bool IsTaskBusy = false;

        public bool IsBusy
        {
            get => isBusy;
            set => _ = SetProperty(ref isBusy, value);
        }

        protected async Task RunIsBusyTaskAsync(Func<Task> awaitableTask)
        {
            if (IsTaskBusy)
            {
                // prevent accidental double-tap calls
                return;
            }
            IsTaskBusy = true;
            try
            {
                await awaitableTask();
            }
            finally
            {
                IsTaskBusy = false;
            }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler changed = PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
