# Table of Contents

* [Diffusion Toolkit](#diffusion-toolkit)
* [Features](#features)
* [Getting Started](#getting-started)
* [Updating your images](#updating-your-images)
   * [Rescan Folders](#rescan-folders)
   * [Watch Folders](#watch-folders)
   * [Rebuild Images](#rebuild-images)
* [Usage](#usage)
* [Preview Pane](#preview-pane)
   * [Zoom](#zoom)
   * [Displaying Metadata (PNGInfo)](#displaying-metadata-pnginfo)
   * [Popout](#popout)
* [Image Viewer](#image-viewer)
* [Folders](#folders)
* [Albums](#albums)
* [Drag and Drop](#drag-and-drop)
* [Tagging Images](#tagging-images)
   * [Favorite](#favorite)
   * [Rating](#rating)
   * [NSFW](#nsfw-tag)
   * [Deleting](#deleting)
* [Managing Images](#managing-images)
   * [Moving Images](#moving-images)
   * [Removing Entries](#removing-entries)
* [Searching](#searching)
* [Advanced Searching](#advanced-searching)
* [Supported Parameters](#supported-parameters)
* [Notes About Search](#notes-about-search)
* [Searching on Multiple Values](#searching-on-multiple-values)
* [Prompts View](#prompts-view)
* [Keyboard Shortcuts](#keyboard-shortcuts)
* [FAQ](#faq)

# Diffusion Toolkit

Diffusion Toolkit is an Image viewer and metadata (prompt) searcher for your AI generated images.

AI image generators embed the parameters used to generate that image in the image as metadata, or store it in a separate text file.

Diffusion Toolkit scans your AI-Generated images for metadata and stores it in a local database for fast retrieval and searching.

# Features

* Scan thousands of images metadata in seconds
* Supported image/metadata formats:
   * JPG/JPEG + EXIF
   * PNG
   * WebP
   * Any image + .TXT (part of filename must match)
* Search through prompt, seed, model hash / name, CFG, etc.
* Supported metadata formats: 
   * AUTOMATIC1111
   * InvokeAI
   * NovelAI
   * Stable Diffusion
   * ComfyUI
* Tag your images with a press of a key:
   * Favorites
   * Rate 1-10 
   * NSFW
* Drag and Drop
   * drag from thumbnail to Explorer to copy file
   * drag from thumbnail to A1111 PNGInfo to copy metadata
   * drag from thumbnail to Imgur/Google drive/web site to upload
   * drag from thumbnail to Photoshop or any image editor to load
   * drag from Explorer to Preview Pane to view any image metadata
* Auto-tag NSFW
   * Set a list of tokens that will be used to detect if an image is NSFW
   * Auto-tag on scan, or auto-tag existing images.
* Folders and Albums
   * Add images to one or more albums to manage your images
   * Browse from your selected folders if you prefer

# Getting Started

On first load, you will be shown the Settings dialog.

![image](https://user-images.githubusercontent.com/1910659/234148070-65c04c81-2d8b-4983-bbc5-5e9dd5b4ddb3.png)

## General Tab

Diffusion Toolkit will scan a set of folders and index images found into its database.  Add the folders you want to scan into the **Diffusion Folders** list.

You can choose **Recursive** to let Diffusion Toolkit recursively scan your Diffusion Folders. This is enabled by default.

If you have subfolders you want to ignore from the **Diffusion Folders** list, add them to the **Excluded Folders** list.

You can choose to **Watch Folders** for new images. See [Watch Folders](#watch-folders) for more information on this option.

You can set what **File Extensions** to scan for. Don't include an asterisk, but do include the dot, and separate with commas e.g. `.png, .jpg, .jpeg, .webp`.

You may **Check for updates on startup** to be notified if there is a new version in Github.

## Checkpoints Tab

Setting the checkpoint **Model Root** folder will allow Diffusion Toolkit to display the checkpoint filename based on the model hash found in the metadata.

As of mid January 2023, AUTOMATIC1111 has changed they way hashes are calculated. In order to use the new hash, you need to set the path to the **A1111 cache (cache.json)** file.

## Images Tab

Diffusion Toolkit does not require you to click through folders to search for an image. Instead, you may search for a term or a parameter used in your PNGINfo and you will recieve a list of all matching images. As the number of matching images can be in the thousands or tens of thousands, the results are paged.

Here you can set the number of **Thumbnails per page**.  

You can also tell Diffusion Toolkit to **Scan for new images on startup** so you don't have to click **Scan folders for new images** when you run the application.

## NSFW Tab

Diffusion Toolkit can auto-tag your images based on prompt text on demand or when they are scanned in. To do this, add the list of tokens (may include spaces), one per line in the **NSFW Tags** text box, and check the option **Auto Tag NSFW**.

## Themes Tab

You can select the **Theme** to use. System will use Light or Dark depending on your current system theme settings (tested on Windows 11).   You may also manually set to Light or Dark theme.

## Database Tab

**Open Folder** will open the application data folder in explorer.

You can **Backup** the database, and it will create a copy of the database in the application data folder with a unique filename containing the date and time of the backup.

You can **Restore** a backup of the database.

**NOTE: Do not perform a Backup or Restore while any query or database operation is running!**

Settings will be applied and saved upon closing the Settings dialog.

You will be prompted to scan your selected folders for images. 

# Indexing your images

Diffusion Toolkit is designed to help you find your images quickly and easily using theie metadata. To do this it needs to scan your images once to read in the metadata and store it in a local database. It can typically index around 10,000 images in seconds on an SSD.

## Scan folders for new images

If you generate new images in your Diffusion folders, or copy images into them, or remove images from them, you should click the **Scan folders for new images** button or press `Ctrl+R` to update your database with the latest changes.

It is not advised to move images (e.g. cut-paste using Explorer), even moving them within the Diffusion folders once you have indexed them, especially if you have favorited or rated them, as you will lose this information when you rescan.

To make sure your favorites and ratings are kept, use **right-click > Move** to move your images.

## Watch folders

The **Watch folders** setting will allow Diffusion Toolkit to recieve a notification everytime an image is added to your Diffusion folders.  The images will be added to the database without needing to click **Scan folders for new images**.

This is best used when generating images via a tool or webgui.  You will only be notified when the application has focus.  Your current view will not be reloaded when this occurs to prevent from interrupting your work.

**NOTE:** Be sure to turn this feature off if you are going to copy a lot of images into the Diffusion folders, as it is not optimized for this. Instead, use **Scan folders for new images**.

## Rebuild Metadata

**Edit > Rebuild Metadata** is intended for use when a new version of the application is released that supports new metadata. This will rescan all the images in your Diffusion folders and update the database with the metadata.

Running this will not affect any existing favorites or ratings you may have tagged.

# Usage

The **Diffusions** view is the default on startup.  Here you can enter your query and press `Enter` to intitate the search. Matching images will be displayed in the thumbnail view. By default, results are limited to 100 per page. This can be changed in **Settings**.

Use the buttons at the bottom of the page to move between pages. The keyboard shortcuts are:

* First Page - `Alt + Home`
* Previous Page - `Alt + Page Up` 
* Next Page - `Alt + Page Down`
* Last Page - `Alt + End`
* F6 - Set focus on search bar

Double-clicking an image or pressing `Enter` with an image selected will launch your default image viewer.

## Thumbnail Size

Click **View > Thumbnails** to select the thumbnail size.

# Tagging Images

You can tag your files with additional metadata (stored in the Diffusion Toolkit database) to help you organize and manage your images further.

## Favorite

While browsing via the thumbnail viewer, or with the Preview in focus, press `F` to toggle **Favorite** on the selected image.  A blue heart will indicate that the image has been tagged as Favorite. This will also be indicated in the Preview Pane.

You can favorite multiple selected images.

If you click on the Favorites Icon in the toolbar, you will be presented with all images tagged as Favorite.  

When in the Favorite View, toggling an image Favorite off will cause the image to be removed from the list and it will not be displayed the next time you visit the Favorite View.

You can use the Favorite search parameter to filter images by favorites. See [Advanced Searching](#advanced-searching) for more information.

## Rating

While browsing via the thumbnail viewer, or with the Preview in focus, press the keys `1-9`, or `0` for 10 to set the **Rating** on the selected image. A star with a number inside will indicate on the thumbnail that the image has been rated. An equivalent number of yellow stars in the Preview Pane will indicate the rating of the image.  Press the same number again to remove the rating.

You can rate multiple selected images.

You can use the Rating search parameter to filter images by rating. See [Advanced Searching](#advanced-searching) for more information.

## NSFW

While browsing via the thumbnail viewer, or with the Preview in focus, press `N` to toggle **NSFW** Tag on the selected image. If the **View > Blur NSFW** option is checked, the image will be blurred to indicate NSFW on the image.  Otherwise there no visible indicator that the image is tagged as NSFW.

Images tagged as NSFW will not be displayed in the Preview Pane if **View > Blur NSFW** is checked.

Press N again to remove the tagging.

If the **View > Hide NSFW** from results option is checked, images tagged NSFW will not be displayed in succeeding search results, unless you specifically search for `nsfw: true`

You can NSFW-tag multiple selected images.

You can use the NSFW search parameter to filter images by nsfw. See [Advanced Searching](#advanced-searching) for more information.

The shortcut for **View > Blur NSFW** is `B`

## Deleting Images

When inside the thumbnail viewer, press `Del` or `X` to toggle **For Deletion** on the selected image.  The thumbnail will become transparent, indicating that the image has been tagged as For Deletion.

If you click on the **Recycle Bin Icon** in the toolbar, you will be presented with all images tagged as For Deletion.  

When in the **Recycle Bin View**, toggling an image delete off will cause the image to be removed from the list and it will not be displayed the next time you visit the Recycle Bin View.

To permanently remove all images marked For Deletion, click **Edit > Empty Recycle Bin**.

**WARNING** - Images will not be sent to Window's Recycle bin. They will be removed permanently!

# Managing Images

## Moving Images

If you want to move images around in your Diffusion Folders, you should use the **Right-click > Move** command. Diffusion Toolkit tracks your Favorites, Ratings, NSFW in its own database, and is linked to the image by the path. Because of this, you should use the internal Move command so that the application can update the reference in the database.

## Removing Entries

You way want to remove an image from the database, without deleting the file from disk. The reasons you may want to do this are:

* Diffusion Toolkit generated a duplicate entry, pointing to the same file.
* You added an **Excluded Folder** (see [General](#general-tab) tab under Settings) and you wish to stop tracking images in the folder.

To do this, select the image and right-click > Remove Entry or press `Ctrl-X` or `Ctrl-Del`.


# Preview Pane

On the right is the **Preview Pane**, where the image preview will be displayed when an thumbnail is selected.

![Screenshot 2023-09-30 125622](https://github.com/RupertAvery/DiffusionToolkit/assets/1910659/6e8ab5e4-9f9a-427b-a06e-6314e532ba3d)

When you rate or favorite an image, an indicator will appear in the bottom left.

![Screenshot 2023-09-30 125823](https://github.com/RupertAvery/DiffusionToolkit/assets/1910659/2399569a-a96e-4b4d-9595-9f1dfeee3166)

You can set the image to automatically fit to the preview area by clicking **View > Fit to Preview**. Otherwise by default the image will be displayed at 100%, and you can Zoom and Pan the image. 

When the Preview Pane is in focus, you can use the same shortcut keys to tag the current image.

You can also use the left and right cursor keys to move between images.

## Zoom

With the Preview Pane in focus and Fit to Preview unchecked, hold `Ctrl` and press `-` or `+` to zoom in and out of the image.

You can also hold `Ctrl` and use mouse scroller to zoom in and out of the image.

Click and drag the image to pan the view when the image is larger than the Preview Pane.

Press `Ctrl + 0` to reset zoom to actual pixels (100%).

Zoom will be reset when you select another image.

## Displaying Metadata (PNGInfo)

To display the prompt and other metadata associated with the image (i.e. PNGInfo) press the **eye icon** located at the bottom right of the Preview Pane, or press `i` while in the Thumbnail View.

![Screenshot 2023-09-30 125805](https://github.com/RupertAvery/DiffusionToolkit/assets/1910659/0164dcac-3fe6-416e-8ce5-2e1646b0baba)

## Popout

Pressing the **Popout** button at the top-right of the preview pane will pop out the preview pane into it's own window. This will allow you to arrange the thumbnail and preview as you like, or even move them to separate desktops.

When the Preview Pane is in popped out, you can still use the same shortcut keys to tag the current image.

You can also use the left and right cursor keys to move between images.

Pressing **Escape** will close the Popout.

# Image Viewer

Double-clicking an image, or pressing Enter on a selected image will display the image in a maximized view of the popout.

See [Preview Pane](#preview-pane) for instructions on how to zoom, navigate and tag your image in the Image Viewer. 

# Folders

Click on the **Folders** icon in the menu bar to browse your images using folders. A Home button, Up button, and an address bar will appear below the search bar.

You will initially be presented with your home location, containing the list of root Diffusion folders.  Clicking on a folder will navigate to the folder, and display subfolders and images inside the folder.

Pressing the Home button will bring you back to the list of Diffusion folders.

Pressing Up will bring you to the current folder's parent.

Currently only indexed images will be displayed in Folder view.  If you navigate to a location outside your Diffusion folders, images will not be displayed.

You can still use the search function to filter images in Folder view.

If coming from a pre-1.0 version of Diffusion Toolkit, you must Rebuild Images to update the folder information. 

# Albums

Albums offer an alternative to folders for image grouping. To create an Album, you can use the **Create Album** button in the Albums panel, or select one or more images and right-click and select **Add to Album > New Album.**

Click on the **Albums** icon in the menu bar to display the list of Albums. A Home button will appear below the search bar, and the current Album name will be displayed next to to.

Pressing the Home button will bring you back to the list of Albums.

Clicking on an Album in the thumbnail view or on the Album panel will open the Album and display

Youu can right click on an Album in the thumbnail view or in the Albums panel to access the Album context menu, which allows you to rename or remove an Album.

If you remove an Album, only the Album will be removed. The images added to the album will still be available in Search, and in other albums they may have been added to.

To add an image to an existing Album, right click an image and select **Add to Album** then select one of the Albums from a list of last 10 updated Albums.

You can also drag and drop one or more selected images to an Album in the Album panel to add images to the Album.

To remove an image from an Album, while in the Album, right click an image and select **Remove from Album**. You will not be prompted for confirmation.

You can still use the search function to filter images in Album view.

# Drag and Drop

You can drag an image from the thumbnail to another app, such as the PNGInfo tab in a WebUI to transfer the metadata, or to an explorer folder to copy the image to the target folder, or into Photoshop to begin editing your masterpiece.

# Searching

Above the thumbnail view is the search box. You can type in search terms to filter your results and find what you are looking for easily.

## Searching by prompt

The basic way to search is by entering text that appears in the prompt. It will work the way yout expect most of the time.  However, commas are special and are used to **separate prompt terms**.

Take this query for example:

```
A man staring into a starry night sky, by Van Gogh
```

This contains two search terms:

* `A man staring into a starry night sky` 
* `by Van Gogh` 

The above query will match prompts that contain both `A man staring into a starry night sky` AND `by Van Gogh` in any order or position.

If you want to match an exact term that contains a comma, place the term in double quotes:

```
"A man staring into a starry night sky, by Van Gogh"
```

The above query will match prompts that contain ONLY `A man staring into a starry night sky, by Van Gogh` in that exact wording.

Note that spaces are important. The following query

```
"A man staring into a starry night sky , by Van Gogh"
```

is not the same as the previous term.


# Advanced Searching

To be able to search by other parameters in the search box, you need to use special tokens.  These are words referring to the parameter followed by a colon, for example `seed: 12345` will add a search term that filters images using a seed value of `12345`.

The prompt query, if any, should always come first, in order to avoid being interpreted as an argument to the parameter token.

```
A man staring into a starry night sky, by Van Gogh steps: 20 cfg:12  
```

Parameters will be ANDed, meaning adding more parameters will filter out more results. The above query will show images that have

* prompt contains `A man staring into a starry night sky` 
* AND `by Van Gogh` 
* AND `steps` is `20` 
* AND `cfg` is `12`  

When parsing the query, each possible parameter is matched and then removed. Any remaining unmatched text will be considered as part of the prompt.

# Supported parameters

## Negative Prompt

* `negative prompt: <term> [,<term>]`
* `negative_prompt: <term> [,<term>]`
* `negative: <term> [,<term>]`

## Steps

* `steps: <number>`
* `steps: <start>-<end>`

## Sampler

* `sampler: <name>` 

	The sampler name varies between AI generators. Some will use names with spaces, and some will use lowercase with underscore. Check what the generator stores in your images. For sampler names that use	spaces, put quotes around the name.

   Here is a list of known samplers used in A1111 and other tools.

   * "Euler a" or `euler_a`
   * Euler or `euler`
   * LMS or `lms`
   * Heun or `heun`
   * DPM2 or `dpm2`
   * "DPM2 a" or `dpm2_a`
   * "DPM++ 2S" a or `dpm++_2s_a`
   * "DPM++ 2M" or `dpm++_2m`
   * "DPM++ SDE" or `dpm++_sde`
   * "DPM fast" or `dpm_fast`
   * "DPM adaptive" or `dpm_adaptive`
   * "LMS Karras" or `lms_karras`
   * "DPM2 Karras" or `dpm2_karras`
   * "DPM2 a Karras" or `dpm2_a_karras`
   * "DPM++ 2S a Karras" or `dpm++_2s_a_karras`
   * "DPM++ 2M Karras" or `dpm++_2s_karras`
   * "DPM++ SDE Karras" or `dpm++_sde_karras`
   * DDIM or `ddim`
   * PLMS or `plms`

## Classifier-Free Guidance Scale (CFG/Scale)

* `cfg: <number>` 
* `cfg_scale: <number>` 
* `cfg scale: <number>` 

## Seed

You can query `seed`, with a number, a range, or wildcards.

* `seed: <number>`
* `seed: <start>-<end>`
* `seed: 123*` 
   * will show all images have a seed that starts with `123`
* `seed: 123456???000` 
   * will show all images have a seed that starts with `123456`, matches any 3 digits, and ends with `000`

## Size

* `size: <width>x<height>` 
* `size: <width>:<height>` 
  
  `width` and `height` can be a number or a question mark (`?`) to match any value. e.g. `size:512x?` will match images with a width of `512` and any height.

## Model Hash

* `model_hash: <hash>`

## Model Name

Wildcards (`?`, `*`) are supported

* `model: <term>`

Some tools don't store the model name in metadata by default.

Diffusion Toolkit will try to perform a hash lookup using the information stored in the AUTOMATIC1111 `cache.json` file if it exists.  It will attempt to lookup the name (partial matches allowed), and take the hash of any matching models, and use a hash query to search for the images. Both old hash algorithm and the newer SHA256 hash are supported.

Note that the `cache.json` file is updated on the fly by AUTOMATIC1111. It computes model hashes for new models it has never loaded before. Model hashes are computed the when you switch to them in the UI.

You should click **Edit > Reload Models** to ensure the application has the latest copy of the json file.

## Aesthetic Score

Aesthetic score is a tag added by the [Aesthetic Image Scorer Extension](https://github.com/tsngo/stable-diffusion-webui-aesthetic-image-scorer) for AUTOMATIC1111 Web UI.

It calculates an aesthetic score for generated images using CLIP+MLP Aesthetic Score Predictor based on Chad Scorer and stores it metadata.

* `aesthetic_score: [<|>|<=|>=|<>] <number>`

You can search for an exact number e.g. `aesthetic_score: 0.6`, but most likely you would like to do a comparative search such as less than `aesthetic_score: < 0.6`

## Hyper Networks

You can search for images that used a hypernetwork, and specify the strength used (AUTOMATIC111)

* `hypernet: <name>`

* `hypernet strength: [<|>|<=|>=|<>] <number>`

## Favorites

Favorite is a Diffusion Toolkit metadata with a value of true or false, entered by the user. See [Favorites](#favorites)

* `favorite: [true|false]`

## Rating

Rating is a Diffusion Toolkit metadata with a value of 1-5, entered by the user. See [Rating](#rating)

* `rating: [<|>|<=|>=|<>] <number>`

## NSFW

NSFW is a Diffusion Toolkit metadata with a value of true or false, entered by the user. See [NSFW Tag](#nsfw-tag). 

If you specify this term explicitly, it will override the **Hide NSFW from Results** option.

* `nsfw: [true|false]`

## No Metadata

This filter will show images that do not have metadata.

* `nometa: [true|false]`
* `nometadata: [true|false]`

## For Deletion

**For Deletion** is a Diffusion Toolkit metadata with a value of true or false, entered by the user. See [Deleting](#deleting)

* `delete: [true|false]` - Filters by files marked for deletion

## Date Created

 **Date Created** is a Diffusion Toolkit metadata taken from the image file's attributes during scanning.

Allows you to search by the file's created date

* `date: <criteria>`

   * `date: today` - Include files from the current date  
   * `date: yesterday` - Include files from the previous date  
   * `date: between 11-11-2022 and yesterday` - Include files from November 11, 2022 the previous date  
   * `date: from 10-10-2022 to 11-11-2022` - alternate syntax
   * `date: before 11-11-2022` - Include files since the beginning of time, up to November 11, 2022
   * `date: since 01-01-2022` - Include files created on January 1, 2022 up to today

  Notes:

   * `YYYY-MM-DD` format is supported
   * `XX-XX-XXXX` dates will be parsed using your computer's date format, i.e. `MM-DD-YYYY` for US and similar regions, `DD-MM-YYYY` for European regions.

## Path
 
 **Path** is a Diffusion Toolkit metadata taken from the image file's attributes during scanning.

You can use wildcards (`?`, `*`), or the criteria `starts with`, `contains`, or `ends with`. 

Path will match the full path, i.e. including the filename, so you will usually want to use wildcards.

Path with wildcards will return matches in subfolders. If you want to search a specific folder, use [Folder](#folder)

* `path: [criteria] <search-term>`  

   * using wildcards:
      * `path: D:\diffusion\images*`      
      * `path: *img2img*`      
      * `path: *.jpg`      

   * using criteria:
      * `path: starts with D:\diffusion\images`      
      * `path: contains img2img`      
      * `path: ends with .jpg`      
    
   If your path constains spaces, wrap your path in double quotes.

   * using globs:
      * `path: "D:\My pics\images**"`      
      * `path: "**funny cats**"`      
   * using criteria:
      * `path: starts with "D:\My pics\images"`      
      * `path: contains "funny cats"`      

## Folder
 
 **Folder** is a Diffusion Toolkit metadata taken from the image file's attributes during scanning.  Searching by folder limits your result to a specific folder, unlike path, which will include images in subfolders.

* `folder: <folder>`  

## Notes About Search

* The parameters e.g. `steps:`, `sampler:` are not case sensitive. You can use `Steps:`, `Sampler:`, so you can copy it from a prompt.
* You can have 0 or more spaces *after* the colon (`:`) and before the parameter value.
    * e.g. `steps:20`, `steps: 20`, `steps:   20` are OK
    * but `steps  :20`, `steps :20` are not

# Searching on Multiple Values 

You can search on multiple values on most parameters. The results will be ORed, meaning adding more values will bring in more results.

* You can specify a range for seed with `seed: <start>-<end>`
  * e.g. `seed: 10000-20000`
* You can specify mutiple values for other parameters using a pipe (`|`) 
  * e.g. `sampler: euler a | ddim | plms`
  * e.g. `cfg: 4.5|7|9|12`
  * e.g. `model_hash: aabbccdd | deadbeef | 12345678`

# Prompts View

The Prompts button in the toolbar will bring you to the Prompts tab.

You can search specifically on prompts here, using the same query language for searching images. Unique prompts are displayed, along with the usage count.

## Full Text and Similarity Slider

If you check **Full Text**, the entire text will be used as a search term, instead of eing broken up by commas.  This will also enable the Similarity slider.

The Similarity Slider will try to do a fuzzy match by applying a Hamming Distance to all prompts. This may be useful for searching for prompts generated using Dynamic Prompts plugin.

# Keyboard Shortcuts

| Shortcut       | Action         | Notes  |
|----------------|----------------|--------|
| `1..9, 0` | Rate 1 - 10 | 
| `I` | Show/Hide Info |  
| `F` | Tag Favorite |  
| `N` | Tag NSFW |
| `B` | Toggle Blur NSFW |
| `X/Del` | Tag for Deletion |
| `Ctrl+X/Del` | Remove Entry |
| `Ctrl+C` | Copy File to Clipboard |
| `Ctrl+Shift+A` | Show/Hide Album Pane |
| `Ctrl+Shift+P` | Show/Hide Preview |
| `Ctrl+Shift+F` | Toggle Fit to Preview |
| `Ctrl+1` | Folders View |
| `Ctrl+2` | Album View |
| `Ctrl+3` | Diffusions View |
| `Ctrl+4` | Favorites Page |
| `Ctrl+5` | Recycle Bin |
| `Ctrl+6` | Prompts View |
| `Ctrl+0` | Reset Zoom | in Preview Pane or Image Viewer
| `Ctrl+R` | Scan folders for new images |
| `Alt+Home` | First Page |
| `Alt+PageUp` | Previous Page |   
| `Alt+PageDown` | Next Page |
| `Alt+End` | Last Page |
| `F6` | Set focus on search bar |


# FAQ

## How do I view my image's metadata (PNGInfo)?

With the Preview Pane visible, press I in the thumbnail view or with the Preview Pane in focus to show or hide the metadata.  You can also click the eye icon at the botton right of the Preview Pane.

## When do I need to Rebuild Images?

You only need to Rebuild Images if a new version of Diffusion Toolkit comes out with support for metadata that exists in your existing images.

## Can I move my images to a different folder?

I you want to move your images to a different folder, but still within a Diffusion folder, you should use the **right-click > Move** command. This allows Diffusion Toolkit to handle the moving of images, and know to keep all the Diffusion Toolkit metadata (Favorites, Rating, NSFW) intact while moving.

If you use Explorer or some other application to move the files, but still under the Diffusion folders, when you Rescan Folders or Rebuild Images Diffusion Toolkit will detect that the images have been removed, then will detect new files added. You will lose any Favorites, Ratings or other Toolkit-specific information. 

## Where are my settings and database stored?

Your settings and the database are stored in `%APPDATA%\DiffusionToolkit`.

If you delete the database, you will lose all Diffusion Toolkit information such as Favorites and Ratings.
