# Diffusion Toolkit

Diffusion Toolkit is an image viewer backed by a SQLite database with the following features:

* Select and recursively scan a set of folders for PNG and JPG+TXT images 
* Parse the metadata (prompt & parameters) into a database
* Supports A1111 and NovelAI PNGInfo formats
* Supports aesthetic score and hypernetwork search
* Search based on prompts and parameters, even file creation date (see [Advanced Searching](#advanced-searching))
* Drag and drop from Diffusion Toolkit to A1111 or NovelAI PNGInfo, or any app for folder to copy the image to the drop target.
* Quickly and easily mark files for later deletion
* Tag images as Favorite

Think [Lexica.art](https://lexica.art/), but with more powerul search, on your local images.

![image](https://user-images.githubusercontent.com/1910659/205200866-cac98b62-658c-4908-a188-09870d13acae.png)

**NOTE:** If you want to just run the program, get the latest release from [here](https://github.com/RupertAvery/DiffusionToolkit/releases). Look for the Assets under the latest release, expand it, then grab the zip file.

This is Windows only. You may be required to install .NET 6 Desktop Runtime (https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## TODO

* List Checkpoints (models) along with hashes and hash_v2
* Snippets and prompt management
* Manage Checkpoint metadata alongside file
* Advanced Search UI?

## Future

* Embed Checkpoint metadata inside checkpoint for use in SD UIs

# Usage

On first load, you should be asked to add your image folders.

![image](https://user-images.githubusercontent.com/1910659/205201001-de9cfd43-554a-447c-bba4-36f674eb0c54.png)

Don't add nested folders, instead add the topmost folder. Folders will be scanned recusively.

You may also add the checkpoint model root folder, but it's not used yet.

You can set what file extensions to scan for. Don't include an asterisk, but do include the dot, and separate with commas e.g. `.png, .jpg`

You can also set the number of images to appear on a page.

The app will immediately scan your folders for images. They will be saved into a SQLite database at `%APPDATA%\DiffusionToolkit\diffusion-toolkit.db`.

After a while, you will get a notice upon completion. You can now begin searching via prompt.

Results are paged. Use the buttons at the bottom of the page to move between pages.

Double-clicking an image or pressing Enter with an image selected will launch your default image viewer.



# Troubleshooting

If you get some sort of database error, try deleting the file `diffusion-toolkit.db` in `%APPDATA%\DiffusionToolkit`

# Buy me a coffee

I've been told that people should be allowed to [buy me a coffee](https://www.buymeacoffee.com/rupertavery)

Beer works too (PayPal)

* [I'm feeling generous](https://www.paypal.me/rupertavery/25.00?locale.x=en_US)
* [Buy me a 2 craft beers](https://www.paypal.me/rupertavery/10.00?locale.x=en_US)
* [Buy me a craft beer](https://www.paypal.me/rupertavery/5.00?locale.x=en_US)
* [Buy me 2 local beers](https://www.paypal.me/rupertavery/3.00?locale.x=en_US)
