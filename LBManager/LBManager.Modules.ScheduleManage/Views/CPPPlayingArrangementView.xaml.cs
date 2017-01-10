using LBManager.Modules.ScheduleManage.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LBManager.Modules.ScheduleManage.Views
{
    /// <summary>
    /// PlayingArrangementView.xaml 的交互逻辑
    /// </summary>
    public partial class CPPPlayingArrangementView : UserControl
    {
        public CPPPlayingArrangementView()
        {
            InitializeComponent();
        }

        private void ListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
           var viewModel =  this.DataContext as ScheduledStageViewModel;
            if (e.Key == Key.Up && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
               int selectedMediaIndex = viewModel.MediaList.IndexOf(viewModel.CurrentMedia);
                if(selectedMediaIndex == 0)
                {
                    return;
                }
                var selectedMedia = viewModel.CurrentMedia;
                viewModel.MediaList.Remove(selectedMedia);
                viewModel.MediaList.Insert(selectedMediaIndex - 1, selectedMedia);
                viewModel.CurrentMedia = selectedMedia;
                e.Handled = true;
            }
            else if (e.Key == Key.Down && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                int selectedMediaIndex = viewModel.MediaList.IndexOf(viewModel.CurrentMedia);
                if (selectedMediaIndex == viewModel.MediaList.Count - 1)
                {
                    return;
                }
                var selectedMedia = viewModel.CurrentMedia;
                viewModel.MediaList.Remove(selectedMedia);
                viewModel.MediaList.Insert(selectedMediaIndex + 1, selectedMedia);
                viewModel.CurrentMedia = selectedMedia;
                e.Handled = true;
            }
           
        }
    }
}
