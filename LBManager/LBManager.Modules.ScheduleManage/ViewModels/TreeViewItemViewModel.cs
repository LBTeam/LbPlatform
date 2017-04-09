using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Modules.ScheduleManage.ViewModels
{
    public class TreeViewItemViewModel : BindableBase
    {
        static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();
        private ObservableCollection<TreeViewItemViewModel> children;
        private static ObservableCollection<TreeViewItemViewModel> checkedItem = new ObservableCollection<TreeViewItemViewModel>();
        readonly TreeViewItemViewModel parent;
        bool isExpanded;
        bool isSelected;
        bool? isChecked = false;

        private TreeViewItemViewModel()
        {
        }

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren)
        {
            this.parent = parent;
            this.children = new ObservableCollection<TreeViewItemViewModel>();
        }

        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get { return children; }
            set
            {
                children = value;
                SetProperty(ref children, value);
            }
        }

        //public bool HasDummyChild
        //{
        //    get { return this.Children.Count > 0 && this.Children[0] == DummyChild; }
        //}

        //public bool IsExpanded
        //{
        //    get { return isExpanded; }
        //    set
        //    {
        //        if (value != isExpanded)
        //        {
        //            isExpanded = value;
        //            SetProperty(ref isExpanded, value);
        //        }

        //        if (isExpanded && parent != null)
        //            parent.isExpanded = true;


        //        //if (isExpanded == false && !ProjectAppHelper.IsPublishViewModel)
        //        //{
        //        //    this.Children = new ObservableCollection<TreeViewItemViewModel>();
        //        //    this.Children.Insert(0, DummyChild);
        //        //}

        //        if (this.HasDummyChild && isExpanded == true)
        //        {
        //            this.Children.Remove(DummyChild);
        //            this.LoadChildren();
        //        }
        //    }
        //}

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    SetProperty(ref isSelected,value);
                }
            }
        }

        //public bool? IsChecked
        //{
        //    get { return isChecked; }
        //    set { this.SetIsChecked(value, true, true); }
        //}

        //void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        //{
        //    //if (value == isChecked)
        //    //    return;

        //    isChecked = value;

        //    if (updateChildren && isChecked.HasValue)
        //    {
        //        if (HasDummyChild)
        //        {
        //            this.Children.Remove(DummyChild);
        //            this.LoadChildren();
        //        }
        //        this.Children.ToList().ForEach(c => c.SetIsChecked(isChecked, true, false));
        //    }

        //    if (updateParent && parent != null)
        //        parent.VerifyCheckState();


        //    SetProperty(ref isChecked,value);

        //    //if (value == true || value == null)
        //    //{
        //    //    if (!this.GetType().Equals(typeof(FeelingFileViewModel)) && this.CheckedItem.Contains(this))
        //    //        return;
        //    //    else
        //    //        this.CheckedItem.Add(this);
        //    //}
        //    //else
        //    //    this.CheckedItem.Remove(this);
        //}

        //void VerifyCheckState()
        //{
        //    bool? state = null;
        //    for (int i = 0; i < this.Children.Count; ++i)
        //    {
        //        bool? current = this.Children[i].IsChecked;
        //        if (i == 0)
        //        {
        //            state = current;
        //        }
        //        else if (state != current)
        //        {
        //            state = null;
        //            break;
        //        }
        //    }
        //    this.SetIsChecked(state, false, true);
        //}

        public virtual void LoadChildren()
        {

        }

        public TreeViewItemViewModel Parent
        {
            get { return parent; }
        }
    }
}
