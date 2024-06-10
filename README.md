# Diffusion Toolkit

Diffusion Toolkit is an image metadata-indexer and viewer for AI-generated images. It aims to help you organize, search and sort your ever-growing collection.

# Usage

Usage should be pretty straightforward, but there are a lot of tips and tricks and shortcuts you can learn. See the documentation for [Getting Started](https://github.com/RupertAvery/DiffusionToolkit/tree/master/Diffusion.Toolkit/Tips.md)

Thanks to Bill Meeks for putting together a demonstration video.

[![Organize your AI Images](https://img.youtube.com/vi/r7J3n1LjojE/hqdefault.jpg)](https://www.youtube.com/watch?v=r7J3n1LjojE&ab_channel=BillMeeks)

# Installation 

* Currently runs on Windows only 
* [Download](https://github.com/RupertAvery/DiffusionToolkit/releases/latest
) the latest release 
    * Look for **> Assets** under the latest release, expand it, then grab the zip file **Diffusion.Toolkit.v1.x.zip**.
* Unzip all the files to a folder
* You may need to install the [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) if you haven't already

## Linux / OSX

* You can build the AvaloniaUI branch (see below)

# Build from source

## Windows

### Requirements

* Visual Studio 2022 (Make sure to install .NET desktop development workload)
* or [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) (includes the desktop runtime)

*NOTE: Visual Studio 2022 is only required if you want to work on WPF. If you only want to build from scratch, you only need the .NET 6 SDK.*

### Building

* Clone this repository
* Run `publish.cmd`

A folder named `build` will be created, containing all the necessary files.

## Linux 

### Requirements

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (See [Install .NET on Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux))
* git

### Building

* Clone this repository
* Run `git checkout AvaloniaUI`
* Run `chmod +x build-linux.sh && ./build-linux.sh`
* The executable `DiffusionToolkit.AvaloniaApp` will be created in `build/linux-x64`

### OSX

### Requirements

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* git

### Building

* Clone this repository
* Run `git checkout AvaloniaUI`
* Run `chmod +x build-osx.sh && ./build-osx.sh`
* The application `Diffusion Toolkit.app` will be created in `build/osx-x64`

You will need to unblock the app the first time you run it. To do this, run the app for the first time. It will be blocked. Click OK.

* Open **System Settings**.
* Click **Privacy & Security**, scroll down, and click the **Open Anyway** button to confirm your intent to open or install the app.



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
* SDNext
* EasyDiffusion
* RuinedFooocus
* FooocusMRE
* ComfyUI + SDXL (Work in progress) 

You can even use it on images without metadata and still use the other features such as rating and albums!

# Screenshots

![Screenshot 2024-02-09 183808](https://github.com/RupertAvery/DiffusionToolkit/assets/1910659/437781da-e905-412a-bbe6-e179f51ac020)

![Screenshot 2024-02-09 183625](https://github.com/RupertAvery/DiffusionToolkit/assets/1910659/20e57f5a-be4e-468f-9bfb-fe309ecfe5f1)

# Keyboard Shortcuts

Make sure the thumbnail view or the preview pane has the focus if the shortcuts don't work.

| Shortcut       | Action         | Notes  |
|----------------|----------------|--------|
| `1..9, 0` | Rate 1 - 10 | In thumbnail or preview
| `I` | Show/Hide (PNG)Info | In thumbnail or preview 
| `F` | Tag Favorite | In thumbnail or preview 
| `N` | Tag NSFW | In thumbnail or preview
| `B` | Toggle Blur NSFW | In thumbnail or preview
| `X/Del` | Tag for Deletion | In thumbnail or preview
| `Ctrl+X/Del` | Remove Entry | Remove image from database, but do not delete
| `Ctrl+C` | Copy File to Clipboard |
| `F3` | Show/Hide Navigation Pane |
| `F4` | Show/Hide Preview |
| `Ctrl+Shift+F` | Toggle Fit to Preview |
| `Ctrl+1` | Folders View |
| `Ctrl+2` | Album View |
| `Ctrl+3` | Diffusions View |
| `Ctrl+4` | Favorites Page |
| `Ctrl+5` | Recycle Bin |
| `Ctrl+6` | Prompts View |
| `Ctrl+0` | Reset Zoom | in Preview Pane or Image Viewer
| `Ctrl+R` | Scan folders for new images |
| `Alt+Home` | First Page |
| `Alt+PageUp` | Previous Page |   
| `Alt+PageDown` | Next Page |
| `Alt+End` | Last Page |
| `F6` | Set focus on search bar |


# FAQ

## How do I view my image's metadata (PNGInfo)?

With the Preview Pane visible, press I in the thumbnail view or with the Preview Pane in focus to show or hide the metadata.  You can also click the eye icon at the botton right of the Preview Pane.

## What is Rebuild Metadata and when should I use it?

Rebuild Metadata will rescan all your images and update the database with any new or updated metadata found. It doesn't affect your custom tags (rating, favorite, nsfw).

You only need to Rebuild Metadata if a new version of Diffusion Toolkit comes out with support for metadata that exists in your existing images.

## Can I move my images to a different folder?

I you want to move your images to a different folder, but still within a Diffusion folder, you should use the **right-click > Move** command. This allows Diffusion Toolkit to handle the moving of images, and know to keep all the Diffusion Toolkit metadata (Favorites, Rating, NSFW) intact while moving.

If you use Explorer or some other application to move the files, but still under the Diffusion folders, when you Rescan Folders or Rebuild Images Diffusion Toolkit will detect that the images have been removed, then will detect new files added. You will lose any Favorites, Ratings or other Toolkit-specific information. 


# Buy me a coffee

I've been told that people should be allowed to [buy me a coffee](https://www.buymeacoffee.com/rupertavery)

Beer works too (PayPal)

* [I'm feeling generous](https://www.paypal.me/rupertavery/25.00?locale.x=en_US)
* [Buy me a 2 craft beers](https://www.paypal.me/rupertavery/10.00?locale.x=en_US)
* [Buy me a craft beer](https://www.paypal.me/rupertavery/5.00?locale.x=en_US)
* [Buy me 2 local beers](https://www.paypal.me/rupertavery/3.00?locale.x=en_US)
