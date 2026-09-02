namespace MagicalPrincess.SaveEditor.Core
{
    /// <summary>Metadata of one save slot, from v10_indexdata.dat.</summary>
    public class SlotInfo
    {
        public int SlotId;
        public bool IsPlaying;
        public string PlayerName = "";
        public int LoopCount;
        public string Date = "";
        public int LevelPhysical;
        public int LevelIntelligence;
        public int LevelCharm;
        public int LevelSense;
        public int LevelBattle;
        public int LevelArts;
        public int LevelMagic;
        public bool Exists;

        public override string ToString() => $"#{SlotId} {PlayerName}";
    }
}