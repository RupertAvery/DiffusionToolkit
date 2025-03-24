using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using Diffusion.Database;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit.Controls
{
    public partial class ThumbnailView
    {
        public static readonly DependencyProperty ImagesProperty =
         DependencyProperty.Register(
             name: nameof(Images),
             propertyType: typeof(ObservableCollection<ImageEntry>),
             ownerType: typeof(ThumbnailView),
             typeMetadata: new FrameworkPropertyMetadata(
                 defaultValue: null,
                 propertyChangedCallback: PropertyChangedCallback)
             );

        public static readonly DependencyProperty SelectedImageEntryProperty =
            DependencyProperty.Register(
                nameof(SelectedImageEntry), 
                typeof(ImageEntry), 
                typeof(ThumbnailView),
                new FrameworkPropertyMetadata(
                    null, 
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
                );

        //public static readonly DependencyProperty DataStoreProperty =
        //    DependencyProperty.Register(
        //        nameof(DataStore), 
        //        typeof(DataStore), 
        //        typeof(ThumbnailView),
        //        new FrameworkPropertyMetadata(
        //            null, 
        //            FrameworkPropertyMetadataOptions.None)
        //        );

        public static readonly DependencyProperty NSFWBlurProperty =
            DependencyProperty.Register(
                name: nameof(NSFWBlur),
                propertyType: typeof(bool),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: false,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty PageProperty =
            DependencyProperty.Register(
                name: nameof(Page),
                propertyType: typeof(int),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: 0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register(
                name: nameof(PageSize),
                propertyType: typeof(int),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: 0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty PagesProperty =
            DependencyProperty.Register(
                name: nameof(Pages),
                propertyType: typeof(int),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: 0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty CurrentImageProperty =
            DependencyProperty.Register(
                name: nameof(CurrentImage),
                propertyType: typeof(ImageViewModel),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty IsEmptyProperty =
            DependencyProperty.Register(
                name: nameof(IsEmpty),
                propertyType: typeof(bool),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: false,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(
                name: nameof(IsBusy),
                propertyType: typeof(bool),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: false,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty ResultsProperty =
            DependencyProperty.Register(
                name: nameof(Results),
                propertyType: typeof(string),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty ResultStatusProperty =
            DependencyProperty.Register(
                name: nameof(ResultStatus),
                propertyType: typeof(string),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty OpenCommandProperty =
            DependencyProperty.Register(
                name: nameof(OpenCommand),
                propertyType: typeof(ICommand),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty AddAlbumCommandProperty =
            DependencyProperty.Register(
                name: nameof(AddAlbumCommand),
                propertyType: typeof(ICommand),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty AddToAlbumCommandProperty =
            DependencyProperty.Register(
                name: nameof(AddToAlbumCommand),
                propertyType: typeof(ICommand),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty RemoveFromAlbumCommandProperty = 
            DependencyProperty.Register(
                nameof(RemoveFromAlbumCommand), 
                typeof(ICommand), 
                typeof(ThumbnailView), 
                new PropertyMetadata(default(object)));

        public static readonly DependencyProperty RenameAlbumCommandProperty =
            DependencyProperty.Register(
                nameof(RenameAlbumCommand),
                typeof(ICommand),
                typeof(ThumbnailView),
                new PropertyMetadata(default(object)));

        public static readonly DependencyProperty RemoveAlbumCommandProperty =
            DependencyProperty.Register(
                nameof(RemoveAlbumCommand),
                typeof(ICommand),
                typeof(ThumbnailView),
                new PropertyMetadata(default(object)));

        public static readonly DependencyProperty ViewModeProperty =
            DependencyProperty.Register(
                name: nameof(ViewMode),
                propertyType: typeof(ViewMode),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: ViewMode.Search,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback));

        public static readonly DependencyProperty CopyCommandProperty =
            DependencyProperty.Register(
                name: nameof(CopyCommand),
                propertyType: typeof(ICommand),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback));

        public static readonly DependencyProperty PageChangedCommandProperty =
            DependencyProperty.Register(
                name: nameof(PageChangedCommand),
                propertyType: typeof(ICommand),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback));

        public static readonly DependencyProperty AlbumsProperty =
            DependencyProperty.Register(
                name: nameof(Albums),
                propertyType: typeof(ObservableCollection<Album>),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback));

        public static readonly DependencyProperty SelectedImagesProperty =
            DependencyProperty.Register(
                name: nameof(SelectedImages),
                propertyType: typeof(ObservableCollection<ImageEntry>),
                ownerType: typeof(ThumbnailView),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    FrameworkPropertyMetadataOptions.None,
                    propertyChangedCallback: PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ThumbnailView thumbnailView)
            {
                switch (e.Property.Name)
                {
                    case nameof(Images):
                        {
                            thumbnailView.Model.Images = (ObservableCollection<ImageEntry>)e.NewValue;

                            if (e.OldValue is INotifyCollectionChanged oldCollection)
                            {
                                oldCollection.CollectionChanged -= VisibleItems_CollectionChanged;
                            }

                            if (e.NewValue is INotifyCollectionChanged newCollection)
                            {
                                newCollection.CollectionChanged += VisibleItems_CollectionChanged;
                            }

                            break;
                        }
                    case nameof(CopyCommand):
                        thumbnailView.Model.CopyFiles = (ICommand)e.NewValue;
                        break;
                    case nameof(ViewMode):
                        thumbnailView.Model.ViewMode = (ViewMode)e.NewValue;
                        break;
                    case nameof(NSFWBlur):
                        thumbnailView.Model.NSFWBlur = (bool)e.NewValue;
                        break;
                    case nameof(Page):
                        thumbnailView.Model.Page = (int)e.NewValue;
                        break;
                    case nameof(Pages):
                        thumbnailView.Model.Pages = (int)e.NewValue;
                        break;
                    case nameof(PageSize):
                        thumbnailView.Model.PageSize = (int)e.NewValue;
                        break;
                    case nameof(IsEmpty):
                        thumbnailView.Model.IsEmpty = (bool)e.NewValue;
                        break;
                    case nameof(IsBusy):
                        thumbnailView.Model.IsBusy = (bool)e.NewValue;
                        break;
                    case nameof(Results):
                        thumbnailView.Model.Results = (string)e.NewValue;
                        break;
                    case nameof(ResultStatus):
                        thumbnailView.Model.ResultStatus = (string)e.NewValue;
                        break;
                    case nameof(CurrentImage):
                        thumbnailView.Model.CurrentImage = (ImageViewModel)e.NewValue;
                        thumbnailView.Model.CurrentImage.SearchModelCommand = new RelayCommand<object>(thumbnailView.SearchModel);
                        thumbnailView.Model.CurrentImage.CopyPathCommand = new RelayCommand<object>(thumbnailView.CopyPath);
                        thumbnailView.Model.CurrentImage.CopyPromptCommand = new RelayCommand<object>(thumbnailView.CopyPrompt);
                        thumbnailView.Model.CurrentImage.CopyNegativePromptCommand = new RelayCommand<object>(thumbnailView.CopyNegative);
                        //_model.CurrentImage.CopySeed = new RelayCommand<object>(CopySeed);
                        //_model.CurrentImage.CopyHash = new RelayCommand<object>(CopyHash);
                        thumbnailView.Model.CurrentImage.CopyOthersCommand = new RelayCommand<object>(thumbnailView.CopyOthers);
                        thumbnailView.Model.CurrentImage.CopyParametersCommand = new RelayCommand<object>(thumbnailView.CopyParameters);
                        thumbnailView.Model.CurrentImage.ShowInExplorerCommand = new RelayCommand<object>(thumbnailView.ShowInExplorer);
                        //_model.CurrentImage.ShowInThumbnails = new RelayCommand<object>(ShowInThumbnails);
                        thumbnailView.Model.CurrentImage.DeleteCommand = new RelayCommand<object>(o => thumbnailView.DeleteSelected());
                        thumbnailView.Model.CurrentImage.FavoriteCommand = new RelayCommand<object>(o => thumbnailView.FavoriteSelected());
                        break;
                    case nameof(Albums):
                        thumbnailView.ReloadAlbums();
                        break;
                }
            }

        }

        public ObservableCollection<ImageEntry> SelectedImages
        {
            get => (ObservableCollection<ImageEntry>)GetValue(SelectedImagesProperty);
            set => SetValue(SelectedImagesProperty, value);
        }

        public ObservableCollection<Album> Albums
        {
            get => (ObservableCollection<Album>)GetValue(AlbumsProperty);
            set => SetValue(AlbumsProperty, value);
        }

        public bool IsEmpty
        {
            get => (bool)GetValue(IsEmptyProperty);
            set => SetValue(IsEmptyProperty, value);
        }


        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public ICommand? AddAlbumCommand
        {
            get => (ICommand?)GetValue(AddAlbumCommandProperty);
            set => SetValue(AddAlbumCommandProperty, value);
        }

        public ICommand? AddToAlbumCommand
        {
            get => (ICommand?)GetValue(AddToAlbumCommandProperty);
            set => SetValue(AddToAlbumCommandProperty, value);
        }

        public ICommand? OpenCommand
        {
            get => (ICommand?)GetValue(OpenCommandProperty);
            set => SetValue(OpenCommandProperty, value);
        }

        public string ResultStatus
        {
            get => (string)GetValue(ResultStatusProperty);
            set => SetValue(ResultStatusProperty, value);
        }

        public string Results
        {
            get => (string)GetValue(ResultsProperty);
            set => SetValue(ResultsProperty, value);
        }

        public ImageViewModel CurrentImage
        {
            get => (ImageViewModel)GetValue(CurrentImageProperty);
            set => SetValue(CurrentImageProperty, value);
        }


        public int Page
        {
            get => (int)GetValue(PageProperty);
            set => SetValue(PageProperty, value);
        }

        public int PageSize
        {
            get => (int)GetValue(PageSizeProperty);
            set => SetValue(PageSizeProperty, value);
        }

        public int Pages
        {
            get => (int)GetValue(PagesProperty);
            set => SetValue(PagesProperty, value);
        }

        public bool NSFWBlur
        {
            get => (bool)GetValue(NSFWBlurProperty);
            set => SetValue(NSFWBlurProperty, value);
        }

        public ObservableCollection<ImageEntry> Images
        {
            get => (ObservableCollection<ImageEntry>)GetValue(ImagesProperty);
            set => SetValue(ImagesProperty, value);
        }

        public ImageEntry? SelectedImageEntry
        {
            get => (ImageEntry)GetValue(SelectedImageEntryProperty);
            set => SetValue(SelectedImageEntryProperty, value);
        }

        public ICommand RemoveFromAlbumCommand
        {
            get => (ICommand)GetValue(RemoveFromAlbumCommandProperty);
            set => SetValue(RemoveFromAlbumCommandProperty, value);
        }

        public ICommand? RenameAlbumCommand
        {
            get => (ICommand)GetValue(RenameAlbumCommandProperty);
            set => SetValue(RenameAlbumCommandProperty, value);
        }

        public ICommand? RemoveAlbumCommand
        {
            get => (ICommand)GetValue(RemoveAlbumCommandProperty);
            set => SetValue(RemoveAlbumCommandProperty, value);
        }

        public ICommand? CopyCommand
        {
            get => (ICommand)GetValue(CopyCommandProperty);
            set => SetValue(CopyCommandProperty, value);
        }

        public ICommand? PageChangedCommand
        {
            get => (ICommand)GetValue(PageChangedCommandProperty);
            set => SetValue(PageChangedCommandProperty, value);
        }

        public ViewMode ViewMode
        {
            get => (ViewMode)GetValue(ViewModeProperty);
            set => SetValue(ViewModeProperty, value);
        }

        //public DataStore DataStore
        //{
        //    get => (DataStore)GetValue(DataStoreProperty);
        //    set => SetValue(DataStoreProperty, value);
        //}

        private static void VisibleItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void ThumbnailListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedImages = new ObservableCollection<ImageEntry>(ThumbnailListView.SelectedItems.Cast<ImageEntry>());

            _debounceUpdateSelection();
        }

        private Action _debounceUpdateSelection;

        private void Init()
        {
            _debounceUpdateSelection = Utility.Debounce(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    Model.SelectionAlbumMenuItems = new ObservableCollection<Control>();

                    var ids = SelectedImages.Select(d => d.Id).ToList();

                    if (ids.Any())
                    {
                        var albums = ServiceLocator.DataStore.GetImageAlbums(ids);

                        foreach (var album in albums)
                        {
                            var menuItem = new MenuItem() { Header = album.Name, Tag = album };
                            menuItem.Click += RemoveFromAlbum_OnClick;
                            Model.SelectionAlbumMenuItems.Add(menuItem);
                        }
                    }
                });
            });
        }
    }
}
