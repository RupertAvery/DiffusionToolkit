# Diffusion Toolkit

Are you tired of dragging your images into PNG-Info to see the metadata?  Annoyed at how slow navigating through Explorer is to view your images? Want to organize your images into albums? Wish you could search for your images by prompt? 

Diffusion Toolkit (https://github.com/RupertAvery/DiffusionToolkit) is an image metadata-indexer and viewer for AI-generated images. It aims to help you organize, search and sort your ever-growing collection.

# Installation

* Currently runs on Windows only 
* [Download](https://github.com/RupertAvery/DiffusionToolkit/releases/latest
) the latest release 
    * Look for **> Assets** under the latest release, expand it, then grab the zip file **Diffusion.Toolkit.v1.8.0.zip**.
* Unzip all the files to a folder
* You may need to install the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) if you haven't already
* An experimental Linux version is available on the AvaloniaUI branch, but features are currently behind. No official build is available.

# Features

* Support for many image metadata formats:
   * AUTOMATIC1111 and A1111-compatible metadata such as
      * Tensor.Art
      * SDNext
      * ComfyUI with [SD Prompt Saver Node](https://github.com/receyuki/comfyui-prompt-reader-node)
      * Stealth-PNG (saved in Alpha Channel) https://github.com/neggles/sd-webui-stealth-pnginfo/
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
* Localization (feel free to contribute and fix the AI-generated translations!)

# What's New in v1.8.0

## ComfyUI Workflow Search

ComfyUI Workflow Search is now implemented! Here's what you need to do to be able to search on ComfyUI workflows. 

You will need to rescan your ComfyUI images. You can do this in a couple of ways:

* **Edit > Rebuild Metadata** - This will rescan all the images currently in your database
* **Search > Rescan Metadata** - This will rescan all the images in current search results
* **Select Folder > Right-click > Rescan** - This will rescan all the images in the selected folder
* **Select images > Right-click > Rescan** - This will rescan the selected images

### How it works

When Diffusion Toolkit scans your images, it parses the workflow and extracts the nodes and node properties and saves them to the database. This should make searching much more efficient and specific than searching the entire workflow. That said, it generates a LOT of data, so we don't
want to always search _every_ property.

### Quick Search

Quick search is done by typing something into the search bar and pressing enter. ComfyUI Workflow Search is now enabled in Quick search by default.

By default, the following properties are used in Quick search:

* `text`
* `text__g`
* `text__l`
* `text__positive`
* `text__negative`

Note that property names seem to use double underscore by convention.

You can edit the properties in the Search Settings (Slider icon in the search bar). You can also disable ComfyUI Search if you so wish.

To find property names, look in the Workflow tab in the Metadata Pane (below the Preview Pane if not hidden) or the Metadata Overlay (Press I to show or hide the Metadata Overlay)

You can also add properties to the list by clicking on the ... button to the left of each node property in the Workflow Pane, and selecting **Add to Default Search**

### Filter

The Filter allows you to customize your searches on node properties.

Open the Filter popup (Click the Filter icon in the search bar, or press CTRL+F) and go the the Workflow tab.

Here you can add properties you want to include in the filter. To include the property in the filter, the checkbox on the left must be checked.

Properties can have wildcards (*).

For example, the property name `text*` will match

* `text`
* `text__g`
* `text__l`
* `text__positive`
* `text__negative`

and any other property that starts with `text`.

NOTE: wildcard searches may be a bit slower than non-wildcard searches

You can set the property value comparison to **contains**, **equals**, **starts with** and **ends with**. This determines how the value will be evaluated when searching.

You can combine filters with the operators OR, AND, and NOT.

You can also add properties to the list by clicking on the ... button to the left of each node property in the Workflow Pane, and selecting **Add to Filters**

# Updates

* ComfyUI Search support!
* Performance improvements:
   * Massive improvements in results loading and paging 
   * Query improvements
   * Added indexes
   * Increased SQLite `cache_size` to 1GB. Memory usage will be increased
   * Added a spinner to indicate progress on some slow queries
* Filtering on multiple albums
* Scroll wheel now works over albums / models / folders
* Fixed Fit to Preview and Actual Size being reset when moving between images in the Preview
* Fixed Prompt Search error
* Fixed some errors scanning NovelAI metadata
* Fixed some issues with Unicode text prompts