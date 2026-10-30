using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac4
{
    public enum ConstraintState : uint { Undecided = 0, Succeeded = 1, Failed = 2 }

    public sealed class MissionStep
    {
        public ulong Id { get; init; }
        public bool IsCheckpoint { get; init; }
        public bool Succeeded { get; init; }
    }

    public sealed class MissionCondition
    {
        public ulong Id { get; init; }
        public uint MetaValue { get; init; }
    }

    public sealed class MissionConstraint
    {
        public ulong Id { get; init; }
        public bool PlayerSelected { get; init; }
        public bool IsRevealed { get; init; }
        public bool BestRunSucceeded { get; init; }
        public ConstraintState State { get; init; }
        public List<MissionCondition> Conditions { get; init; } = new();
    }

    public sealed class Mission
    {
        public ulong Id { get; init; }
        /// <summary>0 = not completed, 100 = completed, 101/102 = completed with 1/2 optional objectives.</summary>
        public int HighestSynchronization { get; init; }
        /// <summary>Raw TimePlayed value (unit not yet confirmed).</summary>
        public uint TimePlayed { get; init; }
        public bool BriefingDisplayedOnce { get; init; }
        public bool BriefingAccepted { get; init; }
        public List<MissionStep> Steps { get; init; } = new();
        public List<MissionConstraint> Constraints { get; init; } = new();
        public bool Completed => HighestSynchronization >= 100;
        public int OptionalObjectivesDone => Completed ? HighestSynchronization - 100 : 0;
    }

    public sealed class MissionGroup
    {
        public ulong Id { get; init; }
        public List<Mission> Missions { get; init; } = new();
    }

    /// <summary>MissionHistory object, used for both NarrativeMissionHistory and SideMissionHistory.</summary>
    public sealed class MissionHistory
    {
        public List<MissionGroup> Groups { get; init; } = new();
        public IEnumerable<Mission> AllMissions { get { foreach (var g in Groups) foreach (var m in g.Missions) yield return m; } }

        public static MissionHistory From(Node history)
        {
            var h = new MissionHistory();
            var groups = history.Find("GroupSaveData");
            if (groups == null) return h;
            foreach (var ge in groups.Elements)
            {
                var g = new MissionGroup { Id = ge.Find("Group")?.Value ?? 0 };
                var ms = ge.Find("MissionSaveData");
                if (ms != null)
                    foreach (var me in ms.Elements)
                        g.Missions.Add(ReadMission(me));
                h.Groups.Add(g);
            }
            return h;
        }

        private static Mission ReadMission(ArrayElement e)
        {
            var steps = new List<MissionStep>();
            foreach (var se in e.Find("MissionStepSaveData")?.Elements ?? new())
                steps.Add(new MissionStep
                {
                    Id = se.Find("Step")?.Value ?? 0,
                    IsCheckpoint = se.Find("IsCheckpoint")?.AsBool ?? false,
                    Succeeded = se.Find("Succeeded")?.AsBool ?? false,
                });
            var cons = new List<MissionConstraint>();
            foreach (var ce in e.Find("MissionConstraintSaveData")?.Elements ?? new())
            {
                var c = new MissionConstraint
                {
                    Id = ce.Find("ConstraintID")?.Value ?? 0,
                    PlayerSelected = ce.Find("PlayerSelected")?.AsBool ?? false,
                    IsRevealed = ce.Find("IsRevealed")?.AsBool ?? false,
                    BestRunSucceeded = ce.Find("BestRunSucceeded")?.AsBool ?? false,
                    State = (ConstraintState)(ce.Find("CurrentRunState")?.Value & 0xFFFFFFFF ?? 0),
                };
                foreach (var de in ce.Find("ConditionData")?.Elements ?? new())
                    c.Conditions.Add(new MissionCondition { Id = de.Find("ConditionID")?.Value ?? 0, MetaValue = (uint)(de.Find("MetaValue")?.Value ?? 0) });
                cons.Add(c);
            }
            return new Mission
            {
                Id = e.Find("Mission")?.Value ?? 0,
                HighestSynchronization = (int)(e.Find("HighestSynchronization")?.Value ?? 0),
                TimePlayed = (uint)(e.Find("TimePlayed")?.Value ?? 0),
                BriefingDisplayedOnce = e.Find("BriefingDisplayedOnce")?.AsBool ?? false,
                BriefingAccepted = e.Find("BriefingAccepted")?.AsBool ?? false,
                Steps = steps,
                Constraints = cons,
            };
        }
    }
}
