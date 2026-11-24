using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acu
{
    public sealed class AcuAccomplishment
    {
        public ulong ObjectId { get; init; }
        public bool Inactive { get; init; }
        public bool Completed { get; init; }
        public uint CurrentCount { get; init; }
    }

    public sealed class AcuTrophyCondition
    {
        public bool Unlocked { get; init; }
        public bool New { get; init; }
    }

    public sealed class AcuTrophyManager
    {
        public List<AcuTrophyCondition> MissionConditions { get; init; } = new();
        public List<AcuTrophyCondition> AccomplishmentConditions { get; init; } = new();
        /// <summary>GearConditionData GearLevels (7 values).</summary>
        public List<int> GearLevels { get; init; } = new();
        public uint UnlockedAchievementCount { get; init; }
        public List<AcuTrophyCondition> LegacyConditions { get; init; } = new();
        public List<AcuTrophyCondition> EliseLetterConditions { get; init; } = new();

        public static AcuTrophyManager From(Props p)
        {
            var t = new AcuTrophyManager();
            Fill(p, "MissionConditionData", t.MissionConditions);
            Fill(p, "AccomplishmentConditionData", t.AccomplishmentConditions);
            Fill(p, "LegacyConditionData", t.LegacyConditions);
            Fill(p, "EliseLetterConditionData", t.EliseLetterConditions);
            var gear = new List<int>();
            foreach (var e in p.Elements("GearConditionData")) foreach (var v in e.Packed("GearLevels")) gear.Add(unchecked((int)(uint)v));
            uint ach = 0;
            foreach (var e in p.Elements("AchievementConditionData")) ach = e.U32("UnlockedAchievementCount");
            return new AcuTrophyManager
            {
                MissionConditions = t.MissionConditions, AccomplishmentConditions = t.AccomplishmentConditions, LegacyConditions = t.LegacyConditions,
                EliseLetterConditions = t.EliseLetterConditions, GearLevels = gear, UnlockedAchievementCount = ach,
            };
        }

        private static void Fill(Props p, string name, List<AcuTrophyCondition> into)
        {
            foreach (var e in p.Elements(name)) into.Add(new AcuTrophyCondition { Unlocked = e.Bool("Unlocked"), New = e.Bool("New") });
        }
    }

    public sealed class AcuStatistic
    {
        public uint StatId { get; init; }
        public uint StatValue { get; init; }
    }

    public sealed class AcuPlayerStatistics
    {
        public List<AcuStatistic> Local { get; init; } = new();
        public List<AcuStatistic> Unsent { get; init; } = new();

        public static AcuPlayerStatistics From(Props p)
        {
            var s = new AcuPlayerStatistics();
            foreach (var e in p.Elements("LocalStatistics")) s.Local.Add(new AcuStatistic { StatId = e.U32("StatId"), StatValue = e.U32("StatValue") });
            foreach (var e in p.Elements("UnsentStatistics")) s.Unsent.Add(new AcuStatistic { StatId = e.U32("StatId"), StatValue = e.U32("StatValue") });
            return s;
        }
    }

    public sealed class AcuBuilding
    {
        public ulong Building { get; init; }
        public bool IsNewlyGained { get; init; }
    }

    /// <summary>WorldUpgradeSaveData and its Snapshot twin.</summary>
    public sealed class AcuWorldUpgrade
    {
        public List<ulong> GainedWorldUpgradeSettings { get; init; } = new();
        public List<AcuBuilding> Buildings { get; init; } = new();
        public int BuildingRuleCount { get; init; }
        public uint CurrentGoldChestAmount { get; init; }
        public uint CurrentIncomeDelay { get; init; }
        public uint PendingIncomeValueForAfs { get; init; }

        public static AcuWorldUpgrade From(Node n)
        {
            var p = new Props(n.Children);
            var b = p.Obj("BuildingsSaveData");
            var w = new AcuWorldUpgrade
            {
                GainedWorldUpgradeSettings = p.Packed("GainedWorldUpgradeSettings"), BuildingRuleCount = b.ElementCount("BuildingsRules"),
                CurrentGoldChestAmount = p.U32("CurrentGoldChestAmount"), CurrentIncomeDelay = p.U32("CurrentIncomeDelay"),
                PendingIncomeValueForAfs = p.U32("PendingIncomeValueForAfs"),
            };
            foreach (var e in b.Elements("Buildings")) w.Buildings.Add(new AcuBuilding { Building = e.U64("Building"), IsNewlyGained = e.Bool("IsNewlyGained") });
            return w;
        }
    }

    /// <summary>One PlayerCharacterItemsDelta entry of SnapshotPlayerProgressionDelta. Its fields beyond the named ones stay in the node tree.</summary>
    public sealed class AcuSnapshotDelta
    {
        public int PlayerCharacterItemsDeltaCount { get; init; }
        public List<ulong> UncollectedInventoryItemsOnMapDelta { get; init; } = new();

        public static AcuSnapshotDelta From(Node n)
        {
            var p = new Props(n.Children);
            return new AcuSnapshotDelta { PlayerCharacterItemsDeltaCount = p.ElementCount("PlayerCharacterItemsDelta"), UncollectedInventoryItemsOnMapDelta = p.Packed("UncollectedInventoryItemsOnMapDelta") };
        }
    }

    public sealed class AcuTimedCollectible
    {
        public uint CollectibleId { get; init; }
        /// <summary>Unix time in seconds as stored.</summary>
        public uint CollectedTimestamp { get; init; }
    }

    public sealed class AcuClanManager
    {
        public int InviteCount { get; init; }
        public int NotificationCount { get; init; }
        public string ClanTag { get; init; } = "";
        public uint LastNotificationDate { get; init; }
        public uint LastApplicationDate { get; init; }
        /// <summary>ACViewer shows this key as Property_0x98CF025E (U32).</summary>
        public uint Property98CF025E { get; init; }
    }

    /// <summary>The main container of a Unity game save: typed models of every named top-level field. Unnamed keys stay in the node tree.</summary>
    public sealed class AcuGameData
    {
        public bool HasGameData { get; private set; }
        public uint ContextId { get; private set; }
        public MissionHistory Narrative { get; private set; } = new();
        public MissionHistory Side { get; private set; } = new();
        public ulong CurrentRoot { get; private set; }
        public ulong CurrentMission { get; private set; }
        public ulong TrackedMission { get; private set; }
        public ulong World { get; private set; }
        public ulong PlayerPortal { get; private set; }
        public ulong TransitionProfile { get; private set; }
        public uint AnimusNumRecentEntries { get; private set; }
        public List<ulong> AnimusUnlockedEntries { get; private set; } = new();
        public List<ulong> AnimusEntriesData { get; private set; } = new();
        public int MapFogInfoCount { get; private set; }
        public int MessageCount { get; private set; }
        public List<ulong> UnlockedTrainings { get; private set; } = new();
        public List<ulong> NewTrainings { get; private set; } = new();
        public List<ulong> CompletedTrainings { get; private set; } = new();
        public List<ulong> UnlockedCases { get; private set; } = new();
        public List<ulong> NewCases { get; private set; } = new();
        public List<ulong> DiscoveredClues { get; private set; } = new();
        public List<ulong> NewClues { get; private set; } = new();
        public bool HasUnlockedAllPassword { get; private set; }
        public bool HasNewEmail { get; private set; }
        public List<ulong> EmailGroupUnlocked { get; private set; } = new();
        public List<AcuAccomplishment> Accomplishments { get; private set; } = new();
        public AcuPlayerProgression PlayerProgression { get; private set; } = new();
        public AcuPlayerProgression SnapshotPlayerProgression { get; private set; } = new();
        public AcuSnapshotDelta SnapshotDelta { get; private set; } = new();
        public AcuWorldUpgrade WorldUpgrade { get; private set; } = new();
        public AcuWorldUpgrade SnapshotWorldUpgrade { get; private set; } = new();
        public AcuTrophyManager Villa { get; private set; } = new();
        public AcuPlayerStatistics Statistics { get; private set; } = new();
        public int RecentContactCount { get; private set; }
        public List<AcuTimedCollectible> TimedCollectibles { get; private set; } = new();
        public AcuOnlineGameEntity OnlineEntity { get; private set; } = new();
        public List<ulong> DiscoveredIcons { get; private set; } = new();
        public uint CompletedCrowdEvents { get; private set; }
        public AcuClanManager ClanManager { get; private set; } = new();
        public uint VeryLongBanStartDate { get; private set; }

        public static AcuGameData From(List<Node> root)
        {
            var r = new AcuGameData();
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
                    case "TrackedMission": r.TrackedMission = n.Value; break;
                    case "World": r.World = n.Value; break;
                    case "PlayerPortal": r.PlayerPortal = n.Value; break;
                    case "TransitionProfile": r.TransitionProfile = n.Value; break;
                    case "AnimusDatabaseData": r.AnimusNumRecentEntries = p.U32("NumRecentEntries"); r.AnimusUnlockedEntries = p.Packed("UnlockedEntries"); r.AnimusEntriesData = p.Packed("EntriesData"); break;
                    case "MapFogData": r.MapFogInfoCount = p.ElementCount("MapFogInfo"); break;
                    case "MessagingDatabaseData": r.MessageCount = p.ElementCount("MDMessages"); break;
                    case "TrainingData": r.UnlockedTrainings = p.Packed("UnlockedTrainings"); r.NewTrainings = p.Packed("NewTrainings"); r.CompletedTrainings = p.Packed("CompletedTrainings"); break;
                    case "CaseFileData": r.UnlockedCases = p.Packed("UnlockedCases"); r.NewCases = p.Packed("NewCases"); r.DiscoveredClues = p.Packed("DiscoveredClues"); r.NewClues = p.Packed("NewClues"); break;
                    case "PresentEmailsData": r.HasUnlockedAllPassword = p.Bool("HasUnlockedAllPassword"); r.HasNewEmail = p.Bool("HasNewEmail"); r.EmailGroupUnlocked = p.Packed("EmailGroupUnlocked"); break;
                    case "AccomplishmentManagerData": r.Accomplishments = AcuAccomplishments(p); break;
                    case "PlayerProgSaveData": r.PlayerProgression = AcuPlayerProgression.From(n); break;
                    case "SnapshotPlayerProgSaveData": r.SnapshotPlayerProgression = AcuPlayerProgression.From(n); break;
                    case "SnapshotPlayerProgressionDelta": r.SnapshotDelta = AcuSnapshotDelta.From(n); break;
                    case "WorldUpgradeSaveData": r.WorldUpgrade = AcuWorldUpgrade.From(n); break;
                    case "SnapshotWorldUpgradeSaveData": r.SnapshotWorldUpgrade = AcuWorldUpgrade.From(n); break;
                    case "VillaManagerSaveData": r.Villa = AcuTrophyManager.From(p.Obj("TrophyManagerSaveData")); break;
                    case "OnlineGameSaveData": r.Statistics = AcuPlayerStatistics.From(p.Obj("PlayerStatisticsSaveData")); break;
                    case "ContactOnlineServiceSaveData": r.RecentContactCount = p.ElementCount("RecentContacts"); break;
                    case "TimedCollectibleSaveData":
                        foreach (var e in p.Elements("TimedCollectibles")) r.TimedCollectibles.Add(new AcuTimedCollectible { CollectibleId = e.U32("CollectibleID"), CollectedTimestamp = e.U32("CollectedTimestamp") });
                        break;
                    case "OnlineGameEntitySaveData": r.OnlineEntity = AcuOnlineGameEntity.From(p); break;
                    case "PermanentIconSaveData": r.DiscoveredIcons = p.Packed("DiscoveredIcons"); break;
                    case "CrowdEventsSaveData": r.CompletedCrowdEvents = p.U32("CompletedCrowdEventsNb"); break;
                    case "ClanManagerData":
                        r.ClanManager = new AcuClanManager
                        {
                            InviteCount = p.ElementCount("ClanInvites"), NotificationCount = p.ElementCount("ClanNotifications"), ClanTag = p.Text("ClanTag"),
                            LastNotificationDate = p.U32("LastNotificationDate"), LastApplicationDate = p.U32("LastApplicationDate"),
                            Property98CF025E = (uint)(p.GetByKey(0x98CF025E)?.Value ?? 0),
                        };
                        break;
                    case "PushMatchmakingData": r.VeryLongBanStartDate = p.U32("VeryLongBanStartDate"); break;
                }
            }
            return r;
        }

        private static List<AcuAccomplishment> AcuAccomplishments(Props p)
        {
            var l = new List<AcuAccomplishment>();
            foreach (var e in p.Elements("AccomplishmentEntries"))
                l.Add(new AcuAccomplishment { ObjectId = e.U64("ObjectID"), Inactive = e.Bool("Inactive"), Completed = e.Bool("Completed"), CurrentCount = e.U32("CurrentCount") });
            return l;
        }
    }
}
