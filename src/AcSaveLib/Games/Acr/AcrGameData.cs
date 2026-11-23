using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acr
{
    /// <summary>
    /// The game data container of a Revelations save (root AssassinSaveGameData): typed models for
    /// every field that has a name. Revelations adds fields with no name yet; they stay in the node tree of the container.
    /// </summary>
    public sealed class AcrGameData
    {
        /// <summary>True when the container holds game data (a NarrativeMissionHistory item is present).</summary>
        public bool HasGameData { get; private set; }
        public AcrMissionHistory Narrative { get; private set; } = new();
        public AcrMissionHistory Side { get; private set; } = new();
        /// <summary>World handle of the saved position (top-level World item).</summary>
        public ulong World { get; private set; }
        public ulong PlayerPortal { get; private set; }
        public AcrPspState Psp { get; private set; } = new();
        public AcrPspState PspSnapshot { get; private set; } = new();
        public List<AcrAnimusEntry> AnimusEntries { get; private set; } = new();
        public AcrUiTutorial UiTutorial { get; private set; } = new();
        public AcrEmails Emails { get; private set; } = new();
        public List<AcrAccomplishment> Accomplishments { get; private set; } = new();
        public AcrGameStats GameStats { get; private set; } = new();
        public AcrTraineesGuild TraineesGuild { get; private set; } = new();
        public AcrFacebookData Facebook { get; private set; } = new();
        public AcrPlayerProgression Progression { get; private set; } = new();
        public AcrPlayerProgression ProgressionSnapshot { get; private set; } = new();
        public AcrProgressionDelta ProgressionDelta { get; private set; } = new();
        public AcrVilla Villa { get; private set; } = new();
        public AcrVilla VillaSnapshot { get; private set; } = new();
        public AcrWorldUpgrade WorldUpgrade { get; private set; } = new();
        public AcrWorldUpgrade WorldUpgradeSnapshot { get; private set; } = new();

        public static AcrGameData From(List<Node> root)
        {
            var r = new AcrGameData();
            var g = new Props(root);
            r.HasGameData = g.Get("NarrativeMissionHistory") != null;
            if (!r.HasGameData) return r;
            r.Narrative = AcrMissionHistory.From(g.Obj("NarrativeMissionHistory"));
            r.Side = AcrMissionHistory.From(g.Obj("SideMissionHistory"));
            r.World = g.U64("World");
            r.PlayerPortal = g.U64("PlayerPortal");
            r.Psp = AcrPspState.From(g.Obj("PSPState"));
            r.PspSnapshot = AcrPspState.From(g.Obj("SnapshotPSPState"));
            foreach (var e in g.Obj("AnimusDatabaseData").Elements("ADBEntries"))
                r.AnimusEntries.Add(new AcrAnimusEntry
                {
                    EntryFoundOrderNumber = e.U32("EntryFoundOrderNumber"), GlyphFoundOrderNumber = e.U32("GlyphFoundOrderNumber"),
                    AfsDisplayedCount = e.U32("AFSDisplayedCount"), IsLgsSolved = e.Bool("IsLGSSolved"),
                });
            r.UiTutorial = AcrUiTutorial.From(g.Obj("UITutorialData"));
            r.Emails = AcrEmails.From(g.Obj("PresentEmailsData"));
            foreach (var e in g.Obj("AccomplishmentManagerData").Elements("AccomplishmentEntries"))
                r.Accomplishments.Add(new AcrAccomplishment { ObjectId = e.U32("ObjectID"), Inactive = e.Bool("Inactive"), Completed = e.Bool("Completed"), CurrentCount = e.U32("CurrentCount") });
            r.GameStats = AcrGameStats.From(g.Obj("GameStatsSaveData"));
            r.TraineesGuild = AcrTraineesGuild.From(g.Obj("TraineesGuildManagerSaveData"));
            r.Facebook = AcrFacebookData.From(g.Obj("FacebookSaveGameData"));
            r.Progression = AcrPlayerProgression.From(g.Obj("PlayerProgSaveData"));
            r.ProgressionSnapshot = AcrPlayerProgression.From(g.Obj("SnapshotPlayerProgSaveData"));
            r.ProgressionDelta = AcrProgressionDelta.From(g.Obj("SnapshotPlayerProgressionDelta"));
            r.Villa = AcrVilla.From(g.Obj("VillaSaveData"));
            r.VillaSnapshot = AcrVilla.From(g.Obj("SnapshotVillaSaveData"));
            r.WorldUpgrade = AcrWorldUpgrade.From(g.Obj("WorldUpgradeSaveData"));
            r.WorldUpgradeSnapshot = AcrWorldUpgrade.From(g.Obj("SnapshotWorldUpgradeSaveData"));
            return r;
        }
    }
}
