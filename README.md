# Magical Princess Save Editor

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Unofficial **open-source save editor** for *Magical Princess* (Steam), the
daughter-raising simulation game by Neotro Inc. / MAGI Inc.

> Chinese: [README.zh-Hans.md](README.zh-Hans.md)

## What it does

- **Edit saves**: lists all 31 slots and lets you edit any value in the save
  as a JSON tree — money, stress, black coins, action points, skill points,
  the 7 stat levels, father favor, items, flags, and more. Quick-edit fields
  for the most common values are one click away.
- **Edit game settings**: resolution tier (up to 3840x2160), fullscreen,
  quality, vsync, language, volumes and text speed.
- **Safe by design**: every write creates a timestamped backup under
  <code>backups\</code> in the save folder, and the original files are never
  modified in any other way.

## How it works (technical)

The game stores saves as JSON encrypted with AES-128-CBC (hardcoded key),
then Base64, in
<code>%USERPROFILE%\AppData\LocalLow\Neotro Inc_\MagicalPrincess\</code>.
This editor decrypts them, lets you edit the JSON, and re-encrypts with the
exact same scheme. No game code is modified and no game assets are included.

## Usage

1. **Close the game** (it writes saves on exit and would overwrite your edits).
2. Run <code>MagicalPrincess.SaveEditor.exe</code>.
3. Pick a slot, edit values, press **Save** (a backup is made automatically).
4. Launch the game and load the slot.

> Steam Cloud: the edited files are synced normally — no special steps needed.

## Download

- GitHub Releases: <https://github.com/PensiveFei/MagicalPrincess-SaveEditor/releases>
- Nexus Mods: <https://www.nexusmods.com/magicalprincess> (Save Games category)

## Build from source

~~~
powershell
dotnet publish src/MagicalPrincess.SaveEditor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist
~~~

## Disclaimer

This is a fan-made, unofficial tool. It is not affiliated with, endorsed by,
or connected to Neotro Inc. or MAGI Inc. It contains no game assets and does
not modify the game program. Use at your own risk; extreme values may break
the intended game balance or achievements.

## License

[MIT](LICENSE)
