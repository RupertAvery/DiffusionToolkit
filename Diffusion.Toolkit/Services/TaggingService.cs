using System;
using System.Windows.Input;

namespace Diffusion.Toolkit.Services;

public enum TagType
{
    Rating,
    NSFW,
    Favorite,
    ForDeletion
}

public class TagEventArguments
{
    public int Id { get; set; }
    public TagType TagType { get; set; }
    public object Value { get; set; }
}

public class TaggingService
{
    public event EventHandler<TagEventArguments> TagUpdated;

    public void Rate(object sender, int id, int? value)
    {
        ServiceLocator.DataStore.SetRating(id, value);
        TagUpdated?.Invoke(sender, new TagEventArguments() { Id =id, TagType = TagType.Rating, Value = value});
    }

    public void Favorite(object sender, int id, bool value)
    {
        ServiceLocator.DataStore.SetFavorite(id, value);
        TagUpdated?.Invoke(sender, new TagEventArguments() { Id = id, TagType = TagType.Favorite, Value = value });
    }
    public void NSFW(object sender, int id, bool value)
    {
        ServiceLocator.DataStore.SetNSFW(id, value);
        TagUpdated?.Invoke(sender, new TagEventArguments() { Id = id, TagType = TagType.NSFW, Value = value });
    }

    public void ForDeletion(object sender, int id, bool value)
    {
        ServiceLocator.DataStore.SetDeleted(id, value);
        TagUpdated?.Invoke(sender, new TagEventArguments() { Id = id, TagType = TagType.ForDeletion, Value = value });
    }

}