# What's New in v1.7.0

After several months without any updates, here's a few that include some features users have suggested or requested. 

Just to name a few, improved handling of unavailable images, with a option to scan for them in Tools. A ComfyUI metadata viewer was added. The nodes are simply displayed in a list, without any layout or wiring. Auto advance on tag and mouse wheel navigation were added. A slideshow function was added to the popout preview. Filters were for excluding terms in the Prompt / Negative Prompt. Now you can properly search for "cape", and exclude "landscape", or "superhero" from the search results.

Please make sure to check each one below to make sure you don't miss out on anything, there are a lot of changes and some might be what you are looking for.

Thanks to Terence for a whole bunch of bug fixes and enhancements!

## Enhancements

* Added VERY basic ComfyUI support
  * Gets the Prompt from the first **CLIPTextEncoder** node
  * Use **Rebuild metadata** to scan in the Prompt for existing images to make them searchable
  * Future plan is to allow user to custom map node values to fields using the metadata viewer
  * Workflow "fingerprints" are tracked, so in the future, if you update the mapping, you will be able to re-run a metadata scan on only those images with the same workflow
* Added a simple **ComfyUI** metadata viewer in the Metadata pane
  * Click the **Workflow** tab next to the **Metadata** tab anywhere the Metadata pane is visible (overlay/preview overlay/metadata pane under preview)
* Refresh search results behavior has changed
   * Press F5 or click the search button in the search bar to **refresh the current page**
   * Makes it easier to reload the current page after marking for deletion, and Hide images for deletion is enabled
   * Hold CTRL and press F5 or click the the search button to **refresh and return to the first page**
* New **Unavailable** tag added
   * This is an internally managed tag that is updated:
      * when the metadata scanner checks existing files on startup
      * when a rescan is performed (click Scan folders for new images or CTRL-R)
      * when manually checked through **Tools > Scan for Unavailable images**
   * Available images will be restored when any of the above is performed
   * The tag can be filtered on (see Added filters below)
* You can now hide images tagged **For Deletion** from search results 
* You can also hide images tagged **Unavailable** from search results (See View menuitem)
* **Auto-advance on tag** option added
   * When enabled, after tagging with a Rating, Favorite, NSFW or Delete, the next image will be selected 
   * Affects Thumbnail View and Popout Preview
   * Go to **View > Auto Advance on Tag** to toggle or CTRL+Shift+T in the Thumbnail View or Popout Preview
* More prominent display of tagged For Deletion status
* **Mouse wheel navigation** added to Preview
   * When enabled, the scrolling mouse wheel will allow you to move between previous/next images
   * When enabled, holding CTRL will perform a zoom
   * Affects Preview and Popout Preview
   * Go to **Settings > Image > Use mouse wheel...** to toggle
* Double-click the preview pane to open the image in your selected viewer (i.e. the Popout preview, or whatever you have configured)
* Added a **Slideshow** function in the Popout Preview window. 
   * Press **Space** to start/stop the slideshow
   * Go to **Settings > Image > Slide show delay** to adjust the time between image changes
* Improved thumbnail loading efficiency  
   * Only thumbnails in current view are loaded
   * Should have slightly better responsiveness when scrolling quickly / scrolling to bottom
* Add **Actual Size** option in Preview
   * Press CTRL+Shift+A to toggle
* Added filters: (Click the Filter button or press CTRL-F)    
   * **Exclude Prompt / Negative** - show images that do not include the specified prompt / negative terms 
   * **Aesthetic Score - No Score** - show images without an Aesthetic Score
   * **In Album** - show images are have been added to an album or not
   * **Unavailable** - show images that have been tagges as Unavailable
* Add support for **Stable Swarm** metadata
* Add support for **Fooocus** metadata
* Albums
   * All albums will be shown on right-click > Add to Albums (not just last 10)


## Bugfixes

* Additional indexes to fix query slowdowns
* Fixed bug where Preview Pane loses focus when changing pages
* Move txt files when moving images #239
* Fix issue with unsetting rating of selected images in thumbnail view #237
* Fixed some startup crashes
* Several bug fixes