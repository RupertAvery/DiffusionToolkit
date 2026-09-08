using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using WPFLocalizeExtension.Providers;

namespace Diffusion.Toolkit.Behaviors
{
    public class DragSortDropEventArgs : RoutedEventArgs
    {
        public int SourceIndex { get; }
        public int TargetIndex { get; }

        public DragSortDropEventArgs(RoutedEvent routedEvent, int sourceIndex, int targetIndex) : base(routedEvent)
        {
            SourceIndex = sourceIndex;
            TargetIndex = targetIndex;
        }

    }




    public static partial class DTBehaviors
    {
        public static readonly DependencyProperty IsDragSortableProperty =
         DependencyProperty.RegisterAttached(
             "IsDragSortable",
             typeof(bool),
             typeof(DTBehaviors),
             new PropertyMetadata(false, OnIsDragSortableChanged));

        public static readonly DependencyProperty OnDragSortDropProperty =
            DependencyProperty.RegisterAttached(
                "OnDragSortDrop",
                typeof(RoutedEventHandler),
                typeof(DTBehaviors),
                new PropertyMetadata(null, OnDragSortDropChanged));


        public static readonly RoutedEvent DragSortDropEvent =
            EventManager.RegisterRoutedEvent(
                "DragSortDropEvent",
                RoutingStrategy.Bubble,
                typeof(EventHandler<DragSortDropEventArgs>),
                typeof(DTBehaviors));


        private static Point _dragStartPoint;
        private static bool _isDragging;

        public static bool GetIsDragSortable(DependencyObject obj) => (bool)obj.GetValue(IsDragSortableProperty);

        public static void SetIsDragSortable(DependencyObject obj, bool value) => obj.SetValue(IsDragSortableProperty, value);


        public static RoutedEventHandler GetOnDragSortDrop(DependencyObject obj) => (RoutedEventHandler)obj.GetValue(OnDragSortDropProperty);

        public static void SetOnDragSortDrop(DependencyObject obj, RoutedEventHandler value) => obj.SetValue(OnDragSortDropProperty, value);

        private static void OnDragSortDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl itemsControl)
            {
                if (e.NewValue != null)
                {
                    itemsControl.AddHandler(DragSortDropEvent, GetOnDragSortDrop(itemsControl));
                }
                else
                {
                    itemsControl.RemoveHandler(DragSortDropEvent, GetOnDragSortDrop(itemsControl));
                }
            }
        }


        private static void OnIsDragSortableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl itemsControl)
            {
                if ((bool)e.NewValue)
                {
                    itemsControl.AllowDrop = true;
                    itemsControl.PreviewMouseMove += OnPreviewMouseMove;
                    itemsControl.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                    itemsControl.DragOver += OnDragOver;
                    itemsControl.Drop += OnDrop;
                    itemsControl.MouseUp += OnMouseUp;
                }
                else
                {
                    itemsControl.AllowDrop = false;
                    itemsControl.PreviewMouseMove -= OnPreviewMouseMove;
                    itemsControl.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                    itemsControl.DragOver -= OnDragOver;
                    itemsControl.Drop -= OnDrop;
                    itemsControl.MouseUp -= OnMouseUp;
                }
            }
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ItemsControl itemsControl)
            {
                _dragStartPoint = e.GetPosition(itemsControl);
                _draggedItem = GetItemUnderMouse(itemsControl, _dragStartPoint);
            }
        }

        private static void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            ClearAdorners();
        }

        private static object? _draggedItem;
        private static AdornerLayer? _adornerLayer;
        private static DraggedItemAdorner? _draggedAdorner;
        private static InsertionLineAdorner? _insertionAdorner;

        private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging || e.LeftButton != MouseButtonState.Pressed)
                return;

            if (sender is not ItemsControl itemsControl || _draggedItem == null)
                return;

            Point currentPos = e.GetPosition(itemsControl);
            Vector diff = currentPos - _dragStartPoint;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDragging = true;

                DragDrop.DoDragDrop(itemsControl, _draggedItem, DragDropEffects.Move);

                _isDragging = false;
                _draggedItem = null;
            }
        }

        private static void OnDragOver(object sender, DragEventArgs e)
        {
            if (sender is not ItemsControl itemsControl || _draggedItem == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            int index = GetItemIndexFromPoint(itemsControl, e.GetPosition(itemsControl));
            e.Effects = index >= 0 ? DragDropEffects.Move : DragDropEffects.None;

            _adornerLayer ??= AdornerLayer.GetAdornerLayer(itemsControl);

            if (_draggedAdorner == null)
            {
                _draggedAdorner = new DraggedItemAdorner(itemsControl, _draggedItem)
                {
                    IsHitTestVisible = false
                };
                _adornerLayer.Add(_draggedAdorner);
            }

            _draggedAdorner.SetPosition(e.GetPosition(itemsControl));

            if (_insertionAdorner != null)
                _adornerLayer.Remove(_insertionAdorner);

            _insertionAdorner = new InsertionLineAdorner(itemsControl, index)
            {
                IsHitTestVisible = false
            };
            _adornerLayer.Add(_insertionAdorner);

            e.Handled = true;
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            
            if (sender is not ItemsControl itemsControl || _draggedItem == null) return;

            int index = GetItemIndexFromPoint(itemsControl, e.GetPosition(itemsControl));
            if (index < 0) return;

            if (itemsControl.ItemsSource is IList collection && collection.Contains(_draggedItem))
            {
                //collection.Remove(_draggedItem);
                //collection.Insert(index, _draggedItem);
                var sourceIndex = collection.IndexOf(_draggedItem);

                itemsControl.RaiseEvent(new DragSortDropEventArgs(DragSortDropEvent, sourceIndex, index));
            }

            _isDragging = false;
            _draggedItem = null;
            ClearAdorners();
        }

        private static object GetItemUnderMouse(ItemsControl itemsControl, Point position)
        {
            foreach (var item in itemsControl.Items)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null)
                {
                    // Convert the mouse position relative to the container
                    Point relativePosition = itemsControl.TranslatePoint(position, container);
                    if (container.InputHitTest(relativePosition) != null)
                        return item;
                }
            }
            return null;
        }

        private static int GetItemIndexFromPoint(ItemsControl itemsControl, Point point)
        {
            var element = itemsControl.InputHitTest(point) as DependencyObject;
            if (element == null)
                return -1;

            var container = ItemsControl.ContainerFromElement(itemsControl, element);
            return container != null
                ? itemsControl.ItemContainerGenerator.IndexFromContainer(container)
                : -1;
        }

        //private static int GetItemIndexFromPoint(ItemsControl itemsControl, Point point)
        //{
        //    for (int i = 0; i < itemsControl.Items.Count; i++)
        //    {
        //        var item = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
        //        if (item != null)
        //        {
        //            Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
        //            if (bounds.Contains(item.TranslatePoint(point, itemsControl)))
        //                return i;
        //        }
        //    }
        //    return -1;
        //}

        private static void ClearAdorners()
        {
            _adornerLayer?.Remove(_draggedAdorner);
            _adornerLayer?.Remove(_insertionAdorner);
            _draggedAdorner = null;
            _insertionAdorner = null;
            _adornerLayer = null;
        }
    }

    public class DraggedItemAdorner : Adorner
    {
        private readonly VisualBrush _visualBrush;
        private Point _position;

        public DraggedItemAdorner(UIElement adornedElement, object draggedItem)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            _visualBrush = new VisualBrush(new TextBlock
            {
                Text = draggedItem.ToString(),
                Background = Brushes.LightGray,
                Opacity = 0.7,
                Padding = new Thickness(5)
            });
        }

        public void SetPosition(Point position)
        {
            _position = position;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(_visualBrush, null, new Rect(_position, new Size(100, 20)));
        }
    }
    public class InsertionLineAdorner : Adorner
    {
        private readonly int _index;

        public InsertionLineAdorner(UIElement adornedElement, int index)
            : base(adornedElement)
        {
            _index = index;
            IsHitTestVisible = false;
        }

        private Brush LineBrush
        {
            get
            {
                field ??= (Brush)Application.Current.FindResource("SecondaryBrush");
                return field;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var listBox = (ItemsControl)AdornedElement;

            if (_index > -1)
            {
                if (listBox.ItemContainerGenerator.ContainerFromIndex(_index) is FrameworkElement item)
                {
                    var position = item.TranslatePoint(new Point(0, -1), listBox);


                    drawingContext.DrawLine(new Pen(LineBrush, 2), new Point(position.X, position.Y), new Point(listBox.ActualWidth, position.Y));
                }
            }


        }
    }
}
