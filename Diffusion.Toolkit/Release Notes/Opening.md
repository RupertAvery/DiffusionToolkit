# Diffusion Toolkit

Are you tired of dragging your images into PNG-Info to see the metadata?  Annoyed at how slow navigating through Explorer is to view your images? Want to organize your images without having to move them around to different folders? Wish you could easily search your images metadata? 

Diffusion Toolkit (https://github.com/RupertAvery/DiffusionToolkit) is an image metadata-indexer and viewer for AI-generated images. It aims to help you organize, search and sort your ever-growing collection of best quality 4k masterpieces.

# Installation

### Windows

* If you haven’t installed it yet, download and install the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* [Download the latest release](https://github.com/RupertAvery/DiffusionToolkit/releases/latest) 
    * Under the latest release, expand Assets and download **Diffusion.Toolkit.v1.9.0.zip**.
* Extract all files into a folder

### Linux / MacOS

An basic experimental version is available on the AvaloniaUI branch. It works, and is functional, but it is now outdated and lacks features due to some bugs I have experienced in Avalonia.  

No official build is currently available, and while it can build on MacOS, getting it to run as an App can be tricky.

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
* Drag and Drop images onto the Preview to view them without scanning
* Open images with External Applications
* Localization (feel free to contribute and fix the AI-generated translations!)
