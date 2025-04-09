# What's New in v1.9.0

There have been a lot of improvements in speeding up the application. Thumbnails in particular will load much faster, as they are now cached to disk (see [Persistent thumbnail caching](#persistent-thumbnail-caching)). Thumbnail loading, changing pages is now so quick, it's starting to actually feel like a real application and not something I work on when I'm supposed to be working on actual work.

There have been subtle **improvements to scanning images** (aside from the ability to Archive folders to skip them entirely). A lot of work has been done to make scanning asynchronous and pipelined.

Images dropped into any watched folders will now be scanned immediately instead of when the copying completes. You can also now drop files into watched folders while rescan is ongoing.

Scans may also complete a bit faster, as now the process is pipelined using Channels, reading the metadata in multiple threads while writing to the database in a separate thread.

While this has been tested through daily use, please let me know if you experience any issues with scanning.

**High DPI Monitor Support** has been added. Sort of. At least it should fix text and image blurriness on some multi-monitor setups, and if anyone experienced the notification popup appearing over the thumbnails, this was probably the cause.

With that out of the way, lets look at some more improvements in detail.

## Improved first-time setup experience

First-time users will now see a wizard-style setup with limited options and more explanations. They should be (mostly) translated in the included languages, but I haven't been able to test if it starts in the user's system language.

## Settings

**Settings** has moved to a page instead of a separate Window dialog. 

One of the effects of this is you are now required to click **Apply Changes** at the top of the page to effect the changes in the application. This is especially important for changes to the folders, since folder changes will trigger a file scan, which may be blocked by an ongoing operation.

**IMPORTANT!** After you update, the **ImagePaths** and **ExcludePaths** settings in `config.json` will be moved into the database and will be ignored in the future (and may probably be deleted in the next update). This shouldn't be a problem, but just in case people might wonder why updating the path settings in JSON doesn't work anymore.

## Unavailable Images Scanning

This has been available for some time, but needs some explaining.

Unavailable Folders are folders that cannot be reached when the application starts. This could be caused by bad network conditions for network folders, or removable drives. Unavailable images can also be caused by removing the images from a folder manually.

Previously, Scanning would perform a mandatory check if *each and every file existed* to make sure they were in the correct state. This can slow down scanning when you have several hundred thousand images.

Scanning will no longer check for unavailable images in order to speed up scanning and rebuilding metadata.

To scan for unavailable images, click **Tools > Scan for Unavailable images**. This will tag images as Unavailable, allowing you can hide them through the View menu. You can also restore images that were tagged as unavailable, or remove them from the database completely.

Unavailable root folders will still be verified on startup to check for removable drives. Clicking on the Refresh button when the drive has been reconnected will restore the unavailable root folder and all the images under it.

## Click-to-Rate

Some users have asked for the ability to click on the stars to set the Rating. This has now been implemented.

to remove the rating on selected images you can now press the tilde button ~ on your keyboard. 

## External Applications

You can now add External applications to pass selected images to when you right-click the thumbnails or the preview. To edit External applications, go to Settings, then click the **External Applications** tab.

External epplications can be launched using the shortcut **Shift+**`<Number>` where Number is the external application index in the order it was configured. 

You can select multiple files and open them in your external application if it supports being passed multiple files in the command line.

## More Folder functionality

A lot more functionality has been added to the Folders section in the Navigation Pane.

### Rescan Individual Folders

You can now rescan individual folders. To Rescan a folder, right click on it and click **Rescan**

### Archive Folders

Archiving a folder excludes it from being scanned for new images during a rescan or rebuild, helping speed up the process.

To archive a folder, right-click on it and select **Archive** or **Archive Tree**. The Archive Tree option will archive the selected folder along with all of its subfolders, while Archive will archive only the selected folder.

You can also unarchive a folder at any time.

Archived folders are indicated by an opaque lock icon on the right.

### Multi-select

Hold down Ctrl to select multiple folders to archive or rescan.

## High DPI Monitor Support

DPI Awareness has been enabled. This might have caused issues for some users with blurry text and thumbnails, and the task completion notification popping up over the thumbnails, instead of the botton-right corner like it's supposed to.  I never experienced it until I reinstalled Windows. Sorry guys.

## Persistent thumbnail caching

Diffusion Toolkit now creates a `dt_thumbnails.db` file in each directory containing indexed images the first time thumbnails are viewed. With thumbnails now saved to disk, they load significantly faster—even after restarting the application.

This reduces disk activity, which is especially helpful for users with disk-based storage. It's also great news for those working with large images, as thumbnails no longer need to be regenerated each time.

Thumbnails are stored at the size you've selected in your settings and will be updated if those settings change.

**Note:** Thumbnails are saved in JPG format within an unencrypted SQLite database and can be viewed using any SQLite browser.

## Change Root Folder Path

You can now change the path of a root folder and all the images under it. This only changes the paths of the folders and images in the database and assumes that the images already exist in the target folder, otherwise they will be unavailable.

## Others

* **Copy Path** from Context Menu
* Fixed crashing on when scanning on startup
* Better handling during shutdown

TODO: 

Click to unset rating in StarRating
Tilde to unset

UnavailableConverter crashing designer in Search.xaml?

* Rescan selected (Channel has been closed) when 1 or a few images are selected
  Race condition?
* Fix selection on page change 
* Check if Rebuild Metadata uses Archived folders
* Handle Folder unavailable when search results (in database, not on disk)
  * Clear results
  * Handle gracefully
* Search after Rescan
* Fix Cancel
* Fix Watched scanned files has error
* Scanning message stuck at < 100%
* Implement Save As - DONE
* Fix NOT filtering not being saved to Query - DONE?
* Test changes to delete image stuff
* find delete marked implementation and others
* Handle Recursive option - how to handle images?

* Broken Scroll / key auto page in preview and popout
* CheckUnavailableFolders conflicts with some other writes
* Make sure Folder Refresh will restore unavailable folders AND all the images under them
  * calls CheckUnavailableFolders DONE

* Add copy path to Preview
* Add external applications to Preview
* Broken Keydown / auto-page in thumbnail - FIXED
