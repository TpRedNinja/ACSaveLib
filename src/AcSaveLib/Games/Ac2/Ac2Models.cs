using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac2
{
    public sealed class Ac2MissionStep
    {
        public ulong Id { get; init; }
        public bool IsCheckpoint { get; init; }
        public bool Succeeded { get; init; }
    }

    public sealed class Ac2Mission
    {
        /// <summary>Mission hash ID (matches AC2_HashNames.txt, e.g. 0x385B98EB Boys Will Be Boys).</summary>
        public ulong Id { get; init; }
        public int HighestSynchronization { get; init; }
        public bool BriefingDisplayedOnce { get; init; }
        public bool BriefingAccepted { get; init; }
        public List<Ac2MissionStep> Steps { get; init; } = new();
        public int ConstraintCount { get; init; }
        /// <summary>LIKELY: HighestSynchronization is 100 for every finished mission and 0 for entries that only hold progress (steps).</summary>
        public bool Completed => HighestSynchronization >= 100;
    }

    public sealed class Ac2MissionGroup
    {
        public ulong Id { get; init; }
        public List<Ac2Mission> Missions { get; init; } = new();
    }

    /// <summary>NarrativeMissionHistory / SideMissionHistory.</summary>
    public sealed class Ac2MissionHistory
    {
        public List<Ac2MissionGroup> Groups { get; init; } = new();
        public IEnumerable<Ac2Mission> AllMissions { get { foreach (var g in Groups) foreach (var m in g.Missions) yield return m; } }

        public static Ac2MissionHistory From(Node? history)
        {
            var h = new Ac2MissionHistory();
            var groups = history?.Find("GroupSaveData");
            if (groups == null) return h;
            foreach (var ge in groups.Elements)
            {
                var g = new Ac2MissionGroup { Id = ge.Find("Group")?.Value ?? 0 };
                foreach (var me in ge.Find("MissionSaveData")?.Elements ?? new())
                {
                    var steps = new List<Ac2MissionStep>();
                    foreach (var se in me.Find("MissionStepSaveData")?.Elements ?? new())
                        steps.Add(new Ac2MissionStep
                        {
                            Id = se.Find("Step")?.Value ?? 0,
                            IsCheckpoint = se.Find("IsCheckpoint")?.AsBool ?? false,
                            Succeeded = se.Find("Succeeded")?.AsBool ?? false,
                        });
                    g.Missions.Add(new Ac2Mission
                    {
                        Id = me.Find("Mission")?.Value ?? 0,
                        HighestSynchronization = (int)(me.Find("HighestSynchronization")?.Value ?? 0),
                        BriefingDisplayedOnce = me.Find("BriefingDisplayedOnce")?.AsBool ?? false,
                        BriefingAccepted = me.Find("BriefingAccepted")?.AsBool ?? false,
                        Steps = steps,
                        ConstraintCount = me.Find("MissionConstraintSaveData")?.Elements.Count ?? 0,
                    });
                }
                h.Groups.Add(g);
            }
            return h;
        }
    }

    public sealed class Ac2AccomplishmentEntry
    {
        public ulong ObjectId { get; init; }
        public bool Inactive { get; init; }
        public bool Completed { get; init; }
        public uint CurrentCount { get; init; }
    }

    /// <summary>QuestItemsCollectedAmounts: 9 counters. Indices 0..3, 5 and 6 match the in-game collectible counters (see docs/FIELDS_AC2.md).</summary>
    public sealed class Ac2CollectedAmounts
    {
        public List<uint> Values { get; init; } = new();
        private uint At(int i) => i < Values.Count ? Values[i] : 0;
        public uint CodexPages => At(0);
        public uint Feathers => At(1);
        public uint Statuettes => At(2);
        public uint AssassinSeals => At(3);
        public uint Chests => At(6);
    }

    /// <summary>GameStatsSaveData. Names confirmed in ACViewer; units are not confirmed.</summary>
    public sealed class Ac2GameStats
    {
        public uint TimePlayed { get; init; }
        public uint TotalTimeBlended { get; init; }
        public uint DeathOccurrences { get; init; }
        public uint Kills { get; init; }
        public uint HiddenBladeAssassinations { get; init; }
        public uint AirAssassinations { get; init; }
        public uint DoubleAirAssassinations { get; init; }
        public uint Disarms { get; init; }
        public uint TotalMoneyEarned { get; init; }
        public uint TotalMoneySpent { get; init; }
        public uint NotoriousOccurrences { get; init; }
        public uint BorgiaCourrierKills { get; init; }
        public uint CourtesansHired { get; init; }
        public uint MercenariesHired { get; init; }
        public uint ThievesHired { get; init; }
        public uint OverallDistanceCovered { get; init; }
        public uint HeightClimbed { get; init; }
        public uint DistanceOnGround { get; init; }
        public uint DistanceSwam { get; init; }
        public uint DistanceHorse { get; init; }
        public uint DistanceGondolas { get; init; }
        public uint DistanceFlown { get; init; }

        public static Ac2GameStats From(Props p) => new Ac2GameStats
        {
            TimePlayed = p.U32("TimePlayed"), TotalTimeBlended = p.U32("TotalTimeBlended"), DeathOccurrences = p.U32("DeathOccurrences"),
            Kills = p.U32("Kills"), HiddenBladeAssassinations = p.U32("HiddenBladeAssassinations"), AirAssassinations = p.U32("AirAssassinations"),
            DoubleAirAssassinations = p.U32("DoubleAirAssassinations"), Disarms = p.U32("Disarms"), TotalMoneyEarned = p.U32("TotalMoneyEarned"),
            TotalMoneySpent = p.U32("TotalMoneySpent"), NotoriousOccurrences = p.U32("NotoriousOccurrences"), BorgiaCourrierKills = p.U32("BorgiaCourrierKills"),
            CourtesansHired = p.U32("CourtesansHired"), MercenariesHired = p.U32("MercenariesHired"), ThievesHired = p.U32("ThievesHired"),
            OverallDistanceCovered = p.U32("OverallDistanceCovered"), HeightClimbed = p.U32("HeightClimbed"), DistanceOnGround = p.U32("DistanceOnGround"),
            DistanceSwam = p.U32("DistanceSwam"), DistanceHorse = p.U32("DistanceHorse"), DistanceGondolas = p.U32("DistanceGondolas"), DistanceFlown = p.U32("DistanceFlown"),
        };
    }

    /// <summary>VillaSaveData (Monteriggioni). Names confirmed in ACViewer.</summary>
    public sealed class Ac2Villa
    {
        public uint StrongholdValue { get; init; }
        public uint CurrentGoldIncome { get; init; }
        public uint CurrentIncomeDelay { get; init; }
        public uint PendingIncomeValueForAfs { get; init; }
        public uint PendingStrValueForAfs { get; init; }
        public List<byte> BuildingsLevels { get; init; } = new();
        public List<bool> CodexPagesDecodedStatus { get; init; } = new();

        public static Ac2Villa From(Props p) => new Ac2Villa
        {
            StrongholdValue = p.U32("StrongholdValue"), CurrentGoldIncome = p.U32("CurrentGoldIncome"), CurrentIncomeDelay = p.U32("CurrentIncomeDelay"),
            PendingIncomeValueForAfs = p.U32("PendingIncomeValueForAfs"), PendingStrValueForAfs = p.U32("PendingStrValueForAfs"),
            BuildingsLevels = p.PackedU8("BuildingsLevels"), CodexPagesDecodedStatus = p.PackedBool("CodexPagesDecodedStatus"),
        };
    }
}
