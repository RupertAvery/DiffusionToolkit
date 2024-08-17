# Diffusion Toolkit

Are you tired of dragging your images into PNG-Info to see the metadata?  Annoyed at how slow navigating through Explorer is to view your images? Want to organize your images into albums? Wish you could search for your images by prompt? 

Diffusion Toolkit (https://github.com/RupertAvery/DiffusionToolkit) is an image metadata-indexer and viewer for AI-generated images. It aims to help you organize, search and sort your ever-growing collection.

[![Organize your AI Images](https://img.youtube.com/vi/r7J3n1LjojE/hqdefault.jpg)](https://www.youtube.com/watch?v=r7J3n1LjojE&ab_channel=BillMeeks)

# Installation

* Currently runs on Windows only 
* [Download](https://github.com/RupertAvery/DiffusionToolkit/releases/latest
) the latest release 
    * Look for **> Assets** under the latest release, expand it, then grab the zip file **Diffusion.Toolkit.v1.x.zip**.
* Unzip all the files to a folder
* You may need to install the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) if you haven't already

# Features

* Support for many image generators
   * AUTOMATIC1111 and A1111-compatible metadata such as
      * Tensor.Art
      * SDNext
   * InvokeAI (Dream/sd-metadata/invokeai_metadata)
   * NovelAI
   * Stable Diffusion
   * EasyDiffusion
   * RuinedFooocus
   * Fooocus
   * FooocusMRE
   * Stable Swarm
* Scans and indexes your images in a database for lightning-fast search
* Search images by metadata (Prompt, seed, model, etc...)
* Custom metadata (stored in database, not in image) 
    * Favorite
    * Rating (1-10)
    * NSFW
* Organize your images 
    * Albums
    * Folder View
* Drag and Drop from Diffusion Toolkit to another app
* Translations - Diffusion Toolkit is localized

# What's New in v1.6.2

Thanks to Terence for a whole bunch of bug fixes and enhancements!

## Enhancements

* Added a **Slideshow** function in the Popout Preview window. 
   * Press Space to start/stop the slideshow
   * Go to **Settings > Image > Slide show delay** to adjust the time between image changes
* Improved thumbnail loading efficiency - only thumbnails in current view are loaded
   * Scrolling quickly will not trigger thumbnail loads
* Add **Actual Size** option in Preview
* Added filters: (Click the Filter button or press CTRL-F)    
   * **Aesthetic Score - No Score** - show images without an Aesthetic Score
   * **In Album** - show images are have been added to an album or not
   * **Exclude Prompt / Negative** - show images that do not include the specified prompt / negative terms 
* Add support for **Stable Swarm** metadata
* Add support for **Fooocus** metadata
* Hide images marked For Deletion from search results 
* Albums
   * Show all albums on right-click > Add to Albums
* CTRL + Refresh 
   * Press F5 or click the search button in the search bar to refresh the current page
   * Hold CTRL and press F5 or click the the search button to reset to the first page

## Bugfixes

* Additional indexes to fix query slowdowns
* Fixed bug where Preview Pane loses focus when changing pages
* Move txt files when moving images #239
* Fix issue with unsetting rating of selected images in thumbnail view #237
* Fixed some startup crashes
* Several bug fixes