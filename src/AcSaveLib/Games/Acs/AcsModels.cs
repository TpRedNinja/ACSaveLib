using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acs
{
    public sealed class AcsAccomplishment
    {
        public ulong ObjectId { get; init; }
        public bool Inactive { get; init; }
        public bool Completed { get; init; }
        public uint CurrentCount { get; init; }
    }

    public sealed class AcsStatistic
    {
        public uint StatId { get; init; }
        public uint StatValue { get; init; }
    }

    public sealed class AcsDistrictAssociate
    {
        public ulong Associate { get; init; }
        public uint Influence { get; init; }
        public uint Level { get; init; }
        public bool Activated { get; init; }
    }

    /// <summary>The main container of a Syndicate game save: typed models of every named top-level field. Unnamed keys stay in the node tree.</summary>
    public sealed class AcsGameData
    {
        public bool HasGameData { get; private set; }
        public uint ContextId { get; private set; }
        public MissionHistory Narrative { get; private set; } = new();
        public MissionHistory Side { get; private set; } = new();
        public ulong CurrentRoot { get; private set; }
        public ulong CurrentMission { get; private set; }
        public ulong World { get; private set; }
        public ulong PlayerPortal { get; private set; }
        public ulong TransitionProfile { get; private set; }
        public uint AnimusNumRecentEntries { get; private set; }
        public List<ulong> AnimusUnlockedEntries { get; private set; } = new();
        public List<ulong> AnimusEntriesData { get; private set; } = new();
        public int MessageCount { get; private set; }
        public List<AcsAccomplishment> Accomplishments { get; private set; } = new();
        public AcsPlayerProgression PlayerProgression { get; private set; } = new();
        public AcsPlayerProgression SnapshotPlayerProgression { get; private set; } = new();
        public uint CurrentGoldChestAmount { get; private set; }
        public uint CurrentIncomeDelay { get; private set; }
        public uint PendingIncomeValueForAfs { get; private set; }
        /// <summary>OnlineGameSaveData > PlayerProfileSaveData > Statistics.</summary>
        public List<AcsStatistic> PlayerStatistics { get; private set; } = new();
        /// <summary>OnlineGameSaveData > HeroStatsSaveData > Statistics.</summary>
        public List<AcsStatistic> HeroStatistics { get; private set; } = new();
        public List<ulong> DiscoveredIcons { get; private set; } = new();
        public uint CompletedCrowdEvents { get; private set; }
        public List<AcsDistrictAssociate> DistrictAssociates { get; private set; } = new();
        public int FightClubCount { get; private set; }
        public int CartRaceCount { get; private set; }

        public static AcsGameData From(List<Node> root)
        {
            var r = new AcsGameData();
            foreach (var n in root)
            {
                var p = new Props(n.Children);
                switch (n.Name)
                {
                    case "ContextID": r.ContextId = (uint)n.Value; break;
                    case "NarrativeMissionHistory": r.HasGameData = true; r.Narrative = MissionHistory.From(n); break;
                    case "SideMissionHistory": r.Side = MissionHistory.From(n); break;
                    case "CurrentRoot": r.CurrentRoot = n.Value; break;
                    case "CurrentMission": r.CurrentMission = n.Value; break;
                    case "World": r.World = n.Value; break;
                    case "PlayerPortal": r.PlayerPortal = n.Value; break;
                    case "TransitionProfile": r.TransitionProfile = n.Value; break;
                    case "AnimusDatabaseData": r.AnimusNumRecentEntries = p.U32("NumRecentEntries"); r.AnimusUnlockedEntries = p.Packed("UnlockedEntries"); r.AnimusEntriesData = p.Packed("EntriesData"); break;
                    case "MessagingDatabaseData": r.MessageCount = p.ElementCount("MDMessages"); break;
                    case "AccomplishmentManagerData":
                        foreach (var e in p.Elements("AccomplishmentEntries"))
                            r.Accomplishments.Add(new AcsAccomplishment { ObjectId = e.U64("ObjectID"), Inactive = e.Bool("Inactive"), Completed = e.Bool("Completed"), CurrentCount = e.U32("CurrentCount") });
                        break;
                    case "PlayerProgSaveData": r.PlayerProgression = AcsPlayerProgression.From(n); break;
                    case "SnapshotPlayerProgSaveData": r.SnapshotPlayerProgression = AcsPlayerProgression.From(n); break;
                    case "WorldUpgradeSaveData": r.CurrentGoldChestAmount = p.U32("CurrentGoldChestAmount"); r.CurrentIncomeDelay = p.U32("CurrentIncomeDelay"); r.PendingIncomeValueForAfs = p.U32("PendingIncomeValueForAfs"); break;
                    case "OnlineGameSaveData":
                        foreach (var e in p.Obj("PlayerProfileSaveData").Elements("Statistics")) r.PlayerStatistics.Add(new AcsStatistic { StatId = e.U32("StatId"), StatValue = e.U32("StatValue") });
                        foreach (var e in p.Obj("HeroStatsSaveData").Elements("Statistics")) r.HeroStatistics.Add(new AcsStatistic { StatId = e.U32("StatId"), StatValue = e.U32("StatValue") });
                        break;
                    case "PermanentIconSaveData": r.DiscoveredIcons = p.Packed("DiscoveredIcons"); break;
                    case "CrowdEventsSaveData": r.CompletedCrowdEvents = p.U32("CompletedCrowdEventsNb"); break;
                    case "DistrictManagerSaveData":
                        foreach (var e in p.Elements("DistrictAssociateSaveData"))
                            r.DistrictAssociates.Add(new AcsDistrictAssociate { Associate = e.U64("Associate"), Influence = e.U32("Influence"), Level = e.U32("Level"), Activated = e.Bool("Activated") });
                        r.FightClubCount = p.ElementCount("FightClubSaveData"); r.CartRaceCount = p.ElementCount("CartRaceSaveData");
                        break;
                }
            }
            return r;
        }
    }
}
