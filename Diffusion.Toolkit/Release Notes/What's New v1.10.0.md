# What's New in v1.10.0

## Updated to .NET 10

Diffusion Toolkit is now compiled with .NET 10

Download and install the [.NET Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

## (Experimental) Video Support

This version adds support for indexing and previewing videos, and reading and indexing embedded ComfyUI workflows.

The ".mp4" extension will be added to **Settings > General > File Extensions**.  You can Scan New on any folders containing videos. Any new scans should include MP4 files.

A new setting has been added: **Settings > Video > Loop Video**. This controls whether the preview loops upon reaching the end of the video.

NOTE: Currently only ComfyUI workflows saved using the **VideoHelperSuite (VHS) - Video Combine** node are guaranteed to be read. If you have a ComfyUI video that has metadata that can't be read, 
please raise an issue and attach the sample file.

## Tags

You can now create custom tags and add them to images in the Metadata panel. Open the Tags tab in any Metadata panel, 
and begin by adding a Tag in the textbox at the bottom. Click the + button to add the tag to the list of available tags. 
Place a check next to the tag name to add the tag to the selected image.

You can also filter by one or more tags in the Tags filter panel on the left. Above the list of Tags you will see the options AND and OR. 
This determines how multiple selected tags will affect the filter.

You can add, rename and remove tags in the Tags filter panel.

The Tags filter should be visible. You can toggle it via **Menu > View > Tags**

## Others

* Add support for ComfyUI workflows in JPEG files
* Fixed errors reading Seed values larger than max INT
* Fixed issues with Remove folders in Folder panel


 



