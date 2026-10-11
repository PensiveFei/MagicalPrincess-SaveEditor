# 发布指南 / Publishing Guide

## 1. GitHub Releases(自动)

推送一个 `v*` 标签即触发 `.github/workflows/release.yml`:

```powershell
git tag v0.1.0
git push origin v0.1.0
```

Actions 会自动:构建 → 打包 zip → 上传 artifact → 创建 Release 并附带 zip。
发布后到 Release 页面补一段中文说明即可。

## 2. NexusMods 上传

1. 打开 <https://www.nexusmods.com/magicalprincess>(Games > Magical Princess);
2. Add a mod → 分类选 **Save Games / 存档**;
3. 上传 zip,名称建议:Magical Princess Save Editor (Open Source);
4. 简介要点:开源、GitHub 链接、功能列表(见 README.md 中文部分「能改什么」一节)、
   使用三步(关游戏 → 改 → 保存),附 GitHub Releases 链接互相引流;
5. 遵守 NexusMods 规则:确认不含游戏素材(本项目只含自研代码)。

## 3. Steam 社区引流(可选)

在 Steam → Magical Princess → 讨论区/指南 发一帖介绍工具,链接 GitHub 与
NexusMods。注意措辞:强调"非官方、开源、只改存档、自动备份",并附免责声明。

## 4. 维护注意事项

- **游戏每次更新后**,先跑一次只读自检脚本
  `powershell -ExecutionPolicy Bypass -File scripts\check-game-compat.ps1`:
  它会从源码里读出密钥/文件名前缀/字段清单,核对当前存档是否还对得上,
  通过返回 0、失败返回 1 并列出缺失项。也可以加 `-SaveDir` 指向备份出来的老档做离线核对;
- 游戏更新(如密钥或存档结构变化)可能使工具失效:跟踪
  `Assembly-CSharp.dll` 中 `Crypt.cs` 与 `SaveData*` 类的变化;
- 用户报「读取槽位列表失败」时,先看弹窗是不是「存档格式可能已变化」——
  那是文件前缀或字段被游戏改掉了,自检脚本能直接定位;
- 版本号:修改 `MagicalPrincess.SaveEditor.csproj` 的 `Version` 后打新 tag;
- 常见问题:若用户报"打不开存档",优先让其检查游戏是否在运行、
  存档路径是否被手动更改过;
- `scripts/check-game-compat.ps1` 必须保持 **UTF-8 (带 BOM)** 保存:
  Windows PowerShell 5.1 会把无 BOM 的 UTF-8 当 ANSI 解码,中文直接语法报错。
