# Diffusion Toolkit

Diffusion Toolkit is an image metadata-indexer and viewer for AI-generated images. It aims to help you organize, search and sort your ever-growing collection, using the data that's right there on the images.

# Usage

Usage should be pretty straightforward, but there are a lot of tips and tricks and shortcuts you can learn. See the documentation for [Getting Started](https://github.com/RupertAvery/DiffusionToolkit/tree/master/Diffusion.Toolkit/Tips.md)

Thanks to Bill Meeks for putting together a demonstration video:

[![Organize your AI Images](https://img.youtube.com/vi/r7J3n1LjojE/hqdefault.jpg)](https://www.youtube.com/watch?v=r7J3n1LjojE&ab_channel=BillMeeks)

Want to see the prompt you used to generate the image? Press I to show the PNGInfo in the preview pane. (provided that your image contains the metadata, or has an accompanying TXT file)

Querying your images is simple, just enter the prompt. Diffusion Toolkit will break your prompt into tokens and search for matching images.

Then, you can refine your search such as adding `seed: 12345678` to filter by seed (or seed range) or a date such as `date: since 2023-09-01` or `date: betweem 2023-10-01 and 2023-11-01`, or even `date: since yesterday`.

Want to organize your images? Create an album and drag your image to them.  You can even go to Tools > Search results and add all the images in your current query to an album.

Want to rank your images? Diffusion Toolkit can read the aesthetic score and display it, and you can sort images by the aesthetic score. You can also rank them yourself, just press 1-9, and 0 to rank from 1-10.  Then, you can search for ranked images with `rank: >5`

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
    * Drag and drop images to another folder to move (CTRL-drag to copy)

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

You can even use it on images without metadata and still use the other features such as rating and albums!

# What's New in v1.4

* Image viewer features and improvements!
   * Full-screen view 
      * Covers taskbar and hides maximize/minimize buttons
      * Press ESC to close
      * Press F11 to switch between full-screen/windowed mode
   * Use system viewer
   * Use custom viewer
   * see **Settings > Images** tab
* Prompt viewer improvements!
   * Selecting a prompt in Prompt view will show the images used by that prompt.  You can add them to albums, tag them, but not view (for now).
   * No thumbnails for Negative Prompts yet.
   * Fixes to prompt queries, counts should be more accurate now
   * Listed Prompts are affected by Show/Hide NSFW 
* Albums Improvements
   * Menu item added for albums
   * You can now sort Albums! See Albums > Sort
      * Name
      * Date
      * Custom
   * You can now see the albums that an image is part of in the metadata info (I).
* Some ComfyUI parsing fixes
   * Errors shouldn't prevent images from loading, but the metadata will be empty
* Civitai integration
   * Use CivitAI model database to get model names from the hash
      * Edit > Download Civitai models (saved to `models.json`)
      * Model names will be lookedup from Civitai model list in addition   to `cache.json` and local models
   * Open the CivitAI model page from PNGInfo Model Hash 
      * Show Metadata info > Model Hash > Click Search
* Tools improvements
   * Add search results to album
* Display the last selected image when selecting multiple images (#143)
* Added reload search (#136) and clear search buttons next to query text field. Buttons are placed on the right.
* Moved Filter button to the right
* Fix openclibboard failed (0x800401D0) (#144)
* Preview - Mousewheel Zoom without CTRL (#142)
* Thinner Scrollbars
* Various bug fixes

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.4
