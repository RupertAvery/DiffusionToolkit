using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls
{
    public class TreeViewItem : BaseNotify
    {
        public int Depth
        {
            get;
            set => SetField(ref field, value);
        }

        public bool Visible
        {
            get;
            set => SetField(ref field, value);
        }

        public FolderState State
        {
            get;
            set => SetField(ref field, value);
        }

        public IEnumerable<TreeViewItem>? Children
        {
            get;
            set
            {
                SetField(ref field, value);
                OnPropertyChanged(nameof(HasChildren));
            }
        }

        public bool HasChildren => Children != null && Children.Any();
    }

    /// <summary>
    /// Interaction logic for TreeView.xaml
    /// </summary>
    public partial class TreeView : UserControl
    {
        
        public static readonly DependencyProperty TreeViewItemContentProperty =
            DependencyProperty.Register(
                name: nameof(TreeViewItemContent),
                propertyType: typeof(object),
                ownerType: typeof(TreeView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: PropertyChangedCallback)
            );


        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                name: nameof(ItemsSource),
                propertyType: typeof(IEnumerable<TreeViewItem>),
                ownerType: typeof(TreeView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public IEnumerable<TreeViewItem> ItemsSource
        {
            get => (IEnumerable<TreeViewItem>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object TreeViewItemContent
        {
            get => (object)GetValue(TreeViewItemContentProperty);
            set => SetValue(TreeViewItemContentProperty, value);
        }


        public TreeView()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                //var parent = NavigationScrollViewer;
                //parent.RaiseEvent(eventArg);
            }
        }


        private async void Expander_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            //    //var folder = ((FrameworkElement)sender).DataContext as FolderViewModel;

            //    //if (!folder.IsUnavailable)
            //    //{
            //    //    await ToggleFolder(folder);
            //    //}

            //    e.Handled = true;
            //}
            //catch (Exception exception)
            //{
            //    Logger.Log(exception.Message);
            //}
        }

    }
}
