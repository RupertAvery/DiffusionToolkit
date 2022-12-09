# Table of Contents

* [Getting Started](#getting-started)
* [Updating your images](#updating-your-images)
   * [Rescan Folders](#rescan-folders)
   * [Rebuild Images](#rebuild-images)
* [Navigation](#navigation)
* [Searching](#searching)
* [Advanced Searching](#advanced-searching)

# Getting Started

On first load, you should be asked to add your image folders.

![image](https://user-images.githubusercontent.com/1910659/206367658-3f322276-3f80-4f34-8385-b47f2ba0ee5b.png)

Don't add nested folders, instead add the topmost folder. Folders will be scanned recusively.

You may also add the checkpoint model root folder. This will allow the app to display the checkpoint filename based on model hash when previewing an image.

You can set what file extensions to scan for. Don't include an asterisk, but do include the dot, and separate with commas e.g. `.png, .jpg`.

You can also set the number of images to appear on a page.

Settings will be save on exit.

When prompted, the app will immediately scan your folders for images. All parsed information will be saved into a SQLite database at `%APPDATA%\DiffusionToolkit\diffusion-toolkit.db`.

You will get a notice upon completion. You can now begin searching via prompt. 

Pressing Enter with an empty search bar will list all images.

# Updating your images

## Rescan Folders

If you generate new images in your target folders, or add or remove images from them, you should click the **Rescan Folders** button to update your database with the latest changes.

It is not advised to move images outside Diffustion Toolkit, especially if you have favorited or rated them, as you will lost this information when you rescan.

## Rebuild Images

**Rebuild Images** is intended for use when a new version of the application is released that supports new searchable parameters that need to be re-scanned from existing images.

Running this should not affect any existing favorites or ratings.

# Navigation

The Search view is the default on startup.  Here you can enter your prompt, and press Enter to intitate the search. Matching images will be displayed in the thumbnail view. By default, results will be limited to 100 per page. This can be changed in settings.

Use the buttons at the bottom of the page to move between pages. The keyboard shortcuts are:

* First Page - Alt + Home
* Previous Page - Alt + Page Up 
* Next Page - Alt + Page Down
* Last Page - Alt + End
* F6 - Focus on search bar

Double-clicking an image or pressing Enter with an image selected will launch your default image viewer.

**NOTE:** You can drag an image from the thumbnail to another app, such as the PNGInfo tab in a WebUI to transfer the metadata, or to an explorer folder to copy the image to the target folder

On the right is the Preview Pane, where the image preview and metadata are displayed.  In the meta data are buttons to Copy the metadata to the clipboard. There is a button on the Path section that will let you show the image in explorer.

# Favorites

When inside the thumbnail viewer, press F to toggle Favorite on the selected image.  A blue heart will indicate that the image has been tagged as Favorite. This will also be indicated in the preview pane.

If you click on the Favorites Icon in the toolbar, you will be presented with all images tagged as Favorite.  When in the Favorite View, toggling an image Favorite off will cause the image to be removed from the list and it will not be displayed the next time you visit the Favorite View.

# Rating

When inside the thumbnail viewer, press 1-5 to set the Rating on the selected image.  A number of yellow stars in the preview pane will indicate the rating of the image.  Press the same number again to remove the rating.

You can use the Rating search parameter to filter images by rating. See [Advanced Searching](#advanced-searching) for more information.

# Deleting

When inside the thumbnail viewer, press F to toggle For Deletion on the selected image.  The thumbnail will become transparent, indicating that the image has been tagged as For Deletion.

If you click on the Recycle Bin Icon in the toolbar, you will be presented with all images tagged as For Deletion.  When in the Recycle Bin View, toggling an image delete off will cause the image to be removed from the list and it will not be displayed the next time you visit the Recycle Bin View.

To permanently remove all images marked For Deletion, click Edit > Empty Recycle Bin.

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

You can also include image generation parameters in your search query. 

Add the parameters to the end of your query, e.g:

```
A man staring into a starry night sky, by Van Gogh steps: 20 cfg:12  
```

Parameters will be ANDed, meaning adding more parameters will filter out more results. 

## Supported parameters

* `steps: <number>` or `steps: <start>-<end>`

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

* `cfg: <number>` or `cfg_scale: <number>` or `cfg scale: <number>`

* `seed: <number>`

* `size: <width>x<height>` or `size: <width>:<height>` 
  
  `width` and `height` can be a number or a question mark (`?`) to match any value. e.g. `size:512x?` 

* `model_hash: <hash>`

* `aesthetic_score: [<|>|<=|>=|<>] <number>`

  You can search for an exact number e.g. `aesthetic_score: 0.6`, but most likely you would like to do a comparative search such as less than `aesthetic_score: < 0.6`

* `hypernet: <name>`

* `hypernet strength: [<|>|<=|>=|<>] <number>`

* `rating: [<|>|<=|>=|<>] <number>`

* `favorite: [true|false]`

* `delete: [true|false]` - Filters by files marked for deletion

* `date:`

  Allows you to search by the file's created date

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

* `path:` You can use GLOBs, or criteria 
   * Globs:
      * `path: D:\diffusion\images**`      
      * `path: **img2img**`      
   * Criteria:
      * `path: starts with D:\diffusion\images`      
      * `path: contains img2img`      
    
   If your path constains spaces, wrap you path in double quotes.

   * Globs:
      * `path: "D:\My pics\images**"`      
      * `path: "**funny cats**"`      
   * Criteria:
      * `path: starts with "D:\My pics\images"`      
      * `path: contains "funny cats"`      


## Notes

* The parameters e.g. `steps:`, `sampler:` are not case sensitive. You can use `Steps:`, `Sampler:`, so you can copy it from a prompt.
* You can have 0 or more spaces *after* the colon (`:`) and before the parameter value.
    * e.g. `steps:20`, `steps: 20`, `steps:   20` are OK
    * but `steps  :20`, `steps :20` are not

## Search on Multiple Values 

You can search on multiple values on most parameters. The results will be ORed, meaning adding more values will add more results.

* You can specify a range for seed with `seed:<start>-<end>`
  * e.g. `seed:10000-20000`
* You can specify mutiple values for other parameters using a pipe (`|`) 
  * e.g. `sampler: euler a | ddim | plms`
  * e.g. `cfg: 4.5 | 7 | 9 | 12`
  * e.g. `model_hash: aabbccdd | deadbeef | 12345678`
