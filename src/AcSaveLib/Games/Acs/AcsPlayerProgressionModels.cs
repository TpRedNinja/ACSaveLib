using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acs
{
    public sealed class AcsInventoryContainer
    {
        public ulong Category { get; init; }
        public uint CurrentContainerLevel { get; init; }
        public uint CurrentItemCount { get; init; }
        public uint OverflowQuantity { get; init; }
        public List<ulong> Items { get; init; } = new();
        public List<ulong> OverflowItems { get; init; } = new();

        public static AcsInventoryContainer From(Props p)
        {
            var c = new AcsInventoryContainer
            {
                Category = p.U64("Category"), CurrentContainerLevel = p.U32("CurrentContainerLevel"),
                CurrentItemCount = p.U32("CurrentItemCount"), OverflowQuantity = p.U32("OverflowQuantity"),
            };
            foreach (var e in p.Elements("ItemList")) c.Items.Add(e.U64("Item"));
            foreach (var e in p.Elements("OverflowItemList")) c.OverflowItems.Add(e.U64("Item"));
            return c;
        }
    }

    /// <summary>PlayerProgressionCharacterData: one per progression character (3 in the saves read). Unnamed keys stay in the node tree.</summary>
    public sealed class AcsProgressionCharacter
    {
        public ulong ProgressionCharacter { get; init; }
        public List<AcsInventoryContainer> Containers { get; init; } = new();
        public List<ulong> GainedInventoryItems { get; init; } = new();
        public List<ulong> LockedInventoryItems { get; init; } = new();
        public bool UseDefaultSelectedWeaponAndEquipment { get; init; }
        public ulong SelectedWeapon { get; init; }
        public ulong SelectedEquipment { get; init; }
        public ulong SelectedRanged { get; init; }
        public List<ulong> NewInventoryItems { get; init; } = new();
        /// <summary>CharacterCustomizationSpawnTag > Tags[] > Tag.</summary>
        public List<ulong> Tags { get; init; } = new();
        public List<ulong> TagAIAction { get; init; } = new();
        public int BrokenHealth { get; init; }
        public int Life { get; init; }
        public int MaxLife { get; init; }
        public List<ulong> UnlockedSkills { get; init; } = new();
        public List<ulong> AcknowledgedSkills { get; init; } = new();
        public List<ulong> AcquiredSkills { get; init; } = new();
        public uint AcquiredSkillPoints { get; init; }
        public ulong LifetimeCreedPoints { get; init; }
        public ulong CurrentCreedPointsIncrease { get; init; }
        public List<ulong> DiscoveredMissions { get; init; } = new();
        public uint Xp { get; init; }
        public uint Tier { get; init; }
        public bool CanGainXp { get; init; }
        public bool CanEarnMoney { get; init; }

        public static AcsProgressionCharacter From(Props p)
        {
            var c = new AcsProgressionCharacter
            {
                ProgressionCharacter = p.Obj("ProgressionCharacterSelector").U64("ProgressionCharacter"),
                GainedInventoryItems = p.Packed("GainedInventoryItems"), LockedInventoryItems = p.Packed("LockedInventoryItems"),
                UseDefaultSelectedWeaponAndEquipment = p.Bool("UseDefaultSelectedWeaponAndEquipment"),
                SelectedWeapon = p.U64("SelectedWeapon"), SelectedEquipment = p.U64("SelectedEquipment"), SelectedRanged = p.U64("SelectedRanged"),
                NewInventoryItems = p.Packed("NewInventoryItems"), TagAIAction = p.Packed("TagAIAction"), BrokenHealth = p.S32("BrokenHealth"),
                Life = p.S32("Life"), MaxLife = p.S32("MaxLife"),
                UnlockedSkills = p.Packed("UnlockedSkills"), AcknowledgedSkills = p.Packed("AcknowledgedSkills"), AcquiredSkills = p.Packed("AcquiredSkills"),
                AcquiredSkillPoints = p.U32("AcquiredSkillPoints"), LifetimeCreedPoints = p.U64("LifetimeCreedPoints"),
                CurrentCreedPointsIncrease = p.U64("CurrentCreedPointsIncrease"), DiscoveredMissions = p.Packed("DiscoveredMissions"),
                Xp = p.U32("XP"), Tier = p.U32("Tier"), CanGainXp = p.Bool("CanGainXP"), CanEarnMoney = p.Bool("CanEarnMoney"),
            };
            foreach (var e in p.Obj("InventorySpecification").Elements("InventoryContainerList")) c.Containers.Add(AcsInventoryContainer.From(e));
            foreach (var e in p.Obj("CharacterCustomizationSpawnTag").Elements("Tags")) c.Tags.Add(e.U64("Tag"));
            return c;
        }
    }

    public sealed class AcsInventoryItemSetting
    {
        public bool IsLock { get; init; }
        public ulong Item { get; init; }
    }

    /// <summary>PlayerProgSaveData and its Snapshot twin.</summary>
    public sealed class AcsPlayerProgression
    {
        public List<ulong> UncollectedInventoryItemsOnMap { get; init; } = new();
        public List<AcsProgressionCharacter> Characters { get; init; } = new();
        public ulong CurrentProgressionCharacter { get; init; }
        public ulong LastLondonProgressionCharacter { get; init; }
        public List<ulong> AcquiredPerks { get; init; } = new();
        public List<ulong> AcknowledgedPerks { get; init; } = new();
        public bool DataInitialized { get; init; }
        public List<AcsInventoryItemSetting> SavedInventoryItems { get; init; } = new();

        public static AcsPlayerProgression From(Node n)
        {
            var p = new Props(n.Children);
            var r = new AcsPlayerProgression { UncollectedInventoryItemsOnMap = p.Packed("UncollectedInventoryItemsOnMap"), DataInitialized = p.Bool("DataInitialized"),
                CurrentProgressionCharacter = p.U64("CurrentProgressionCharacter"), LastLondonProgressionCharacter = p.U64("LastLondonProgressionCharacter"),
                AcquiredPerks = p.Packed("AcquiredPerks"), AcknowledgedPerks = p.Packed("AcknowledgedPerks") };
            foreach (var e in p.Elements("ProgressionCharacterData")) r.Characters.Add(AcsProgressionCharacter.From(e));
            foreach (var e in p.Elements("SavedInventoryItems")) r.SavedInventoryItems.Add(new AcsInventoryItemSetting { IsLock = e.Bool("IsLock"), Item = e.U64("InventoryItems") });
            return r;
        }
    }

    public sealed class AcsConsumableQuantity
    {
        public ulong Category { get; init; }
        public uint Quantity { get; init; }
    }

    /// <summary>SnapshotPlayerProgressionDelta > PlayerCharacterItemsDelta: one per progression character.</summary>
    public sealed class AcsCharacterDelta
    {
        public ulong ProgressionCharacter { get; init; }
        public List<ulong> ItemsDelta { get; init; } = new();
        public List<ulong> GainedInventoryItemsDelta { get; init; } = new();
        public List<AcsConsumableQuantity> ConsumableQuantities { get; init; } = new();
        public uint Xp { get; init; }
        public uint AcquiredSkillPoints { get; init; }
        public List<ulong> AcquiredSkills { get; init; } = new();
        public List<ulong> UnlockedSkills { get; init; } = new();
        public List<ulong> AcknowledgedSkills { get; init; } = new();

        public static AcsCharacterDelta From(Props p)
        {
            var d = new AcsCharacterDelta
            {
                ProgressionCharacter = p.U64("ProgressionCharacter"), ItemsDelta = p.Packed("ItemsDelta"),
                GainedInventoryItemsDelta = p.Packed("GainedInventoryItemsDelta"), Xp = p.U32("XP"),
                AcquiredSkillPoints = p.U32("AcquiredSkillPoints"), AcquiredSkills = p.Packed("AcquiredSkills"),
                UnlockedSkills = p.Packed("UnlockedSkills"), AcknowledgedSkills = p.Packed("AcknowledgedSkills"),
            };
            foreach (var e in p.Elements("PlayerCharacterConsumableQuantity"))
                d.ConsumableQuantities.Add(new AcsConsumableQuantity { Category = e.U64("Category"), Quantity = e.U32("Quantity") });
            return d;
        }
    }

    public sealed class AcsProgressionDelta
    {
        public List<AcsCharacterDelta> Characters { get; init; } = new();
        public List<ulong> UncollectedInventoryItemsOnMapDelta { get; init; } = new();
        public List<ulong> AcquiredPerks { get; init; } = new();
        public List<ulong> AcknowledgedPerks { get; init; } = new();

        public static AcsProgressionDelta From(Node n)
        {
            var p = new Props(n.Children);
            var r = new AcsProgressionDelta
            {
                UncollectedInventoryItemsOnMapDelta = p.Packed("UncollectedInventoryItemsOnMapDelta"),
                AcquiredPerks = p.Packed("AcquiredPerks"), AcknowledgedPerks = p.Packed("AcknowledgedPerks"),
            };
            foreach (var e in p.Elements("PlayerCharacterItemsDelta")) r.Characters.Add(AcsCharacterDelta.From(e));
            return r;
        }
    }
}
