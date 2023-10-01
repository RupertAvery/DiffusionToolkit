# Diffusion Toolkit Release v1.3

Diffusion Toolkit is an image metadata-indexer and viewer for AI-generated images.  It can scan thousands of in seconds and then lets you search them by prompt, seed, hash and more.

You can also tag your images as favorites, rate them 1-10, sort them by aesthetic score, mark them as NSFW, blur images marked as NSFW, and auto-tag NSFW images by looking at keywords in the prompt.

You can arrange them in albums, view them by folder, see your most used prompts.

You can sort by Date Created, Aesthetic Score and Rating.

And of course, you can view the metadata stored with your image.

Diffusion Toolkit supports various image and metadata:

* JPG/JPEG + EXIF
* PNG
* WebP
* .TXT metadata

Metadata formats supported are:

* AUTOMATIC1111
* InvokeAI (Dream/sd-metadata/invokeai_metadata)
* NovelAI
* Stable Diffusion
* ComfyUI + SDXL (Work in progress) 
* EasyDiffusion

You can even use it on images without metadata and still use the other features such as rating and albums!

How to get it:

* [Download](https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.3
) (Windows)
* Requires [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 
* [Usage](https://github.com/RupertAvery/DiffusionToolkit/blob/master/Diffusion.Toolkit/Tips.md)

# What's New in v1.3

It's been a while since the last update. I've been busy moving to another country, and now that I'm settled in, we can get back to updates.

Thanks once again for everyone who submitted bug reports.

## BugFixes

* Show info key does not work when the preview has focus (#119)
* Clicking on scroll bar opens image in full screen (#124)
* Shift + right arrow doesn't go to the next row (#128)
* Retain copied parameter content in the Windows Clipboard when closing Diffusion Toolkit (#130)
* Use case-insensitive search for model hash (#131)
* Fix some album issues #132
* Fix some crashes when clicking thumbnails
* Fix toggling info with I in popout Preview

## New Features 

* Added support for EasyDiffusion metadata
* Added support for ComfyUI SDXL workflow metadata (experimental)
* Added support for another InvokeAI metadata format (invokeai_metadata)
* Tools > Add search results to album (#138)

## Enhancements

* Pressing Escape in dialogs now closes them (if I missed something, let me know) (#123)
* Added a button to clear the search box (and reload all images)
* Added a button to refresh the current search results (#136)

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.3
