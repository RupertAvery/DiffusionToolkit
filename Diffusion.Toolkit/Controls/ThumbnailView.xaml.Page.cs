using System;

namespace Diffusion.Toolkit.Controls
{
    public partial class ThumbnailView
    {
        public event EventHandler<int>? PageChangedEvent;

        public void GoFirstPage()
        {
            Model.Page = 1;

            PageChangedEvent?.Invoke(this, Model.Page);
        }

        public void GoLastPage()
        {
            Model.Page = Model.Pages;

            PageChangedEvent?.Invoke(this, Model.Page);
        }

        public void GoPrevPage()
        {
            Model.Page--;

            PageChangedEvent?.Invoke(this, Model.Page);
        }

        public void GoNextPage()
        {
            Model.Page++;

            PageChangedEvent?.Invoke(this, Model.Page);
        }

        public void SetPagingEnabled()
        {
            Model.SetPagingEnabled(Model.Page);
        }
    }
}
