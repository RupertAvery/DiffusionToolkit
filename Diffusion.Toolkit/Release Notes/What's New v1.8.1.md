# What's New in v1.8.1

## High DPI Monitor Support

DPI Awareness has been enabled. This might have caused issues for some users with blurry text and thumbnails, and the task completion notification popping up over the thumbnails, instead of the botton-right corner like it's supposed to.  I never experienced it until I reinstalled Windows. Sorry guys.

## Persistent thumbnail caching

Diffusion Toolkit now creates a `dt_thumbnails.db` file in every directory where an indexed image is located, the first time the image is loaded for generating thumbnails. Since thumbails are persisted, your thumbnails will now load a LOT faster, even after closing and reopening Diffusion Toolkit.

This means less churn on your hard disk! This is also great new for people with larger images as they won't have to regenerate the thumbnails every time as before.

Note: Thumbnails are stored unencrypted in a SQLite database in JPG format, and can be viewed by anyone with a SQLite browser.

## Change Folder Path

You can now change the path of a root folder and all the images under it. This only changes the paths of the folders and images. This assumes that  the images exist in the target folder, otherwise they will be unavailable.

Currently there is no built-in method to move a folder and all it's images as I will need to add logic to handle failures and retries, especially when there are a lot of images that need to be moved.

* Rescan individual folders
   * Right-Click Folder > Rescan
* Persistent thumbnail caching
* Improved first-time setup experience
* Added External Applications
* Copy Path from Context Menu
* Fixed crashing on when scanning on startup
* Moved Settings to its own page
* Better handling during shutdown
* Scanning will no longer check for unavailable files
   * To scan for unavailable files, use the option in the Tools menu
   * Unavailable folders will still be checked on startup
* 


TODO: 

Handle Folder unavailable when search results (in database, not on disk)
  * Clear results
  * Handle gracefully
Search after Rescan
Fix Cancel
Fix Watched scanned files has error
Scanning message stuck at < 100%
Implement Save As - DONE
Fix NOT filtering not being saved to Query - DONE?
Test changes to delete image stuff
find delete marked implementation and others
Handle Recursive option - how to handle images?
