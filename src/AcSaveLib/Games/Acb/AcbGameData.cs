using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acb
{
    /// <summary>
    /// The game data container of a Brotherhood or Revelations save (root AssassinSaveGameData and its look-alikes): typed models for
    /// every field that has a name. Revelations adds fields with no name yet; they stay in the node tree of the container.
    /// </summary>
    public sealed class AcbGameData
    {
        /// <summary>True when the container holds game data (a NarrativeMissionHistory item is present).</summary>
        public bool HasGameData { get; private set; }
        public AcbMissionHistory Narrative { get; private set; } = new();
        public AcbMissionHistory Side { get; private set; } = new();
        /// <summary>World handle of the saved position (top-level World item).</summary>
        public ulong World { get; private set; }
        public ulong PlayerPortal { get; private set; }
        public AcbPspState Psp { get; private set; } = new();
        public AcbPspState PspSnapshot { get; private set; } = new();
        public List<AcbAnimusEntry> AnimusEntries { get; private set; } = new();
        public AcbUiTutorial UiTutorial { get; private set; } = new();
        public AcbEmails Emails { get; private set; } = new();
        public List<AcbAccomplishment> Accomplishments { get; private set; } = new();
        public AcbGameStats GameStats { get; private set; } = new();
        public AcbTraineesGuild TraineesGuild { get; private set; } = new();
        public AcbFacebookData Facebook { get; private set; } = new();
        public AcbPlayerProgression Progression { get; private set; } = new();
        public AcbPlayerProgression ProgressionSnapshot { get; private set; } = new();
        public AcbProgressionDelta ProgressionDelta { get; private set; } = new();
        public AcbVilla Villa { get; private set; } = new();
        public AcbVilla VillaSnapshot { get; private set; } = new();
        public AcbWorldUpgrade WorldUpgrade { get; private set; } = new();
        public AcbWorldUpgrade WorldUpgradeSnapshot { get; private set; } = new();

        public static AcbGameData From(List<Node> root)
        {
            var r = new AcbGameData();
            var g = new Props(root);
            r.HasGameData = g.Get("NarrativeMissionHistory") != null;
            if (!r.HasGameData) return r;
            r.Narrative = AcbMissionHistory.From(g.Obj("NarrativeMissionHistory"));
            r.Side = AcbMissionHistory.From(g.Obj("SideMissionHistory"));
            r.World = g.U64("World");
            r.PlayerPortal = g.U64("PlayerPortal");
            r.Psp = AcbPspState.From(g.Obj("PSPState"));
            r.PspSnapshot = AcbPspState.From(g.Obj("SnapshotPSPState"));
            foreach (var e in g.Obj("AnimusDatabaseData").Elements("ADBEntries"))
                r.AnimusEntries.Add(new AcbAnimusEntry
                {
                    EntryFoundOrderNumber = e.U32("EntryFoundOrderNumber"), GlyphFoundOrderNumber = e.U32("GlyphFoundOrderNumber"),
                    AfsDisplayedCount = e.U32("AFSDisplayedCount"), IsLgsSolved = e.Bool("IsLGSSolved"),
                });
            r.UiTutorial = AcbUiTutorial.From(g.Obj("UITutorialData"));
            r.Emails = AcbEmails.From(g.Obj("PresentEmailsData"));
            foreach (var e in g.Obj("AccomplishmentManagerData").Elements("AccomplishmentEntries"))
                r.Accomplishments.Add(new AcbAccomplishment { ObjectId = e.U32("ObjectID"), Inactive = e.Bool("Inactive"), Completed = e.Bool("Completed"), CurrentCount = e.U32("CurrentCount") });
            r.GameStats = AcbGameStats.From(g.Obj("GameStatsSaveData"));
            r.TraineesGuild = AcbTraineesGuild.From(g.Obj("TraineesGuildManagerSaveData"));
            r.Facebook = AcbFacebookData.From(g.Obj("FacebookSaveGameData"));
            r.Progression = AcbPlayerProgression.From(g.Obj("PlayerProgSaveData"));
            r.ProgressionSnapshot = AcbPlayerProgression.From(g.Obj("SnapshotPlayerProgSaveData"));
            r.ProgressionDelta = AcbProgressionDelta.From(g.Obj("SnapshotPlayerProgressionDelta"));
            r.Villa = AcbVilla.From(g.Obj("VillaSaveData"));
            r.VillaSnapshot = AcbVilla.From(g.Obj("SnapshotVillaSaveData"));
            r.WorldUpgrade = AcbWorldUpgrade.From(g.Obj("WorldUpgradeSaveData"));
            r.WorldUpgradeSnapshot = AcbWorldUpgrade.From(g.Obj("SnapshotWorldUpgradeSaveData"));
            return r;
        }
    }
}
