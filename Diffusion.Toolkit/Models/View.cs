using Diffusion.Database;
using Diffusion.Toolkit.Common;
using Diffusion.Toolkit.Pages;
using System;
using System.Windows.Media.Animation;

namespace Diffusion.Toolkit.Models;

public class View
{
    private readonly TimelineModel? _timeline;
    private readonly string _query;
    private readonly Filter _filter;
    private readonly ModeSettings _modeSettings;
    private readonly MainModel _mainModel;

    public string GetQuery(bool useTimeline)
    {
        var start = _timeline?.StartDay;
        var end = _timeline?.EndDay;
        var query = _query;

        if (_modeSettings.IsFavorite)
        {
            query = $"{query} favorite: true";
        }
        else if (_modeSettings.IsMarkedForDeletion)
        {
            query = $"{query} delete: true";
        }
        else if (_modeSettings.ViewMode == ViewMode.Folder)
        {
            if (_modeSettings.CurrentFolder != "$")
            {
                query = $"{query} folder: \"{_modeSettings.CurrentFolder}\"";
            }
        }
        else if (_modeSettings.ViewMode == ViewMode.Album)
        {
            if (_mainModel.CurrentAlbum != null)
            {
                query = $"{query} album: \"{_mainModel.CurrentAlbum.Name}\"";
            }
        }
        else if (_modeSettings.ViewMode == ViewMode.Model)
        {
            if (_mainModel.CurrentModel != null)
            {
                query = $"{query} model_or_hash: \"{_mainModel.CurrentModel.Name}\"|{_mainModel.CurrentModel.Hash}";
            }
        }

        if (useTimeline)
        {
            if (start != null)
            {
                if (end != null)
                {
                    var dateFilter = $"date: from {start.Month.Year:0000}-{start.Month.Month:00}-{start.Day:00} to {end.Month.Year:0000}-{end.Month.Month:00}-{end.Day:00}";
                    query = query + " " + dateFilter;
                }
                else
                {
                    var dateFilter = $"date: {start.Month.Year:0000}-{start.Month.Month:00}-{start.Day:00}";
                    query = query + " " + dateFilter;
                }
            }
        }

        return query;
    }

    public Filter GetFilter(bool useTimeline)
    {
        var filter = _filter.Clone();

        if (_modeSettings.IsFavorite)
        {
            filter.UseFavorite = true;
            filter.Favorite = true;
        }
        else if (_modeSettings.IsMarkedForDeletion)
        {
            filter.ForDeletion = true;
            filter.UseForDeletion = true;
        }
        else if (_modeSettings.ViewMode == ViewMode.Folder)
        {
            if (_modeSettings.CurrentFolder != "$")
            {
                filter.Folder = _modeSettings.CurrentFolder;
            }
        }
        else if (_modeSettings.ViewMode == ViewMode.Album)
        {
            if (_mainModel.CurrentAlbum != null)
            {
                filter.Album = _mainModel.CurrentAlbum.Name;
            }
        }
        else if (_modeSettings.ViewMode == ViewMode.Model)
        {
            if (_mainModel.CurrentModel != null)
            {
                filter.ModelHash = _mainModel.CurrentModel.Hash;
                filter.ModelName = _mainModel.CurrentModel.Name;
            }
        }

        if (useTimeline)
        {
            var start = _timeline.StartDay;
            var end = _timeline.EndDay;

            if (start != null)
            {
                filter.Start = new DateTime(start.Month.Year, start.Month.Month, start.Day);
                filter.UseCreationDate = true;
            }
            if (end != null)
            {
                filter.End = new DateTime(end.Month.Year, end.Month.Month, end.Day);
            }
        }

        return filter;
    }


    public bool UseFilter { get; }

    public View(string query, Filter filter, bool useFilter, ModeSettings modeSettings, MainModel mainModel, TimelineModel? timeline)
    {
        UseFilter = useFilter;

        _timeline = timeline;
        _filter = filter;
        _modeSettings = modeSettings;
        _mainModel = mainModel;
        _query = query;
    }

    public (int, long) GetCountSize(DataStore dataStore)
    {
        if (UseFilter)
        {
            var filter = GetFilter(true);
            var count = dataStore.Count(filter);
            var size = dataStore.CountFileSize(filter);
            return (count, size);
        }
        else
        {
            var query = GetQuery(true);
            var count = dataStore.Count(query);
            var size = dataStore.CountFileSize(query);
            return (count, size);
        }
    }
}