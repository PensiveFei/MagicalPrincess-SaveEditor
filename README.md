# Magical Princess Save Editor(存档修改器 · 非官方)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**EN** — A small open-source save editor for *Magical Princess* (Steam), the daughter-raising sim by Neotro Inc. / MAGI Inc.

**中文** —— 给《Magical Princess》(Steam 版,Neotro Inc. / MAGI Inc. 的养女儿模拟游戏)写的一个小存档修改器(只改存档文件,不碰游戏程序)。开源、MIT、非官方。

---

## English

### About the game

The game has its flaws, but all in all it's pretty good: polished UI, full voice acting, and a ton of CG art — and the daughter is just so kawaii! I recommend grabbing *Magical Princess* on Steam and playing the original first.

Once the late-game stat grinding starts to feel repetitive, that's when this editor comes in: tweak the numbers, unlock branches, and just enjoy all the different story endings.

Steam: <https://store.steampowered.com/app/3562120/>

### What it does

- "Quick edit" tab: money, stress, black coins, action points, skill points, the 7 stat levels, father favor... labeled fields with current values pre-filled, blank = unchanged, one-click save;
- "Advanced edit" tab lays the whole save out as a tree — item counts, friendship, skill unlocks, event flags, all the niche stuff too;
- "Game settings": resolution (up to 3840x2160), fullscreen/windowed, quality, vsync, language, volumes, text speed;
- Every save first makes a timestamped backup of the original file in the backups folder, so you can always undo.

### How to use

1. Close the game first (it writes saves on exit and would overwrite your edits);
2. Run MagicalPrincess.SaveEditor.exe, no install needed;
3. Pick a slot → change values → save;
4. Load the slot in game.

Steam Cloud syncs the edited files normally, nothing extra to set up.

The release build is self-contained: double-click and run, no .NET install
needed. If you run a source/dev build and see a "You must install or update
.NET to run this application." dialog, click Download it now to install the
.NET 8 Desktop Runtime.

### Download

- GitHub Releases: <https://github.com/PensiveFei/MagicalPrincess-SaveEditor/releases>
- Nexus Mods: <https://www.nexusmods.com/magicalprincess> (Save Games)

### Build it yourself

~~~
powershell
dotnet publish src/MagicalPrincess.SaveEditor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist
~~~

### Important note

This is an unofficial save editor.
The prebuilt binary is provided for convenience; if you have any concerns, build it from source yourself.
All consequences of use are borne by the user.

---

## 中文

### 关于《Magical Princess》

游戏虽然有槽点,但总得来说还是不错的,精美的UI+全配音+海量插画cg,关键女儿真的很卡哇伊!推荐大家前往Steam购买《Magical Princess》体验原作

当你已经感受到后期刷属性的重复枯燥之后,再使用本存档编辑器,用来调整数值、解锁分支,专心体验各式各样的故事结局。

Steam 商店: <https://store.steampowered.com/app/3562120/>

### 能改什么

- 「常用修改」:金钱、压力、黑币、行动力、技能点、七项等级、父亲好感……中文标签,当前值自动带出,留空就不动,一键保存;
- 「高级编辑」页把存档整个摊开成树,物品数量、好友好感、技能解锁、事件标记这些冷门东西也能直接改
- 「游戏设置」分辨率(最高 3840x2160)、全屏/窗口、画质、垂直同步、语言、音量、文本速度;
- 每次保存前都把原文件按时间戳备份到 backups 文件夹,改坏了能还原。

### 怎么用

1. 先关游戏(它退出时会写存档,不然会盖掉你的修改);
2. 运行 MagicalPrincess.SaveEditor.exe,免安装
3. 选槽位 → 改数值 → 保存;
4. 进游戏读档

Steam 云存档会正常同步改过的文件,不用额外设置。

发布版自带运行环境,双击就能跑,不用装 .NET。若你运行的是源码开发版并弹出
"You must install or update .NET to run this application." 提示,点 Download it
now 安装 .NET 8 Desktop Runtime 即可。

### 下载

- GitHub Releases: <https://github.com/PensiveFei/MagicalPrincess-SaveEditor/releases>
- Nexus Mods: <https://www.nexusmods.com/magicalprincess>(Save Games 分类)

### 自己编译

~~~
powershell
dotnet publish src/MagicalPrincess.SaveEditor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist
~~~

### 重要提示

本工具为非官方存档编辑工具。
预编译程序仅供方便使用,心存顾虑可以自行从源码编译。
一切使用后果由使用者自行承担。

---

## License / 许可证

[MIT](LICENSE)
