using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acs
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
        /// <summary>Raw HighestSynchronization value. Values seen in saves: 0, 1, 2 and 52; the meaning is not confirmed.</summary>
        public int HighestSynchronization { get; init; }
        public uint PersistentFlags { get; init; }
        public uint ResetMissionConstraintSaveData { get; init; }
        /// <summary>Raw TimePlayed value (unit not yet confirmed).</summary>
        public uint TimePlayed { get; init; }
        public bool BriefingDisplayedOnce { get; init; }
        public bool BriefingAccepted { get; init; }
        public List<MissionStep> Steps { get; init; } = new();
        public List<MissionConstraint> Constraints { get; init; } = new();
        /// <summary>MissionOutputSaveData entries: the Output id of each.</summary>
        public List<ulong> Outputs { get; init; } = new();
        /// <summary>MissionOutputSaveData entries: the CurrentState id of each, in the same order as Outputs.</summary>
        public List<ulong> OutputStates { get; init; } = new();
        /// <summary>CompletedSteps (a list of MissionStep handles; empty in every save read).</summary>
        public List<ulong> CompletedSteps { get; init; } = new();
    }

    public sealed class MissionGroup
    {
        public ulong Id { get; init; }
        public List<Mission> Missions { get; init; } = new();
    }

    /// <summary>MissionHistory object, used for both NarrativeMissionHistory and SideMissionHistory.</summary>
    public sealed class MissionHistory
    {
        /// <summary>MissionHistoryTrackIdentifiers enum value as stored (low 32 bits are the value, high 32 bits the name hash).</summary>
        public ulong Identifier { get; init; }
        public ulong LastConcurrentStepPortal { get; init; }
        public List<MissionGroup> Groups { get; init; } = new();
        public IEnumerable<Mission> AllMissions { get { foreach (var g in Groups) foreach (var m in g.Missions) yield return m; } }

        public static MissionHistory From(Node history)
        {
            var h = new MissionHistory { Identifier = history.Find("Identifier")?.Value ?? 0, LastConcurrentStepPortal = history.Find("LastConcurrentStepPortal")?.Value ?? 0 };
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
            var outs = new List<ulong>();
            var outStates = new List<ulong>();
            foreach (var oe in e.Find("MissionOutputSaveData")?.Elements ?? new())
            {
                outs.Add(oe.Find("Output")?.Value ?? 0);
                outStates.Add(oe.Find("CurrentState")?.Value ?? 0);
            }
            return new Mission
            {
                Outputs = outs,
                OutputStates = outStates,
                CompletedSteps = e.Find("CompletedSteps")?.Packed ?? new(),
                Id = e.Find("Mission")?.Value ?? 0,
                HighestSynchronization = (int)(e.Find("HighestSynchronization")?.Value ?? 0),
                TimePlayed = (uint)(e.Find("TimePlayed")?.Value ?? 0),
                PersistentFlags = (uint)(e.Find("PersistentFlags")?.Value ?? 0),
                ResetMissionConstraintSaveData = (uint)(e.Find("ResetMissionConstraintSaveData")?.Value ?? 0),
                BriefingDisplayedOnce = e.Find("BriefingDisplayedOnce")?.AsBool ?? false,
                BriefingAccepted = e.Find("BriefingAccepted")?.AsBool ?? false,
                Steps = steps,
                Constraints = cons,
            };
        }
    }
}
