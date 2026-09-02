using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MagicalPrincess.SaveEditor.Core
{
    /// <summary>
    /// --headless mode: verify the whole read/modify/write pipeline on the real
    /// save files WITHOUT touching them (writes go to the given output dir).
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
            Console.WriteLine("slot0 player: " + user["gstatus"]?["pn"] + "  money: " + user["status"]?["m"]);

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
    }
}