
# What's New in v0.9

* Rescan folders renamed to Scan folders for new images (icon also changed)
* Rebuild images button moved to Edit > Rebuild Metadata
* Support WebP image formats
   * Add ".webp" to Settings > General > File Extensions
* Added search in the Models page
* Support for AUTOMATIC1111's new hash method (SHA256)
   * To use the new hash, you must set the path to the A1111 cache (cache.json) in the Checkpoints tab under Settings
   * Edit > Reload models menu item added to allow you to refresh the cache.json after A1111 calculates a new hash.
* Auto-tag NSFW images
   * To use, create a list of tokens that will be searched in the prompt of newly added images in NSFW tab under Settings
   * You may also update all existing images with Tools > Auto tag NSFW
* Settings Database tab added with options to allow you to backup or restore a database.
   * Don't backup or restore if an operation is in progress!
* New Tools menu  
   * Mark all matching files for Deletion and Unmark all matching files for Deletion lets you use the results of a query to quickly tag several hundreds of images for deletion.
   * Remove all matching files from the database lets you remove images without deleting them, similar to CTRL-X or CTRL-Del.
   * Auto-tag NSFW
* Drag-and-drop into preview
   * You can drop any image, including images outside your set folders, into the preview pane to view the PNGInfo.
   

   

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/beta_v0.9
