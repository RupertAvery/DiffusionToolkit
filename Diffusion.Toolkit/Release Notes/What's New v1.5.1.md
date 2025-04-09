# What's New in v1.5.1

* Faster paging
* Load thumbnails on demand instead of everything up front
* **Auto Refresh** for watched folders
   * Watched folders will automatically add newly generated images to the database but does not refresh results in case you are in the middle of something. Auto Refresh will refresh the current query/filter when new images are added. 
   * Enable in **Settings > General tab > Diffusion Folders > Auto Refresh** or **View > Auto Refresh**
* Remember Preview/Metadata splitter position
* Remember screen position on startup
   * DiffusionToolkit will open on the monitor it was when it closed on multi-monitor setups

## Bugfixes

* Remove images from metadata when folder is removed (#170)
    * Removing a folder after scanning it currently leaves the images in the database.
    * This version will now remove images when the folder is removed.
      * A backup of your database will be created automatically before removal.
    * You can manually cleanup leftover images with **Tools > Folders > Clean Removed Folders**
* Fixed A1111 generated images not triggering watched folders
    * Some time ago A1111 stopped triggering the watched folder when a new image was generated
    * This seems to be caused by A1111 first generating a .tmp file before renaming it to the final filename
* Fixed missing highlight on mouseover on menu items and context menus
* Fixed theme issues - metadata not visible in light mode (#166)
* Fixed ModelName not appearing (introduced in v1.5)
* Reduce duplicate model names appearing
* Fixed issues related to Album image counts (#165)
    * Removed images were left in the Album-Image reference table
    * This version will now correctly remove the references to images when they are deleted
    * It will also cleanup the reference table to correct the counts

**NOTE:** This version makes changes to the database schema. On startup, a copy of your database will be created before changes are made. 

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.5.1
