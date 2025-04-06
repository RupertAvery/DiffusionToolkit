# What's New in v1.6

## Important Changes

**Duplicate entries will be removed when you upgrade!**  This only applies to entries that have the exact same path, which can be caused by manually scanning while Diffusion Toolkit is still working on newly detected files (watched folders).  

**Images entries will no longer be automatically removed when the image is unreachable.** This change is primarily for users who work with removable storage. Previously, scanning would look for any missing images and remove them. With an offline drive, images would suddenly be removed upon starting Diffusion Toolkit.

**Images that cannot be loaded will display as "unavailable"**. This will also affect images that were moved or deleted outside the app. To avoid this, it's recommended to move or delete previously indexed images using Diffusion Toolkit.  To remove an image marked unavailable, select the image and right-click > Remove, or press CTRL-X or CTRL-Delete.

**Diffusion Toolkit is localized!** Translations are currently mostly ChatGPT-provided, so expect grammatical errors.  Some areas are not yet translated. Want to contribute to localization? See `Localization\README.md` 

**The Albums pane has been replaced with a general-purpose navigation / filter pane.**  If you find your albums missing, show it with View > Albums.

**A folder treeview has been added.** While this supports basic adding/removing a folder, some expected functions like being able to move folders is not currently implemented. 
You can drag images to folders to move them.

**Fit-to-preview no longer locks the zoom in the preview.**  Instead, it will remain on fit to preview until you middle-scroll to zoom or press CTRL - or +.

**The popout preview button has been moved to the title bar** next to the minimize button.  If you close the popout, you can show the preview in the app with the Toggle preview button next to the popout button.

## Enhancements

* "Modernized" toolbar
* Popout preview controls moved to title bar
* New navigation / filter pane
   * View > Folder / Model / Album
   * Folder tree view
      * Drag images to folder
      * Create / rename / delete folder
      * Image path metadata is updated with folder updates
   * Models list
      * List of models hashes/names from your images metadata
      * names from local or Civitai will be used when model name is missing from metadata
      * Image counts may not be exact since I search on model name OR model hash
   * Albums 
* Filter > Model name uses a drop down to allow selecting from all model names found in metadata. 
* Collapsible metadata (#179)
   * Right-click to collapse / expand all
* Diffusion Toolkit speaks your language! 
   * Most of the translations are ChatGPT-provided
   * Settings > Themes > Language
      * English 
      * German
      * French
      * Spanish
      * Japanese
   * Some text is still untranslated 
* Sort by Filename / Random (#182) 
* Improved zoom behavior
* Dragging onto albums / folders will highlight the folder now
* Added Sampler as a separate metadata section
* Input dialog now sets focus on textbox
* Smaller folder / album icons
* More responsive when loading large images

## Bugfixes

* Remember Preview/Metadata splitter position (not actually working in v1.5.1)
* The first image is automatically selected on search / refresh (#184)
* Fix broken NovelAI metadata reading introduced by FooocusMRE. (#175)

 Thanks to all contributors!



