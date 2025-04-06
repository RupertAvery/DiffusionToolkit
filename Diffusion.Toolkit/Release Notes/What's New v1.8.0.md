# What's New in v1.8.0

Diffusion Toolkit can now search on raw metadata and ComfyUI workflow data. To do this, you need to enable the following settings in **Settings > Metadata**:

* **Store raw Metadata for searching**
* **Store ComfyUI Workflow for searching**

*Note: Storing Metadata and/or ComfyUI Workflow will increase the size of your database significantly.  Once the metadata or workflow is stored, unchecking the option will not remove it.*

You can expect your database size to double if you enable these options.

If you only want to search through ComfyUI Node Properties, you do not need to enable **Store raw Metadata**.

**Store ComfyUI Workflow** will only have an effect if your image has a ComfyUI Workflow.

You will still be able to view the workflow and the raw metadata in the Metadata Pane regardless of this setting.

Once either of these settings are enabled, you will need to rescan your images using one of the following methods:

* **Edit > Rebuild Metadata** – Rescans all images in your database.
* **Search > Rescan Metadata** – Rescans images in current search results.
* **Right-click a Folder > Rescan** – Rescans all images in a selected folder.
* **Right-click Selected Images > Rescan** – Rescans only selected images.

## ComfyUI Workflow Search

### How it works

Diffusion Toolkit scans images, extracts workflow nodes and properties, and saves them to the database. When you search, Diffusion toolkit can search on specific properties instead of the entire workflow. This makes searches faster, more efficient and precise.

There are two ways to search through ComfyUI properties.

### Quick Search

Quick Search now includes searching through specific workflow properties. Simply type in the search bar and press Enter. By default, it searches the following properties:

* `text`
* `text_g`
* `text_l`
* `text_positive`
* `text_negative`

You can modify these settings in **Search Settings** (the Slider icon in the search bar). 

To find property names, check the **Workflow** tab in the **Metadata Pane** or in the **Metadata Overlay** (press I to toggle). 

To add properties directly to the list in Search Settings, click `...` next to a node property in the Workflow Pane and select **Add to Default Search**.

### Filter

The Filter now allows you to refine searches based on node properties. Open it by clicking the Filter icon in the search bar or pressing CTRL+F, then go to the Workflow tab.

* Include properties to filter by checking the box next to them. Unchecked properties will not be included in the search.
* Use wildcards (\*) to match multiple properties (e.g., `text*` matches `text`, `text_g`, etc.).
* Choose property value comparisons: `contains`, `equals`, `starts with`, or `ends with`.
* Combine filters with `OR`, `AND`, and `NOT` operators.

To add properties, click `...` next to a node property in the Workflow Pane and select **Add to Filters**.

## Raw Metadata Search

Searching in raw metadata is disabled by default because it is much slower and should only be used when you really need it.  Go into **Search Settings** in the search bar to enable it.

## Raw Metadata View

You can now view the raw metadata in the Metadata Pane under the Raw Metadata tab

## Performance Improvements

There have been a lot of improvements in querying and loading data. Search will slow down a bit when including ComfyUI Workflow results, but overall querying have been vastly improved.  Paging is now more snappier due to reusing the thumbnail controls, though folder views with lots of folders still take a hit. Removing images from albums or otherwise refreshing the current search results with changes will no longer result in the entire page reloading and resetting to the top.

## Album and Model filtering on multiple items

Album and Model "Views" have been removed. They are now treated as filters, and you can freely select multiple albums and models to filter on at the same time.

## Increased Max Thumbnails per page to 1000

Due to improved loading performance, you can now load 1000 images at a time, if you wish. The recommended is still 250-500.

# Updates Summary

* ComfyUI Worklow Search
* Raw Metadata Search
* Raw Metadata View
* Performance improvements:
   * Massive improvements in results loading and paging 
   * Query improvements
   * Added indexes
   * Increased SQLite `cache_size` to 1GB. Memory usage will be increased
   * Added a spinner to indicate progress on some slow queries
* Filtering on multiple albums and models
* Increased max thumbnails per page to 1000
* Scroll wheel now works over albums / models / folders
* Fixed Fit to Preview and Actual Size being reset when moving between images in the Preview
* Fixed Prompt Search error
* Fixed some errors scanning NovelAI metadata
* Fixed some issues with Unicode text prompts
* Page no longer resets position when removing an image from an album or deleting
* Fixed Metadata not loaded for first image
* Fixed Model name not showing for some local models