using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac4
{
    public sealed class PhysicalInventoryItem
    {
        /// <summary>InventoryItemSettings ID.</summary>
        public ulong Item { get; init; }
        public ulong Entity { get; init; }
    }

    public sealed class InventoryContainer
    {
        /// <summary>InventoryItemCategory enum value (1 Money, 5 SmokeBomb, 9 QuestItem, ...).</summary>
        public uint Category { get; init; }
        public uint CurrentContainerLevel { get; init; }
        /// <summary>For money this is the amount (verified: 199854 in wr.save).</summary>
        public uint CurrentItemCount { get; init; }
        public List<PhysicalInventoryItem> ItemList { get; init; } = new();
        public List<PhysicalInventoryItem> OverflowItemList { get; init; } = new();
        public uint OverflowQuantity { get; init; }
    }

    public sealed class AttributesBuilder
    {
        public ulong EntityBuilder { get; init; }
        public byte AgeLevel, ChestArmorLevel, BootLevel, RightArmLevel, ShoulderLevel, LeftArmLevel, RightArmHBLevel, PoisonBladeLevel,
                    MetalFistLevel, KnivesLevel, HealthObjectPouchLevel, SmokeBombPouchLevel, ParachutesPouchLevel, CapeLevel, ColorLevel,
                    NarrativeLevel, AssassinMarkLevel;
    }

    public readonly record struct QuickSlotItem(uint ItemEnumValue, uint ItemEnumType);

    /// <summary>One entry of ProgressionCharacterData (Edward, other characters and the ship each have one).</summary>
    public sealed class ProgressionCharacterData
    {
        /// <summary>ProgressionCharacter ID (Forge index type ProgressionCharacter).</summary>
        public ulong ProgressionCharacter { get; init; }
        public List<InventoryContainer> Containers { get; init; } = new();
        public int BackupInventoryEntryCount { get; init; }
        public List<ulong> GainedInventoryItems { get; init; } = new();
        public List<ulong> LockedInventoryItems { get; init; } = new();
        public List<AttributesBuilder> Attributes { get; init; } = new();
        public int ArmorStateCount { get; init; }
        public int BrokenHealth { get; init; }
        public int Life { get; init; }
        public int MaxLife { get; init; }
        public float ShipLife { get; init; }
        public ulong ShipHealthOverrideHandle { get; init; }
        public bool UseDefaultSelectedWeaponAndEquipment { get; init; }
        /// <summary>AssassinSelectedWeapon enum value (0 Unarmed, 9 DualWield, ...).</summary>
        public uint SelectedWeapon { get; init; }
        /// <summary>AssassinSelectedEquipment enum value (0 Nothing, 1 Pistols, ...).</summary>
        public uint SelectedEquipment { get; init; }
        public List<QuickSlotItem> QuickSlotItems { get; init; } = new();
        public List<QuickSlotItem> QuickSlotItemsNaval { get; init; } = new();

        public InventoryContainer? Container(uint category) => Containers.Find(c => c.Category == category);
        public uint Money => Container(1)?.CurrentItemCount ?? 0;

        private static PhysicalInventoryItem Item(Props e) => new() { Item = e.U64("Item"), Entity = e.U64("Entity") };

        public static ProgressionCharacterData From(Props p)
        {
            var d = new ProgressionCharacterData
            {
                ProgressionCharacter = p.Obj("ProgressionCharacterSelector").U64("ProgressionCharacter"),
                BackupInventoryEntryCount = p.Obj("BackupInventoryItems").ElementCount("InventoryItemListEntries"),
                GainedInventoryItems = p.Packed("GainedInventoryItems"),
                LockedInventoryItems = p.Packed("LockedInventoryItems"),
                ArmorStateCount = p.ElementCount("ArmorStates"),
                BrokenHealth = p.S32("BrokenHealth"), Life = p.S32("Life"), MaxLife = p.S32("MaxLife"),
                ShipLife = p.Float("ShipLife"), ShipHealthOverrideHandle = p.U64("ShipHealthOverrideHandle"),
                UseDefaultSelectedWeaponAndEquipment = p.Bool("UseDefaultSelectedWeaponAndEquipment"),
                SelectedWeapon = p.Enum("SelectedWeapon"), SelectedEquipment = p.Enum("SelectedEquipment"),
            };
            foreach (var c in p.Obj("InventorySpecification").Elements("InventoryContainerList"))
            {
                var ic = new InventoryContainer
                {
                    Category = c.Enum("Category"), CurrentContainerLevel = c.U32("CurrentContainerLevel"), CurrentItemCount = c.U32("CurrentItemCount"),
                    OverflowQuantity = c.U32("OverflowQuantity"),
                };
                foreach (var i in c.Elements("ItemList")) ic.ItemList.Add(Item(i));
                foreach (var i in c.Elements("OverflowItemList")) ic.OverflowItemList.Add(Item(i));
                d.Containers.Add(ic);
            }
            foreach (var a in p.Elements("AttributesBuilderData"))
                d.Attributes.Add(new AttributesBuilder
                {
                    EntityBuilder = a.U64("EntityBuilder"), AgeLevel = a.U8("AgeLevel"), ChestArmorLevel = a.U8("ChestArmorLevel"), BootLevel = a.U8("BootLevel"),
                    RightArmLevel = a.U8("RightArmLevel"), ShoulderLevel = a.U8("ShoulderLevel"), LeftArmLevel = a.U8("LeftArmLevel"), RightArmHBLevel = a.U8("RightArmHBLevel"),
                    PoisonBladeLevel = a.U8("PoisonBladeLevel"), MetalFistLevel = a.U8("MetalFistLevel"), KnivesLevel = a.U8("KnivesLevel"),
                    HealthObjectPouchLevel = a.U8("HealthObjectPouchLevel"), SmokeBombPouchLevel = a.U8("SmokeBombPouchLevel"), ParachutesPouchLevel = a.U8("ParachutesPouchLevel"),
                    CapeLevel = a.U8("CapeLevel"), ColorLevel = a.U8("ColorLevel"), NarrativeLevel = a.U8("NarrativeLevel"), AssassinMarkLevel = a.U8("AssassinMarkLevel"),
                });
            foreach (var q in p.Elements("QuickSlotItems")) d.QuickSlotItems.Add(new QuickSlotItem(q.U32("ItemEnumValue"), q.Enum("ItemEnumType")));
            foreach (var q in p.Elements("QuickSlotItems_Naval")) d.QuickSlotItemsNaval.Add(new QuickSlotItem(q.U32("ItemEnumValue"), q.Enum("ItemEnumType")));
            return d;
        }
    }

    public readonly record struct WorldNotoriety(ulong World, int Value, uint Level);
    public readonly record struct NavalConvoy(ulong Convoy, bool Discovered, float CooldownTime);
    /// <summary>OasisWeaponName is an S32 line ID in the game's text table (not a Forge ID).</summary>
    public readonly record struct KillItem(int OasisWeaponName, uint KillCount);
    public readonly record struct PresentDayEntityState(ulong Entity, uint NormalState, uint OverriddenState);
    public readonly record struct RangedWeaponAmmo(uint WeaponType, uint LoadedAmmoCount);

    /// <summary>PlayerProgSaveData and SnapshotPlayerProgSaveData share this class.</summary>
    public sealed class PlayerProgression
    {
        public List<ulong> UncollectedInventoryItemsOnMap { get; init; } = new();
        public List<ProgressionCharacterData> Characters { get; init; } = new();
        public List<ProgressionCharacterData> CharacterSnapshots { get; init; } = new();
        public List<uint> QuestItemsCollectedAmounts { get; init; } = new();
        public bool DataInitialized { get; init; }
        public List<WorldNotoriety> WorldNotorieties { get; init; } = new();
        public uint WantedLevelMeter { get; init; }
        public uint WantedLevel { get; init; }
        public uint LastWantedLevel { get; init; }
        public List<NavalConvoy> NavalConvoys { get; init; } = new();
        public List<KillItem> WeaponKills { get; init; } = new();
        public List<KillItem> ToolKills { get; init; } = new();
        public ulong OverridePlayerEntityBuilder { get; init; }
        public bool UplayMoneyGiven { get; init; }
        public bool IsFirstGameCompletedBocce { get; init; }
        public bool IsFirstGameCompletedMorris { get; init; }
        public bool IsFirstGameCompletedFanorona { get; init; }
        public bool IsFirstGameCompletedCheckers { get; init; }
        public bool IsFirstGameMatrixFrogger { get; init; }
        public bool IsFirstGamePrimes { get; init; }
        public bool IsFirstGameSphereHacking { get; init; }
        public bool IsHoodDown { get; init; }
        public List<PresentDayEntityState> PresentDayEntityStates { get; init; } = new();
        public List<RangedWeaponAmmo> RangedWeaponLoadedAmmo { get; init; } = new();
        public int WeaponAmmoCountDataCount { get; init; }
        public uint ExtraData { get; init; }

        /// <summary>Edward Kenway's entry: the one that holds the most inventory containers.</summary>
        public ProgressionCharacterData? Player
        {
            get
            {
                ProgressionCharacterData? best = null;
                foreach (var c in Characters) if (best == null || c.Containers.Count > best.Containers.Count || (c.Containers.Count == best.Containers.Count && c.Money > best.Money)) best = c;
                return best;
            }
        }

        public static PlayerProgression From(Props p)
        {
            var notor = p.Obj("WorldNotorieties");
            var d = new PlayerProgression
            {
                UncollectedInventoryItemsOnMap = p.Packed("UncollectedInventoryItemsOnMap"),
                QuestItemsCollectedAmounts = p.PackedU32("QuestItemsCollectedAmounts"),
                DataInitialized = p.Bool("DataInitialized"),
                WantedLevelMeter = notor.U32("WantedLevelMeter"), WantedLevel = notor.Enum("WantedLevel"), LastWantedLevel = notor.Enum("LastWantedLevel"),
                OverridePlayerEntityBuilder = p.U64("OverridePlayerEntityBuilder"),
                UplayMoneyGiven = p.Bool("UplayMoneyGiven"),
                IsFirstGameCompletedBocce = p.Bool("IsFirstGameCompletedBocce"), IsFirstGameCompletedMorris = p.Bool("IsFirstGameCompletedMorris"),
                IsFirstGameCompletedFanorona = p.Bool("IsFirstGameCompletedFanorona"), IsFirstGameCompletedCheckers = p.Bool("IsFirstGameCompletedCheckers"),
                IsFirstGameMatrixFrogger = p.Bool("IsFirstGameMatrixFrogger"), IsFirstGamePrimes = p.Bool("IsFirstGamePrimes"),
                IsFirstGameSphereHacking = p.Bool("IsFirstGameSphereHacking"), IsHoodDown = p.Bool("IsHoodDown"),
                WeaponAmmoCountDataCount = p.ElementCount("WeaponAmmoCountData"), ExtraData = p.U32("ExtraData"),
            };
            foreach (var c in p.Elements("ProgressionCharacterData")) d.Characters.Add(ProgressionCharacterData.From(c));
            foreach (var c in p.Elements("PlayerProgressionCharacterDataSnapshots")) d.CharacterSnapshots.Add(ProgressionCharacterData.From(c));
            foreach (var n in notor.Elements("WorldNotorietyList")) d.WorldNotorieties.Add(new WorldNotoriety(n.U64("NotorietyWorld"), n.S32("NotorietyValue"), n.Enum("NotorietyLevel")));
            foreach (var n in p.Obj("NavalConvoySaveData").Elements("ItemArray")) d.NavalConvoys.Add(new NavalConvoy(n.U64("Convoy"), n.Bool("Discovered"), n.Float("CooldownTime")));
            var kills = p.Obj("KillInfoSaveData");
            foreach (var k in kills.Elements("WeaponKills")) d.WeaponKills.Add(new KillItem(k.S32("OasisWeaponName"), k.U32("KillCount")));
            foreach (var k in kills.Elements("ToolKills")) d.ToolKills.Add(new KillItem(k.S32("OasisWeaponName"), k.U32("KillCount")));
            foreach (var e in p.Obj("PresentDayManagerSaveData").Elements("EntityStateArray"))
            {
                var st = e.Obj("EntityState");
                d.PresentDayEntityStates.Add(new PresentDayEntityState(e.U64("Entity"), st.Enum("NormalState"), st.Enum("OverriddenState")));
            }
            foreach (var r in p.Elements("RangedWeaponLoadedAmmoCountData")) d.RangedWeaponLoadedAmmo.Add(new RangedWeaponAmmo(r.Enum("WeaponType"), r.U32("LoadedAmmoCount")));
            return d;
        }
    }
}
