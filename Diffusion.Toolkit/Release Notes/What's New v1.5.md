# What's New in v1.5

* Metadata Viewer changes
  * Metadata is now always shown below the preview
  * In the popout/fullscreen, it it shown to the right
  * Added separate sections for Seed, CFG, Steps and Size after Negative Prompt
* Fixes to keyboard navigation
  * Fixed navigation issues when using Page up/Page down
  * Fixed multi-selection issues with CTRL
* Support for **RuinedFooocus** metadata
* Folders
  * Images might be missing in folders after a move.  
  * Run **Tools > Folders > Fix missing images** if you don't see indexed images in folder view.  
* Excluded Folders
   * Adding an excluded folder will now remove any indexed images that are under the excluded folder
   * Run **Tools > Folders > Remove excluded images** to remove them manually 
   * To re-add previously excluded images, perform a Scan.
* Albums enhancements and fixes
   * Implemented a cleaner album list with image counts
   * Indicator in thumbnail view shows if an image is already in an album
   * Images were not being removed from the Album-Image tracking table when removing an album
   resulting in orphaned entries. The app will run a cleanup on each run. This is fast and should be unnoticeable.
   * Fixed scrolling for Album list
   * Click on an album in metadata list to open the album in the thumbnail view.
* Filter UI enhancements
    * Press CTRL+F to open the filter
    * Now displays as a popup instead of fully covering the thumbnail view

## Bugfixes

* Add "Core ML" to CivitAI model formats
* Loading a large image should no longer block the UI
* Folder not updating after move (#152)
* Crash opening app: "An unhandled exception occured: 'N' is an invalid start of a value  (#155) (thanks to kwaegel)
* DiffusionToolkit not reading separately stored aesthetic score tags from A1111 files in certain circumstances (#156) (thanks to curiousjp)

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.5
