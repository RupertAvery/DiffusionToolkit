# Diffusion Toolkit

Are you tired of dragging your images into PNG-Info to see the metadata?  Annoyed at how slow navigating through Explorer is to view your images? Want to organize your images without having to move them around to different folders? Wish you could easily search your images metadata? 

Diffusion Toolkit (https://github.com/RupertAvery/DiffusionToolkit) is an image metadata-indexer and viewer for AI-generated images. It aims to help you organize, search and sort your ever-growing collection of AI-generated high-quality masterpieces.

# Installation

* Currently available for Windows only.
* [Download the latest release](https://github.com/RupertAvery/DiffusionToolkit/releases/latest
) 
    * Under the latest release, expand Assets and download **Diffusion.Toolkit.v1.8.0.zip**.
* Extract all files into a folder.
* Prerequisite: If you haven’t installed it yet, download and install the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* Linux Support: An experimental version is available on the AvaloniaUI branch, but it lacks some features. No official build is available.

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

ComfyUI Workflow Search is now implemented! Here's what you need to do to be able to search on ComfyUI workflows:

To search workflows effectively, you need to rescan your ComfyUI images. Choose one of the following methods:

* **Edit > Rebuild Metadata** – Rescans all images in your database.
* **Search > Rescan Metadata** – Rescans images in current search results.
* **Right-click a Folder > Rescan** – Rescans all images in a selected folder.
* **Right-click Selected Images > Rescan** – Rescans only selected images.

### How it works

Diffusion Toolkit scans images, extracts workflow nodes and properties, and saves them to the database. This makes searches more efficient and precise. However, since it generates a lot of data, not all properties are searched by default.

### Quick Search

Quick Search now includes searching through workflow properties. Simply type in the search bar and press Enter. By default, it searches the following properties:

* `text`
* `text_g`
* `text_l`
* `text_positive`
* `text_negative`

You can modify these settings in **Search Settings** (the Slider icon in the search bar). 

To find property names, check the **Workflow** tab in the **Metadata Pane** or in the **Metadata Overlay** (press I to toggle). To add properties directly to the list, click `...` next to a node property in the Workflow Pane and select **Add to Default Search**.

### Filter

The Filter now allows you to refine searches based on node properties. Open it by clicking the Filter icon in the search bar or pressing CTRL+F, then go to the Workflow tab.

* Include properties to filter by checking the box next to them.
* Use wildcards (\*) to match multiple properties (e.g., `text*` matches `text`, `text_g`, etc.).
* Choose property value comparisons: `contains`, `equals`, `starts with`, or `ends with`.
* Combine filters with `OR`, `AND`, and `NOT` operators.

To add properties, click `...` next to a node property in the Workflow Pane and select **Add to Filters**.

# Updates

* ComfyUI Search support!
* Performance improvements:
   * Massive improvements in results loading and paging 
   * Query improvements
   * Added indexes
   * Increased SQLite `cache_size` to 1GB. Memory usage will be increased
   * Added a spinner to indicate progress on some slow queries
* Filtering on multiple albums and models
* Scroll wheel now works over albums / models / folders
* Fixed Fit to Preview and Actual Size being reset when moving between images in the Preview
* Fixed Prompt Search error
* Fixed some errors scanning NovelAI metadata
* Fixed some issues with Unicode text prompts
* Page no longer resets position when removing an image from an album or deleting
* Fixed Metadata not loaded for first image
* Fixed Model name not showing for some local models

