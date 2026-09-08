# Diffusion Toolkit

Diffusion Toolkit is an image metadata-indexer and viewer for AI-generated images. It aims to help you organize, search and sort your ever-growing collection.

# Usage

Usage should be pretty straightforward, but there are a lot of tips and tricks and shortcuts you can learn. See the documentation for [Getting Started](https://github.com/RupertAvery/DiffusionToolkit/tree/master/Diffusion.Toolkit/Tips.md)

Thanks to Bill Meeks for putting together a demonstration video. This is for an older version.

[![Organize your AI Images](https://img.youtube.com/vi/r7J3n1LjojE/hqdefault.jpg)](https://www.youtube.com/watch?v=r7J3n1LjojE&ab_channel=BillMeeks)

# Installation

* Currently runs on Windows only 
* [Download](https://github.com/RupertAvery/DiffusionToolkit/releases/latest
) the latest release 
    * Look for **> Assets** under the latest release, expand it, then grab the zip file **Diffusion.Toolkit.v1.x.zip**.
* Unzip all the files to a folder
* You may need to install the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) if you haven't already

# Build from source

## Prerequisites

* Requires Visual Studio 2026
* [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (includes the desktop runtime)

## Building

* Clone this repository
* Run `publish.cmd`

A folder named `build` will be created, containing all the necessary files.

# Features

* Scan images and videos, store and index prompts and other metadata (PNGInfo)
* View images and the metadata easily
* Search for your images and videos through their metadata 
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
* Custom Tags
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
* MP4 

# Supported Metadata formats

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

You can even use it on images without metadata and still use the other features such as rating and albums!

# Donate

<a href="https://www.buymeacoffee.com/rupertavery" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-green.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" ></a>

or

<a href="https://www.paypal.me/rupertavery" target="_blank"><img src="https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif" alt="Donate"></a>

# Screenshots

![Screenshot 2024-02-09 183808](https://github.com/RupertAvery/DiffusionToolkit/assets/1910659/437781da-e905-412a-bbe6-e179f51ac020)

![Screenshot 2024-02-09 183625](https://github.com/RupertAvery/DiffusionToolkit/assets/1910659/20e57f5a-be4e-468f-9bfb-fe309ecfe5f1)


# FAQ

## How do I view my image's metadata (PNGInfo)?

With the Preview Pane visible, press I in the thumbnail view or with the Preview Pane in focus to show or hide the metadata.  You can also click the eye icon at the botton right of the Preview Pane.

## What is Rebuild Metadata and when should I use it?

Rebuild Metadata will rescan all your images and update the database with any new or updated metadata found. It doesn't affect your custom tags (rating, favorite, nsfw).

You only need to Rebuild Metadata if a new version of Diffusion Toolkit comes out with support for metadata that exists in your existing images.

## Can I move my images to a different folder?

I you want to move your images to a different folder, but still within a Diffusion folder, you should use the **right-click > Move** command. This allows Diffusion Toolkit to handle the moving of images, and know to keep all the Diffusion Toolkit metadata (Favorites, Rating, NSFW) intact while moving.

If you use Explorer or some other application to move the files, but still under the Diffusion folders, when you Rescan Folders or Rebuild Images Diffusion Toolkit will detect that the images have been removed, then will detect new files added. You will lose any Favorites, Ratings or other Toolkit-specific information. 

