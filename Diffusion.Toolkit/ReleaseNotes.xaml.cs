using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Diffusion.Common;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit
{
    public class ReleaseNotesModel : BaseNotify
    {

        private List<string> _files;
        private int _currentFile;
        private List<FolderChange> _folderChanges = new List<FolderChange>();
        private string _markdown;
        private bool _canNext;
        private bool _canPrevious;

        public ICommand Escape { get; set; }
        public ICommand PreviousCommand { get; set; }
        public ICommand NextCommand { get; set; }

        public bool CanPrevious
        {
            get => _canPrevious;
            set => SetField(ref _canPrevious, value);
        }
        public bool CanNext
        {
            get => _canNext;
            set => SetField(ref _canNext, value);
        }

        public string Markdown
        {
            get => _markdown;
            set => SetField(ref _markdown, value);
        }

        public Style Style { get; set; }
        public Action Reset { get; set; }

        public ReleaseNotesModel()
        {
            var releaseNotes = ResourceHelper.GetResources("Diffusion.Toolkit.Release_Notes");

            _files = releaseNotes.Where(SemanticVersion.IsSemanticVersion).OrderBy(SemanticVersion.Parse).ToList();

            _currentFile = _files.Count - 1;

            void UpdateButtons()
            {
                CanNext = _currentFile < _files.Count - 1;
                CanPrevious = _currentFile > 0;
            }

            PreviousCommand = new RelayCommand<object>((o) =>
            {
                if (_currentFile > 0)
                {
                    _currentFile--;
                    UpdateButtons();
                    LoadFile(_files[_currentFile]);
                    Reset?.Invoke();
                }
            });

            NextCommand = new RelayCommand<object>((o) =>
            {
                if (_currentFile < _files.Count - 1)
                {
                    _currentFile++;
                    UpdateButtons();
                    LoadFile(_files[_currentFile]);
                    Reset?.Invoke();
                }
            });

            Style = MdStyles.CustomStyles.BetterGithub;

            LoadFile(_files[_currentFile]);
            UpdateButtons();
        }

        private void LoadFile(string path)
        {
            Markdown = ResourceHelper.GetString(path);
        }
    }

    public partial class ReleaseNotesWindow : BorderlessWindow
    {
        private readonly ReleaseNotesModel _model = new ReleaseNotesModel();

        public ReleaseNotesWindow()
        {
            InitializeComponent();
            DataContext = _model;

            _model.Reset = Reset;
        }

        private void Reset()
        {
            var scrollViewer = GetScrollViewer(ReleaseNotesViewer) as ScrollViewer;
            scrollViewer.ScrollToTop();
        }



        public static DependencyObject? GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            {
                return o;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }

    }
}
