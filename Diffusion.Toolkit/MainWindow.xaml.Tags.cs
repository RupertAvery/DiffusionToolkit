using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private void InitTags()
        {
            ServiceLocator.TagService.LoadTags = LoadTags;

            _model.ImportTagsCommand = new AsyncCommand<object>(async (o) =>
            {
                var dialog = ServiceLocator.DialogService;

                var result = await dialog.GetFiles(this, GetLocalizedText("Actions.Tags.ImportTags.Title"), new [] { "Text files|*.txt", "All files|*.*" }, "txt");

                if (result != null)
                {
                    var lines = result;
                    var allTags = ServiceLocator.DataStore.GetTags();

                    var tagLookup = allTags.Select(d => d.Name).ToHashSet();

                    var normalizedlines = lines.Select(d => d.Trim()).Distinct().Where(d => d.Length > 0 && !tagLookup.Contains(d));

                    ServiceLocator.TagService.CreateTags(normalizedlines);

                    LoadTags();
                }
            });

            _model.ExportTagsCommand = new AsyncCommand<object>(async (o) =>
            {
                var dialog = ServiceLocator.DialogService;
                var result = await dialog.GetFile(this, GetLocalizedText("Actions.Tags.ExportTags.Title"), new[] { "Text files|*.txt", "All files|*.*" }, "txt");

                if (result != null)
                {
                    var tags = ServiceLocator.DataStore.GetTags();

                    File.WriteAllLines(result, tags.Select(d => d.Name));
                }
            });

            _model.CreateTagCommand = new AsyncCommand<object>(async (o) =>
            {
                var title = GetLocalizedText("Actions.Tags.Create.Title");

                var (result, name) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Tags.Create.Message"), title);

                if (result == PopupResult.OK)
                {
                    name = name.Trim();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Tags.CannotBeEmpty.Message"), title, PopupButtons.OK);
                        return;
                    }

                    ServiceLocator.TagService.CreateTag(name);

                    LoadTags();
                }
            });


            _model.RenameTagCommand = new AsyncCommand<TagFilterView>(async (tag) =>
            {
                var title = GetLocalizedText("Actions.Tags.Rename.Title");

                var (result, name) = await ServiceLocator.MessageService.ShowInput(GetLocalizedText("Actions.Tags.Rename.Message"), title, tag.Name);

                name = name.Trim();

                if (result == PopupResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await ServiceLocator.MessageService.Show(GetLocalizedText("Actions.Tags.CannotBeEmpty.Message"), title, PopupButtons.OK);
                        return;
                    }

                    ServiceLocator.TagService.UpdateTag(tag.Id, name);

                    UpdateTagName(tag.Id, name);

                    LoadTags();
                }
            });

            _model.RemoveTagCommand = new AsyncCommand<TagFilterView>(async (tag) =>
            {
                var title = GetLocalizedText("Actions.Tags.Remove.Title");

                var result = await _messagePopupManager.Show(GetLocalizedText("Actions.Tags.Remove.Message").Replace("{tag}", tag.Name), title, PopupButtons.YesNo);

                if (result == PopupResult.Yes)
                {
                    _dataStore.RemoveTag(tag.Id);

                    if (_search.QueryOptions.TagIds is { Count: > 0 })
                    {
                        if (_search.QueryOptions.TagIds.Contains(tag.Id))
                        {
                            _search.QueryOptions.TagIds = _search.QueryOptions.TagIds.Except(new[] { tag.Id }).ToList();
                        }
                    }

                    LoadTags();

                    // TODO: Detect whether we need to refresh?
                    _search.ReloadMatches(null);
                }
            });


        }

        private void LoadTags()
        {
            HashSet<int> currentSelected = new HashSet<int>();

            if (_model.Tags is { Count: > 0 })
            {
                currentSelected = _model.Tags.Where(d => d.IsTicked).Select(d => d.Id).ToHashSet();
            }

            _model.Tags = ServiceLocator.TagService.GetTagFilterViews();

            if (currentSelected.Any())
            {
                foreach (var tag in _model.Tags)
                {
                    tag.IsTicked = currentSelected.Contains(tag.Id);
                }
            }
        }

        public void UpdateTagName(int id, string name)
        {
            _model.Tags.First(d => d.Id == id).Name = name;
        }


    }
}