using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acu
{
    public sealed class AcuInventoryContainer
    {
        public ulong Category { get; init; }
        public uint CurrentContainerLevel { get; init; }
        public uint CurrentItemCount { get; init; }
        public uint OverflowQuantity { get; init; }
        public List<ulong> Items { get; init; } = new();
        public List<ulong> OverflowItems { get; init; } = new();

        public static AcuInventoryContainer From(Props p)
        {
            var c = new AcuInventoryContainer
            {
                Category = p.U64("Category"), CurrentContainerLevel = p.U32("CurrentContainerLevel"),
                CurrentItemCount = p.U32("CurrentItemCount"), OverflowQuantity = p.U32("OverflowQuantity"),
            };
            foreach (var e in p.Elements("ItemList")) c.Items.Add(e.U64("Item"));
            foreach (var e in p.Elements("OverflowItemList")) c.OverflowItems.Add(e.U64("Item"));
            return c;
        }
    }

    /// <summary>One entry of the three Property_0x InventoryItemLists (ACViewer shows the list keys without names).</summary>
    public sealed class AcuInventoryItemEntry
    {
        public uint Count { get; init; }
        public ulong Type { get; init; }
        public ulong Item { get; init; }
        public ulong RechargeableCategory { get; init; }
    }

    public sealed class AcuQuickSlot
    {
        public uint ItemEnumValue { get; init; }
        public ulong ItemEnumType { get; init; }
    }

    public sealed class AcuAvatarLoadout
    {
        public ulong WaistSlot { get; init; }
        public ulong ChestSlot { get; init; }
        public ulong ForearmsSlot { get; init; }
        public ulong HeadSlot { get; init; }
        public ulong LegsSlot { get; init; }
        public ulong MeleeWeaponSlot { get; init; }
        public ulong RangedWeaponSlot { get; init; }
        public ulong DlcItemSlot { get; init; }
        public ulong CostumeSlot { get; init; }
        public ulong ColorSlot { get; init; }
        public ulong CurrentCoopSkill { get; init; }
        public ulong CurrentSelectedBooster { get; init; }

        public static AcuAvatarLoadout From(Props p) => new AcuAvatarLoadout
        {
            WaistSlot = p.U64("WaistSlot"), ChestSlot = p.U64("ChestSlot"), ForearmsSlot = p.U64("ForearmsSlot"), HeadSlot = p.U64("HeadSlot"),
            LegsSlot = p.U64("LegsSlot"), MeleeWeaponSlot = p.U64("MeleeWeaponSlot"), RangedWeaponSlot = p.U64("RangedWeaponSlot"),
            DlcItemSlot = p.U64("DLCItemSlot"), CostumeSlot = p.U64("CostumeSlot"), ColorSlot = p.U64("ColorSlot"),
            CurrentCoopSkill = p.U64("CurrentCoopSkill"), CurrentSelectedBooster = p.U64("CurrentSelectedBooster"),
        };
    }

    /// <summary>Gained/locked/new item lists and selected equipment. Used for the character itself and for BackupInventoryItems.</summary>
    public sealed class AcuInventoryState
    {
        public List<ulong> GainedInventoryItems { get; init; } = new();
        public List<ulong> LockedInventoryItems { get; init; } = new();
        public int ArmorStateCount { get; init; }
        public bool UseDefaultSelectedWeaponAndEquipment { get; init; }
        public ulong SelectedWeapon { get; init; }
        public ulong SelectedEquipment { get; init; }
        public ulong SelectedRanged { get; init; }
        public List<AcuQuickSlot> QuickSlotItems { get; init; } = new();
        public List<AcuQuickSlot> QuickSlotWeapons { get; init; } = new();
        public List<ulong> NewInventoryItems { get; init; } = new();

        public static AcuInventoryState From(Props p) => new AcuInventoryState
        {
            GainedInventoryItems = p.Packed("GainedInventoryItems"), LockedInventoryItems = p.Packed("LockedInventoryItems"),
            ArmorStateCount = p.ElementCount("ArmorStates"),
            UseDefaultSelectedWeaponAndEquipment = p.Bool("UseDefaultSelectedWeaponAndEquipment"),
            SelectedWeapon = p.U64("SelectedWeapon"), SelectedEquipment = p.U64("SelectedEquipment"), SelectedRanged = p.U64("SelectedRanged"),
            QuickSlotItems = Slots(p, "QuickSlotItems"), QuickSlotWeapons = Slots(p, "QuickSlotWeapons"),
            NewInventoryItems = p.Packed("NewInventoryItems"),
        };

        private static List<AcuQuickSlot> Slots(Props p, string name)
        {
            var l = new List<AcuQuickSlot>();
            foreach (var e in p.Elements(name)) l.Add(new AcuQuickSlot { ItemEnumValue = e.U32("ItemEnumValue"), ItemEnumType = e.U64("ItemEnumType") });
            return l;
        }
    }

    /// <summary>PlayerProgressionCharacterData: one per progression character (3 in a game save) and per snapshot character.</summary>
    public sealed class AcuProgressionCharacter
    {
        public AcuInventoryState Inventory { get; init; } = new();
        public AcuInventoryState Backup { get; init; } = new();
        public List<AcuInventoryContainer> Containers { get; init; } = new();
        /// <summary>Entries of the first Property_0x7063A919 list (ItemDescriptor with Type, Item, RechargeableCategory, plus Count).</summary>
        public List<AcuInventoryItemEntry> ItemListA { get; init; } = new();
        /// <summary>Property_0xBFA174B0, same layout.</summary>
        public List<AcuInventoryItemEntry> ItemListB { get; init; } = new();
        /// <summary>Property_0x83F9AC79, same layout.</summary>
        public List<AcuInventoryItemEntry> ItemListC { get; init; } = new();
        public AcuAvatarLoadout MainAvatarLoadout { get; init; } = new();
        public List<ulong> TagAIAction { get; init; } = new();
        public bool BrokenHealth { get; init; }
        public uint Life { get; init; }
        public uint MaxLife { get; init; }
        public ulong AvatarSkills { get; init; }
        public ulong AvatarSkillsUnlock { get; init; }
        public uint SpendedAvatarSkillPoint { get; init; }
        public uint LifetimeCreedPoints { get; init; }
        public uint CurrentCreedPointsIncrease { get; init; }
        public ulong CurrentChallenge { get; init; }
        public List<ulong> DiscoveredMissions { get; init; } = new();

        public static AcuProgressionCharacter From(Props p)
        {
            var c = new AcuProgressionCharacter
            {
                Inventory = AcuInventoryState.From(p), Backup = AcuInventoryState.From(p.Obj("BackupInventoryItems")),
                ItemListA = Entries(p.Obj("Property_0x7063A919")), ItemListB = Entries(p.Obj("Property_0xBFA174B0")), ItemListC = Entries(p.Obj("Property_0x83F9AC79")),
                MainAvatarLoadout = AcuAvatarLoadout.From(p.Obj("MainAvatarLoadout")),
                TagAIAction = p.Packed("TagAIAction"), BrokenHealth = p.Bool("BrokenHealth"),
                Life = p.U32("Life"), MaxLife = p.U32("MaxLife"), AvatarSkills = p.U64("AvatarSkills"), AvatarSkillsUnlock = p.U64("AvatarSkillsUnlock"),
                SpendedAvatarSkillPoint = p.U32("SpendedAvatarSkillPoint"), LifetimeCreedPoints = p.U32("LifetimeCreedPoints"),
                CurrentCreedPointsIncrease = p.U32("CurrentCreedPointsIncrease"), CurrentChallenge = p.U64("CurrentChallenge"),
                DiscoveredMissions = p.Packed("DiscoveredMissions"),
            };
            foreach (var e in p.Obj("InventorySpecification").Elements("InventoryContainerList")) c.Containers.Add(AcuInventoryContainer.From(e));
            return c;
        }

        private static List<AcuInventoryItemEntry> Entries(Props list)
        {
            var l = new List<AcuInventoryItemEntry>();
            foreach (var e in list.Elements("InventoryItemListEntries"))
            {
                var d = e.Obj("ItemDescriptor");
                l.Add(new AcuInventoryItemEntry { Count = e.U32("Count"), Type = d.U64("Type"), Item = d.U64("Item"), RechargeableCategory = d.U64("RechargeableCategory") });
            }
            return l;
        }
    }

    public sealed class AcuWorldNotoriety
    {
        public ulong World { get; init; }
        public uint Value { get; init; }
        public ulong Level { get; init; }
    }

    public sealed class AcuInventoryItemSetting
    {
        public bool IsLock { get; init; }
        public ulong Item { get; init; }
    }

    /// <summary>PlayerProgSaveData and its Snapshot twin.</summary>
    public sealed class AcuPlayerProgression
    {
        public List<ulong> UncollectedInventoryItemsOnMap { get; init; } = new();
        public List<AcuProgressionCharacter> Characters { get; init; } = new();
        public List<AcuProgressionCharacter> CharacterSnapshots { get; init; } = new();
        public List<ulong> QuestItemsCollectedAmounts { get; init; } = new();
        public ulong PlayedCollectiblesGlyph { get; init; }
        public uint StackOfLootMoney { get; init; }
        public bool DataInitialized { get; init; }
        public List<AcuWorldNotoriety> WorldNotorieties { get; init; } = new();
        public bool MainWorldsHorseMounted { get; init; }
        public bool OtherWorldsHorseMounted { get; init; }
        public bool ReplayHorseMounted { get; init; }
        public bool FirstGhostSeen { get; init; }
        public List<AcuInventoryItemSetting> SavedInventoryItems { get; init; } = new();

        public static AcuPlayerProgression From(Node n)
        {
            var p = new Props(n.Children);
            var r = new AcuPlayerProgression
            {
                UncollectedInventoryItemsOnMap = p.Packed("UncollectedInventoryItemsOnMap"),
                QuestItemsCollectedAmounts = p.Packed("QuestItemsCollectedAmounts"),
                PlayedCollectiblesGlyph = p.U64("PlayedCollectiblesGlyph"), StackOfLootMoney = p.U32("StackOfLootMoney"), DataInitialized = p.Bool("DataInitialized"),
                MainWorldsHorseMounted = p.Obj("MainWorldsHorse").Bool("Mounted"), OtherWorldsHorseMounted = p.Obj("OtherWorldsHorse").Bool("Mounted"),
                ReplayHorseMounted = p.Obj("ReplayHorse").Bool("Mounted"), FirstGhostSeen = p.Obj("GhostSave").Bool("FirstGhostSeen"),
            };
            foreach (var e in p.Elements("ProgressionCharacterData")) r.Characters.Add(AcuProgressionCharacter.From(e));
            foreach (var e in p.Elements("PlayerProgressionCharacterDataSnapshots")) r.CharacterSnapshots.Add(AcuProgressionCharacter.From(e));
            foreach (var e in p.Obj("WorldNotorieties").Elements("WorldNotorietyList"))
                r.WorldNotorieties.Add(new AcuWorldNotoriety { World = e.U64("NotorietyWorld"), Value = e.U32("NotorietyValue"), Level = e.U64("NotorietyLevel") });
            foreach (var e in p.Elements("SavedInventoryItems"))
                r.SavedInventoryItems.Add(new AcuInventoryItemSetting { IsLock = e.Bool("IsLock"), Item = e.U64("InventoryItems") });
            return r;
        }
    }
}
