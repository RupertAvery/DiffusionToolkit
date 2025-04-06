# Diffusion Toolkit Release v1.2

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

* [Download](https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.2
) (Windows)
* Requires [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 
* [Usage](https://github.com/RupertAvery/DiffusionToolkit/blob/master/Diffusion.Toolkit/Tips.md)

# What's New in v1.2

## BugFixes

* Fixed **UTF-8 prompts not being decoded correctly**.  This fixes issues with **emoji** and **East-Asian characters** appearing incorrectly.
* Fixed "**Could not find Diffusion.Updater.dll**" error.  It's no longer needed.
* Fixed a bug that caused the **metadata (PNGInfo) in the Preview overlay to be unselectable**.
* **Added missing indexes** on the AlbumImage table - this may have causes slowdowns on Albums.
* Fixed a **file size bug** that was causing negative values to appear. 
* Fixed a bug when **moving an image to a subfolder** of a watched folder doesn't remove it from the database, even if recursion is off, or if the folder is excluded.
* Fixed a bug when **adding a filter to an Album** caused an error message.

## New Features 

* **Dragging from the thumbnail into Explorer** now works as Explorer does. By default, the event performed is a Move. Hold **Ctrl** down while dragging to perform a Copy (the cusor will change)

   **WARNING** - a move performed using this feature is the same as doing it in explorer: Diffusion Toolkit has no control over this, and you will lose Diffusion Toolkit metadata (favorites, rating, nsfw) even when moving to a nested Diffusion Folder.

* You can now **drag from the Preview** pane / window into Explorer / any app that is a drop target, such as PNGInfo, any browser, Photoshop, etc.
   
   This only takes effect when the image is not zoomed it (no scrollbars). 

* Added `in_album: <true|false>` query to allow users primarily to find images that are not yet in any album. This should eventually work is way into some easier-to-access UI setting, not to mention the filter UI.

* Added **alternate aesthetic score**. (https://github.com/vladmandic/sd-extension-aesthetic-scorer) The metadata Tag Description is "Score:", but it is mapped to same field as "aesthetic_score" just so it can be sorted on and filtered without having a separate field.  Thanks to abariba for the code suggestion.

## Enhancements

* **Faster Loading of thumbnails** - Paging is now almost instantaneous at 100 thumbnails per page, and tolerable at 500 (on my machine). 


   


https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.2
