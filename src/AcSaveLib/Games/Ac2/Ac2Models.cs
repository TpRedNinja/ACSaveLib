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
}
