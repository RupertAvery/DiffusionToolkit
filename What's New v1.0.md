# Diffusion Toolkit Release v1.0

Diffusion Toolkit is an image metadata-indexer and viewer for AI-generated images.  It scans your 50,000 image collection in seconds and lets you search them by prompt, seed, hash and more.

You can also tag your images as favorites, rate them 1-10, sort them by aesthetic score, mark them as NSFW, blur images marked as NSFW, and auto-tag NSFW images by looking at keywords in the prompt.

You can arrange them in albums, view them by folder, see your most used prompts.

Diffusion Toolkit supports

* JPG/JPEG + EXIF
* PNG
* WebP
* .TXT metadata

Metadata formats supported are:

* AUTOMATIC1111
* InvokeAI
* NovelAI
* Stable Diffusion
* ComfyUI (Experimental)

How to get it:

* [Download](https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.0
) (Windows)
* Requires [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 
* [Usage](https://github.com/RupertAvery/DiffusionToolkit/blob/master/Diffusion.Toolkit/Tips.md)

# What's New in v1.0

* Less files! All .NET assemblies are now packaged into a single executable. If you're coming from an older version, you may want to start anew and remove all the old files, as the updater doesn't remove existing files.
* Portable mode. Diffusion Toolkit can run using settings and database in your executable folder.
   * Check the **Portable mode** checkbox in settings to move your `config.json` and `diffusion-toolkit.db` to your executable folder.  You can also do it manually.
* You can now **sort** images by
   * Date Created
   * Aesthetic Score
   * Rating   
   * Ascending / Descending
* **Folders** feature has been added.
   * Browse from your Diffusion folders
   * Only indexed images will be displayed
   * **If coming from a pre-1.0 version of Diffusion Toolkit, you must Rebuild Images to update the folder information.** 
* **Albums** feature has been added. You can
   * Add images to one or more Albums
   * Create an Album
   * Rename Albums
   * Right-click a thumbnail then Add to Album 
      * Add to new Album
      * Add to existing Albums
   * Drag-and-drop a selection of images onto an Album in the Album panel on the left of the Thumbnail View.
* Using **Right-click > Move** to move a file outside the Diffusion folders will now stop tracking the images, removing them from the database.
* Pressing **CTRL+C** with one or more images selected in the thumbnail view will copy the files to the clipboard.
* Copy parameter context menu items (Copy seed, Copy prompt, etc,) have been moved to **Right-click > Copy Parameters**. This allows you to press **Right-click C, S** to copy the Seed, for example.
* Path queries no longer uses GLOB as it is case-sensitive.
* A new Folder query now lets you search in a specific folder, not including subfolders.
* Seed can now be queried with wildcards.
   * `seed: 123*` will show all images have a seed that starts with `123`
   * `seed: 123456???000` will show all images have a seed that starts with `123456`, matches any 3 digits, and ends with `000`
* Support for **ComfyUI** metadata
   * Experimental, as I'm not completely sure about the format.


https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.0
