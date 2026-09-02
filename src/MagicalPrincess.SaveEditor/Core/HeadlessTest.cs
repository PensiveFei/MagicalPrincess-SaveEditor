using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MagicalPrincess.SaveEditor.Core
{
    /// <summary>
    /// --headless mode: verify the whole read/modify/write pipeline on the real
    /// save files WITHOUT touching them (writes go to the given output dir).
    /// Also dumps a labeled menu of editable fields with current values to
    /// menu-dump.txt (UTF-8).
    /// </summary>
    public static class HeadlessTest
    {
        public static void Run(string outDir)
        {
            outDir = outDir ?? Path.Combine(Path.GetTempPath(), "mpse-headless");
            Directory.CreateDirectory(outDir);
            var src = SaveStore.DetectSaveDir();
            Console.WriteLine("save dir detected: " + (src ?? "(none)"));
            if (src == null) return;

            var store = new SaveStore(src);

            var index = store.LoadIndex();
            Console.WriteLine("index slots: " + (index["dataList"] as JArray)?.Count);

            var user = store.LoadUserData(0);

            var menu = new StringWriter();
            DumpMenu(user, menu);
            File.WriteAllText(Path.Combine(outDir, "menu-dump.txt"), menu.ToString());

            // modify money and re-serialize -> encrypt -> write into outDir (not the real dir!)
            ((JObject)user["status"])["m"] = new JValue(12345);
            var plain = user.ToString(Newtonsoft.Json.Formatting.None);
            var enc = SaveCrypto.Encrypt(plain);
            File.WriteAllText(Path.Combine(outDir, store.UserDataFile(0)), enc);

            // read it back through the normal pipeline
            var testStore = new SaveStore(outDir);
            var back = testStore.LoadUserData(0);
            var moneyBack = back["status"]?.Value<int>("m");
            Console.WriteLine("roundtrip money: " + moneyBack + (moneyBack == 12345 ? "  [OK]" : "  [FAIL]"));
            Console.WriteLine("headless done");
        }

        private static void DumpMenu(JObject user, TextWriter w)
        {
            var gs = user["gstatus"] as JObject;
            var st = user["status"] as JObject;
            var groups = new (string, (string, string, bool)[])[]
            {
                ("基本资料", new[]
                {
                    ("玩家名", "pn", true), ("父亲名", "fn", true), ("父亲昵称", "fg", true),
                    ("周目", "c", true), ("功绩点", "ap", true), ("柯内特最高好感", "cmf", true),
                }),
                ("金钱与资源", new[]
                {
                    ("金钱", "m", false), ("累计获得金钱", "mgt", false), ("黑币", "bc", false),
                    ("行动力", "ap", false), ("行动力上限", "am", false), ("技能点", "skp", false),
                }),
                ("心态与关系", new[]
                {
                    ("压力", "st", false), ("善良", "ga", false), ("恶行", "ba", false),
                    ("善恶平衡", "bl", false), ("父亲好感", "ff", false), ("父亲好感等级", "fv", false),
                    ("名誉", "r", false),
                }),
                ("七项等级", new[]
                {
                    ("体力等级", "lp", false), ("智力等级", "li", false), ("魅力等级", "lc", false),
                    ("感性等级", "ls", false), ("战斗等级", "lb", false), ("艺术等级", "la", false),
                    ("魔法等级", "lm", false), ("体力经验", "vp", false), ("智力经验", "vi", false),
                    ("魅力经验", "vc", false), ("感性经验", "vs", false),
                }),
                ("细项属性", new[]
                {
                    ("筋力", "p1", false), ("生命", "p2", false), ("根性", "p3", false), ("敏捷", "p4", false),
                    ("文学", "i1", false), ("算数", "i2", false), ("魔术", "i3", false), ("信仰", "i4", false),
                    ("美貌", "c1", false), ("社交", "c2", false), ("礼仪", "c3", false), ("道德", "c4", false),
                    ("创造", "s1", false), ("创作", "s2", false), ("音感", "s3", false), ("美感", "s4", false),
                }),
                ("战斗与装备", new[]
                {
                    ("战斗经验", "b0", false), ("衣服(-1=无)", "ec", false), ("衣服外观", "ecl", false),
                    ("武器(-1=无)", "ew", false), ("护甲(-1=无)", "ea", false),
                }),
                ("倍率%", new[]
                {
                    ("工资倍率", "ksl", false), ("购买价格倍率", "bpr", false), ("出售价格倍率", "spr", false),
                }),
            };
            w.WriteLine("=== MENU (current values, slot 0) ===");
            foreach (var (title, fields) in groups)
            {
                w.WriteLine("[" + title + "]");
                foreach (var (label, key, isG) in fields)
                {
                    var obj = isG ? gs : st;
                    var v = obj?[key];
                    w.WriteLine("  " + label + " = " + (v?.Type == JTokenType.Null || v == null ? "(无)" : v.ToString()));
                }
            }
            w.WriteLine("[列表数据(高级编辑页可改)]");
            foreach (var key in new[] { "itemDataParamList(物品: id/数量)", "friendDataParamList(好友: 好感/约会/送礼)", "skillDataParamList(技能: 解锁/学习)", "battleArtsDataParamList(战斗技能)", "activityDataParamList(活动)", "curriculumDataParamList(课程)", "statusDataHistory(成长历史)", "flagEventReaded(已读事件)" })
            {
                var t = user[key.Split('(')[0]] as JArray;
                w.WriteLine("  " + key + " -> " + (t == null ? "(无)" : "共 " + t.Count + " 条"));
            }
        }
    }
}