# Diffusion Toolkit Release v1.1

Diffusion Toolkit is an image metadata-indexer and viewer for AI-generated images.  It scans your 50,000 image collection in seconds and lets you search them by prompt, seed, hash and more.

You can also tag your images as favorites, rate them 1-10, sort them by aesthetic score, mark them as NSFW, blur images marked as NSFW, and auto-tag NSFW images by looking at keywords in the prompt.

You can arrange them in albums, view them by folder, see your most used prompts.

You can sort by Date Created, Aesthetic Score and Rating.

And of course, you can view the metadata stored with your image.

Diffusion Toolkit supports

* JPG/JPEG + EXIF
* PNG
* WebP
* .TXT metadata

Metadata formats supported are:

* AUTOMATIC1111
* InvokeAI
* NovelAI
* Stable Diffusion
* ComfyUI

How to get it:

* [Download](https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.1
) (Windows)
* Requires [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 
* [Usage](https://github.com/RupertAvery/DiffusionToolkit/blob/master/Diffusion.Toolkit/Tips.md)

# What's New in v1.1

## BugFixes

* Fixed bug where removed images would not reappear in folder view when rescanned. Probably affects newly scanned images not appearing in folders as well. Please **Edit > Rebuild Metadata** to update folder information.

* Fixed issue for **Rebuilding Metadata** and **AutoTag NSFW Tool**, any previously manually tagged NSFW would be untagged.

## Enhancements

* **Borderless Window** - Gets rid of the unsightly light-colored titlebar, especially in Dark mode. May have some minor side effects like missing the window alignment popup menu on the Maximize/Restore button.

   Also applied to popout Preview for a better viewing experience. 

* Built-in **Image Viewer** - Double-clicking an image will now open a maximized Preview Popout if the Preview is not already popped out. 

   While in this view, you can navigate images with the Left and Right cursor keys.  You can also cross pages once you reach the beginning or end of the current page. See **Improved Navigation in Thumbnail** for more details. Note that there will be a slight delay when crossing pages.

   You can dismiss the image viewer/popout with **Escape**.

* Optional **Recursive** Folder Scan for Diffusion Folders. See **Settings > General** tab

* **Exclude Folders** from Folder Scan. Only relevant if **Recursive** is enabled, and only allows subfolders of Diffusion Folders.

* **Improved Zoom** in Preview / Image Viewer

   * Zooming with the mouse scroll now keeps the center of the zoom at the mouse position.
   * Zooming with the `Ctrl +/-` now keeps the center of the zoom at center of the viewport.

* **Show/Hide Albums Panel** - Put the Albums panel away when not in use. Shortcut: `Ctrl+Shift+A`

* **Faster Mouse Scrolling Behavior in Thumbnail** - Scrolling was painfully slow before. It's much faster now.

* **Improved Navigation in Thumbnail**

   * You can now cross pages using cursor keys
   * Pressing **Left** at the first thumbnail of the page will move you to the previous page, at the last thumbnail.
   * Pressing **Right** at the last thumbnail of the page will move you to the next page, at the first thumbnail.
   * **Page guards** - If you reached the first or last thumbnail by holding down left/right from a thumbnail in between, you will not immediately move to the previous or next page. Release the key and press again to continue to the previous or next page.

* New **Keyboard Shortcuts**

   * `Ctrl+Shift+A` - Show/Hide Album Pane
   * `Ctrl+Shift+P` - Show/Hide Preview
   * `Ctrl+1` - Folders View
   * `Ctrl+2` - Album View
   * `Ctrl+3` - Diffusions View
   * `Ctrl+4` - Favorites Page
   * `Ctrl+5` - Recycle Bin
   * `Ctrl+6` - Prompts View
   * `Ctrl+R` - Scan folders for new images
   * `F5` - Refreshes the current view

* **Improved Manual** (F1) - Table of Contents hyperlinks now work


https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.1
