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
using Diffusion.Database;
using Diffusion.Toolkit.Common;

namespace Diffusion.Toolkit.Controls
{
    public enum RatingSize
    {
        Standard,
        Compact
    }

    /// <summary>
    /// Interaction logic for StarRating.xaml
    /// </summary>
    public partial class StarRating : UserControl
    {
        public static readonly DependencyProperty RatingSizeProperty =
            DependencyProperty.Register(
                name: nameof(RatingSize),
                propertyType: typeof(RatingSize),
                ownerType: typeof(StarRating),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: RatingSize.Standard,
                    propertyChangedCallback: PropertyChangedCallback)
            );

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
                if (e.NewValue is ImageViewModel model)
                {
                    model.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(ImageViewModel.Rating))
                        {
                            if (!host.internalSet)
                            {
                                host.originalRating = model.Rating;
                            }
                        }
                    };
                    host.originalRating = model.Rating;
                }
            }
        }


        private int? originalRating;

        public ImageViewModel? Image
        {
            get => (ImageViewModel)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public RatingSize RatingSize
        {
            get => (RatingSize)GetValue(RatingSizeProperty);
            set => SetValue(RatingSizeProperty, value);
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

            int? rating;

            rating = int.Parse((string)value);
            
            if (originalRating == rating)
            {
                rating = null;
            }

            Image.Rating = rating;
            originalRating = rating;

            ServiceLocator.TaggingService.Rate(this, Image.Id, rating);

            e.Handled = true;
        }

        private bool internalSet = false;

        private void Ratings_OnMouseMove(object sender, MouseEventArgs e)
        {
            var value = ((FrameworkElement)sender).Tag;
            var rating = int.Parse((string)value);
            internalSet = true;
            Image.Rating = rating;
            internalSet = false;
        }

        private void Favorite_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Image.Favorite = !Image.Favorite;
            ServiceLocator.TaggingService.Favorite(this, Image.Id, Image.Favorite);
            e.Handled = true;
        }

        private void Delete_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Image.ForDeletion = !Image.ForDeletion;
            ServiceLocator.TaggingService.ForDeletion(this, Image.Id, Image.ForDeletion);
            e.Handled = true;
        }

        private void NSFW_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Image.NSFW = !Image.NSFW;
            ServiceLocator.TaggingService.NSFW(this, Image.Id, Image.NSFW);
            e.Handled = true;
        }
    }
}
