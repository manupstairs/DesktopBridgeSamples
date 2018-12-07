using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel;
using Windows.UI.Popups;

namespace AutoLaunchApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool isChecked;

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                this.OnPropertyChanged();
                UpdateStartupTaskState();
            }
        }

        private StartupTaskState state;

        public StartupTaskState State
        {
            get { return state; }
            private set
            {
                state = value;
                this.OnPropertyChanged();
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async Task UpdateStartupTaskState()
        {
            var startupTask = await StartupTask.GetAsync("AutoLaunchAppTask");
            State = startupTask.State;
            if (IsChecked)
            {
                switch (State)
                {
                    case StartupTaskState.Disabled:
                        // Task is disabled but can be enabled.
                        State = await startupTask.RequestEnableAsync(); // ensure that you are on a UI thread when you call RequestEnableAsync()
                        break;
                    case StartupTaskState.DisabledByUser:
                        // Task is disabled and user must enable it manually.
                        MessageDialog dialog = new MessageDialog(
                            "You have disabled this app's ability to run " +
                            "as soon as you sign in, but if you change your mind, " +
                            "you can enable this in the Startup tab in Task Manager.",
                            "TestStartup");
                        await dialog.ShowAsync();
                        break;
                    case StartupTaskState.DisabledByPolicy:
                        Debug.WriteLine("Startup disabled by group policy, or not supported on this device");
                        break;
                }
            }
            else
            {
                startupTask.Disable();
                State = startupTask.State;
            }
        }
    }
}
