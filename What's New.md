
# What's New in v0.8

* **File Size metadata** - Run **Rebuild Images** to read file sizes into your database. This is used to report the total file size of matching images. You only need to do this once for existing images. New images will have this value populated.  This is not searchable yet.
* **NFSW Tagging**- Press `N` to tag an image as NSFW.  There are options to blur and hide NSFW-tagged images from the search results. See [documentation](https://github.com/RupertAvery/DiffusionToolkit/blob/master/Diffusion.Toolkit/Tips.md).
* **Check for updates** - Diffusion Toolkit can now update itself. See **Help > Check for updates**, and the option to **Check for updates on startup** in **Settings**.
* **Scan for images on startup** - See the **Images** tab under Settings.

## Other stuff

* Improved layout for Getting Started viewer (adds more space between paragraphs)

* Notifications for images added from watched folders will disappear after 10 seconds. This is in case you leave the app unattended and active while generating images.  For those of you who can generate images in less than 10 seconds... don't leave it active and unattended, or you'll have to wade through tons of popups.  Until I figure out the best way to handle this.

* Improvements to tagging multiple images for Favorite/Rating/NSFW.
   * Using bulk updates, tagging 200+ images is now almost instant
   * When tagging multiple images with different values, the most dominant value is used to ensure all the images have the same value.
     * e.g. With 10 images selected, 6 of which are tagged as favorite, pressing F will remove the favorite tag.

* A bug fix for new users unable to launch the application

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/beta_v0.8

# What's New in v0.7

v0.7 was a stealth release aimed at fixing issues for some users.

Highlights:

* Important: Metadata pane has been removed in favor of an overlay on top of preview to give more space to image. Press I to toggle.
* Adds support for InvokeAI PNGInfo format
* Adds experimental Watch Folder
* Logging on startup, and global exception handler added

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/beta_v0.7
