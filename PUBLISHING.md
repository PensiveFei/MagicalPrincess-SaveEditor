# 发布指南 / Publishing Guide

## 1. GitHub Releases(自动)

推送一个 <code>v*</code> 标签即触发 <code>.github/workflows/release.yml</code>:

~~~
powershell
git tag v0.1.0
git push origin v0.1.0
~~~

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

- 游戏更新(如密钥或存档结构变化)可能使工具失效:跟踪
  <code>Assembly-CSharp.dll</code> 中 <code>Crypt.cs</code> 与
  <code>SaveData*</code> 类的变化;
- 版本号:修改 <code>MagicalPrincess.SaveEditor.csproj</code> 的
  <code>Version</code> 后打新 tag;
- 常见问题:若用户报"打不开存档",优先让其检查游戏是否在运行、
  存档路径是否被手动更改过。
