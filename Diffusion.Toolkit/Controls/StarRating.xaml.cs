using Diffusion.Toolkit.Behaviors;
using Diffusion.Toolkit.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for StarRating.xaml
    /// </summary>
    public partial class StarRating : UserControl
    {

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(
                name: nameof(Image),
                propertyType: typeof(ImageViewModel),
                ownerType: typeof(StarRating),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: default(ImageViewModel),
                    propertyChangedCallback: PropertyChangedCallback)
            );

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StarRating host)
            {
                host.originalRating = ((ImageViewModel)e.NewValue).Rating;
            }
        }


        private int? originalRating;

        public ImageViewModel? Image
        {
            get => (ImageViewModel)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public StarRating()
        {
            InitializeComponent();
        }

        private void Ratings_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Image.Rating = originalRating;
        }

        private void Ratings_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var value = ((FrameworkElement)sender).Tag;

            var rating = int.Parse((string)value);

            Image.Rating = rating;
            originalRating = rating;
            ServiceLocator.TaggingService.Rate(this, Image.Id, rating);
        }

        private void Ratings_OnMouseMove(object sender, MouseEventArgs e)
        {
            var value = ((FrameworkElement)sender).Tag;
            var rating = int.Parse((string)value);
            Image.Rating = rating;
        }
    }
}
