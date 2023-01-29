
# What's New in v0.9

* Rescan folders renamed to **Scan folders for new images** (icon also changed)
* Rebuild images button moved to **Edit > Rebuild** Metadata
* Moved Checkpoint settings to Checkpoint tab.
* Support WebP image formats
   * Add ".webp" to Settings > General > File Extensions
* Added ability to filter models by name and hash in the Models page
* Support for AUTOMATIC1111's new hash method (SHA256)
   * To use the new hash, you must set the path to the A1111 cache (cache.json) in the Checkpoints tab under Settings
   * Use **Edit > Reload models** menu item to reload the cache.json after A1111 loads a model and calculates a it's hash.
* Added a **Database** tab in Settings with options to allow you to backup or restore a database.
   * Do not backup or restore if scanning or rebuilding, or some database operation is in progress!
* New **Tools** menu  
   * Mark all matching files for Deletion and Unmark all matching files for Deletion lets you use the results of a query to quickly tag several hundreds of images for deletion.
   * Remove all matching files from the database lets you remove images without deleting them, similar to CTRL-X or CTRL-Del.
   * Auto-tag NSFW
	   * To use, go to **NSFW** tab under Settings and create a list of tokens that will be searched in the prompt of newly added images.
      * Check **Auto Tag NSFW**
	   * You may also update all existing images with Tools > Auto tag NSFW
* Drag-and-drop into preview
   * You can drop any image, including images outside your set folders, into the preview pane to view the image and PNGInfo.
* Added Search GUI
   * Press the sliders icon next to the Search textbox / dropdown. 
   * Easier to search on specific parameters
   * Supports searching by model file name (requires Model root to be set). 
       * This does not use the "Model Name" PNGInfo parameter (That parameter is not indexed). 
       * It uses the list of models to do a "reverse" hash search
       * You can search with partial names, like "protogen".
* Preview Pane context menu added
   * Right-click > Fit to Preview

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/beta_v0.9
