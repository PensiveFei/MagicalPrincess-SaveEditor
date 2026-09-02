using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MagicalPrincess.SaveEditor.Core
{
    /// <summary>
    /// Locates the save directory, reads/writes the encrypted save files and
    /// keeps timestamped backups of every file it overwrites.
    /// </summary>
    public class SaveStore
    {
        public const string IndexFile = "v10_indexdata.dat";
        public const string ConfigFile = "v10_configdata.dat";
        public const string DeviceFile = "v10_devicedata.cfg";
        public const int SlotTotal = 31;

        public string RootDir { get; }
        public string BackupDir => Path.Combine(RootDir, "backups");

        public SaveStore(string rootDir)
        {
            RootDir = rootDir;
        }

        /// <summary>Auto-detect the game save folder under LocalLow.</summary>
        public static string DetectSaveDir()
        {
            var localLow = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow");
            if (!Directory.Exists(localLow)) return null;
            foreach (var dir in Directory.GetDirectories(localLow, "Neotro Inc*"))
            {
                var game = Path.Combine(dir, "MagicalPrincess");
                if (Directory.Exists(game)) return game;
            }
            return null;
        }

        public string UserDataFile(int slot) => "v10_userdata" + slot + ".dat";

        public string FullPath(string fileName) => Path.Combine(RootDir, fileName);

        public bool Exists(string fileName) => File.Exists(FullPath(fileName));

        public JObject LoadJson(string fileName)
        {
            var text = File.ReadAllText(FullPath(fileName));
            return JObject.Parse(SaveCrypto.Decrypt(text));
        }

        private void Backup(string fileName)
        {
            if (!Exists(fileName)) return;
            Directory.CreateDirectory(BackupDir);
            var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var dest = Path.Combine(BackupDir, fileName + ".bak-" + stamp);
            File.Copy(FullPath(fileName), dest, true);
        }

        public void SaveJson(string fileName, JObject json)
        {
            Backup(fileName);
            var plain = json.ToString(Formatting.None);
            var encrypted = SaveCrypto.Encrypt(plain);
            var path = FullPath(fileName);
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, encrypted, new System.Text.UTF8Encoding(false));
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        public JObject LoadUserData(int slot) => LoadJson(UserDataFile(slot));
        public void SaveUserData(int slot, JObject json) => SaveJson(UserDataFile(slot), json);
        public JObject LoadIndex() => LoadJson(IndexFile);
        public void SaveIndex(JObject json) => SaveJson(IndexFile, json);
        public JObject LoadConfig() => LoadJson(ConfigFile);
        public void SaveConfig(JObject json) => SaveJson(ConfigFile, json);
        public JObject LoadDevice() => LoadJson(DeviceFile);
        public void SaveDevice(JObject json) => SaveJson(DeviceFile, json);

        public List<SlotInfo> ReadSlots()
        {
            var result = new List<SlotInfo>();
            var index = LoadIndex();
            var list = index["dataList"] as JArray;
            for (int i = 0; i < SlotTotal; i++)
            {
                var info = new SlotInfo { SlotId = i, Exists = false };
                if (list != null && i < list.Count && list[i] is JObject o)
                {
                    info.Exists = Exists(UserDataFile(i));
                    info.IsPlaying = o.Value<bool?>("ip") ?? false;
                    info.PlayerName = o.Value<string>("pn") ?? "";
                    info.LoopCount = o.Value<int?>("lo") ?? 1;
                    info.Date = o.Value<string>("dt") ?? "";
                    info.LevelPhysical = o.Value<int?>("lp") ?? 0;
                    info.LevelIntelligence = o.Value<int?>("li") ?? 0;
                    info.LevelCharm = o.Value<int?>("lc") ?? 0;
                    info.LevelSense = o.Value<int?>("ls") ?? 0;
                    info.LevelBattle = o.Value<int?>("lb") ?? 0;
                    info.LevelArts = o.Value<int?>("la") ?? 0;
                    info.LevelMagic = o.Value<int?>("lm") ?? 0;
                }
                result.Add(info);
            }
            return result;
        }

        /// <summary>Sync slot metadata in the index file after editing a user save.</summary>
        public void SyncIndexFromUserData(int slot, JObject userData)
        {
            var index = LoadIndex();
            var list = index["dataList"] as JArray;
            if (list == null || slot >= list.Count || !(list[slot] is JObject meta)) return;
            var gs = userData["gstatus"] as JObject;
            var st = userData["status"] as JObject;
            if (gs != null)
            {
                meta["pn"] = gs.Value<string>("pn") ?? "";
                meta["lo"] = gs.Value<int?>("c") ?? 1;
            }
            if (st != null)
            {
                meta["lp"] = st.Value<int?>("lp") ?? 0;
                meta["li"] = st.Value<int?>("li") ?? 0;
                meta["lc"] = st.Value<int?>("lc") ?? 0;
                meta["ls"] = st.Value<int?>("ls") ?? 0;
                meta["lb"] = st.Value<int?>("lb") ?? 0;
                meta["la"] = st.Value<int?>("la") ?? 0;
                meta["lm"] = st.Value<int?>("lm") ?? 0;
            }
            SaveIndex(index);
        }
    }
}