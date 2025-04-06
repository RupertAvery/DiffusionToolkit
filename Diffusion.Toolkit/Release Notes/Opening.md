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
