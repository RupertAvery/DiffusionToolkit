using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Diffusion.Toolkit.Behaviors
{

    public static partial class DTBehaviors
    {
        public static readonly DependencyProperty IsDragSortableProperty =
         DependencyProperty.RegisterAttached(
             "IsDragSortable",
             typeof(bool),
             typeof(DTBehaviors),
             new PropertyMetadata(false, OnIsDragSortableChanged));

        public static bool GetIsDragSortable(DependencyObject obj) => (bool)obj.GetValue(IsDragSortableProperty);
        public static void SetIsDragSortable(DependencyObject obj, bool value) => obj.SetValue(IsDragSortableProperty, value);

        private static void OnIsDragSortableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl itemsControl)
            {
                if ((bool)e.NewValue)
                {
                    itemsControl.AllowDrop = true;
                    itemsControl.PreviewMouseMove += OnPreviewMouseMove;
                    itemsControl.DragOver += OnDragOver;
                    itemsControl.Drop += OnDrop;
                }
                else
                {
                    itemsControl.AllowDrop = false;
                    itemsControl.PreviewMouseMove -= OnPreviewMouseMove;
                    itemsControl.DragOver -= OnDragOver;
                    itemsControl.Drop -= OnDrop;
                }
            }
        }

        private static object? _draggedItem;
        private static AdornerLayer? _adornerLayer;
        private static DraggedItemAdorner? _draggedAdorner;
        private static InsertionLineAdorner? _insertionAdorner;

        private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ItemsControl itemsControl)
            {
                _draggedItem = GetItemUnderMouse(itemsControl, e.GetPosition(itemsControl));
                if (_draggedItem != null)
                {
                    DragDrop.DoDragDrop(itemsControl, _draggedItem, DragDropEffects.Move);
                }
            }
        }

        private static void OnDragOver(object sender, DragEventArgs e)
        {
            if (sender is not ItemsControl itemsControl || _draggedItem == null)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            int index = GetItemIndexFromPoint(itemsControl, e.GetPosition(itemsControl));
            e.Effects = index >= 0 ? DragDropEffects.Move : DragDropEffects.None;

            // Get or create the adorner layer
            _adornerLayer ??= AdornerLayer.GetAdornerLayer(itemsControl);

            // Check if the dragged item adorner exists before adding
            if (_draggedAdorner == null)
            {
                _draggedAdorner = new DraggedItemAdorner(itemsControl, _draggedItem);
                _adornerLayer.Add(_draggedAdorner);
            }
            _draggedAdorner.SetPosition(e.GetPosition(itemsControl));

            // Check if insertion adorner exists before adding
            if (_insertionAdorner != null)
                _adornerLayer.Remove(_insertionAdorner); // Ensure only one insertion adorner

            _insertionAdorner = new InsertionLineAdorner(itemsControl, index);
            _adornerLayer.Add(_insertionAdorner);
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            if (sender is not ItemsControl itemsControl || _draggedItem == null) return;

            int index = GetItemIndexFromPoint(itemsControl, e.GetPosition(itemsControl));
            if (index < 0) return;

            if (itemsControl.ItemsSource is IList collection && collection.Contains(_draggedItem))
            {
                collection.Remove(_draggedItem);
                collection.Insert(index, _draggedItem);
            }

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
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                var item = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                if (item != null)
                {
                    Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
                    if (bounds.Contains(item.TranslatePoint(point, itemsControl)))
                        return i;
                }
            }
            return -1;
        }

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
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var listBox = (ItemsControl)AdornedElement;

            if (_index > -1)
            {
                var item = listBox.ItemContainerGenerator.ContainerFromIndex(_index) as FrameworkElement;

                if (item != null)
                {
                    var position = item.TranslatePoint(new Point(0, -1), listBox);
                    drawingContext.DrawLine(new Pen(Brushes.Red, 2), new Point(position.X, position.Y), new Point(listBox.ActualWidth, position.Y));
                }
            }


        }
    }
}
