# Table of Contents

* [Getting Started](#getting-started)
* [Updating your images](#updating-your-images)
   * [Rescan Folders](#rescan-folders)
   * [Watch Folders](#watch-folders)
   * [Rebuild Images](#rebuild-images)
* [Navigation](#navigation)
* [Preview](#preview)
   * [Displaying Metadata (PNGInfo)](#displaying-metadata-pnginfo)
* [Drag and Drop](#drag-and-drop)
* [Tagging](#tagging)
   * [Favorites](#favorites)
   * [Rating](#rating)
   * [NSFW](#nsfw-tag)
   * [Deleting](#deleting)
* [Searching](#searching)
* [Advanced Searching](#advanced-searching)
* [Supported Parameters](#supported-parameters)
* [Notes](#notes)
* [Search on Multiple Values](#search-on-multiple-values)


# Getting Started

On first load, you will be shown the settings dialog, where you can add your image folders.

![image](https://user-images.githubusercontent.com/1910659/210345314-e1e6a37c-94d0-47cc-994d-77a01ad712ee.png)

If you have your images stored in several nested folders, don't add the nested folders, just add the topmost folder. Folders will be scanned recusively.

You can choose to Watch Folders for new images. See [Watch Folders](#watch-folders) for more information on this option.

You may also add the checkpoint model root folder. This will allow the app to display the checkpoint filename based on model hash when previewing an image.

You can set what file extensions to scan for. Don't include an asterisk, but do include the dot, and separate with commas e.g. `.png, .jpg`.

Diffusion Toolkit does not require you to click through folders to search for an image. Instead, search for a term or a parameter used in your PNGINfo and you will recieve a list of all matching images. As the number of matching images can be in the thousands or tens of thousands, the results arg paged.

Here you can set the number of images to appear on a page.  

You can also select the theme to use. System will use Light or Dark depending on your current system theme settings (tested on Windows 11).   You may also manually set to Light or Dark theme.

Settings will be applied and saved upon closing the Settings dialog.

You will be prompted to scan your selected folders for images. All parsed information will be saved into a SQLite database at `%APPDATA%\DiffusionToolkit\diffusion-toolkit.db`.

You will get a notice upon completion. You can now begin searching via prompt. 

Pressing Enter with an empty search bar will list all images.

# Updating your images

## Rescan Folders

If you generate new images in your target folders, or add or remove images from them, you should click the **Rescan Folders** button to update your database with the latest changes.

It is not advised to move images outside Diffustion Toolkit, especially if you have favorited or rated them, as you will lost this information when you rescan.

## Watch folders

The **Watch folders** setting will allow Diffusion Toolkit to recieve a notification everytime an image is added to one of the nested folders in your target folders.  The images will be added to the database without needing to click **Rescan Folders**.

This is best used when generating images via a tool or webgui.  You will only be notified when the application has focus.  Your current search will not be updated when this occurs, to prevent from interrupting your work.

**NOTE:** Be sure to turn this off if you are going to copy a lot of images into the target folders, as it is not optimized for this.

## Rebuild Images

**Rebuild Images** is intended for use when a new version of the application is released that supports new searchable parameters that need to be re-scanned from existing images.

Running this should not affect any existing favorites or ratings

# Navigation

The Search view is the default on startup.  Here you can enter your prompt, and press Enter to intitate the search. Matching images will be displayed in the thumbnail view. By default, results will be limited to 100 per page. This can be changed in settings.

Use the buttons at the bottom of the page to move between pages. The keyboard shortcuts are:

* First Page - Alt + Home
* Previous Page - Alt + Page Up 
* Next Page - Alt + Page Down
* Last Page - Alt + End
* F6 - Focus on search bar

Double-clicking an image or pressing Enter with an image selected will launch your default image viewer.

## Thumbnail Size

Click View > Thumbnails to select the thumbnail size.

# Tagging

You can tag your files with additional metadata (only in Diffusion Toolkit) to help you organize and manage your images further.

## Favorites

When inside the thumbnail viewer, press `F` to toggle **Favorite** on the selected image.  A blue heart will indicate that the image has been tagged as Favorite. This will also be indicated in the Preview Pane.

You can favorite multiple selected images.

If you click on the Favorites Icon in the toolbar, you will be presented with all images tagged as Favorite.  

When in the Favorite View, toggling an image Favorite off will cause the image to be removed from the list and it will not be displayed the next time you visit the Favorite View.

You can use the Favorite search parameter to filter images by favorites. See [Advanced Searching](#advanced-searching) for more information.

## Rating

When inside the thumbnail viewer, press `1-5` to set the **Rating** on the selected image. A star will indicate that the image has been rated. An equivalent number of yellow stars in the Preview Pane will indicate the rating of the image.  Press the same number again to remove the rating.

You can rate multiple selected images.

You can use the Rating search parameter to filter images by rating. See [Advanced Searching](#advanced-searching) for more information.

## NSFW Tag

When inside the thumbnail viewer, press `N` to toggle **NSFW** Tag on the selected image. If the **View > Blur NSFW** option is checked, the image will be blurred to indicate NSFW on the image.  Otherwise there no visible indicator that the image is tagged as NSFW.

Images tagged as NSFW will not be displayed in the Preview Pane if **View > Blur NSFW** is checked.

Press N again to remove the tagging.

If the **View > Hide NSFW** from results option is checked, images tagged NSFW will not be displayed in succeeding search results, unless you specifically search for `nsfw: true`

You can NSFW-tag multiple selected images.

You can use the NSFW search parameter to filter images by nsfw. See [Advanced Searching](#advanced-searching) for more information.

The shortcut for **View > Blur NSFW** is `B`

## Deleting

When inside the thumbnail viewer, press `Del` or `X` to toggle **For Deletion** on the selected image.  The thumbnail will become transparent, indicating that the image has been tagged as For Deletion.

If you click on the **Recycle Bin Icon **in the toolbar, you will be presented with all images tagged as For Deletion.  

When in the **Recycle Bin View**, toggling an image delete off will cause the image to be removed from the list and it will not be displayed the next time you visit the Recycle Bin View.

To permanently remove all images marked For Deletion, click **Edit > Empty Recycle Bin**.

# Preview

On the right is the Preview Pane, where the image preview will be displayed when an thumbnail is selected.

![image](https://user-images.githubusercontent.com/1910659/210345752-d0a2deef-174d-4bae-b191-393387a8fce6.png)

When you rate or favorite an image, an indicator will appear in the bottom left.

![image](https://user-images.githubusercontent.com/1910659/210347580-5e5bfbf7-63e6-4581-99c7-56806b002377.png)

You can set the image to automatically fit to the preview area by clicking **View > Fit to Preview**. Otherwise by default the image will be displayed at 100%, and you can Zoom and Pan the image. 

When the Preview Pane is in focus, you can use the same shortcut keys to tag the current image.

You can also use the left and right cursor keys to move between images.

## Zoom

With the Preview Pane in focus, hold CTRL and press - or + to zoom in and out of the image.

You may also hold CTRL and use the mouse scroller to zoom in and out of the image.

Click and drag the image to pan the view when the image or zoom is larger than the Preview Pane.

Zoom will be reset when you select another image.

Zoom functionality is a bit basic, and it will not zoom in where your cursor is when zooming with the mouse scroll button.

## Displaying Metadata (PNGInfo)

To display the prompt and other metadata associated with the image (i.e. PNGInfo) press the **eye icon** located at the bottom right of the Preview Pane, or press `i` while in the Thumbnail View.

![image](https://user-images.githubusercontent.com/1910659/210345696-914d36bf-76dc-4717-ab3c-cc622e80d07b.png)

## Popout

Pressing the Popout button at the top-right of the preview pane will pop out the preview pane into it's own window. This will allow you to arrange the thumbnail and preview as you like, or even move them to separate desktops.

When the Preview Pane is in popped out, you can still use the same shortcut keys to tag the current image.

You can also use the left and right cursor keys to move between images.

# Drag and Drop

You can drag an image from the thumbnail to another app, such as the PNGInfo tab in a WebUI to transfer the metadata, or to an explorer folder to copy the image to the target folder


# Searching

Separate search terms using a comma. Each search term is ANDed to produce the final filter. 

For example:

```
A man staring into a starry night sky, by Van Gogh
```

This contains two search terms.

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

The prompt, if any, must always come first.  

You can also include image generation parameters in your search query. 

Add the parameters to the end of your query, e.g:

```
A man staring into a starry night sky, by Van Gogh steps: 20 cfg:12  
```

Parameters will be ANDed, meaning adding more parameters will filter out more results. 

The way the query is parsed is, each possible parameter is matched and then removed from the query. Any remaining unmatched text will be considered as part of the prompt.

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

	To seaerch for a sampler name, use whatever is stored in the metadata.
	Sometimes, this will vary from tool to tool.  Also, for sampler names that use
	spaces, put quotes around the name.

   * Euler a or `euler_a`
   * Euler or `euler`
   * LMS or `lms`
   * Heun or `heun`
   * DPM2 or `dpm2`
   * DPM2 a or `dpm2_a`
   * DPM++ 2S a or `dpm++_2s_a`
   * DPM++ 2M or `dpm++_2m`
   * DPM++ SDE or `dpm++_sde`
   * DPM fast or `dpm_fast`
   * DPM adaptive or `dpm_adaptive`
   * LMS Karras or `lms_karras`
   * DPM2 Karras or `dpm2_karras`
   * DPM2 a Karras or `dpm2_a_karras`
   * DPM++ 2S a Karras or `dpm++_2s_a_karras`
   * DPM++ 2M Karras or `dpm++_2s_karras`
   * DPM++ SDE Karras or `dpm++_sde_karras`
   * DDIM or `ddim`
   * PLMS or `plms`

## Classifier-Free Guidance Scale (CFG/Scale)

* `cfg: <number>` 
* `cfg_scale: <number>` 
* `cfg scale: <number>` 

## Seed

* `seed: <number>`
* `seed: <start>-<end>`

## Size

* `size: <width>x<height>` 
* `size: <width>:<height>` 
  
  `width` and `height` can be a number or a question mark (`?`) to match any value. e.g. `size:512x?` 

## Model Hash

* `model_hash: <hash>`

## Aesthetic Score

Aesthetic score is a tag added by the [Aesthetic Image Scorer Extension](https://github.com/tsngo/stable-diffusion-webui-aesthetic-image-scorer) for AUTOMATIC111 Web UI.

It calculates aestetic score for generated images using CLIP+MLP Aesthetic Score Predictor based on Chad Scorer and stores it metadata.

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

For Deletion is a Diffusion Toolkit metadata with a value of true or false, entered by the user. See [Deleting](#deleting)

* `delete: [true|false]` - Filters by files marked for deletion

## File Creation Date

File Creation Date is a Diffusion Toolkit metadata taken from the image file's attributes during scanning.

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
   * `XX-XX-XXXX` dates will be parsed using your computer's date format, i.e. 
`MM-DD-YYYY` for US and similar regions, `DD-MM-YYYY` for European regions.

## File Path

File Creation Date is a Diffusion Toolkit metadata taken from the image file's path during scanning.

You can use [globs](https://www.sqlitetutorial.net/sqlite-glob/), or the criteria `starts with`, `contains`, or `ends with`. 

* `path: <term>`  

   * using globs:
      * `path: D:\diffusion\images**`      
      * `path: **img2img**`      
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

# Notes about Search

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

# Prompt Search

The Prompts button in the toolbar will bring you to the Prompts tab.

You can search specifically on prompts here, using the same query language for searching images. Unique prompts are displayed, along with the usage count.

## Full Text and Similarity Slider

If you check **Full Text**, the entire text will be used as a search term, instead of eing broken up by commas.  This will also enable the Similarity slider.

The Similarity Slider will try to do a fuzzy match by applying a Hamming Distance to all prompts. This may be useful for searching for prompts generated using Dynamic Prompts plugin.

# FAQ

## Do I need to Rebuild Images?

No, you only need to Rebuild Images if a new version of Diffusion Toolkit comes out with support for metadata that exists in your existing images.

## Can I move my files to a different folder?

If you move your files to a different folder, but still under the target folders, when you Rescan Folders or Rebuild Images Diffusion Toolkit will detect that the images have been removed, and will detect new files added.

You will lose any Favorites, Ratings or other Toolkit-specific information. 

## Where are my settings stored?

Your settings and the database are stored in `%APPDATA%\DiffusionToolkit`.

If you delete the database, you will lose all Diffusion Toolkit information such as Favorites and Ratings.
