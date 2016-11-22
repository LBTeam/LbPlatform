
using Common.Logging;
using LBManager.Infrastructure.Common.Event;
using LBManager.Infrastructure.Common.Utility;
using LBManager.Infrastructure.Models;
using LBManager.Job;
using LBManager.Modules.ScheduleManage.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LBManager
{
    public class ShellViewModel : BindableBase
    {


        //private ScheduleListViewModel ScheduleList;
        private System.Threading.Timer _heartbeatTimer;
        
        private ILoggerFacade _logger;
        public ShellViewModel()/**/
        {
            _logger = ServiceLocator.Current.GetInstance<ILoggerFacade>();

            
            //ScheduleDetailViewModel = new ProgramScheduleDetailViewModel();
            ScheduleList = new ScheduleSummaryListViewModel();

            ScreenList = new ScreenListViewModel(new ScreenService(), ScheduleList);

            LoginCommand = new DelegateCommand(() => { OpenLoginDialog(); });
            // NewScheduleCommand = new DelegateCommand(() => { NewSchedule(); });

            Messager.Default.EventAggregator.GetEvent<OnLoginEvent>().Subscribe(state =>
            {
                this.LoginStatus = state.Status;
                if (LoginStatus)
                {
                    LoginAccount = state.Account;
                    HeartbeatState = HeartbeatStatus.STOP;
                    _heartbeatTimer = new System.Threading.Timer(StartHeartbeat, null, 100, 10000);
                }
                else
                {
                    LoginAccount = string.Empty;
                }
            });

            Messager.Default.EventAggregator.GetEvent<OnHeartbeatEvent>().Subscribe(arg => { HeartbeatEventHandle(arg.Status); });
        }

        private async void StartHeartbeat(object state)
        {
            _logger.Log("开始心跳", Category.Debug, Priority.Medium);
            if (HeartbeatState == HeartbeatStatus.OK || HeartbeatState == HeartbeatStatus.STOP)
            {

                var response = await Heartbeat();

                if (response.Code == "000000")
                {
                    Messager.Default.EventAggregator.GetEvent<OnHeartbeatEvent>().Publish(new OnHeartbeatEventArg(HeartbeatStatus.OK));
                }
                else if (response.Code == "010002")
                {
                    Messager.Default.EventAggregator.GetEvent<OnHeartbeatEvent>().Publish(new OnHeartbeatEventArg(HeartbeatStatus.TokenInvalid));
                }
                else if (response.Code == "010003")
                {
                    Messager.Default.EventAggregator.GetEvent<OnHeartbeatEvent>().Publish(new OnHeartbeatEventArg(HeartbeatStatus.TokenExpired));
                }
            }
        }

        private async Task<HeartbeatResponse> Heartbeat()
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(string.Format("http://lbcloud.ddt123.cn/?s=api/Manager/heartbeat&token={0}", App.SessionToken));

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<HeartbeatResponse>(content));
        }

        private async void HeartbeatEventHandle(HeartbeatStatus status)
        {
            if (status == HeartbeatStatus.OK)
            {

            }
            else if (status == HeartbeatStatus.TokenExpired)
            {
                var response = await RefreshToken();
                if (response.Code == "00000")
                {
                    HeartbeatState = HeartbeatStatus.OK;
                }
                else
                {
                    HeartbeatState = HeartbeatStatus.TokenExpired;
                }
            }
            else if (status == HeartbeatStatus.TokenInvalid)
            {
                HeartbeatState = HeartbeatStatus.TokenInvalid;
                await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                 {
                     var view = new TokenInvalidPromptDialog();

                     //show the dialog
                     var result = await DialogHost.Show(view, "RootDialog", TokenInvalidPromptDialogOpenedEventHandler, TokenInvalidPromptDialogClosingEventHandler);
                 }), null);

            }
        }

        private void TokenInvalidPromptDialogOpenedEventHandler(object sender, DialogOpenedEventArgs eventArgs)
        {
            //throw new NotImplementedException();
        }

        private void TokenInvalidPromptDialogClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == false) return;
            eventArgs.Cancel();

            Messager.Default.EventAggregator.GetEvent<OnLoginEvent>().Publish(new OnLoginEventArgs(false, string.Empty));

            Task.Delay(TimeSpan.FromMilliseconds(50))
                 .ContinueWith((t, _) => eventArgs.Session.Close(false), null,
                     TaskScheduler.FromCurrentSynchronizationContext());
        }


        public async Task<RefreshTokenResponse> RefreshToken()
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(string.Format("http://lbcloud.ddt123.cn/?s=api/Manager/refresh_token&token={0}", App.SessionToken));

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<RefreshTokenResponse>(content));
        }

        private ISchedulerFactory sf = new StdSchedulerFactory();
        private IScheduler sched;
        private IJobDetail heartbeatJob;
        private ISimpleTrigger heartbeatTrigger;
        private void StartHeartbeat()
        {
            JobDataMap dataMap = new JobDataMap();
            dataMap.Put(HeartbeatJob.HeartbeatState, this.HeartbeatState);
            ScheduleManager.StartJob("HeartbeatJob", typeof(HeartbeatJob), dataMap, "0/15 * * * * ?");
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
            string scheduleFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LBManager", "Media", viewModel.ScheduleName + ".playprog");

            eventArgs.Session.UpdateContent(new SampleProgressDialog());

            if (!File.Exists(scheduleFilePath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(scheduleFilePath))
                {
                    ScheduleFile scheduleFile = new ScheduleFile();
                    scheduleFile.Id = Guid.NewGuid().ToString();
                    scheduleFile.Name = string.Format("{0}.{1}", viewModel.ScheduleName, "playprog");
                    scheduleFile.Type = viewModel.Type;
                    scheduleFile.MediaList = new List<MediaFile>();
                    foreach (var item in viewModel.MediaList)
                    {
                        var mediaFile = new MediaFile();
                        mediaFile.FilePath = item.URL;
                        mediaFile.Type = item.Type;
                        mediaFile.MD5 = item.MD5;
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

        private ScheduleSummaryListViewModel _scheduleList;
        public ScheduleSummaryListViewModel ScheduleList
        {
            get { return _scheduleList; }
            set { SetProperty(ref _scheduleList, value); }
        }

        private bool _loginStatus = false;
        public bool LoginStatus
        {
            get { return _loginStatus; }
            set { SetProperty(ref _loginStatus, value); }
        }

        private string _loginAccount = string.Empty;
        public string LoginAccount
        {
            get { return _loginAccount; }
            set { SetProperty(ref _loginAccount, value); }
        }

        private HeartbeatStatus _heartbeatState = HeartbeatStatus.STOP;
        public HeartbeatStatus HeartbeatState
        {
            get { return _heartbeatState; }
            set { SetProperty(ref _heartbeatState, value); }
        }

        public DelegateCommand LoginCommand { get; private set; }

        public DelegateCommand NewScheduleCommand { get; private set; }



        private void ExtendedOpenedEventHandler(object sender, DialogOpenedEventArgs eventargs)
        {
            Console.WriteLine("You could intercept the open and affect the dialog using eventArgs.Session.");
        }

        private async void ExtendedClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter == false) return;
            eventArgs.Cancel();

            DialogHost dialog = sender as DialogHost;
            var view = dialog.DialogContent as LoginDialog;
            var viewModel = view.DataContext as LoginDialogViewModel;

            //...now, lets update the "session" with some new content!
            eventArgs.Session.UpdateContent(new SampleProgressDialog());
            //note, you can also grab the session when the dialog opens via the DialogOpenedEventHandler

            var loginResponse = await viewModel.Login();

            if (loginResponse.Code == "000000")
            {
                App.SessionToken = loginResponse.TokenInfo.Value;
                Messager.Default.EventAggregator.GetEvent<OnLoginEvent>().Publish(new OnLoginEventArgs(true, viewModel.Name));
            }
            else
            {
                Messager.Default.EventAggregator.GetEvent<OnLoginEvent>().Publish(new OnLoginEventArgs(false, string.Empty));
            }

            ////lets run a fake operation for 3 seconds then close this baby.
            Task.Delay(TimeSpan.FromMilliseconds(50))
                .ContinueWith((t, _) => eventArgs.Session.Close(false), null,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
