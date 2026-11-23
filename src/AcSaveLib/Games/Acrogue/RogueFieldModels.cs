using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acrogue
{
    // Models for the top-level fields of a Rogue save, other than the mission histories (see RogueMissionModels.cs).
    // Field names follow the names shown by ACViewer. Enum values are the raw numbers stored in the save.

    public sealed class AnimusDatabase
    {
        public byte NumRecentEntries { get; init; }
        /// <summary>IDs of unlocked Animus database entries (AnimusDatabaseEntry in the Forge index).</summary>
        public List<ulong> UnlockedEntries { get; init; } = new();
        public List<byte> EntriesData { get; init; } = new();
        public static AnimusDatabase From(Props p) => new()
        {
            NumRecentEntries = p.U8("NumRecentEntries"),
            UnlockedEntries = p.Packed("UnlockedEntries"),
            EntriesData = p.PackedU8("EntriesData"),
        };
    }

    public sealed class MapIcon
    {
        public ulong KeyObjectId { get; init; }
        public byte KeyIndex { get; init; }
        public bool IsEnabled { get; init; }
        public bool IsInterior { get; init; }
        public bool IsBlinking { get; init; }
        public bool IsFogged { get; init; }
        public bool IsKnown { get; init; }
        public byte ImageIndex { get; init; }
    }

    public sealed class MapIconWorld
    {
        public ulong WorldId { get; init; }
        public List<MapIcon> Icons { get; init; } = new();
    }

    public sealed class MapIconData
    {
        public List<MapIconWorld> SavedIcons { get; init; } = new();
        public static MapIconData From(Props p)
        {
            var d = new MapIconData();
            foreach (var w in p.Elements("SavedIcons"))
            {
                var mw = new MapIconWorld { WorldId = w.U64("WorldID") };
                foreach (var i in w.Elements("Icons"))
                    mw.Icons.Add(new MapIcon
                    {
                        KeyObjectId = i.U64("KeyObjectID"), KeyIndex = i.U8("KeyIndex"), IsEnabled = i.Bool("IsEnabled"),
                        IsInterior = i.Bool("IsInterior"), IsBlinking = i.Bool("IsBlinking"), IsFogged = i.Bool("IsFogged"),
                        IsKnown = i.Bool("IsKnown"), ImageIndex = i.U8("ImageIndex"),
                    });
                d.SavedIcons.Add(mw);
            }
            return d;
        }
    }

    public sealed class MapFogData
    {
        /// <summary>Number of MapFogInfo entries (0 in every sample; the entry layout is not known yet).</summary>
        public int MapFogInfoCount { get; init; }
        public static MapFogData From(Props p) => new() { MapFogInfoCount = p.ElementCount("MapFogInfo") };
    }

    public sealed class Message
    {
        public string EntryTitle { get; init; } = "";
        public string EntryDescription { get; init; } = "";
        /// <summary>Unix time (seconds), by its magnitude.</summary>
        public uint EntryTime { get; init; }
        public uint AFSCategory { get; init; }
        public uint MessageAction { get; init; }
        public uint AFSIcon { get; init; }
        public ulong AFSImage { get; init; }
        public uint LinkActionClassId { get; init; }
        public List<byte> LinkActionData { get; init; } = new();
    }

    public sealed class MessagingDatabase
    {
        public List<Message> Messages { get; init; } = new();
        public static MessagingDatabase From(Props p)
        {
            var d = new MessagingDatabase();
            foreach (var m in p.Elements("MDMessages"))
                d.Messages.Add(new Message
                {
                    EntryTitle = m.Text("EntryTitle"), EntryDescription = m.Text("EntryDescription"), EntryTime = m.U32("EntryTime"),
                    AFSCategory = m.Enum("AFSCategory"), MessageAction = m.U32("MessageAction"), AFSIcon = m.Enum("AFSIcon"),
                    AFSImage = m.U64("AFSImage"), LinkActionClassId = m.U32("LinkActionClassID"), LinkActionData = m.PackedU8("LinkActionData"),
                });
            return d;
        }
    }

    public sealed class UITutorialData
    {
        /// <summary>Entry counts of HUD/System/Controls/Fight/Tactics/World tutorial lists (AFSTutorialInfo; entry layout unknown, empty in samples).</summary>
        public Dictionary<string, int> TutorialCounts { get; init; } = new();
        public int OasisCurrentMissionObjectiveLineId { get; init; }
        public List<uint> VRMissionBestTimes { get; init; } = new();
        public List<uint> VRMissionBestCounts { get; init; } = new();
        public List<ulong> VRMissionLoaded { get; init; } = new();
        public List<uint> FightClubUnlockedLevels { get; init; } = new();
        public static UITutorialData From(Props p)
        {
            var d = new UITutorialData
            {
                OasisCurrentMissionObjectiveLineId = p.S32("OasisCurrentMissionObjectiveLineID"),
                VRMissionBestTimes = p.PackedU32("VRMissionBestTimes"),
                VRMissionBestCounts = p.PackedU32("VRMissionBestCounts"),
                VRMissionLoaded = p.Packed("VRMissionLoaded"),
                FightClubUnlockedLevels = p.PackedU32("FightClubUnlockedLevels"),
            };
            foreach (var n in new[] { "HUDTutorials", "SystemTutorials", "ControlsTutorials", "FightTutorials", "TacticsTutorials", "WorldTutorials" })
                d.TutorialCounts[n] = p.ElementCount(n);
            return d;
        }
    }

    public sealed class PresentEmails
    {
        public bool HasUnlockedAllPassword { get; init; }
        public bool HasNewEmail { get; init; }
        public List<bool> IsEmailDeleted { get; init; } = new();
        public List<bool> EmailGroupUnlocked { get; init; } = new();
        public List<bool> IsEmailNew { get; init; } = new();
        public static PresentEmails From(Props p) => new()
        {
            HasUnlockedAllPassword = p.Bool("HasUnlockedAllPassword"), HasNewEmail = p.Bool("HasNewEmail"),
            IsEmailDeleted = p.PackedBool("IsEmailDeleted"), EmailGroupUnlocked = p.PackedBool("EmailGroupUnlocked"), IsEmailNew = p.PackedBool("IsEmailNew"),
        };
    }

    public sealed class PresentRewardItem
    {
        public ulong RewardItem { get; init; }
        public bool IsAvailable { get; init; }
        public bool IsNew { get; init; }
    }

    public sealed class PresentRewards
    {
        public List<PresentRewardItem> RewardItems { get; init; } = new();
        public static PresentRewards From(Props p)
        {
            var d = new PresentRewards();
            foreach (var e in p.Elements("RewardItems"))
                d.RewardItems.Add(new PresentRewardItem { RewardItem = e.U64("RewardItem"), IsAvailable = e.Bool("IsAvailable"), IsNew = e.Bool("IsNew") });
            return d;
        }
    }

    public sealed class AccomplishmentEntry
    {
        /// <summary>Accomplishment ID (type Accomplishment in the Forge index).</summary>
        public ulong ObjectId { get; init; }
        public bool Inactive { get; init; }
        public bool Completed { get; init; }
        public uint CurrentCount { get; init; }
    }

    public sealed class AccomplishmentManager
    {
        public List<AccomplishmentEntry> Entries { get; init; } = new();
        public static AccomplishmentManager From(Props p)
        {
            var d = new AccomplishmentManager();
            foreach (var e in p.Elements("AccomplishmentEntries"))
                d.Entries.Add(new AccomplishmentEntry { ObjectId = e.U64("ObjectID"), Inactive = e.Bool("Inactive"), Completed = e.Bool("Completed"), CurrentCount = e.U32("CurrentCount") });
            return d;
        }
    }

    public readonly record struct StatEntry(uint StatId, uint StatValue);

    public sealed class GameStats
    {
        public List<StatEntry> StatEntries { get; init; } = new();
        public List<StatEntry> StatEntriesToPost { get; init; } = new();
        public List<uint> LastStatMilestonesReached { get; init; } = new();
        public static GameStats From(Props p)
        {
            var d = new GameStats { LastStatMilestonesReached = p.PackedU32("LastStatMilestonesReached") };
            foreach (var e in p.Elements("StatEntries")) d.StatEntries.Add(new StatEntry(e.U32("StatID"), e.U32("StatValue")));
            foreach (var e in p.Elements("StatEntriesToPost")) d.StatEntriesToPost.Add(new StatEntry(e.U32("StatID"), e.U32("StatValue")));
            return d;
        }
    }

    public sealed class CommunityChallenge
    {
        /// <summary>Number of UnsavedProgress entries (0 in every sample; entry layout unknown).</summary>
        public int UnsavedProgressCount { get; init; }
        public static CommunityChallenge From(Props p) => new() { UnsavedProgressCount = p.ElementCount("UnsavedProgress") };
    }

    public sealed class FleetCapturedBoat
    {
        public uint Archetype { get; init; }
        public uint Level { get; init; }
        public int OasisLineId { get; init; }
    }

    public sealed class FleetBoat
    {
        public uint BoatId { get; init; }
        /// <summary>Localization id of the boat name.</summary>
        public int BoatName { get; init; }
        public uint TemplateId { get; init; }
        public uint Health { get; init; }
        public uint Status { get; init; }
        public uint UnavailableTimeSeconds { get; init; }
        public ulong FetchTimeMilliseconds { get; init; }
    }

    public sealed class FleetMission
    {
        public string OwnerProfileId { get; init; } = "";
        public uint MissionId { get; init; }
        public string HelpedByProfileId { get; init; } = "";
        public uint TemplateId { get; init; }
        public uint DurationSeconds { get; init; }
        public List<uint> AssignedBoatIds { get; init; } = new();
        public uint Odds { get; init; }
        public uint Roll { get; init; }
        public int RemainingTimeSeconds { get; init; }
        /// <summary>Unix time in seconds, by its magnitude.</summary>
        public uint StartTime { get; init; }
        public ulong FetchTimeMilliseconds { get; init; }
    }

    public sealed class FleetReward
    {
        /// <summary>Enum FleetRewardID as stored (low 32 bits); ACViewer shows 1 as FleetCollectible_01.</summary>
        public uint RewardType { get; init; }
        public uint Value { get; init; }
        /// <summary>Unix time in seconds, by its magnitude.</summary>
        public uint TimeUnlocked { get; init; }
    }

    public sealed class FleetRoute
    {
        public uint RouteId { get; init; }
        public uint DifficultyModifier { get; init; }
        public uint Status { get; init; }
        public uint LastUpdateTimeSeconds { get; init; }
        /// <summary>Unix time in seconds, by its magnitude.</summary>
        public uint LastUpdateTime { get; init; }
    }

    /// <summary>FleetPlayerGameData: the player's fleet (Rogue only).</summary>
    public sealed class FleetPlayer
    {
        public string ProfileId { get; init; } = "";
        public uint Status { get; init; }
        public uint UnlockedBoatSlots { get; init; }
        public List<FleetBoat> Boats { get; init; } = new();
        /// <summary>Value of each FleetResourceGameData entry, in file order.</summary>
        public List<uint> Resources { get; init; } = new();
        public List<FleetRoute> Routes { get; init; } = new();
        public List<FleetMission> Missions { get; init; } = new();
        public List<FleetReward> Rewards { get; init; } = new();
        public uint BoatAutoGenId { get; init; }
        public uint MissionAutoGenId { get; init; }
        public static FleetPlayer From(Props p)
        {
            var d = new FleetPlayer
            {
                ProfileId = p.Text("ProfileId"), Status = p.U32("Status"), UnlockedBoatSlots = p.U32("UnlockedBoatSlots"),
                BoatAutoGenId = p.U32("BoatAutoGenID"), MissionAutoGenId = p.U32("MissionAutoGenID"),
            };
            foreach (var e in p.Elements("Boats"))
                d.Boats.Add(new FleetBoat { BoatId = e.U32("BoatId"), BoatName = e.S32("BoatName"), TemplateId = e.U32("TemplateId"), Health = e.U32("Health"), Status = e.U32("Status"), UnavailableTimeSeconds = e.U32("UnavailableTimeSeconds"), FetchTimeMilliseconds = e.U64("FetchTimeMilliseconds") });
            foreach (var e in p.Elements("Missions"))
                d.Missions.Add(new FleetMission
                {
                    OwnerProfileId = e.Text("OwnerProfileId"), MissionId = e.U32("MissionId"), HelpedByProfileId = e.Text("HelpedByProfileId"), TemplateId = e.U32("TemplateId"),
                    DurationSeconds = e.U32("DurationSeconds"), AssignedBoatIds = e.PackedU32("AssignedBoatIds"), Odds = e.U32("Odds"), Roll = e.U32("Roll"),
                    RemainingTimeSeconds = e.S32("RemainingTimeSeconds"), StartTime = e.U32("StartTime"), FetchTimeMilliseconds = e.U64("FetchTimeMilliseconds"),
                });
            foreach (var e in p.Elements("Rewards")) d.Rewards.Add(new FleetReward { RewardType = e.Enum("RewardType"), Value = e.U32("Value"), TimeUnlocked = e.U32("TimeUnlocked") });
            foreach (var e in p.Elements("Resources")) d.Resources.Add(e.U32("Value"));
            foreach (var e in p.Elements("Routes"))
                d.Routes.Add(new FleetRoute { RouteId = e.U32("RouteId"), DifficultyModifier = e.U32("DifficultyModifier"), Status = e.U32("Status"), LastUpdateTimeSeconds = e.U32("LastUpdateTimeSeconds"), LastUpdateTime = e.U32("LastUpdateTime") });
            return d;
        }
    }

    public sealed class BusinessManager
    {
        public int PurchasedBusinessLevelCount { get; init; }
        public uint MoneyInBank { get; init; }
        public uint TimePassedSinceLastReward { get; init; }
        public static BusinessManager From(Props p) => new() { PurchasedBusinessLevelCount = p.Packed("PurchasedBusinessLevels").Count, MoneyInBank = p.U32("MoneyInBank"), TimePassedSinceLastReward = p.U32("TimePassedSinceLastReward") };
    }

    public sealed class FleetData
    {
        public uint CaptureBoatCount { get; init; }
        public uint CaptureBoatNextSlot { get; init; }
        public List<FleetCapturedBoat> CapturedBoats { get; init; } = new();
        public FleetPlayer Player { get; init; } = new();
        public static FleetData From(Props p)
        {
            var d = new FleetData { CaptureBoatCount = p.U32("CaptureBoatCount"), CaptureBoatNextSlot = p.U32("CaptureBoatNextSlot"), Player = FleetPlayer.From(p.Obj("FleetPlayerGameData")) };
            foreach (var e in p.Elements("CapturedBoats"))
                d.CapturedBoats.Add(new FleetCapturedBoat { Archetype = e.Enum("Archetype"), Level = e.Enum("Level"), OasisLineId = e.S32("OasisLineId") });
            return d;
        }
    }

    public sealed class SnapshotDelta
    {
        public sealed class CharacterItemsDelta
        {
            public ulong ProgressionCharacter { get; init; }
            public List<ulong> ItemsDelta { get; init; } = new();
            public List<ulong> GainedInventoryItemsDelta { get; init; } = new();
        }
        public List<CharacterItemsDelta> PlayerCharacterItemsDelta { get; init; } = new();
        public List<ulong> UncollectedInventoryItemsOnMapDelta { get; init; } = new();
        public static SnapshotDelta From(Props p)
        {
            var d = new SnapshotDelta { UncollectedInventoryItemsOnMapDelta = p.Packed("UncollectedInventoryItemsOnMapDelta") };
            foreach (var e in p.Elements("PlayerCharacterItemsDelta"))
                d.PlayerCharacterItemsDelta.Add(new CharacterItemsDelta { ProgressionCharacter = e.U64("ProgressionCharacter"), ItemsDelta = e.Packed("ItemsDelta"), GainedInventoryItemsDelta = e.Packed("GainedInventoryItemsDelta") });
            return d;
        }
    }
}
