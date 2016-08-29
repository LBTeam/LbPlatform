using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager
{
    public class ShellViewModel : BindableBase
    {
        public ShellViewModel()
        {
            ScheduleDetailViewModel = new ProgramScheduleDetailViewModel();
            ScreenList = new ScreenListViewModel(new ScreenService());
            LoginCommand = new DelegateCommand(() => { OpenLoginDialog(); });
        }

        private async void OpenLoginDialog()
        {
            var view = new LoginDialog
            {
                DataContext = new LoginDialogViewModel()
            };

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ExtendedOpenedEventHandler, ExtendedClosingEventHandler);
        }

        private ProgramScheduleDetailViewModel _scheduleDetailViewModel;
        public ProgramScheduleDetailViewModel ScheduleDetailViewModel
        {
            get { return _scheduleDetailViewModel; }
            set { SetProperty(ref _scheduleDetailViewModel, value); }
        }

        private ScreenListViewModel _screenList;
        public ScreenListViewModel ScreenList
        {
            get { return _screenList; }
            set { SetProperty(ref _screenList, value); }
        }

        public DelegateCommand LoginCommand { get; private set; }

        private void ExtendedOpenedEventHandler(object sender, DialogOpenedEventArgs eventargs)
        {
            Console.WriteLine("You could intercept the open and affect the dialog using eventArgs.Session.");
        }

        private void ExtendedClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == false) return;

            //OK, lets cancel the close...
            eventArgs.Cancel();

            //...now, lets update the "session" with some new content!
            eventArgs.Session.UpdateContent(new SampleProgressDialog());
            //note, you can also grab the session when the dialog opens via the DialogOpenedEventHandler

            //lets run a fake operation for 3 seconds then close this baby.
            Task.Delay(TimeSpan.FromSeconds(3))
                .ContinueWith((t, _) => eventArgs.Session.Close(false), null,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
