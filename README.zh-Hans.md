# Magical Princess 存档编辑器(非官方)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Neotro Inc. / MAGI Inc. 出品的养女儿模拟游戏 *Magical Princess*(Steam 版)
的开源**第三方存档编辑器**。

> English: [README.md](README.md)

## 功能

- **存档编辑**:列出全部 31 个槽位,以 JSON 树的形式编辑存档中任意字段——
  金钱、压力、黑币、行动力、技能点、七项等级、父亲好感、物品、事件标记等。
  常用数值提供一键快捷修改。
- **游戏设置**:分辨率档位(最高 3840x2160)、全屏/窗口、画质、垂直同步、
  语言、音量、文本速度。
- **安全设计**:每次写入前都会把原文件备份为时间戳副本,存放在存档目录的
  <code>backups\</code> 下,可随时找回。

## 原理(技术)

游戏把存档序列化为 JSON,用 AES-128-CBC(密钥硬编码)加密后 Base64 存盘,
位于 <code>%USERPROFILE%\AppData\LocalLow\Neotro Inc_\MagicalPrincess\</code>。
本工具用完全相同的算法解密 → 编辑 → 重新加密写回。
**不修改游戏程序本体,也不包含任何游戏素材。**

## 使用方法

1. **先关闭游戏**(游戏退出时会写存档,可能覆盖你的修改);
2. 运行 <code>MagicalPrincess.SaveEditor.exe</code>;
3. 选择槽位 → 修改数值 → 点「保存到存档」(自动备份);
4. 进游戏读档,确认生效。

> Steam 云存档:修改后的文件会正常同步,无需额外操作。

## 下载

- GitHub Releases: <https://github.com/PensiveFei/MagicalPrincess-SaveEditor/releases>
- Nexus Mods: <https://www.nexusmods.com/magicalprincess>(Save Games 分类)

## 从源码构建

~~~
powershell
dotnet publish src/MagicalPrincess.SaveEditor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist
~~~

## 免责声明

本工具为爱好者制作的非官方工具,与 Neotro Inc.、MAGI Inc. 无任何关联。
不含游戏素材,不修改游戏程序。请适度修改,极端数值可能破坏游戏体验或成就。
风险自负。

## 许可证

[MIT](LICENSE)
