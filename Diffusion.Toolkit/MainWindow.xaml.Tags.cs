using Diffusion.Database;
using System.Windows.Controls;
using System.Windows;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;
using Diffusion.Database.Models;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private void InitTags()
        {
            ServiceLocator.TagService.LoadTags = LoadTags;

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
            _model.Tags = ServiceLocator.TagService.GetTagFilterViews();
        }

        public void UpdateTagName(int id, string name)
        {
            _model.Tags.First(d => d.Id == id).Name = name;
        }


    }
}