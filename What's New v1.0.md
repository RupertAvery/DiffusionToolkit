
# What's New in v1.0

* Sort images by
   * Date Created
   * Aesthetic Score
   * Rating   
   * Ascending / Descending
* Folders
   * Browse from your Diffusion folders
   * Only indexed images will be displayed
* Albums
   * Add images to one or more Albums
   * Create Album
   * Rename Albums
   * Right-click a thumbnail then Add to Album 
      * Add to new Album
      * Add to existing Albums
   * Drag and drop a selection of images onto an Album in the Album panel on the left of the Thumbnail View.
* Using **Right-click > Move** to move a file outside the Diffusion folders will now stop tracking the images, removing them from the database.
* Pressing **CTRL+C** with one or more thumbnails selected will copy the files to the clipboard.
* Copy parameter context menu items have been moved to **Right-click > Copy Parameters**. This allows you to press **Right-click C, S** to copy the Seed, for example.
* Seed can now be queried with wildcards.
   * `seed: 123*` will show all images have a seed that starts with `123`
   * `seed: 123456???000` will show all images have a seed that starts with `123456`, matches any 3 digits, and ends with `000`
* Support for ComfyUI metadata
   * Experimental, as I'm not completely sure about the format.
* Less files! All .NET assemblies are now packaged into a single executable.

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.0
