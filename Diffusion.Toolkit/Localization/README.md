# Localization

To add a language, copy `default.json` and rename to the locale you want.

Open `langauges.json` and add an entry for your language there. The key will be the name that appears in the language dropdown, while the value
should be the name of the `.json` file that contains the translations.

ChatGPT can provide decent translations as a base, then you can adjust the grammar. You can even give it the json file to translate, but it will probably choke halfway due to the size,
so I usually split the file.

Some tools will eventually be provided to diff json files if there are new entries in the default that need to be added to localized files.

You should keep the order of the entries consistent with the default to make them easy to find.

