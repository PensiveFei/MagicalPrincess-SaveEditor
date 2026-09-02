# Magical Princess Save Editor / 《Magical Princess》存档编辑器(非官方)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**EN** — Unofficial **open-source save editor** for *Magical Princess* (Steam),
the daughter-raising simulation game by Neotro Inc. / MAGI Inc.

**中文** —— Neotro Inc. / MAGI Inc. 出品的养女儿模拟游戏
*Magical Princess*(Steam 版)的开源**第三方存档编辑器**。

---

## English

### About the game

It has its rough edges, but overall *Magical Princess* is a lovely game:
beautiful UI, full voice acting and a huge amount of CG art — and the
daughter is really kawaii. If that sounds good to you, grab the original on
Steam and experience it yourself:
<https://store.steampowered.com/app/3562120/>

Once the late-game stat-grinding starts to feel repetitive, that is where
this editor comes in: tweak values, unlock routes, and focus on enjoying
the many story endings.

### What it does

- **Edit saves**: lists all 31 slots and lets you edit any value in the save
  as a JSON tree — money, stress, black coins, action points, skill points,
  the 7 stat levels, father favor, items, flags, and more. A beginner tab
  with labeled quick-edit fields for the most common values is one click away.
- **Edit game settings**: resolution tier (up to 3840x2160), fullscreen,
  quality, vsync, language, volumes and text speed.
- **Safe by design**: every write creates a timestamped backup under
  <code>backups\</code> in the save folder, and the original files are never
  modified in any other way.

### How it works (technical)

The game stores saves as JSON encrypted with AES-128-CBC (hardcoded key),
then Base64, in
<code>%USERPROFILE%\AppData\LocalLow\Neotro Inc_\MagicalPrincess\</code>.
This editor decrypts them, lets you edit the JSON, and re-encrypts with the
exact same scheme. No game code is modified and no game assets are included.

### Usage

1. **Close the game** (it writes saves on exit and would overwrite your edits).
2. Run <code>MagicalPrincess.SaveEditor.exe</code>.
3. Pick a slot, edit values, press **Save** (a backup is made automatically).
4. Launch the game and load the slot.

> Steam Cloud: the edited files are synced normally — no special steps needed.

### Download

- GitHub Releases: <https://github.com/PensiveFei/MagicalPrincess-SaveEditor/releases>
- Nexus Mods: <https://www.nexusmods.com/magicalprincess> (Save Games category)

### Build from source

~~~
powershell
dotnet publish src/MagicalPrincess.SaveEditor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist
~~~

### Disclaimer

This is a fan-made, unofficial tool. It is not affiliated with, endorsed by,
or connected to Neotro Inc. or MAGI Inc. It contains no game assets and does
not modify the game program. Use at your own risk; extreme values may break
the intended game balance or achievements.

---

## 中文

### 关于《Magical Princess》

游戏虽然有槽点,但总的来说还是不错的:精美的 UI + 全配音 + 海量插画 CG,
关键女儿真的很卡哇伊。感兴趣的推荐大家前往 Steam 购买《Magical Princess》
体验原作:<https://store.steampowered.com/app/3562120/>

当你已经感受到后期刷属性的重复枯燥之后,再使用本存档编辑器——用来调整
数值、解锁分支,专心体验各式各样的故事结局。

### 功能

- **存档编辑**:列出全部 31 个槽位,提供「常用修改(新手)」页——中文标签、
  自动带出当前值,金钱、压力、黑币、行动力、技能点、七项等级、父亲好感
  等一键修改;「高级编辑」页还能以 JSON 树的形式编辑任意字段(物品数量、
  好友好感、技能解锁、事件标记等)。
- **游戏设置**:分辨率档位(最高 3840x2160)、全屏/窗口、画质、垂直同步、
  语言、音量、文本速度。
- **安全设计**:每次写入前都会把原文件备份为时间戳副本,存放在存档目录的
  <code>backups\</code> 下,可随时找回。

### 原理(技术)

游戏把存档序列化为 JSON,用 AES-128-CBC(密钥硬编码)加密后 Base64 存盘,
位于 <code>%USERPROFILE%\AppData\LocalLow\Neotro Inc_\MagicalPrincess\</code>。
本工具用完全相同的算法解密 → 编辑 → 重新加密写回。
**不修改游戏程序本体,也不包含任何游戏素材。**

### 使用方法

1. **先关闭游戏**(游戏退出时会写存档,可能覆盖你的修改);
2. 运行 <code>MagicalPrincess.SaveEditor.exe</code>;
3. 打开「存档编辑」页选择槽位 → 回「常用修改(新手)」页修改数值 →
   点「保存全部修改」(自动备份);
4. 进游戏读档,确认生效。

> Steam 云存档:修改后的文件会正常同步,无需额外操作。

### 下载

- GitHub Releases: <https://github.com/PensiveFei/MagicalPrincess-SaveEditor/releases>
- Nexus Mods: <https://www.nexusmods.com/magicalprincess>(Save Games 分类)

### 从源码构建

~~~
powershell
dotnet publish src/MagicalPrincess.SaveEditor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist
~~~

### 免责声明

本工具为爱好者制作的非官方工具,与 Neotro Inc.、MAGI Inc. 无任何关联。
不含游戏素材,不修改游戏程序。请适度修改,极端数值可能破坏游戏体验或成就。
风险自负。

---

## License / 许可证

[MIT](LICENSE)
