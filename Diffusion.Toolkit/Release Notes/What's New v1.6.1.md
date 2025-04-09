# What's New in v1.6.1

## Enhancements

* Metadata panel overlay has returned for those who prefer a full-height preview pane. As before, press I to toggle.
* Improved navigation pane sizing
    * Resizable panes
    * Last pane will always take up the rest of the space (and not be resizeable)
    * Basically works more like you would expect
* Support opening images that don't match the file extension.
    * e.g. some tools might generate PNGs with a JPEG extension. Image viewers can open the image, but the metadata parser fails to read it correctly based on extension. This has now been fixed.
* New shortcuts: toggle Navigation Pane with F3, Preview Pane with F4

## Bugfixes

* Some fixes for `UNIQUE constraint failed: Image.Path`. In case you are affected, try looking at 'https://github.com/RupertAvery/DiffusionToolkit/issues/218#issuecomment-1939824894'
* Fix for layout issues. If your thumbnail appear to have only one line, this should fix it.  There is also a View > Reset layout in case layout gets messed up.
* Should fix some `Value cannot be null (Parameter 'element')` errors

