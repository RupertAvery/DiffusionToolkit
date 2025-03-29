using System;
using System.Windows.Input;

namespace Diffusion.Toolkit.Services;

public class TaggingService
{
    public event EventHandler<int> Rate;
    public event EventHandler<bool> Favorite;
    public event EventHandler<bool> NSFW;
    public event EventHandler<bool> ForDeletion;

    public void SetForDeletion(bool forDeletion)
    {
        ForDeletion?.Invoke(this, forDeletion);
    }

    public void SetRating(int value)
    {
        Rate?.Invoke(this, value);
    }

    public void SetFavorite(bool value)
    {
        Favorite?.Invoke(this, value);
    }

    public void SetNSFW(bool value)
    {
        NSFW?.Invoke(this, value);
    }
}