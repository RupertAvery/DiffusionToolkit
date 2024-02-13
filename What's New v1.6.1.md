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
   * FooocusMRE
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

# What's New in v1.6.1

## Enhancements

* Metadata panel overlay has returned for those who prefer a full-height preview pane. As before, press I to toggle.
* Improved navigation pane sizing
    * Resizable panes
    * Last pane will always take up the rest of the space (and not be resizeable)
    * Basically works more like you would expect
* Support opening images that don't match the file extension.
    * e.g. some tools might generate PNGs with a JPEG extension. Image viewers can open the image, but the metadata parser fails to read it correctly based on extension. This has now been fixed.
* New shortcuts: toggle Navigation Pane with F3, Preview Pane with F4

## Bugfixes

* Some fixes for `UNIQUE constraint failed: Image.Path`. In case you are affected, try looking at 'https://github.com/RupertAvery/DiffusionToolkit/issues/218#issuecomment-1939824894'
* Fix for layout issues. If your thumbnail appear to have only one line, this should fix it.  There is also a View > Reset layout in case layout gets messed up.
* Should fix some `Value cannot be null (Parameter 'element')` errors

