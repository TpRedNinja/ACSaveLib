using System.Collections.Generic;
using System.Linq;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac2
{
    /// <summary>AssassinSaveGameData: typed models of every named field of an AC2 game save. Missing items yield empty models.</summary>
    public sealed class Ac2GameData
    {
        public uint ContextId { get; init; }
        public Ac2MissionHistory Narrative { get; init; } = new();
        public Ac2MissionHistory Side { get; init; } = new();
        public ulong World { get; init; }
        public Ac2PspState PspState { get; init; } = new();
        public Ac2PspState SnapshotPspState { get; init; } = new();
        public ulong PlayerPortal { get; init; }
        public Ac2AnimusDatabase AnimusDatabase { get; init; } = new();
        public Ac2UITutorialData UITutorial { get; init; } = new();
        public List<Ac2AccomplishmentEntry> Accomplishments { get; init; } = new();
        public Ac2GameStats GameStats { get; init; } = new();
        public Ac2PlayerProgression Progression { get; init; } = new();
        public Ac2PlayerProgression SnapshotProgression { get; init; } = new();
        public Ac2ProgressionDelta SnapshotDelta { get; init; } = new();
        public Ac2Villa Villa { get; init; } = new();
        public Ac2Villa SnapshotVilla { get; init; } = new();

        public static Ac2GameData From(ArrayElement game)
        {
            var g = new Props(game.Children);
            var d = new Ac2GameData
            {
                ContextId = g.U32("ContextID"),
                Narrative = Ac2MissionHistory.From(game.Find("NarrativeMissionHistory")),
                Side = Ac2MissionHistory.From(game.Find("SideMissionHistory")),
                World = g.U64("World"), PlayerPortal = g.U64("PlayerPortal"),
                PspState = Ac2PspState.From(g.Obj("PSPState")), SnapshotPspState = Ac2PspState.From(g.Obj("SnapshotPSPState")),
                AnimusDatabase = Ac2AnimusDatabase.From(g.Obj("AnimusDatabaseData")),
                UITutorial = Ac2UITutorialData.From(g.Obj("UITutorialData")),
                GameStats = Ac2GameStats.From(g.Obj("GameStatsSaveData")),
                Progression = Ac2PlayerProgression.From(g.Obj("PlayerProgSaveData")),
                SnapshotProgression = Ac2PlayerProgression.From(g.Obj("SnapshotPlayerProgSaveData")),
                SnapshotDelta = Ac2ProgressionDelta.From(g.Obj("SnapshotPlayerProgressionDelta")),
                Villa = Ac2Villa.From(g.Obj("VillaSaveData")), SnapshotVilla = Ac2Villa.From(g.Obj("SnapshotVillaSaveData")),
            };
            foreach (var e in g.Obj("AccomplishmentManagerData").Elements("AccomplishmentEntries"))
                d.Accomplishments.Add(new Ac2AccomplishmentEntry { ObjectId = e.U64("ObjectID"), Inactive = e.Bool("Inactive"), Completed = e.Bool("Completed"), CurrentCount = e.U32("CurrentCount") });
            return d;
        }
    }
}
