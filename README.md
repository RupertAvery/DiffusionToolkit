# Diffusion Toolkit

Diffusion Toolkit is an image viewer backed by a SQLite database with the following features:

* Select and recursively scan a set of folders for PNG and JPG+TXT images 
* Parse the metadata (prompt & parameters) into a database
* Searched based on prompts and parameters (see [Advanced Searching](#advanced-searching))

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

# Searching

Searching is performed by checking if the prompt contains the search term.  Separate search terms using a comma. Each search term is ANDed to produce the final filter. 

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


# Advanced Searching

You can also include image generation parameters in your search query. 

Add the parameters to the end of your query, e.g:

```
A man staring into a starry night sky, by Van Gogh steps: 20 cfg:12  
```

Parameters will be ANDed, meaning adding more parameters will filter out more results. 

The following parameters are supported:

* `steps:<number>`

* `sampler:<name>` - Replace name with the sampler name, but with the spaces replaced with an underscore (_). Not case sensitive. Any future samplers will work this way automatically.

  * Euler a - `euler_a`
  * Euler - `euler`
  * LMS - `lms`
  * Heun - `heun`
  * DPM2 - `dpm2`
  * DPM2 a - `dpm2_a`
  * DPM++ 2S a - `dpm++_2s_a`
  * DPM++ 2M - `dpm++_2m`
  * DPM++ SDE - `dpm++_SDE`
  * DPM fast - `dpm_fast`
  * DPM adaptive - `dpm_adaptive`
  * LMS Karras - `lms_karras`
  * DPM2 Karras - `dpm2_karras`
  * DPM2 a Karras - `dpm2_a_karras`
  * DPM++ 2S a Karras - `dpm++_2s_a_karras`
  * DPM++ 2M Karras - `dpm++_2s_karras`
  * DPM++ SDE Karras - `dpm++_sde_karras`
  * DDIM - `ddim`
  * PLMS - `plms`

* `cfg:<number>` or `cfg_scale:<number>` or `cfg scale:<number>`

* `seed:<number>`

* `size:<width>x<height>` or `size:<width>:<height>` 
  * `width` and `height` can be a number or a question mark (`?`) to match any value. e.g. `size:512x?` 

* `model_hash:<hash>`

## Notes

* The parameters e.g. `steps:`, `sampler:` are not case sensitive. You can use `Steps:`, `Sampler:`, so you can copy it from a prompt.
* You can have 0 or more spaces *after* the colon (`:`) and before the parameter value.
  * e.g. `steps:20`, `steps: 20`, `steps:   20` are OK
  * but `steps  :20`, `steps :20` are not

## Multiple Values search

You can search on multiple values on most parameters. The results will be ORed, meaning adding more values will add more results.

* You can specify a range for seed with `seed:<start>-<end>`
  * e.g. `seed:10000-20000`
* You can specify mutiple values for other parameters using a pipe (`|`) 
  * e.g. `sampler:euler_a|ddim|plms`
  * e.g. `cfg:4.5|7|9|12`
  * e.g. `model_hash:aabbccdd|deadbeef|12345678`


# Troubleshooting

If you get some sort of database error, try deleting the file `diffusion-toolkit.db` in `%APPDATA%\DiffusionToolkit`

# Buy me a coffee

I've been told that people should be allowed to [buy me a coffee](https://www.buymeacoffee.com/rupertavery)

Beer works too (PayPal)

* [I'm feeling generous](https://www.paypal.me/rupertavery/25.00?locale.x=en_US)
* [Buy me a 2 craft beers](https://www.paypal.me/rupertavery/10.00?locale.x=en_US)
* [Buy me a craft beer](https://www.paypal.me/rupertavery/5.00?locale.x=en_US)
* [Buy me 2 local beers](https://www.paypal.me/rupertavery/3.00?locale.x=en_US)
