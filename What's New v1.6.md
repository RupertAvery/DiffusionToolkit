# What's New in v1.6

The Albums pane has been replaced with a general-purpose navigation / filter pane.  If you find your albums missing, show it with View > Albums.

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

Want to contribute to localization? See `Localization\default.json` (this is provided for reference, updating it will not update the default text).

Or, fork the repo and create a PR for translation additions or updates.


