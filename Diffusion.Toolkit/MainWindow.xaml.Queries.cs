using Diffusion.Database;
using SQLite;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Diffusion.Toolkit.Classes;
using Diffusion.Toolkit.Models;
using Diffusion.Toolkit.Services;

namespace Diffusion.Toolkit
{
    public partial class MainWindow
    {
        private void InitQueries()
        {
            _model.SaveQuery = new AsyncCommand<object>(async (o) =>
            {
                if (!await CheckIfQueryEmpty("Save Query/Filter"))
                {
                    return;
                }

                var (result, name) = await _messagePopupManager.ShowInput("Enter a name for the query", "New Query");

                if (result == PopupResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _messagePopupManager.Show("Query name cannot be empty.", "New Query", PopupButtons.OK);
                        return;
                    }

                    name = name.Trim();

                    if (_dataStore.QueryExists(name))
                    {
                        result = await _messagePopupManager.Show($"Do you want to overwrite the query \"{name}\"?", "New Query", PopupButtons.YesNo);
                        if (result == PopupResult.No)
                        {
                            return;
                        }
                    }

                    await CreateOrUpdateQuery(name, _search.QueryOptions);
                }
            });

            _model.RemoveQueryCommand = new AsyncCommand<QueryModel>(async (queryModel) =>
            {
                var result = await _messagePopupManager.Show($"Are you sure you want to remove \"{queryModel.Name}\"?", "Remove Query", PopupButtons.YesNo);

                if (result == PopupResult.Yes)
                {
                    _dataStore.RemoveQuery(queryModel.Id);

                    LoadQueries();
                }
            });

            _model.RenameQueryCommand = new AsyncCommand<QueryModel>(async (queryModel) =>
            {
                var (result, name) = await _messagePopupManager.ShowInput("Enter a new name for the album", "Rename Query", queryModel.Name);

                name = name.Trim();

                if (result == PopupResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await _messagePopupManager.Show("Query name cannot be empty.", "Rename Query", PopupButtons.OK);
                        return;
                    }

                    _dataStore.RenameQuery(queryModel.Id, name);

                    queryModel.Name = name;
                }
            });
        }

        private void LoadQueries()
        {
            //var currentAlbums = _model.Queries is { } ? _model.Queries.ToList() : Enumerable.Empty<QueryModel>();

            var albums = _dataStore.GetQueries().Select(a => new QueryModel()
            {
                Id = a.Id,
                Name = a.Name,
            }).ToList();

            //foreach (var album in albums)
            //{
            //    var prevAlbum = currentAlbums.FirstOrDefault(d => d.Id == album.Id);

            //    if (prevAlbum != null)
            //    {
            //        album.IsSelected = prevAlbum.IsSelected;
            //    }

            //}

            switch (_settings.SortQueriesBy)
            {
                case "Name":
                    _model.Queries = new ObservableCollection<QueryModel>(albums.OrderBy(a => a.Name));
                    break;
                default:
                    _model.Queries = new ObservableCollection<QueryModel>(albums.OrderBy(a => a.Name));
                    break;
                    //case "Date":
                    //    _model.Albums = new ObservableCollection<AlbumModel>(albums.OrderBy(a => a.LastUpdated));
                    //    break;
                    //case "Custom":
                    //    _model.Albums = new ObservableCollection<AlbumModel>(albums.OrderBy(a => a.Order));
                    //    break;
            }


        }

        private async Task CreateOrUpdateQuery(string name, QueryOptions queryOptions)
        {
            _dataStore.CreateOrUpdateQuery(name, queryOptions);

            ServiceLocator.ToastService.Toast($"Query \"{name}\" created.", "New Query");

            LoadQueries();
        }
    }
}