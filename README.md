# Diffusion Toolkit

THIS IS IN BETA

With the growing popularity and relative ease of use of Stable Diffusion, many users will generate hundreds to thousands of images with no real way to catalog them.

Images generated using popular diffusion UIs will embed metadata in the PNG file that contains information on what prompt was used to generate the image and other settings.

This is an image viewer backed by a SQLite database with the following features:

* Select and recursively scan a set of folders for PNG images 
* Parse the metadata into the database
* Allow images to be searched based on prompts (search by contains only for now).

## TODO

* List Checkpoints (models) along with hashes
* Implement HashV2
* Manage Checkpoint metadata alongside file
* Advanced searching
* Snippets and prompt management

## Future

* Embed Checkpoint metadata inside checkpoint for use in SD UIs

# Usage

On first load, you should be asked to add your image folders. Don't add nested folders, instead add the topmost folder. Folders will be scanned recusively.

You may also add the checkpoint model root folder, but it's not used yet.

The app will immediately scan your folders. They will be saved into a SQLite database at `%APPDATA%\DiffusionToolkit\diffusion-toolkit.db`.

After a while, you will get a notice upon completion. You can now begin searching via prompt.

Results are paged in sets of 250. Use the buttons at the bottom of the page to move between pages.

Thumbnail loading isn't the best right now, but good enough.  It's threaded, or should be, so it shouldn't block the UI that much.

Double-clicking an image or pressing enter while an image is selected will launch your default image viewer.