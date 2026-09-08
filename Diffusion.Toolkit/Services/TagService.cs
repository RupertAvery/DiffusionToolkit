using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Services;

public class TagService
{
    public Action LoadTags;

    public void CreateTags(IEnumerable<string> names)
    {
        ServiceLocator.DataStore.CreateTags(names);
    }

    public void CreateTag(string name)
    {
        ServiceLocator.DataStore.CreateTag(name);
    }

    public void UpdateTag(int id, string name)
    {
        ServiceLocator.DataStore.UpdateTag(id, name);
    }

    public void RemoveTag(int id, string name)
    {
        ServiceLocator.DataStore.UpdateTag(id, name);
    }

    public ObservableCollection<TagFilterView> GetTagFilterViews()
    {
        var allTags = ServiceLocator.DataStore.GetTagsWithCount();

        return new ObservableCollection<TagFilterView>(allTags.Select(d => new TagFilterView()
        {
            Id = d.Id,
            Name = d.Name,
            TagCount = d.Count
        }));
    }

    public IReadOnlyCollection<ImageTagView> GetImageTagViews(int imageModelId)
    {
        var allTags = ServiceLocator.DataStore.GetTags();
        var imageTags = ServiceLocator.DataStore.GetImageTags(imageModelId).ToHashSet();

        return allTags.Select(d =>
        {
            var imageTag = new ImageTagView()
            {
                Id = d.Id,
                IsTicked = imageTags.Contains(d.Id),
                Name = d.Name,
            };

            imageTag.PropertyChanged += (sender, args) =>
            {
                if (ServiceLocator.MainModel.SelectedImages.Count > 1)
                {
                    if (args.PropertyName == nameof(ImageTagView.IsTicked))
                    {
                        var ids = ServiceLocator.MainModel.SelectedImages.Select(d => d.Id);

                        if (imageTag.IsTicked)
                        {
                            ServiceLocator.DataStore.AddImagesTag(ids, imageTag.Id);
                        }
                        else
                        {
                            ServiceLocator.DataStore.RemoveImagesTag(ids, imageTag.Id);
                        }

                        LoadTags?.Invoke();
                    }
                }
                else
                {
                    if (args.PropertyName == nameof(ImageTagView.IsTicked))
                    {
                        if (imageTag.IsTicked)
                        {
                            ServiceLocator.DataStore.AddImageTag(imageModelId, imageTag.Id);
                        }
                        else
                        {
                            ServiceLocator.DataStore.RemoveImageTag(imageModelId, imageTag.Id);
                        }

                        LoadTags?.Invoke();
                    }
                }

            };

            return imageTag;
        }).ToList();
    }


}