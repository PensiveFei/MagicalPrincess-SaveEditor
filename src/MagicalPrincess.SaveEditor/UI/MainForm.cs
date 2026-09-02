using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MagicalPrincess.SaveEditor.Core;
using Newtonsoft.Json.Linq;

namespace MagicalPrincess.SaveEditor.UI
{
    public class MainForm : Form
    {
        private static readonly string[] ResolutionNames =
        {
            "800x452", "960x540", "1280x720", "1600x900",
            "1920x1080", "2560x1440", "3840x2160"
        };
        private static readonly string[] QualityNames = { "低 LOW", "中 MEDIUM", "高 HIGH" };
        private static readonly string[] LangNames = { "English", "日本語", "简体中文", "繁體中文" };
        private static readonly string[] MsgSpeedNames = { "1.0x", "1.5x", "2.0x", "2.5x", "极速 (instant)" };
        private static readonly float[] MsgSpeedValues = { 1f, 1.5f, 2f, 2.5f, 1000f };

        private SaveStore store;
        private JObject currentUser;
        private int currentSlot = -1;
        private bool userDirty;

        private TabControl tabs;
        private ListView slotList;
        private Button btnOpenSlot, btnReloadSlots, btnSave, btnApplyQuick, btnApplyEdit;
        private TreeView jsonTree;
        private Label lblEditPath, lblDirty, pathLabel;
        private TextBox txtEditValue, qMoney, qStress, qBlackCoin, qActivePower, qSkillPoint, qFatherFav;
        private ComboBox cmbEditBool, cmbSlot;
        private CheckBox chkFullScreen, chkVSync;
        private ComboBox cmbResolution, cmbQuality, cmbLang, cmbMsgSpeed;
        private TextBox txtBGM, txtSE, txtVoice;
        private Button btnSaveSettings, btnReloadSettings;
        private readonly List<BeginnerField> beginnerFields = new List<BeginnerField>();
        private readonly List<FriendField> friendFields = new List<FriendField>();
        private FlowLayoutPanel friendFlow;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        public MainForm()
        {
            Text = "Magical Princess Save Editor v0.1 (非官方 / unofficial)";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1240, 800);
            MinimumSize = new Size(1000, 640);
            Font = new Font("Microsoft YaHei UI", 9f);

            var detected = SaveStore.DetectSaveDir();
            store = detected != null ? new SaveStore(detected) : null;

            BuildMenu();
            BuildTabs();
            BuildStatus();
            RefreshSlotList();
            LoadSettingsTab();
        }

        // ---------------------------------------------------------------- menu

        private void BuildMenu()
        {
            var menu = new MenuStrip();
            var file = new ToolStripMenuItem("文件(&F)");
            file.DropDownItems.Add("自动定位存档目录", null, (s, e) => AutoLocateSaveDir());
            file.DropDownItems.Add("打开存档目录", null, (s, e) => OpenSaveDir());
            file.DropDownItems.Add("手动指定存档目录…", null, (s, e) => ManualChooseSaveDir());
            file.DropDownItems.Add("打开备份文件夹", null, (s, e) =>
            {
                if (store != null && System.IO.Directory.Exists(store.BackupDir))
                    System.Diagnostics.Process.Start("explorer.exe", store.BackupDir);
                else MessageBox.Show("还没有备份。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add("退出", null, (s, e) => Close());
            var help = new ToolStripMenuItem("帮助(&H)");
            help.DropDownItems.Add("项目主页 (GitHub)", null, (s, e) =>
            {
                try { System.Diagnostics.Process.Start("explorer.exe", "https://github.com/PensiveFei/MagicalPrincess-SaveEditor"); }
                catch { }
            });
            help.DropDownItems.Add("关于", null, (s, e) => MessageBox.Show(
                "Magical Princess Save Editor v0.1\n\n非官方存档修改器(只改存档文件,不碰游戏程序)。\n- 修改前请先关闭游戏。\n- 每次保存会自动备份原文件到存档目录 backups\\ 下。\n- 本项目不含任何游戏素材,与开发商/发行商无关。\n\n风险自负,开心游戏!",
                "关于", MessageBoxButtons.OK, MessageBoxIcon.Information));
            menu.Items.Add(file);
            menu.Items.Add(help);
            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        private void ApplyStore(SaveStore s)
        {
            store = s;
            pathLabel.Text = "存档目录: " + (s?.RootDir ?? "未定位");
            RefreshSlotList();
            LoadSettingsTab();
        }

        private void AutoLocateSaveDir()
        {
            var detected = SaveStore.DetectSaveDir();
            if (detected == null)
            {
                MessageBox.Show(
                    "没有自动找到存档目录。\n\n默认位置:\n" +
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Neotro Inc_", "MagicalPrincess") +
                    "\n\n注意:AppData 是 Windows 隐藏文件夹,资源管理器里要开启「查看 → 隐藏的项目」才看得见。\n也可以点「手动指定存档目录」直接粘贴路径。",
                    "自动定位失败", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ApplyStore(new SaveStore(detected));
            statusLabel.Text = "存档目录: " + detected;
            MessageBox.Show(
                "已找到存档目录:\n" + detected + "\n\n槽位列表已刷新,直接到「存档编辑」页选槽位即可。\n想看文件的话,点「文件 → 打开存档目录」(但不用打开任何文件,工具会自动读取)。",
                "自动定位成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenSaveDir()
        {
            if (store == null)
            {
                var detected = SaveStore.DetectSaveDir();
                if (detected == null)
                {
                    MessageBox.Show("还没有定位到存档目录,先点「自动定位存档目录」。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                store = new SaveStore(detected);
            }
            try { System.Diagnostics.Process.Start("explorer.exe", store.RootDir); }
            catch { }
        }

        private void ManualChooseSaveDir()
        {
            using var dlg = new SaveDirDialog(store?.RootDir);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                ApplyStore(new SaveStore(dlg.SelectedPath));
                statusLabel.Text = "存档目录: " + dlg.SelectedPath;
            }
        }

        private class SaveDirDialog : Form
        {
            public string SelectedPath => txt.Text.Trim();
            private readonly TextBox txt;

            public SaveDirDialog(string currentPath)
            {
                Text = "手动指定存档目录";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                ClientSize = new Size(640, 250);

                var tip = new Label
                {
                    Text = "这里填的是【文件夹】路径,不是文件——文件夹里应能看到 v10_userdata0.dat 等文件。\n默认位置:%USERPROFILE%\\AppData\\LocalLow\\Neotro Inc_\\MagicalPrincess(AppData 是隐藏文件夹,\n资源管理器里需开启「查看 → 隐藏的项目」才能看到)。\n不知道路径的话,直接点「自动检测」即可。",
                    Left = 12, Top = 10, Width = 616, Height = 92
                };
                txt = new TextBox { Left = 12, Top = 110, Width = 616 };
                txt.Text = currentPath ?? "";
                var btnDetect = new Button { Text = "自动检测", Left = 12, Top = 142, Width = 90 };
                btnDetect.Click += (s, e) =>
                {
                    var d = SaveStore.DetectSaveDir();
                    txt.Text = d ?? PathHint();
                };
                var btnBrowse = new Button { Text = "浏览…", Left = 110, Top = 142, Width = 90 };
                btnBrowse.Click += (s, e) =>
                {
                    using var fbd = new FolderBrowserDialog
                    {
                        Description = "选择包含 v10_*.dat 的文件夹(若看不到 AppData,请直接在输入框粘贴路径)",
                        SelectedPath = txt.Text
                    };
                    if (fbd.ShowDialog(this) == DialogResult.OK) txt.Text = fbd.SelectedPath;
                };
                var btnOk = new Button { Text = "确定", Left = 12, Top = 178, Width = 90, DialogResult = DialogResult.OK };
                var btnCancel = new Button { Text = "取消", Left = 110, Top = 178, Width = 90, DialogResult = DialogResult.Cancel };
                AcceptButton = btnOk;
                CancelButton = btnCancel;
                Controls.AddRange(new Control[] { tip, txt, btnDetect, btnBrowse, btnOk, btnCancel });
            }

            private static string PathHint() =>
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData", "LocalLow", "Neotro Inc_", "MagicalPrincess");

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                if (DialogResult == DialogResult.OK && !System.IO.Directory.Exists(SelectedPath))
                {
                    MessageBox.Show("这个路径不存在,请检查后再试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
                base.OnFormClosing(e);
            }
        }

        // ---------------------------------------------------------------- tabs

        private void BuildTabs()
        {
            tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(BuildBeginnerTab());
            tabs.TabPages.Add(BuildSaveTab());
            tabs.TabPages.Add(BuildSettingsTab());
            tabs.TabPages.Add(BuildAboutTab());
            Controls.Add(tabs);
        }

        private TabPage BuildSaveTab()
        {
            var page = new TabPage("存档编辑");
            var layout = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 380 };

            // left: slot list + open
            var left = new Panel { Dock = DockStyle.Fill };
            slotList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false };
            var cols = new[] { ("#", 40), ("名字", 110), ("周目", 45), ("日期", 130), ("体", 40), ("智", 40), ("魅", 40), ("感", 40), ("战", 40), ("艺", 40), ("魔", 40) };
            foreach (var (t, w) in cols) slotList.Columns.Add(t, w);
            slotList.DoubleClick += (s, e) => OpenSelectedSlot();
            var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(4) };
            btnOpenSlot = new Button { Text = "打开所选槽位", Width = 120, Height = 28 };
            btnOpenSlot.Click += (s, e) => OpenSelectedSlot();
            btnReloadSlots = new Button { Text = "刷新列表", Width = 90, Height = 28 };
            btnReloadSlots.Click += (s, e) => RefreshSlotList();
            bar.Controls.Add(btnOpenSlot);
            bar.Controls.Add(btnReloadSlots);
            pathLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(6, 4, 0, 0),
                AutoEllipsis = true
            };
            pathLabel.Text = "存档目录: " + (store?.RootDir ?? "未定位");
            left.Controls.Add(slotList);
            left.Controls.Add(pathLabel);
            left.Controls.Add(bar);

            // right: tree + edit + quick + save
            var right = new Panel { Dock = DockStyle.Fill };
            var rightSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 480 };

            jsonTree = new TreeView { Dock = DockStyle.Fill, HideSelection = false };
            jsonTree.BeforeExpand += TreeBeforeExpand;
            jsonTree.AfterSelect += TreeAfterSelect;

            var bottom = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            var grpEdit = new GroupBox { Text = "编辑选中字段", Dock = DockStyle.Top, Height = 150, Padding = new Padding(6) };
            lblEditPath = new Label { Dock = DockStyle.Top, Height = 44, AutoEllipsis = true };
            var editRow = new FlowLayoutPanel { Dock = DockStyle.Fill };
            txtEditValue = new TextBox { Width = 300, Height = 28 };
            cmbEditBool = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEditBool.Items.AddRange(new object[] { "true", "false" });
            cmbEditBool.Visible = false;
            btnApplyEdit = new Button { Text = "应用修改", Width = 90, Height = 28 };
            btnApplyEdit.Click += (s, e) => ApplyEditToTree();
            editRow.Controls.Add(lblEditPath);
            editRow.Controls.Add(txtEditValue);
            editRow.Controls.Add(cmbEditBool);
            editRow.Controls.Add(btnApplyEdit);
            grpEdit.Controls.Add(lblEditPath);
            grpEdit.Controls.Add(editRow);

            var grpQuick = new GroupBox { Text = "快捷修改 (留空 = 不改)", Dock = DockStyle.Top, Height = 120, Padding = new Padding(6) };
            var qRow1 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            var qRow2 = new FlowLayoutPanel { Dock = DockStyle.Fill };
            qMoney = MakeQuickBox(qRow1, "金钱 money");
            qStress = MakeQuickBox(qRow1, "压力 stress");
            qBlackCoin = MakeQuickBox(qRow1, "黑币 blackCoin");
            qActivePower = MakeQuickBox(qRow2, "行动力 activePower");
            qSkillPoint = MakeQuickBox(qRow2, "技能点 skillPoint");
            qFatherFav = MakeQuickBox(qRow2, "父亲好感 fatherFav");
            btnApplyQuick = new Button { Text = "应用快捷修改", Width = 110, Height = 28 };
            btnApplyQuick.Click += (s, e) => ApplyQuickEdits();
            qRow2.Controls.Add(btnApplyQuick);
            grpQuick.Controls.Add(qRow1);
            grpQuick.Controls.Add(qRow2);

            var saveRow = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(4) };
            btnSave = new Button { Text = "保存到存档 (自动备份)", Width = 180, Height = 30 };
            btnSave.Click += (s, e) => SaveCurrent();
            lblDirty = new Label { Text = "未修改", AutoSize = true, Padding = new Padding(8, 6, 0, 0) };
            saveRow.Controls.Add(btnSave);
            saveRow.Controls.Add(lblDirty);

            bottom.Controls.Add(grpQuick);
            bottom.Controls.Add(grpEdit);
            bottom.Controls.Add(saveRow);
            rightSplit.Panel1.Controls.Add(jsonTree);
            rightSplit.Panel2.Controls.Add(bottom);
            right.Controls.Add(rightSplit);

            layout.Panel1.Controls.Add(left);
            layout.Panel2.Controls.Add(right);
            page.Controls.Add(layout);
            return page;
        }

        private TextBox MakeQuickBox(FlowLayoutPanel parent, string hint)
        {
            var box = new TextBox { Width = 150, Height = 26, Margin = new Padding(4) };
            var tt = new ToolTip();
            tt.SetToolTip(box, hint);
            parent.Controls.Add(box);
            return box;
        }

        private TabPage BuildSettingsTab()
        {
            var page = new TabPage("游戏设置");
            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };

            var grpGfx = new GroupBox { Text = "画面设置 (v10_devicedata.cfg)", Left = 12, Top = 12, Width = 520, Height = 170 };
            chkFullScreen = new CheckBox { Text = "全屏 (fs)", Left = 16, Top = 24, AutoSize = true };
            var lblRes = new Label { Text = "分辨率 (re):", Left = 16, Top = 56, AutoSize = true };
            cmbResolution = new ComboBox { Left = 120, Top = 52, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbResolution.Items.AddRange(ResolutionNames);
            var lblQ = new Label { Text = "画质 (qlt):", Left = 16, Top = 92, AutoSize = true };
            cmbQuality = new ComboBox { Left = 120, Top = 88, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbQuality.Items.AddRange(QualityNames);
            chkVSync = new CheckBox { Text = "垂直同步 (vs)", Left = 16, Top = 128, AutoSize = true };
            grpGfx.Controls.AddRange(new Control[] { chkFullScreen, lblRes, cmbResolution, lblQ, cmbQuality, chkVSync });

            var grpCfg = new GroupBox { Text = "常规设置 (v10_configdata.dat)", Left = 12, Top = 196, Width = 520, Height = 200 };
            var lblLang = new Label { Text = "语言 (lt):", Left = 16, Top = 24, AutoSize = true };
            cmbLang = new ComboBox { Left = 120, Top = 20, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLang.Items.AddRange(LangNames);
            var lblMs = new Label { Text = "文本速度 (ms):", Left = 16, Top = 60, AutoSize = true };
            cmbMsgSpeed = new ComboBox { Left = 120, Top = 56, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMsgSpeed.Items.AddRange(MsgSpeedNames);
            var lblBGM = new Label { Text = "BGM 音量 (vb):", Left = 16, Top = 96, AutoSize = true };
            txtBGM = new TextBox { Left = 120, Top = 92, Width = 200 };
            var lblSE = new Label { Text = "音效音量 (ve):", Left = 16, Top = 132, AutoSize = true };
            txtSE = new TextBox { Left = 120, Top = 128, Width = 200 };
            var lblVoice = new Label { Text = "语音音量 (vv):", Left = 16, Top = 168, AutoSize = true };
            txtVoice = new TextBox { Left = 120, Top = 164, Width = 200 };
            grpCfg.Controls.AddRange(new Control[] { lblLang, cmbLang, lblMs, cmbMsgSpeed, lblBGM, txtBGM, lblSE, txtSE, lblVoice, txtVoice });

            var btnRow = new FlowLayoutPanel { Left = 12, Top = 410, Width = 520, Height = 48 };
            btnSaveSettings = new Button { Text = "保存设置", Width = 120, Height = 30 };
            btnSaveSettings.Click += (s, e) => SaveSettings();
            btnReloadSettings = new Button { Text = "重新读取", Width = 120, Height = 30 };
            btnReloadSettings.Click += (s, e) => LoadSettingsTab();
            btnRow.Controls.Add(btnSaveSettings);
            btnRow.Controls.Add(btnReloadSettings);
            var lblNote = new Label
            {
                Text = "提示:修改前请关闭游戏。分辨率档位对应游戏内置选项;全屏+窗口化由 fs 控制。",
                Left = 12, Top = 470, AutoSize = true, ForeColor = Color.Gray
            };
            panel.Controls.AddRange(new Control[] { grpGfx, grpCfg, btnRow, lblNote });
            page.Controls.Add(panel);
            return page;
        }

        private TabPage BuildAboutTab()
        {
            var page = new TabPage("说明");
            var box = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Microsoft YaHei UI", 10f)
            };
            box.Text = @"Magical Princess Save Editor — 非官方存档修改器(只改存档文件,不碰游戏程序)

【功能】
1. 存档编辑:列出 31 个槽位,以树状结构编辑任意 JSON 字段(金钱、压力、
   黑币、等级、物品、好感度等),支持快捷修改常用数值。
2. 游戏设置:修改分辨率档位 / 全屏 / 画质 / 垂直同步 / 语言 / 音量 / 文本速度。
3. 每次保存自动备份原文件到存档目录 backups 下。

【使用步骤】
0. 如果没找到存档:它默认在 %USERPROFILE%\AppData\LocalLow\Neotro Inc_\MagicalPrincess
   (AppData 是隐藏文件夹,资源管理器需开启「查看 → 隐藏的项目」)。
   存档目录里的 .dat 文件是加密数据,双击会弹「打开方式」窗口,属正常现象,
   不用理会——本工具自动读取整个文件夹,你不需要打开任何文件。
1. 关闭游戏(重要!游戏退出时会写存档,会覆盖你的修改)。
2. 打开本工具,选择槽位,修改数值,点「保存到存档」。
3. 进游戏读档,确认数值生效。

【安全与合规】
- 本工具不包含任何游戏素材,不修改游戏程序本体,只读写存档文件。
- 与开发商 Neotro Inc. / 发行商 MAGI Inc. 无关,非官方。
- 备份文件可在「文件 → 打开备份文件夹」中找回。
- Steam 云存档:修改后的文件会被正常同步,一般无影响。
- 风险自负:极端数值可能破坏游戏体验或触发成就异常,建议适度修改。";
            page.Controls.Add(box);
            return page;
        }

        // ---------------------------------------------------------------- slots

        private void RefreshSlotList()
        {
            slotList.Items.Clear();
            if (store == null)
            {
                statusLabel.Text = "未找到存档目录,请用「文件 → 选择存档目录」手动指定。";
                return;
            }
            statusLabel.Text = "存档目录: " + store.RootDir;
            try
            {
                foreach (var s in store.ReadSlots())
                {
                    var item = new ListViewItem(s.SlotId.ToString())
                    {
                        Tag = s.SlotId,
                        SubItems =
                        {
                            s.PlayerName, s.LoopCount.ToString(), s.Date,
                            s.LevelPhysical.ToString(), s.LevelIntelligence.ToString(),
                            s.LevelCharm.ToString(), s.LevelSense.ToString(),
                            s.LevelBattle.ToString(), s.LevelArts.ToString(), s.LevelMagic.ToString()
                        }
                    };
                    item.ForeColor = s.Exists ? SystemColors.WindowText : Color.Gray;
                    slotList.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取槽位列表失败:\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            RefreshSlotCombo();
        }

        private void OpenSelectedSlot()
        {
            if (store == null) return;
            if (slotList.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选择一个槽位。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            LoadSlot((int)slotList.SelectedItems[0].Tag);
        }

        private void LoadSlot(int slot)
        {
            if (store == null) return;
            try
            {
                currentUser = store.LoadUserData(slot);
                currentSlot = slot;
                userDirty = false;
                UpdateDirtyLabel();
                BuildTreeRoot();
                PopulateBeginnerTab();
                statusLabel.Text = "已打开槽位 " + slot + " | 存档目录: " + store.RootDir;
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开槽位失败:\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class SlotItem
        {
            public int Id;
            public string Text;
            public override string ToString() => Text;
        }

        private void RefreshSlotCombo()
        {
            if (cmbSlot == null) return;
            int selected = cmbSlot.SelectedItem is SlotItem s ? s.Id : -1;
            cmbSlot.BeginUpdate();
            cmbSlot.Items.Clear();
            if (store != null)
            {
                try
                {
                    foreach (var info in store.ReadSlots())
                    {
                        var text = info.Exists
                            ? "#" + info.SlotId + " | " + (string.IsNullOrEmpty(info.PlayerName) ? "(未命名)" : info.PlayerName)
                              + " | 周目" + info.LoopCount + " | " + info.Date
                            : "#" + info.SlotId + " | (空)";
                        cmbSlot.Items.Add(new SlotItem { Id = info.SlotId, Text = text });
                    }
                }
                catch { }
            }
            for (int i = 0; i < cmbSlot.Items.Count; i++)
            {
                if (((SlotItem)cmbSlot.Items[i]).Id == selected)
                {
                    cmbSlot.SelectedIndex = i;
                    break;
                }
            }
            cmbSlot.EndUpdate();
        }

        // ---------------------------------------------------------------- tree

        private void BuildTreeRoot()
        {
            jsonTree.BeginUpdate();
            jsonTree.Nodes.Clear();
            foreach (var prop in currentUser.Properties())
            {
                var node = new TreeNode { Tag = prop.Value };
                node.Text = NodeText(prop.Name, prop.Value);
                if (IsContainer(prop.Value)) node.Nodes.Add(DummyNode);
                jsonTree.Nodes.Add(node);
            }
            jsonTree.EndUpdate();
        }

        private static readonly TreeNode DummyNode = new TreeNode("…");

        private static bool IsContainer(JToken t) =>
            (t is JObject o && o.Count > 0) || (t is JArray a && a.Count > 0);

        private static string NodeText(string name, JToken t)
        {
            if (t is JObject o) return name + "  {" + o.Count + "}";
            if (t is JArray a) return name + "  [" + a.Count + "]";
            return name + "  =  " + ValueText((JValue)t);
        }

        private static string ValueText(JValue v)
        {
            switch (v.Type)
            {
                case JTokenType.String: return "\"" + (string)v.Value + "\"";
                case JTokenType.Boolean: return (bool)v.Value ? "true" : "false";
                case JTokenType.Null: return "null";
                default: return Convert.ToString(v.Value, CultureInfo.InvariantCulture);
            }
        }

        private void TreeBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node;
            if (node.Tag is JToken tok && node.Nodes.Count == 1 && node.Nodes[0].Text == "…")
            {
                jsonTree.BeginUpdate();
                node.Nodes.Clear();
                if (tok is JObject obj)
                {
                    foreach (var prop in obj.Properties())
                    {
                        var child = new TreeNode { Tag = prop.Value };
                        child.Text = NodeText(prop.Name, prop.Value);
                        if (IsContainer(prop.Value)) child.Nodes.Add(DummyNode);
                        node.Nodes.Add(child);
                    }
                }
                else if (tok is JArray arr)
                {
                    for (int i = 0; i < arr.Count; i++)
                    {
                        var item = arr[i];
                        var child = new TreeNode { Tag = item };
                        child.Text = NodeText("[" + i + "]", item);
                        if (IsContainer(item)) child.Nodes.Add(DummyNode);
                        node.Nodes.Add(child);
                    }
                }
                jsonTree.EndUpdate();
            }
        }

        private void TreeAfterSelect(object sender, TreeViewEventArgs e)
        {
            var node = e.Node;
            if (node?.Tag is JValue val)
            {
                lblEditPath.Text = "路径: " + node.FullPath.Replace(jsonTree.PathSeparator, ".");
                if (val.Type == JTokenType.Boolean)
                {
                    cmbEditBool.Visible = true;
                    cmbEditBool.SelectedIndex = (bool)val.Value ? 0 : 1;
                    txtEditValue.Visible = false;
                }
                else
                {
                    cmbEditBool.Visible = false;
                    txtEditValue.Visible = true;
                    txtEditValue.Text = val.Type == JTokenType.String
                        ? (string)val.Value
                        : Convert.ToString(val.Value, CultureInfo.InvariantCulture);
                }
            }
            else
            {
                lblEditPath.Text = "选中一个叶子字段后可编辑";
                cmbEditBool.Visible = false;
                txtEditValue.Visible = true;
                txtEditValue.Text = "";
            }
        }

        private void ApplyEditToTree()
        {
            var node = jsonTree.SelectedNode;
            if (node?.Tag is not JValue old)
            {
                MessageBox.Show("请先选中一个叶子字段。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                JValue replacement;
                if (old.Type == JTokenType.Boolean)
                {
                    replacement = new JValue(cmbEditBool.Text == "true");
                }
                else if (old.Type == JTokenType.Integer)
                {
                    if (!long.TryParse(txtEditValue.Text.Trim(), out var iv))
                    {
                        MessageBox.Show("请输入整数。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    replacement = new JValue(iv);
                }
                else if (old.Type == JTokenType.Float)
                {
                    if (!double.TryParse(txtEditValue.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var dv))
                    {
                        MessageBox.Show("请输入数字(可用小数)。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    replacement = new JValue(dv);
                }
                else
                {
                    replacement = new JValue(txtEditValue.Text);
                }
                old.Replace(replacement);
                node.Tag = replacement;
                node.Text = NodeText(LeafName(node), replacement);
                userDirty = true;
                UpdateDirtyLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改失败:\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string LeafName(TreeNode node)
        {
            var text = node.Text;
            var idx = text.IndexOf("  =  ", StringComparison.Ordinal);
            return idx >= 0 ? text.Substring(0, idx) : text;
        }

        // ---------------------------------------------------------------- quick edits

        private void ApplyQuickEdits()
        {
            if (currentUser == null)
            {
                MessageBox.Show("请先打开一个槽位。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var status = currentUser["status"] as JObject;
            if (status == null)
            {
                MessageBox.Show("存档中没有 status 对象。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var changed = false;
            changed |= SetInt(status, "m", qMoney);
            changed |= SetInt(status, "st", qStress);
            changed |= SetInt(status, "bc", qBlackCoin);
            changed |= SetInt(status, "ap", qActivePower);
            changed |= SetInt(status, "skp", qSkillPoint);
            changed |= SetInt(status, "ff", qFatherFav);
            if (changed)
            {
                userDirty = true;
                UpdateDirtyLabel();
                BuildTreeRoot();
                statusLabel.Text = "快捷修改已应用到内存,点「保存到存档」写入文件。";
            }
        }

        private static bool SetInt(JObject obj, string prop, TextBox box)
        {
            var t = box.Text.Trim();
            if (t.Length == 0) return false;
            if (!int.TryParse(t, out var v))
            {
                MessageBox.Show("字段 " + prop + " 请输入整数。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            obj[prop] = new JValue(v);
            return true;
        }

        private void SaveCurrent()
        {
            if (store == null || currentUser == null || currentSlot < 0)
            {
                MessageBox.Show("请先打开一个槽位。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                store.SaveUserData(currentSlot, currentUser);
                store.SyncIndexFromUserData(currentSlot, currentUser);
                userDirty = false;
                UpdateDirtyLabel();
                RefreshSlotList();
                statusLabel.Text = "已保存槽位 " + currentSlot + ",原文件已备份到 backups 文件夹。";
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败:\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDirtyLabel()
        {
            lblDirty.Text = userDirty ? "有未保存的修改!" : "未修改";
            lblDirty.ForeColor = userDirty ? Color.Red : Color.Gray;
        }

        // ---------------------------------------------------------------- beginner tab

        private class BeginnerField
        {
            public string Label;
            public string Key;
            public bool IsGStatus;
            public TextBox Box;
            public Label Cur;
        }

        private class FriendField
        {
            public int Id;
            public TextBox Box;
            public Label Cur;
        }

        private static readonly Dictionary<int, string> FriendNames = new Dictionary<int, string>
        {
            { 0, "库洛瓦" }, { 1, "修可拉" }, { 2, "弗兰" }, { 3, "可罗奈莉亚" },
            { 4, "夏尔" }, { 5, "哈希斯" }, { 6, "诺亚" },
        };

        private TabPage BuildBeginnerTab()
        {
            var page = new TabPage("常用修改 (新手)");
            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };

            var slotBar = new FlowLayoutPanel { Left = 12, Top = 4, Width = 920, Height = 44 };
            var lblSlot = new Label { Text = "选择存档槽位:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
            cmbSlot = new ComboBox { Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSlot.SelectedIndexChanged += (s, e) =>
            {
                if (cmbSlot.SelectedItem is SlotItem item) LoadSlot(item.Id);
            };
            var btnRefreshSlots = new Button { Text = "刷新槽位", Width = 90, Height = 28 };
            btnRefreshSlots.Click += (s, e) => RefreshSlotList();
            slotBar.Controls.Add(lblSlot);
            slotBar.Controls.Add(cmbSlot);
            slotBar.Controls.Add(btnRefreshSlots);
            panel.Controls.Add(slotBar);

            var groups = new (string, (string, string, bool)[])[]
            {
                ("基本资料", new[]
                {
                    ("玩家名 (文字)", "pn", true),
                    ("父亲名 (文字)", "fn", true),
                    ("父亲昵称 (文字)", "fg", true),
                    ("周目", "c", true),
                    ("功绩点", "ap", true),
                    ("可罗奈莉亚最高好感", "cmf", true),
                }),
                ("顶部栏目", new[]
                {
                    ("金钱", "m", false),
                    ("累计获得金钱", "mgt", false),
                    ("行动力", "ap", false),
                    ("行动力上限", "am", false),
                    ("压力", "st", false),
                    ("东亚硬币", "bc", false),
                    ("善行", "ga", false),
                    ("恶行", "ba", false),
                }),
                ("属性 (体力/智力/魅力/感性由下列细项自动算出,改细项即可)", new[]
                {
                    ("力量", "p1", false), ("生命", "p2", false), ("意志", "p3", false), ("敏捷", "p4", false),
                    ("文学", "i1", false), ("算术", "i2", false), ("魔术", "i3", false), ("信仰", "i4", false),
                    ("美貌", "c1", false), ("社交", "c2", false), ("礼仪", "c3", false), ("道德", "c4", false),
                    ("想象", "s1", false), ("创意", "s2", false), ("音乐", "s3", false), ("美术", "s4", false),
                    ("父女感情", "ff", false),
                    ("名声", "r", false),
                    ("技能点数", "skp", false),
                }),
                ("七项等级", new[]
                {
                    ("体力等级", "lp", false),
                    ("智力等级", "li", false),
                    ("魅力等级", "lc", false),
                    ("感性等级", "ls", false),
                    ("战斗等级", "lb", false),
                    ("艺术等级", "la", false),
                    ("魔法等级", "lm", false),
                }),
                ("战斗与装备", new[]
                {
                    ("战斗经验", "b0", false),
                    ("衣服 (-1=未装备)", "ec", false),
                    ("衣服外观", "ecl", false),
                    ("武器 (-1=未装备)", "ew", false),
                    ("护甲 (-1=未装备)", "ea", false),
                }),
                ("价格/工资倍率 (%)", new[]
                {
                    ("工资倍率", "ksl", false),
                    ("购买价格倍率", "bpr", false),
                    ("出售价格倍率", "spr", false),
                }),
            };

            int y = 52;
            foreach (var (title, fields) in groups)
            {
                var grp = new GroupBox { Text = title, Left = 12, Top = y, Width = 900 };
                int rowY = 24;
                int col = 0;
                foreach (var (label, key, isG) in fields)
                {
                    var rowX = 12 + (col % 3) * 290;
                    var lbl = new Label { Text = label, Left = rowX, Top = rowY, Width = 128 };
                    var box = new TextBox { Left = rowX + 132, Top = rowY - 2, Width = 70 };
                    var cur = new Label { Left = rowX + 206, Top = rowY, Width = 82, ForeColor = Color.Gray, AutoSize = true };
                    grp.Controls.Add(lbl);
                    grp.Controls.Add(box);
                    grp.Controls.Add(cur);
                    beginnerFields.Add(new BeginnerField { Label = label, Key = key, IsGStatus = isG, Box = box, Cur = cur });
                    col++;
                    if (col % 3 == 0) rowY += 30;
                }
                grp.Height = rowY + 46;
                y += grp.Height + 8;
                panel.Controls.Add(grp);
            }

            var friendGrp = new GroupBox
            {
                Text = "伙伴好感 (friendDataParamList)",
                Left = 12, Top = y, Width = 900, Height = 430
            };
            friendFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(6) };
            friendGrp.Controls.Add(friendFlow);
            panel.Controls.Add(friendGrp);
            y += 438;

            var saveRow = new FlowLayoutPanel { Left = 12, Top = y, Width = 900, Height = 56 };
            var btnSaveAll = new Button { Text = "保存全部修改 (留空 = 不改)", Width = 240, Height = 32 };
            btnSaveAll.Click += (s, e) => SaveBeginner();
            var tip = new Label
            {
                Text = "用法:先到「存档编辑」页选择槽位,再回到本页修改 → 保存。输入框留空表示保持原值。",
                AutoSize = true, Padding = new Padding(10, 8, 0, 0)
            };
            saveRow.Controls.Add(btnSaveAll);
            saveRow.Controls.Add(tip);
            panel.Controls.Add(saveRow);

            page.Controls.Add(panel);
            return page;
        }

        private void PopulateBeginnerTab()
        {
            foreach (var f in beginnerFields)
            {
                var obj = f.IsGStatus ? currentUser["gstatus"] as JObject : currentUser["status"] as JObject;
                var tok = obj?[f.Key];
                if (tok is JValue v && v.Type != JTokenType.Null)
                {
                    f.Box.Text = v.Type == JTokenType.String
                        ? (string)v.Value
                        : Convert.ToString(v.Value, CultureInfo.InvariantCulture);
                    f.Cur.Text = "当前: " + f.Box.Text;
                }
                else
                {
                    f.Box.Text = "";
                    f.Cur.Text = "当前: (无)";
                }
            }

            friendFlow.Controls.Clear();
            friendFields.Clear();
            var friends = currentUser["friendDataParamList"] as JArray;
            if (friends != null)
            {
                foreach (var t in friends)
                {
                    if (t is not JObject fo) continue;
                    int id = fo.Value<int?>("id") ?? -1;
                    if (id < 0 || !FriendNames.ContainsKey(id)) continue;
                    string name = FriendNames[id];
                    var row = new Panel { Width = 284, Height = 30 };
                    var lbl = new Label { Text = name + " (#" + id + ")", Left = 2, Top = 5, Width = 150, AutoEllipsis = true };
                    var box = new TextBox { Left = 156, Top = 2, Width = 64 };
                    var cur = new Label { Left = 226, Top = 5, Width = 58, ForeColor = Color.Gray, AutoSize = true };
                    var fav = fo["f"];
                    if (fav is JValue fv && fv.Type != JTokenType.Null)
                    {
                        box.Text = Convert.ToString(fv.Value, CultureInfo.InvariantCulture);
                        cur.Text = fv.ToString();
                    }
                    row.Controls.Add(lbl);
                    row.Controls.Add(box);
                    row.Controls.Add(cur);
                    friendFlow.Controls.Add(row);
                    friendFields.Add(new FriendField { Id = id, Box = box, Cur = cur });
                }
            }
        }

        private void SaveBeginner()
        {
            if (store == null || currentUser == null || currentSlot < 0)
            {
                MessageBox.Show("请先打开一个槽位。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var changed = false;
            foreach (var f in beginnerFields)
            {
                var text = f.Box.Text.Trim();
                if (text.Length == 0) continue;
                var obj = f.IsGStatus ? currentUser["gstatus"] as JObject : currentUser["status"] as JObject;
                if (obj == null || obj[f.Key] == null) continue;
                try
                {
                    if (f.Key == "pn" || f.Key == "fn" || f.Key == "fg")
                    {
                        obj[f.Key] = new JValue(text);
                    }
                    else if (!int.TryParse(text, out var iv))
                    {
                        MessageBox.Show("字段「" + f.Label + "」请输入整数。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        obj[f.Key] = new JValue(iv);
                    }
                    changed = true;
                }
                catch { }
            }
            var friendArr = currentUser["friendDataParamList"] as JArray;
            foreach (var f in friendFields)
            {
                var text = f.Box.Text.Trim();
                if (text.Length == 0) continue;
                if (!int.TryParse(text, out var fv))
                {
                    MessageBox.Show("伙伴 #" + f.Id + " 的好感请输入整数。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (friendArr != null)
                {
                    foreach (var t in friendArr)
                    {
                        if (t is JObject fo && fo.Value<int?>("id") == f.Id)
                        {
                            fo["f"] = new JValue(fv);
                            changed = true;
                            break;
                        }
                    }
                }
            }
            if (!changed)
            {
                MessageBox.Show("没有需要修改的内容(输入框都为空)。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                store.SaveUserData(currentSlot, currentUser);
                store.SyncIndexFromUserData(currentSlot, currentUser);
                userDirty = false;
                UpdateDirtyLabel();
                RefreshSlotList();
                BuildTreeRoot();
                PopulateBeginnerTab();
                statusLabel.Text = "已保存槽位 " + currentSlot + ",原文件已备份到 backups 文件夹。";
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败:\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------------------- settings tab

        private void LoadSettingsTab()
        {
            if (store == null) return;
            try
            {
                if (store.Exists(SaveStore.DeviceFile))
                {
                    var dev = store.LoadDevice();
                    chkFullScreen.Checked = dev.Value<bool?>("fs") ?? true;
                    var re = dev.Value<int?>("re") ?? 4;
                    cmbResolution.SelectedIndex = Math.Clamp(re, 0, ResolutionNames.Length - 1);
                    var qlt = dev.Value<int?>("qlt") ?? 2;
                    cmbQuality.SelectedIndex = Math.Clamp(qlt, 0, QualityNames.Length - 1);
                    chkVSync.Checked = dev.Value<bool?>("vs") ?? false;
                }
                if (store.Exists(SaveStore.ConfigFile))
                {
                    var cfg = store.LoadConfig();
                    var lt = cfg.Value<int?>("lt") ?? 0;
                    cmbLang.SelectedIndex = Math.Clamp(lt, 0, LangNames.Length - 1);
                    var ms = cfg.Value<float?>("ms") ?? 1f;
                    int msIdx = 0;
                    for (int i = 0; i < MsgSpeedValues.Length; i++)
                        if (Math.Abs(MsgSpeedValues[i] - ms) < 0.001f) msIdx = i;
                    cmbMsgSpeed.SelectedIndex = msIdx;
                    txtBGM.Text = (cfg.Value<float?>("vb") ?? 0.5f).ToString(CultureInfo.InvariantCulture);
                    txtSE.Text = (cfg.Value<float?>("ve") ?? 0.5f).ToString(CultureInfo.InvariantCulture);
                    txtVoice.Text = (cfg.Value<float?>("vv") ?? 1f).ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取设置失败:\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            if (store == null) return;
            try
            {
                var dev = store.Exists(SaveStore.DeviceFile) ? store.LoadDevice() : new JObject();
                dev["fs"] = chkFullScreen.Checked;
                dev["re"] = cmbResolution.SelectedIndex;
                dev["qlt"] = cmbQuality.SelectedIndex;
                dev["vs"] = chkVSync.Checked;
                store.SaveDevice(dev);

                var cfg = store.Exists(SaveStore.ConfigFile) ? store.LoadConfig() : new JObject();
                cfg["lt"] = cmbLang.SelectedIndex;
                cfg["ms"] = MsgSpeedValues[cmbMsgSpeed.SelectedIndex];
                if (float.TryParse(txtBGM.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var vb)) cfg["vb"] = vb;
                if (float.TryParse(txtSE.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var ve)) cfg["ve"] = ve;
                if (float.TryParse(txtVoice.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var vv)) cfg["vv"] = vv;
                store.SaveConfig(cfg);

                statusLabel.Text = "设置已保存(已备份原文件)。重启游戏生效。";
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存设置失败:\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------------------- status bar

        private void BuildStatus()
        {
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("就绪");
            statusStrip.Items.Add(statusLabel);
            Controls.Add(statusStrip);
        }
    }
}