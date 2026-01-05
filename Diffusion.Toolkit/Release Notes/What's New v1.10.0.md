# What's New in v1.10.0

## Updated to .NET 10

Diffusion Toolkit is now compiled with .NET 10

Download and install the [.NET Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

## (Experimental) Video Support

This version adds support for indexing and previewing videos, and reading and indexing embedded ComfyUI workflows.

The `.mp4`, `.webm` and `.mkv` extensions have been automatically added to **Settings > General > File Extensions**.  You can Scan New on any folders containing videos. Any new scans should include video files.

A new setting has been added: **Settings > Video > Loop Video**. This controls whether the preview loops upon reaching the end of the video.

**NOTE 1:** Currently only ComfyUI workflows saved using the **VideoHelperSuite (VHS) - Video Combine** node are guaranteed to be read. 

**NOTE 2:** WebM and MKV are not supported for playback in the preview, and it will display as blank. This is because the .NET WPF MediaElement does not support these formats.

If you have a ComfyUI video that has metadata that can't be read, please raise an issue on Github and attach the sample file

You can now filter by **Type** (Image, Video) using the Filter popup, or by adding the query `type: image` or `type: video`

## Custom Tags

You can now create custom tags and add them to media in the **Metadata** panel. Open the **Tags** tab in any Metadata panel, 
and begin by adding a Tag in the textbox at the bottom. Click the + button to add the tag to the list of available tags. 
Place a check next to the tag name to add the tag to the selected media.

If multiple media are selected, the tags will be applied to all the selected media.

You can also filter by one or more tags in the Tags filter panel on the left. Above the list of Tags you will see the options AND and OR. 
This determines how multiple selected tags will affect the filter.

You can add, rename and remove tags in the Tags filter panel.

The Tags filter should be visible. You can toggle it via **Menu > View > Tags**

## Others

* Add support for ComfyUI workflows in JPEG files
* Fixed errors reading Seed values larger than max INT
* Fixed issues with Remove folders in Folder panel
  * You can now remove root folders directly from the panel
  * You can remove unavailable root folders from the panel
* Added some useful buttons to the Folders panel header
* **Rebuild Thumbnail** command added to thumbnail pane context menu
  * You can now regenerate cached thumbnails. This is useful if you replace a file with another one with the same filename and Diffusion Toolkit still shows the old thumbnail. 
  

 



