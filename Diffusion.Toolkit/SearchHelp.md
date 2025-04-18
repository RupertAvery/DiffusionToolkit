# Search Help

You can search images using querying or filtering. 

[Querying](#querying) lets you type in your query and is intended for quick searches. You can search on more than just the prompt, see [Parameter Search](#parameter-search) for more information.

[Filtering](#filtering) gives you a bit more control, with each parameter separated, and more control over ComfyUI property searching and is intended for those who don't want to learn the Query Syntax (it's quite simple though) or who just want an interface-based approach to searching.

Either way, you can save your queries using the **Search > Save Query / Filter** option.

* [Querying](#querying)
    * [Query Syntax](#query-syntax)
    * [Simple Prompt Search](#simple-prompt-search)
    * [Parameter Search](#parameter-search)
    * [Supported parameters](#supported-parameters)
        * [Negative Prompt](#negative-prompt)
        * [Steps](#steps)
        * [Sampler](#sampler)
        * [Classifier-Free Guidance Scale (CFG/Scale)](#classifier-free-guidance-scale-cfgscale)
        * [Seed](#seed)
        * [Size](#size)
        * [Model Hash](#model-hash)
        * [Model Name](#model-name)
        * [Aesthetic Score](#aesthetic-score)
        * [Hyper Networks](#hyper-networks)
        * [Favorites](#favorites)
        * [Rating](#rating)
        * [NSFW](#nsfw)
        * [No Metadata](#no-metadata)
        * [For Deletion](#for-deletion)
        * [Date Created](#date-created)
        * [Path](#path)
        * [Folder](#folder)
    * [Notes About Search](#notes-about-search)
    * [Searching on Multiple Values](#searching-on-multiple-values)
    * [Querying Workflow Properties and Raw Metadata](#querying-workflow-properties-and-raw-metadata)
* [Filtering](#filtering)
    * [Metadata tab](#metadata-tab)
    * [Workflow tab](#workflow-tab)


# Query Syntax

The text box above the thumbnail area is the **Query** input. Here you can type in what you want to look for.  You can search on more than just the prompt, using **Query Syntax**. You can search on the [path](#path) of a file for example, or a range of [dates](#date*created) as well. You can combine criteria as well for a more refined search.

If the query syntax is a bit difficult to wrap your head around, or too verbose for you, you can use the [Filter](#filtering) instead.

## Simple Prompt Search

The basic way to search is by entering text that appears in the prompt. It will work the way yout expect most of the time.  However, commas are treated differently from they way they are used in prompts. In Query Syntax, they are used to separate  **search terms**.

Take this query for example:

```
A man staring into a starry night sky, by Van Gogh
```

This contains two search terms:

* `A man staring into a starry night sky` 
* `by Van Gogh` 

The above query will match prompts that contain both `A man staring into a starry night sky` AND `by Van Gogh` in any order or position.

This will match all of the following prompts:

```
A man staring into a starry night sky, by Van Gogh 
A man staring into a starry night sky, pencil sketch, by Van Gogh 
A man staring into a starry night sky, oil painting, by Van Gogh 
by Van Gogh, a man staring into a starry night sky 
```

If you want to match an exact term that contains a comma, place the term in double quotes:

```
"A man staring into a starry night sky, by Van Gogh"
```

The above query will match prompts that contain ONLY `A man staring into a starry night sky, by Van Gogh` in that exact wording (including spaces!).

An exmaple of some prompts that will match the query above are:

```
A man staring into a starry night sky, by Van Gogh 
A man staring into a starry night sky, by Van Gogh, pencil sketch
oil painting, A man staring into a starry night sky, by Van Gogh 
```

Note that spaces are important. The following query

```
"A man staring into a starry night sky , by Van Gogh"
```

Will not have the same results as the previous example.

## Parameter Search 

To be able to filter on other parameters, you need to use special *query tokens*.  These are usually words referring to the parameter followed by a colon, for example `seed: 12345` will add a parameter query that filters images using a seed value of `12345`.

The prompt query, if any, should always come first, in order to avoid being interpreted as an argument to the query tokens, for example:

```
A man staring into a starry night sky, by Van Gogh steps: 20 cfg:12  
```

Parameter queries will be ANDed, meaning adding more parameters will give fewer results that are more filtered. 

The above query will show images that match

* prompt contains `A man staring into a starry night sky` 
* AND `by Van Gogh` 
* AND `steps` is `20` 
* AND `cfg` is `12`

## Supported parameters

### Negative Prompt

* `negative prompt: <term> [,<term>]`
* `negative_prompt: <term> [,<term>]`
* `negative: <term> [,<term>]`

### Steps

* `steps: <number>`
* `steps: <start>-<end>`

### Sampler

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

### Classifier-Free Guidance Scale (CFG/Scale)

* `cfg: <number>` 
* `cfg_scale: <number>` 
* `cfg scale: <number>` 

### Seed

You can query `seed`, with a number, a range, or wildcards.

* `seed: <number>`
* `seed: <start>-<end>`
* `seed: 123*` 
   * will show all images have a seed that starts with `123`
* `seed: 123456???000` 
   * will show all images have a seed that starts with `123456`, matches any 3 digits, and ends with `000`

### Size

* `size: <width>x<height>` 
  
  `width` and `height` can be a number or a question mark (`?`) to match any value. e.g. `size:512x?` will match images with a width of `512` and any height.

* `size: <width>:<height>` (ratio e.g 16:9)

* `size: <orientation>`

   `orientation` can be one of the following:

   * `landscape`
   * `portrait`
   * `square`

### Model Hash

* `model_hash: <hash>`

### Model Name

* `model: <model name>`

Wildcards (`?`, `*`) are supported in the model name

*NOTE: The following information may be outdated:*

Some iamge generators don't store the model name in metadata, instead storing the model hash.

Diffusion Toolkit will try to perform a hash lookup using the information stored in the AUTOMATIC1111 `cache.json` file if it exists.  It will attempt to lookup the name (partial matches allowed), and take the hash of any matching models, and use a hash query to search for the images. Both old hash algorithm and the newer SHA256 hash are supported.

Note that the `cache.json` file is updated on the fly by AUTOMATIC1111. It computes model hashes for new models it has never loaded before. Model hashes are computed the when you switch to them in the UI.

You should click **Edit > Reload Models** to ensure the application has the latest copy of the json file.

### Aesthetic Score

Aesthetic score is a tag added by the [Aesthetic Image Scorer Extension](https://github.com/tsngo/stable-diffusion-webui-aesthetic-image-scorer) for AUTOMATIC1111 Web UI.

It calculates an aesthetic score for generated images using CLIP+MLP Aesthetic Score Predictor based on Chad Scorer and stores it metadata.

* `aesthetic_score: [<|>|<=|>=|<>] <number>`

You can search for an exact number e.g. `aesthetic_score: 0.6`, but most likely you would like to do a comparative search such as less than `aesthetic_score: < 0.6`

### Hyper Networks

You can search for images that used a hypernetwork, and specify the strength used (AUTOMATIC111)

* `hypernet: <name>`

* `hypernet strength: [<|>|<=|>=|<>] <number>`

### Favorites

Favorite is a Diffusion Toolkit metadata with a value of true or false. Use it to find images that you have tagged as favorite (true), or images you have not tagged as favorite (false).

* `favorite: [true|false]`

### Rating

Rating is a Diffusion Toolkit metadata with a value of 1-10.

* `rating: [<|>|<=|>=|<>] <number>`

You can use an optional comparison operator.

### NSFW

NSFW is a Diffusion Toolkit metadata with a value of true or false. Use it to find images that you have tagged as NSFW (true), or images you have not tagged as NSFW (false).

If you specify this term explicitly, it will override the **Hide NSFW from Results** option.

* `nsfw: [true|false]`

### No Metadata

This filter will show images that do not have metadata.

* `nometa: [true|false]`
* `nometadata: [true|false]`

### For Deletion

Delete is a Diffusion Toolkit metadata with a value of true or false. Use it to find images that you have tagged as For Deletion (true), or images you have not tagged as For Deletion (false).

* `delete: [true|false]` - Filters by files marked for deletion

### Date Created

**Date Created** is a Diffusion Toolkit metadata taken from the image file's attributes during scanning.

It allows you to search by the file's created date

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

### Path
 
**Path** is a Diffusion Toolkit metadata that uses the complete path of the file when it was scanned.

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

### Folder
 
 **Folder** is a Diffusion Toolkit metadata taken from the image file's attributes during scanning.  Searching by folder limits your result to a specific folder, unlike path, which will include images in subfolders.

* `folder: <folder>`  

## Notes About Search

* The parameters e.g. `steps:`, `sampler:` are not case sensitive. You can use `Steps:`, `Sampler:`, so you can copy it from a prompt.
* You can have 0 or more spaces *after* the colon (`:`) but not before it 
    * e.g. `steps:20`, `steps: 20`, `steps:   20` are OK
    * but `steps  :20`, `steps :20` are not

## Searching on Multiple Values 

You can search on multiple values on most parameters. The results will be ORed, meaning adding more values will bring in more results.

* You can specify a range for seed with `seed: <start>-<end>`
  * e.g. `seed: 10000-20000`
* You can specify mutiple values for other parameters using a pipe (`|`) 
  * e.g. `sampler: euler a | ddim | plms`
  * e.g. `cfg: 4.5|7|9|12`
  * e.g. `model_hash: aabbccdd | deadbeef | 12345678`

## Querying Workflow Properties and Raw Metadata

You can have Diffusion Toolkit search through ComfyUI Workflows or Raw Metadata through the Query input.

First you must have enabled scanning workflow and raw metadata in Settings, then Rescanned your images.

Then click on the Settings icon in the Query bar to configure what properties you want to search on.

To find property names, look in the Metadata Pane Workflow tab and press the ... button on the right of each property. for the option Copy Property Name.

# Filtering

Pressing the Filter button will bring up the Filter dialog, with the **Metadata** tab and the **Workflow** tab.

## Metadata tab

Here you can select what parameters you want to filter on. The Prompt is [parsed](#prompt-querying) the same way as in the query.

Make sure the checkbox next to the parameter you want to search on is checked, otherwise it will be ignored.

Near the bottom of the tab you will see a bunch of parameters with **True** and **False** options. These are used to search whether an image is *tagged*  (True) or *not tagged* (False).

## Workflow tab

The Workflow tab will filter images with ComfyUI metadata.  Here you can select what properties you want to search on and how to search on the values. For text properties you usually want to use *contains*, while other methods might be useful such as *starts with*.

You can combine filters with *and*, *or*, *not* operators.  The order of the operators matters, as the results of a filter will be modified with the next filter, so try to plan your filters accordingly.

