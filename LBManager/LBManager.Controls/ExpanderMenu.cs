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

namespace LBManager.Controls
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:LBManager.Controls"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根 
    /// 元素中: 
    ///
    ///     xmlns:MyNamespace="clr-namespace:LBManager.Controls;assembly=LBManager.Controls"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误: 
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:ExpanderMenu/>
    ///
    /// </summary>
    public class ExpanderMenu : Menu
    {
        static ExpanderMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpanderMenu), new FrameworkPropertyMetadata(typeof(ExpanderMenu)));
        }

        #region Dependency Properties

        public static readonly DependencyProperty ExpandDirectionProperty = DependencyProperty.Register(
            "ExpandDirection",
            typeof(ExpandDirection),
            typeof(ExpanderMenu),
            new PropertyMetadata(ExpandDirection.Right, null));

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded",
            typeof(bool),
            typeof(ExpanderMenu),
            new PropertyMetadata(false));

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the expand direction.
        /// </summary>
        /// <value>
        /// The expand direction.
        /// </value>
        public ExpandDirection ExpandDirection
        {
            get { return (ExpandDirection)this.GetValue(ExpandDirectionProperty); }
            set { this.SetValue(ExpandDirectionProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is expanded; otherwise, <c>false</c>.
        /// </value>
        public bool IsExpanded
        {
            get { return (bool)this.GetValue(IsExpandedProperty); }
            set { this.SetValue(IsExpandedProperty, value); }
        }

        #endregion
    }
}
