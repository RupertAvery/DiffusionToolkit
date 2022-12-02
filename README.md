# Diffusion Toolkit

Diffusion Toolkit is an image viewer backed by a SQLite database with the following features:

* Select and recursively scan a set of folders for PNG images 
* Parse the metadata (prompt & parameters) into a database
* Allow images to be searched based on prompts.

Think [Lexica.art](https://lexica.art/), but for your local images.

![image](https://user-images.githubusercontent.com/1910659/205200866-cac98b62-658c-4908-a188-09870d13acae.png)

## TODO

* List Checkpoints (models) along with hashes
* Implement HashV2
* Manage Checkpoint metadata alongside file
* Advanced searching
* Snippets and prompt management

## Future

* Embed Checkpoint metadata inside checkpoint for use in SD UIs

# Usage

On first load, you should be asked to add your image folders.

![image](https://user-images.githubusercontent.com/1910659/205201001-de9cfd43-554a-447c-bba4-36f674eb0c54.png)

Don't add nested folders, instead add the topmost folder. Folders will be scanned recusively.

You may also add the checkpoint model root folder, but it's not used yet.

The app will immediately scan your folders for images. They will be saved into a SQLite database at `%APPDATA%\DiffusionToolkit\diffusion-toolkit.db`.

After a while, you will get a notice upon completion. You can now begin searching via prompt.

Results are paged. Use the buttons at the bottom of the page to move between pages.

Double-clicking an image or pressing Enter with an image selected will launch your default image viewer.

# Searching

Searching is performed by checking if a prompt contains the search term.  Separate search terms using a comma. Each search term is ANDed to produce the final filter. 

For example:

```
A man staring into a starry night sky, by Van Gogh
```

will match prompts that contain both `A man staring into a starry night sky` AND `by Van Gogh` in any order or position.

If you want to match an exact term that contains a comma, place the term in double quotes:

```
"A man staring into a starry night sky, by Van Gogh"
```

will match prompts that contain ONLY `A man staring into a starry night sky, by Van Gogh` in that exact wording.

Note that spaces are important.

```
"A man staring into a starry night sky , by Van Gogh"
```

will not match the previous term.



# Troubleshooting

If you get some sort of database error, try deleting the file `diffusion-toolkit.db` in `%APPDATA%\DiffusionToolkit`

# Buy me a coffee

I've been told, that people should be allowed to [buy me a coffee](https://www.buymeacoffee.com/rupertavery)

Beer works too (PayPal)

* [I'm feeling generous](https://www.paypal.me/rupertavery/?locale.x=en_US)
* [Buy me a 2 craft beers](https://www.paypal.me/rupertavery/10.00?locale.x=en_US)
* [Buy me a craft beer](https://www.paypal.me/rupertavery/5.00?locale.x=en_US)
* [Buy me a 2 local beers](https://www.paypal.me/rupertavery/3.00?locale.x=en_US)
