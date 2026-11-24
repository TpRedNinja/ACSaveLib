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

    public sealed class AcsFightClub
    {
        public ulong MissionId { get; init; }
        public uint HighestRound { get; init; }
    }

    public sealed class AcsCartRace
    {
        public ulong MissionId { get; init; }
        public ulong BestTime { get; init; }
        public uint BestPosition { get; init; }
    }

    /// <summary>SecondaryStoreSaveData: the *Given counters.</summary>
    public sealed class AcsSecondaryStore
    {
        public uint PoundsGiven { get; init; }
        public uint LeatherGiven { get; init; }
        public uint MetalGiven { get; init; }
        public uint GlassGiven { get; init; }
        public uint SilkGiven { get; init; }
        public uint ChemicalsGiven { get; init; }
        public uint JewelsGiven { get; init; }
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
        public AcsProgressionDelta SnapshotProgressionDelta { get; private set; } = new();
        public List<ulong> UnlockedTutorials { get; private set; } = new();
        public List<ulong> NewTutorials { get; private set; } = new();
        public List<ulong> ReadItemsProgressionLog { get; private set; } = new();
        public List<ulong> ReadRecipes { get; private set; } = new();
        public List<ulong> DiscoveredPOIElements { get; private set; } = new();
        public List<ulong> LondonUpgradeReadItems { get; private set; } = new();
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
        public int FightClubCount => FightClubs.Count;
        public int CartRaceCount => CartRaces.Count;
        public List<AcsFightClub> FightClubs { get; private set; } = new();
        public List<AcsCartRace> CartRaces { get; private set; } = new();
        public AcsSecondaryStore SecondaryStore { get; private set; } = new();
        public List<ulong> CollectedCollectables { get; private set; } = new();

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
                    case "SnapshotPlayerProgressionDelta": r.SnapshotProgressionDelta = AcsProgressionDelta.From(n); break;
                    case "TutorialData": r.UnlockedTutorials = p.Packed("UnlockedTutorials"); r.NewTutorials = p.Packed("NewTutorials"); break;
                    case "CraftingData": r.ReadRecipes = p.Packed("ReadRecipes"); break;
                    case "BlackBoxData": r.DiscoveredPOIElements = p.Packed("DiscoveredPOIElements"); break;
                    case "LondonUpgradeData": r.LondonUpgradeReadItems = p.Packed("ReadItems"); break;
                    case "ProgressionLogSaveGameData": r.ReadItemsProgressionLog = p.Packed("ReadItems"); break;
                    case "WorldUpgradeSaveData": r.CurrentGoldChestAmount = p.U32("CurrentGoldChestAmount"); r.CurrentIncomeDelay = p.U32("CurrentIncomeDelay"); r.PendingIncomeValueForAfs = p.U32("PendingIncomeValueForAfs"); break;
                    case "OnlineGameSaveData":
                        r.CollectedCollectables = p.Obj("SocialCollectablesSaveData").Packed("CollectedCollectables");
                        {
                            var s = p.Obj("SecondaryStoreSaveData");
                            r.SecondaryStore = new AcsSecondaryStore { PoundsGiven = s.U32("PoundsGiven"), LeatherGiven = s.U32("LeatherGiven"), MetalGiven = s.U32("MetalGiven"), GlassGiven = s.U32("GlassGiven"), SilkGiven = s.U32("SilkGiven"), ChemicalsGiven = s.U32("ChemicalsGiven"), JewelsGiven = s.U32("JewelsGiven") };
                        }
                        foreach (var e in p.Obj("PlayerProfileSaveData").Elements("Statistics")) r.PlayerStatistics.Add(new AcsStatistic { StatId = e.U32("StatId"), StatValue = e.U32("StatValue") });
                        foreach (var e in p.Obj("HeroStatsSaveData").Elements("Statistics")) r.HeroStatistics.Add(new AcsStatistic { StatId = e.U32("StatId"), StatValue = e.U32("StatValue") });
                        break;
                    case "PermanentIconSaveData": r.DiscoveredIcons = p.Packed("DiscoveredIcons"); break;
                    case "CrowdEventsSaveData": r.CompletedCrowdEvents = p.U32("CompletedCrowdEventsNb"); break;
                    case "DistrictManagerSaveData":
                        foreach (var e in p.Elements("DistrictAssociateSaveData"))
                            r.DistrictAssociates.Add(new AcsDistrictAssociate { Associate = e.U64("Associate"), Influence = e.U32("Influence"), Level = e.U32("Level"), Activated = e.Bool("Activated") });
                        foreach (var e in p.Elements("FightClubSaveData")) r.FightClubs.Add(new AcsFightClub { MissionId = e.U64("MissionID"), HighestRound = e.U32("HighestRound") });
                        foreach (var e in p.Elements("CartRaceSaveData")) r.CartRaces.Add(new AcsCartRace { MissionId = e.U64("MissionID"), BestTime = e.U64("BestTime"), BestPosition = e.U32("BestPosition") });
                        break;
                }
            }
            return r;
        }
    }
}
