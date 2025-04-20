# What's New in v1.9.0

There have been a lot of improvements in speeding up the application, especially around how images are scanned and how thumbnails are loaded and displayed.

A lot of functionality has been added to folders.  You can now set folders as **Archived**. Archived folders will be ignored when scanning for new files, or when rescanning. This will reduce disk churn and speed up scanning. see [More Folder functionality](#more-folder-functionality) for more details.

[External Applications](#external-applications) were added!

There has been some work done to support [moving files outside of Diffusion Toolkit](#moving-files-outside-of-diffusion-toolkit) and restoring image entries by matching hashes. On that note, you can actually drag images to folders to move them. That feature has been around for some time, and is a recommended over external movement, though it has its limitations.

A new [Compact View](#compact-view) has been added. This allows more portrait oriented images to be displayed on one line, with landscape pictures being displayed much larger.

[Filenames](#filename-visibility-and-renaming) and folders can now be displayed _and_ renamed from the thumbnail pane!

These were some important highlights, but a lot of features were added. Please take a close look so you don't miss anything.

* [Release Notes Viewer](#release-notes-viewer)
* [Improved first-time setup experience](#improved-first-time-setup-experience)
* [Settings](#settings)
* [Compact View](#compact-view)
* [FileName Visibility and Renaming](#filename-visibility-and-renaming)
* [File Deletion Changes](#file-deletion-changes)
* [Unavailable Images Scanning](#unavailable-images-scanning)
* [Tagging UI](#tagging-ui)
* [External Applications](#external-applications)
* [More Folder functionality](#more-folder-functionality)
* [High DPI Monitor Support](#high-dpi-monitor-support)
* [Persistent thumbnail caching](#persistent-thumbnail-caching)
* [Moving Files outside of Diffusion Toolkit](#moving-files-outside-of-diffusion-toolkit)
* [Show/Hide Notifications](#showhide-notifications)
* [Change Root Folder Path](#change-root-folder-path)
* [Search Help](#search-help)
* [Size Searching](#size-searching)
* [Sort by Last Viewed and Last Updated](#sort-by-last-viewed-and-last-updated)
* [Image Size Metadata](#image-size-metadata)
* [Others](#others)

## Release Notes Viewer

Never miss out on what's new! Release Notes will automatically show for new versions. After that you can go to **Help > Release Notes** to view them anytime.

You can also read the notes in Markdown format in the Release Notes folder.

## Improved first-time setup experience

First-time users will now see a wizard-style setup with limited options and more explanations. They should be (mostly) translated in the included languages, but I haven't been able to test if it starts in the user's system language.

## Settings

**Settings** has moved to a page instead of a separate Window dialog. 

One of the effects of this is you are now required to click **Apply Changes** at the top of the page to effect the changes in the application. This is especially important for changes to the folders, since folder changes will trigger a file scan, which may be blocked by an ongoing operation.

**IMPORTANT!** After you update, the **ImagePaths** and **ExcludePaths** settings in `config.json` will be moved into the database and will be ignored in the future (and may probably be deleted in the next update). This shouldn't be a problem, but just in case people might wonder why updating the path settings in JSON doesn't work anymore.

## Compact View

Thumbnails can now be displayed in **Compact View**, removing the spacing between icons and displaying them staggered in case the widths are not equal between icons.

The spacing between icons in Compact View can be controlled via a slider at the bottom of the Thumbnail Pane.

Switching between view modes can be done through **View > Compact** and **View > Classic**.

In Compact View, the positioning of thumbnails is dynamic and will depend on thumbnails being loaded in "above" the window. This will lead to keyboard navigation and selection being a bit awkward as the position changes during loading. 

## FileName Visibility and Renaming

You can now show or hide filenames in the thumbnail pane. Toggle the setting via **View > Show Filenames** or in the **Settings** page under the **Images** tab.

You can also rename files and folders within Diffusion Toolkit.  Press F2 with an image or folder selected, or right click > Rename.

## File Deletion Changes

Diffusion Toolkit can now delete files to the Windows Recycle Bin. This is enabled by default.

The Recycle Bin view has been renamed **Trash**, to avoid confusion with the Windows Recycle Bin.

Pressing `Shift+Delete` or `Shift+X` will bypass tagging the file For Deletion and send it directly to the Windows Recycle Bin, deleting the entry from the database and removing all metadata associated with it.

To delete the file permanently the way it worked before enable the setting **Permanently delete files (do not send to Recycle Bin)** in Settings, under the Images tab.

By default, you will be prompted for confirmation before deleting. You can change this with the settings **Ask for confirmation before deleting files**

## Unavailable Images Scanning

This has been available for some time, but needs some explaining.

Unavailable Folders are folders that cannot be reached when the application starts. This could be caused by bad network conditions for network folders, or removable drives. Unavailable images can also be caused by removing the images from a folder manually.

Previously, Scanning would perform a mandatory check if *each and every file existed* to make sure they were in the correct state. This can slow down scanning when you have several hundred thousand images.

Scanning will no longer check for unavailable images in order to speed up scanning and rebuilding metadata.

To scan for unavailable images, click **Tools > Scan for Unavailable images**. This will tag images as Unavailable, allowing you can hide them through the View menu. You can also restore images that were tagged as unavailable, or remove them from the database completely.

Unavailable root folders will still be verified on startup to check for removable drives. Clicking on the Refresh button when the drive has been reconnected will restore the unavailable root folder and all the images under it.

## Tagging UI

You can now tag images interactively by clicking on the stars displayed at the bottom of the Preview.  You can also tag as Favorite, For deletion and NSFW. If you don't want to see the Tagging UI, you can hide it by clicking on the **star icon** above the Preview or in the Settings under the Image tab.

To remove the rating on selected images you can now press the tilde button ~ on your keyboard.

## External Applications

You can now configure external applications to open selected images directly from the thumbnail or preview via right-click. To set this up, go to **Settings** and open the **External Applications** tab.

You can also launch external applications using the shortcut **Shift+**`<Key>`, where `<Key>` corresponds to the application's position in your configured list. The keys 1–9 and 0 are available, with 0 representing the 10th application. You can reorder the list to change shortcut assignments.

Multiple files can be selected and opened at once, as long as the external application supports receiving multiple files via the command line.

## More Folder functionality

A lot more functionality has been added to the Folders section in the Navigation Pane. If Watch Folders is enabled, newly created folders will appear in the list without needing to refresh. More context menu options have been added. Chevrons now properly indicate if a folder has children. Unavailable folders will be indicated with strikeout.

### Rescan Individual Folders

You can now rescan individual folders. To Rescan a folder, right click on it and click **Rescan**. The folder and all it's descendants will be rescanned. Archived folders will be ignored.

### Archive Folders

Archiving a folder excludes it from being scanned for new images during a rescan or rebuild, helping speed up the process.

To archive a folder, right-click on it and select **Archive** or **Archive Tree**. The Archive Tree option will archive the selected folder along with all of its subfolders, while Archive will archive only the selected folder.

You can also unarchive a folder at any time.

Archived folders are indicated by an opaque lock icon on the right. A solid white lock icon indicates that all the folders in the tree are Archived. A blue lock icon indicates that the folder is archived, but one or more of the folders in the tree are Unarchived. A transparent lock icon means the folder is Unarchived.

### Multi-select

Hold down Ctrl to select multiple folders to archive or rescan.

### Keyboard support

Folders now accept focus. You can now use they keyboard for basic folder navigattion. This is mostly experimental and added for convenience. 

## High DPI Monitor Support

DPI Awareness has been enabled. This might have caused issues for some users with blurry text and thumbnails, and the task completion notification popping up over the thumbnails, instead of the botton-right corner like it's supposed to.

## Persistent thumbnail caching

Diffusion Toolkit now creates a `dt_thumbnails.db` file in each directory containing indexed images the first time thumbnails are viewed. With thumbnails now saved to disk, they load significantly faster—even after restarting the application.

This reduces disk activity, which is especially helpful for users with disk-based storage. It's also great news for those working with large images, as thumbnails no longer need to be regenerated each time.

Thumbnails are stored at the size you've selected in your settings and will be updated if those settings change.

**Note:** Thumbnails are saved in JPG format within an unencrypted SQLite database and can be viewed using any SQLite browser.

## Moving Files outside of Diffusion Toolkit

Diffusion Toolkit can now track files moved outside the application.

For this to work, you will need to rescan your images to generate the file's SHA-256 hashes. This is a fingerprint of the file and uniquely identifies them. You can rescan images by right-clicking a selection of images and clicking Rescan, or right-clicking a non-archived folder and clicking Rescan.

You can then move the files outside of Diffusion Toolkit to another folder that is under a root folder. When you try to view the moved images in Diffusion Toolkit, they will be unavailable.

Once the files have been moved, rescanning the destination folder should locate the existing metadata and point them automatically to the new destination.

How it works:

When an image matching the hash of an existing image is scanned in, Diffusion Toolkit will check if the original image path is unavailable. If so, it will move the existing image to point to the new image path.

In the rare case you have duplicate unavailable images, Diffusion Toolkit will use the first one it sees.

Note that it's still recommended you move files inside Diffusion Toolkit. You can select files and drag them to a folder in the Folder Pane to move them.

## Show/Hide Notifications

You can now chose to disable the popup that shows how many images have been scanned. Click on the **bell icon** above the Preview or in the Settings under the General tab.

## Change Root Folder Path

You can now change the path of a root folder and all the images under it. This only changes the paths of the folders and images in the database and assumes that the images already exist in the target folder, otherwise they will be unavailable.

## Search Help

Query Syntax is a great way to quickly refine your search. You simply type your prompt query and add any additional parameter queries.

Click on the ? icon in the Query bar for more details on Query Syntax.

For example, to find all images containing cat and hat in the prompt, landscape orientation, created between 11/31/2024 and yesterday, you can query:

```
cat, hat size: landscape date: between 11/31/2024 and yesterday
```

NOTE: Dates are parsed according to system settings, so it should just work as expected, otherwise use YYYY/MM/DD

## Size Searching

The size query syntax now supports the following options:

**Pixel size** (*current*)

* `size: <width>x<height>` 
  
  `width` and `height` can be a number or a question mark (`?`) to match any value. e.g. `size:512x?` will match images with a width of `512` and any height.

**Ratio**

* `size: <width>:<height>` (e.g 16:9)

**Orientation**

* `size: <orientation>`

   `orientation` can be one of the following:

   * `landscape`
   * `portrait`
   * `square`

Options to filter on ratio and orientation have also been added to the Filter.

## Sort by Last Viewed and Last Updated 

Diffusion Toolkit tracks when you view an image. An image is counted as viewed when stay on an image for 2 seconds.

Diffusion Toolkit also tracks when you whenever you update a tag an image.

You can then sort images from the **Sort by** drop down with the new Last Updated and Last Viewed sort options.

## Image Size Metadata

Image size was previously read only from AI-generated metadata. Diffusion Toolkit will now read the width and height from the image format directly. You will need to rescan your images to update your metadata. This is mostly useful for non-AI-generated images or images with incorrect or missing width and height.

## Others

* **Copy Path** added to Context Menu
* Fixed crashing on for some users startup
* Toggle Switches added to top-right of window (above Preview)
  * Show/Hide notifications
  * Show/Hide Tagging UI
  * Advance on Tag toggle
