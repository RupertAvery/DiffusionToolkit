# What's New in v1.4

* Image viewer features and improvements!
   * Full-screen view 
      * Covers taskbar and hides maximize/minimize buttons
      * Press ESC to close
      * Press F11 to switch between full-screen/windowed mode
   * Use system viewer
   * Use custom viewer
   * see **Settings > Images** tab
* Prompt viewer improvements!
   * Selecting a prompt in Prompt view will show the images used by that prompt.  You can add them to albums, tag them, but not view (for now).
   * No thumbnails for Negative Prompts yet.
   * Fixes to prompt queries, counts should be more accurate now
   * Listed Prompts are affected by Show/Hide NSFW 
* Albums Improvements
   * Menu item added for albums
   * You can now sort Albums! See Albums > Sort
      * Name
      * Date
      * Custom
   * You can now see the albums that an image is part of in the metadata info (I).
* Some ComfyUI parsing fixes
   * Errors shouldn't prevent images from loading, but the metadata will be empty
* Civitai integration
   * Use CivitAI model database to get model names from the hash
      * Edit > Download Civitai models (saved to `models.json`)
      * Model names will be lookedup from Civitai model list in addition   to `cache.json` and local models
   * Open the CivitAI model page from PNGInfo Model Hash 
      * Show Metadata info > Model Hash > Click Search
* Tools improvements
   * Add search results to album
* Display the last selected image when selecting multiple images (#143)
* Added reload search (#136) and clear search buttons next to query text field. Buttons are placed on the right.
* Moved Filter button to the right
* Fix openclibboard failed (0x800401D0) (#144)
* Preview - Mousewheel Zoom without CTRL (#142)
* Thinner Scrollbars
* Various bug fixes

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.4
