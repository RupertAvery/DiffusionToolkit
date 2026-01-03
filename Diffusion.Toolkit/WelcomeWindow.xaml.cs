using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Diffusion.Toolkit.Configuration;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using SQLite;

namespace Diffusion.Toolkit
{
    public class WelcomeModel : BaseNotify
    {
        private List<FolderChange> _folderChanges = new List<FolderChange>();

        public WelcomeModel()
        {
            Step = 1;
        }

        public ICommand Escape
        {
            get;
            set => SetField(ref field, value);
        }

        public int Step
        {
            get;
            set
            {
                SetField(ref field, value);
                OnPropertyChanged("NotStart");
            }
        }

        public int SelectedIndex
        {
            get;
            set => SetField(ref field, value);
        }

        public ObservableCollection<string> ImagePaths
        {
            get;
            set => SetField(ref field, value);
        }

        public bool StoreWorkflow
        {
            get;
            set => SetField(ref field, value);
        }

        public bool StoreMetadata
        {
            get;
            set => SetField(ref field, value);
        }

        public bool NotStart
        {
            get
            {
                switch (Step)
                {
                    case 1:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public bool ScanForNewImagesOnStartup
        {
            get;
            set => SetField(ref field, value);
        }
    }



    /// <summary>
    /// Interaction logic for Tips.xaml
    /// </summary>
    public partial class WelcomeWindow : BorderlessWindow
    {
        private readonly Settings _settings;
        private readonly WelcomeModel _model = new WelcomeModel();

        public IReadOnlyList<string> SelectedPaths { get; private set; }


        private class MinimalFolder
        {
            public string Path { get; set; }
        }


        List<MinimalFolder> FindRootFolders(SQLiteConnection db)
        {
            var allFolders = db.Query<MinimalFolder>("SELECT Path FROM Folder").ToList();

            var rootFolders = allFolders.Where(d => !allFolders.Any(p => d.Path.StartsWith(p.Path) && p.Path != d.Path)).ToList();

            return rootFolders;
        }

        public WelcomeWindow(Settings settings)
        {
            _settings = settings;

            InitializeComponent();

            _model.ImagePaths = new ObservableCollection<string>();

            try
            {
                // For 1.9+ users starting with no config, but with an existing database
                // Try to load the current root folders
                var db = ServiceLocator.DataStore.OpenConnection();

                List<MinimalFolder> folders = new List<MinimalFolder>();


                if (db.TableExist("Folder"))
                {
                    try
                    {
                        var hasIsRoot = db.HasColumn("Folder", "IsRoot");
                        if (hasIsRoot)
                        {
                            folders = db.Query<MinimalFolder>("SELECT Path FROM Folder WHERE IsRoot = 1").ToList();
                        }
                        else
                        {
                            folders = FindRootFolders(db);
                        }
                    }
                    catch (Exception e)
                    {
                        folders = FindRootFolders(db);
                    }
                }

                if (folders.Any())
                {
                    _model.ImagePaths = new ObservableCollection<string>(folders.Select(d => d.Path));
                }
            }
            catch (Exception e)
            {
                // Swallow exceptions in case the schema hasn't been updated yet
            }

            Closing += (sender, args) =>
            {
                settings.SetPristine();
                SelectedPaths = _model.ImagePaths.ToList();
                settings.StoreWorkflow = _model.StoreWorkflow;
                settings.StoreMetadata = _model.StoreMetadata;
                settings.ScanForNewImagesOnStartup = _model.ScanForNewImagesOnStartup;
            };

            DataContext = _model;
        }

        //private void HyperLink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        //{
        //    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
        //    {
        //        UseShellExecute = true,
        //    });
        //    e.Handled = true;
        //}


        private void AddFolder_OnClick(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                if (_model.ImagePaths.Any(d => dialog.FileName.StartsWith(d + "\\")))
                {
                    MessageBox.Show(this,
                        "The selected folder is already on the path of one of the included folders",
                        "Add folder", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
                else if (_model.ImagePaths.Any(d => d.StartsWith(dialog.FileName)))
                {
                    MessageBox.Show(this,
                        "One of the included folders is on the path of the selected folder! It is recommended that you remove it.",
                        "Add folder", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                _model.ImagePaths.Add(dialog.FileName);
            }

        }

        private void RemoveFolder_OnClick(object sender, RoutedEventArgs e)
        {
            _model.ImagePaths.RemoveAt(_model.SelectedIndex);
        }

        private void Back_OnClick(object sender, RoutedEventArgs e)
        {
            if (_model.Step > 1)
            {
                _model.Step -= 1;
            }
        }

        private void Next_OnClick(object sender, RoutedEventArgs e)
        {
            if (_model.Step < 4)
            {
                _model.Step += 1;
            }
            else
            {
                Close();
            }
        }

    }
}
