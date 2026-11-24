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
        /// <summary>Tags entries inside the unnamed object a0da2041.</summary>
        public List<ulong> Tags { get; init; } = new();
        public List<ulong> TagAIAction { get; init; } = new();
        public bool BrokenHealth { get; init; }
        public uint Life { get; init; }
        public uint MaxLife { get; init; }
        public uint LifetimeCreedPoints { get; init; }
        public uint CurrentCreedPointsIncrease { get; init; }
        public List<ulong> DiscoveredMissions { get; init; } = new();
        public uint Xp { get; init; }
        public uint Tier { get; init; }

        public static AcsProgressionCharacter From(Props p)
        {
            var c = new AcsProgressionCharacter
            {
                ProgressionCharacter = p.Obj("ProgressionCharacterSelector").U64("ProgressionCharacter"),
                GainedInventoryItems = p.Packed("GainedInventoryItems"), LockedInventoryItems = p.Packed("LockedInventoryItems"),
                UseDefaultSelectedWeaponAndEquipment = p.Bool("UseDefaultSelectedWeaponAndEquipment"),
                SelectedWeapon = p.U64("SelectedWeapon"), SelectedEquipment = p.U64("SelectedEquipment"), SelectedRanged = p.U64("SelectedRanged"),
                NewInventoryItems = p.Packed("NewInventoryItems"), TagAIAction = p.Packed("TagAIAction"), BrokenHealth = p.Bool("BrokenHealth"),
                Life = p.U32("Life"), MaxLife = p.U32("MaxLife"), LifetimeCreedPoints = p.U32("LifetimeCreedPoints"),
                CurrentCreedPointsIncrease = p.U32("CurrentCreedPointsIncrease"), DiscoveredMissions = p.Packed("DiscoveredMissions"),
                Xp = p.U32("XP"), Tier = p.U32("Tier"),
            };
            foreach (var e in p.Obj("InventorySpecification").Elements("InventoryContainerList")) c.Containers.Add(AcsInventoryContainer.From(e));
            var tags = new Props(p.GetByKey(0xA0DA2041)?.Children);
            foreach (var e in tags.Elements("Tags")) c.Tags.Add(e.U64("Tag"));
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
        public bool DataInitialized { get; init; }
        public List<AcsInventoryItemSetting> SavedInventoryItems { get; init; } = new();

        public static AcsPlayerProgression From(Node n)
        {
            var p = new Props(n.Children);
            var r = new AcsPlayerProgression { UncollectedInventoryItemsOnMap = p.Packed("UncollectedInventoryItemsOnMap"), DataInitialized = p.Bool("DataInitialized") };
            foreach (var e in p.Elements("ProgressionCharacterData")) r.Characters.Add(AcsProgressionCharacter.From(e));
            foreach (var e in p.Elements("SavedInventoryItems")) r.SavedInventoryItems.Add(new AcsInventoryItemSetting { IsLock = e.Bool("IsLock"), Item = e.U64("InventoryItems") });
            return r;
        }
    }
}
