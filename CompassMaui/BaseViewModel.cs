using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CompassMaui
{
    public class BaseViewModel : ObservableObject
    {
        private bool isBusy = false;
        private bool IsTaskBusy = false;
        private bool isPro = false;

        public bool IsBusy
        {
            get => isBusy;
            set => _ = SetProperty(ref isBusy, value);
        }

        public bool IsPro
        {
            get => isPro;
            set => _ = SetProperty(ref isPro, value);
        }

        private string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
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

        //protected bool SetProperty<T>(ref T backingStore, T value,
        //    [CallerMemberName] string propertyName = "",
        //    Action onChanged = null)
        //{
        //    if (EqualityComparer<T>.Default.Equals(backingStore, value))
        //    {
        //        return false;
        //    }

        //    backingStore = value;
        //    onChanged?.Invoke();
        //    OnPropertyChanged(propertyName);
        //    return true;
        //}

        //#region INotifyPropertyChanged
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        //{
        //    PropertyChangedEventHandler changed = PropertyChanged;
        //    if (changed == null)
        //    {
        //        return;
        //    }

        //    changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        //#endregion
    }
}
