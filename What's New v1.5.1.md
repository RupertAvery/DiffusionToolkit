# What's New in v1.5.1

* **Auto Refresh** for watched folders
   * Watched folders will automatically add newly generated images to the database but does not refresh results in case you are in the middle of something. Auto Refresh will refresh the current query/filter when new images are added. 
   * Enable in **Settings > General tab > Diffusion Folders > Auto Refresh** or **View > Auto Refresh**
* Remember Preview/Metadata splitter position
* Remember screen position on startup
   * DiffusionToolkit will open on the monitor it was when it closed on multi-monitor setups

## Bugfixes

* Remove images from metadata when folder is removed (#170)
    * Manually cleanup images with **Tools > Folders > Clean Removed Folders**
* Fixed A1111 generated images not triggering watched folders
* Fixed missing highlight on mouseover on menu items and context menus.
* Fixed theme issues - metadata not visible in light mode (#166)
* Fixed ModelName not appearing (introduced in v1.5)
* Reduce duplicate model names appearing
* Fixed several issues related to Album image counts (#165)

**NOTE:** This version makes changes to the database. On startup, a copy of your database will be created before changes are made. 

https://github.com/RupertAvery/DiffusionToolkit/releases/tag/v1.5.1
