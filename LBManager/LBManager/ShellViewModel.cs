using LBManager.Infrastructure.Models;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager
{
    public class ShellViewModel : BindableBase
    {
        private ProgramScheduleListViewModel _scheduleListViewModel;
        public ShellViewModel()
        {
            ScheduleDetailViewModel = new ProgramScheduleDetailViewModel();
            _scheduleListViewModel = new ProgramScheduleListViewModel();

            ScreenList = new ScreenListViewModel(new ScreenService(), _scheduleListViewModel);

            LoginCommand = new DelegateCommand(() => { OpenLoginDialog(); });
            NewScheduleCommand = new DelegateCommand(() => { NewSchedule(); });
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

        private async void NewSchedule()
        {
            var view = new EditScheduleView()
            {
                DataContext = new EditScheduleViewModel()
            };

            var result = await DialogHost.Show(view, "RootDialog", null, EditScheduleViewClosingEventHandler);
        }

        private void EditScheduleViewClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == false) return;
            eventArgs.Cancel();

            

            DialogHost dialog = sender as DialogHost;
            var view = dialog.DialogContent as EditScheduleView;
            var viewModel = view.DataContext as EditScheduleViewModel;
            string scheduleFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LBManager", "Media", viewModel.ScheduleName+".playprog");

            eventArgs.Session.UpdateContent(new SampleProgressDialog());

            if (!File.Exists(scheduleFilePath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(scheduleFilePath))
                {
                    ScheduleFile scheduleFile = new ScheduleFile();
                    scheduleFile.Id = Guid.NewGuid().ToString();
                    scheduleFile.Name = string.Format("{0}.{1}", viewModel.ScheduleName, "playprog");
                    scheduleFile.MediaList = new List<MediaFile>();
                    foreach (var item in viewModel.MediaList)
                    {
                        var mediaFile = new MediaFile();
                        mediaFile.FilePath = item.FilePath;
                        mediaFile.Type = item.FileType;
                        mediaFile.MD5 = item.FileMD5;
                        scheduleFile.MediaList.Add(mediaFile);
                    }
                    sw.WriteLine(JsonConvert.SerializeObject(scheduleFile));
                }
            }

            Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith((t, _) =>
                    eventArgs.Session.Close(false), 
                    null,
                    TaskScheduler.FromCurrentSynchronizationContext());

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

        public DelegateCommand NewScheduleCommand { get; private set; }

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
