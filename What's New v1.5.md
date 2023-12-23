# Diffusion Toolkit

Diffusion Toolkit (https://github.com/RupertAvery/DiffusionToolkit) is an image metadata-indexer and viewer for AI-generated images. It aims to help you organize, search and sort your ever-growing collection, using the data that's right there on the images.

# Usage

Usage should be pretty straightforward, but there are a lot of tips and tricks and shortcuts you can learn. See the documentation for [Getting Started](https://github.com/RupertAvery/DiffusionToolkit/tree/master/Diffusion.Toolkit/Tips.md)

Thanks to Bill Meeks for putting together a demonstration video:

[![Organize your AI Images](https://img.youtube.com/vi/r7J3n1LjojE/hqdefault.jpg)](https://www.youtube.com/watch?v=r7J3n1LjojE&ab_channel=BillMeeks)

Want to see the prompt you used to generate the image? Press I to show the PNGInfo in the preview. (provided that your image contains the metadata, or has an accompanying TXT file)

Querying your images is simple, just enter the prompt. Diffusion Toolkit will break your prompt into tokens and search for matching images.

You can refine your search such as adding more conditions after your prompt query, such as `seed: 12345678` to filter by seed (or seed range) or a date such as `date: since 2023-09-01` or `date: between 2023-10-01 and 2023-11-01`, or even `date: since yesterday`.

Want to organize your images? Create an album and drag your image to them.  You can even go to Tools > Search results and add all the images in your current query to an album.

Want to rank your images? Diffusion Toolkit can read the aesthetic score and display it, and you can sort images by the aesthetic score. You can also rank them yourself, just press 1-9, and 0 to rank from 1-10.  Then, you can search for ranked images with `rank: 10` or `rank: >5`

There is also a UI-based filter if you don't want to type out a query.

Make sure to read the help file to see how you can sort and filter your images.

# Installation

* [Download](https://github.com/RupertAvery/DiffusionToolkit/releases/v1.4
) (Windows)
* Requires [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 


Look for **> Assets** under the latest release, expand it, then grab the zip file **Diffusion.Toolkit.v1.x.zip**.

# Features

* Scan images, store and index prompts and other metadata (PNGInfo)
* Search for your images
    * Using a simple query
    * Using the filter
* View images and the metadata easily
    * Toggle PNGInfo
* Tag your images 
    * Favorite
    * Rating (1-10)
    * NSFW
* Sort images
    * by Date Created 
    * by Aesthetic Score
    * by Rating   
* Auto tag NFSW by keywords
* Blur images tagged as NSFW 
    * NSFW
* Albums
    * Select images, right-click > Add to Album
    * Drag and drop images to albums
* Folder View
* View and search prompts
    * List Prompts and usage
    * List Negative Prompts and usage
    * List images associated with prompts
* Drag and Drop
    * Drag and drop images to a WebUI (Inpaint)
    * Drag and drop images to an application to open
    * Drag and drop images to a Windows Exporer folder to move (hold CTRL to copy)

# Supported formats

* JPG/JPEG + EXIF
* PNG
* WebP
* .TXT metadata

# Supported Metadata formats

* AUTOMATIC1111
* InvokeAI (Dream/sd-metadata/invokeai_metadata)
* NovelAI
* Stable Diffusion
* ComfyUI + SDXL (Work in progress) 
* EasyDiffusion
* RuinedFooocus

You can even use it on images without metadata and still use the other features such as rating and albums!

# What's New in v1.5

* Metadata Viewer:
  * Metadata is now always shown below the preview
  * In the popout/fullscreen, it it shown to the right
  * Added separate sections for Seed, CFG, Steps and Size after Negative Prompt
* Fixes to keyboard navigation
  * Fixed navigation issues when using Pageup/Pagedown
  * Fixed multi-selection issues with CTRL
* Support for RuinedFooocus metadata
* Folders
  * Images might be missing in folders after a move.  Run Tools > Folders > Fix missing images if you don't see indexed images in folder view.  
* Excluded Folders
   * Adding an excluded folder will now remove any indexed images
   * Run Tools > Folders > Remove excluded images to remove them manually 
   * To re-add previously excluded images, perform a Scan.
* Albums enhancements and fixes
   * Implemented a cleaner album list with image counts
   * Indicator in thumbnail view shows if an image is already in an album
   * Images were not being removed from the Album-Image tracking table when removing an album
   resulting in orphaned entries. The app will run a cleanup on each run. This is fast and should be unnoticeable.
* Filter UI enhancements
    * Press CTRL+F to open the filter
    * Now displays as a popup instead of fully covering the thumbnail view

## Bugfixes

* Loading a large image should no longer block the UI
* Folder not updating after move (#152)
* Crash opening app: "An unhandled exception occured: 'N' is an invalid start of a value  (#155) (thanks to kwaegel)
* DiffusionToolkit not reading separately stored aesthetic score tags from A1111 files in certain circumstances (#156) (thanks to curiousjp)

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.5
