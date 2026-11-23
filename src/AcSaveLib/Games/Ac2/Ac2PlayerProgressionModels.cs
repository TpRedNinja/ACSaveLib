using System.Collections.Generic;
using System.Linq;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac2
{
    /// <summary>QuestItemsCollectedAmounts: 9 counters. Indices 0..3, 5 and 6 match the in-game collectible counters (see docs/FIELDS_AC2.md).</summary>
    public sealed class Ac2CollectedAmounts
    {
        public List<uint> Values { get; init; } = new();
        private uint At(int i) => i < Values.Count ? Values[i] : 0;
        public uint CodexPages => At(0);
        public uint Feathers => At(1);
        public uint Statuettes => At(2);
        public uint AssassinSeals => At(3);
        public uint Chests => At(6);
    }

    /// <summary>PhysicalInventoryItem (elements of ItemList and OverflowItemList). Names confirmed in ACViewer.</summary>
    public sealed class Ac2PhysicalInventoryItem
    {
        /// <summary>InventoryItemSettings ID (hash of an AC2 inventory item name).</summary>
        public ulong Item { get; init; }
        public ulong Entity { get; init; }
    }

    /// <summary>InventoryItemContainer or InventoryRechargeableContainer (elements of InventoryContainerList). Names confirmed in ACViewer.</summary>
    public sealed class Ac2InventoryContainer
    {
        /// <summary>True for InventoryRechargeableContainer (counted items such as money), false for InventoryItemContainer (listed items).</summary>
        public bool IsRechargeable { get; init; }
        /// <summary>Category as stored: a 32-bit hash in the high half and a container index in the low half. LIKELY the high half of the first container (0x12F6B6F1) is InvItem_Money.</summary>
        public ulong Category { get; init; }
        public uint CategoryHash => (uint)(Category >> 32);
        public uint CategoryIndex => (uint)(Category & 0xFFFFFFFF);
        public uint CurrentContainerLevel { get; init; }
        /// <summary>For the first container this is the money held (LIKELY, 953500 in the 100% save).</summary>
        public uint CurrentItemCount { get; init; }
        public uint MaximumItemCount { get; init; }
        public List<Ac2PhysicalInventoryItem> ItemList { get; init; } = new();
        public List<Ac2PhysicalInventoryItem> OverflowItemList { get; init; } = new();
        public uint OverflowQuantity { get; init; }

        private static List<Ac2PhysicalInventoryItem> Items(Props p, string name)
        {
            var r = new List<Ac2PhysicalInventoryItem>();
            foreach (var e in p.Elements(name)) r.Add(new Ac2PhysicalInventoryItem { Item = e.U64("Item"), Entity = e.U64("Entity") });
            return r;
        }

        public static Ac2InventoryContainer From(ArrayElement e)
        {
            var p = new Props(e.Children);
            return new Ac2InventoryContainer
            {
                IsRechargeable = e.Name == "InventoryRechargeableContainer",
                Category = p.U64("Category"), CurrentContainerLevel = p.U32("CurrentContainerLevel"), CurrentItemCount = p.U32("CurrentItemCount"),
                MaximumItemCount = p.U32("MaximumItemCount"), ItemList = Items(p, "ItemList"), OverflowItemList = Items(p, "OverflowItemList"),
                OverflowQuantity = p.U32("OverflowQuantity"),
            };
        }
    }

    /// <summary>ArmorState (elements of ArmorStates). Names confirmed in ACViewer.</summary>
    public sealed class Ac2ArmorState
    {
        /// <summary>InventoryItemSettings ID of the armor piece.</summary>
        public ulong InventoryItemSettings { get; init; }
        public uint Resistance { get; init; }
    }

    /// <summary>PlayerAttributesBuilderData: the 16 upgrade levels of one outfit. Names confirmed in ACViewer.</summary>
    public sealed class Ac2AttributesBuilder
    {
        public ulong PlayerEntityBuilder { get; init; }
        public byte AgeLevel, ChestArmorLevel, BootLevel, RightArmLevel, ShoulderLevel, LeftArmLevel, RightArmHBLevel, PoisonBladeLevel,
                    MetalFistLevel, KnivesLevel, HealthObjectPouchLevel, SmokeBombPouchLevel, CapeLevel, ColorLevel, NarrativeLevel, AssassinMarkLevel;

        public static Ac2AttributesBuilder From(Props p) => new Ac2AttributesBuilder
        {
            PlayerEntityBuilder = p.U64("PlayerEntityBuilder"), AgeLevel = p.U8("AgeLevel"), ChestArmorLevel = p.U8("ChestArmorLevel"), BootLevel = p.U8("BootLevel"),
            RightArmLevel = p.U8("RightArmLevel"), ShoulderLevel = p.U8("ShoulderLevel"), LeftArmLevel = p.U8("LeftArmLevel"), RightArmHBLevel = p.U8("RightArmHBLevel"),
            PoisonBladeLevel = p.U8("PoisonBladeLevel"), MetalFistLevel = p.U8("MetalFistLevel"), KnivesLevel = p.U8("KnivesLevel"),
            HealthObjectPouchLevel = p.U8("HealthObjectPouchLevel"), SmokeBombPouchLevel = p.U8("SmokeBombPouchLevel"), CapeLevel = p.U8("CapeLevel"),
            ColorLevel = p.U8("ColorLevel"), NarrativeLevel = p.U8("NarrativeLevel"), AssassinMarkLevel = p.U8("AssassinMarkLevel"),
        };
    }

    /// <summary>One PlayerCharacterData (three per save, plus the same type in PlayerCharacterDataSnapshots).</summary>
    public sealed class Ac2PlayerCharacterData
    {
        public ulong PlayerCharacter { get; init; }
        public List<Ac2InventoryContainer> Containers { get; init; } = new();
        public List<Ac2ArmorState> ArmorStates { get; init; } = new();
        public uint Life { get; init; }
        public uint MaxLife { get; init; }
        public uint BrokenHealth { get; init; }
        public List<ulong> LockedInventoryItems { get; init; } = new();
        public List<Ac2AttributesBuilder> Attributes { get; init; } = new();

        /// <summary>LIKELY: CurrentItemCount of the first container (953500 in the 100% save).</summary>
        public uint Money => Containers.Count > 0 ? Containers[0].CurrentItemCount : 0;

        public static Ac2PlayerCharacterData From(Props p)
        {
            var d = new Ac2PlayerCharacterData
            {
                PlayerCharacter = p.U64("PlayerCharacter"), Life = p.U32("Life"), MaxLife = p.U32("MaxLife"), BrokenHealth = p.U32("BrokenHealth"),
                LockedInventoryItems = p.Packed("LockedInventoryItems"),
            };
            var spec = p.Get("PlayerInventorySpecification");
            var list = spec?.Find("InventoryContainerList");
            if (list != null) foreach (var e in list.Elements) d.Containers.Add(Ac2InventoryContainer.From(e));
            foreach (var e in p.Elements("ArmorStates")) d.ArmorStates.Add(new Ac2ArmorState { InventoryItemSettings = e.U64("InventoryItemSettings"), Resistance = e.U32("Resistance") });
            foreach (var e in p.Elements("PlayerAttributesBuilderData")) d.Attributes.Add(Ac2AttributesBuilder.From(e));
            return d;
        }
    }

    /// <summary>WorldNotorietyValue (elements of WorldNotorietyList, 20 in a full save). Names confirmed in ACViewer.</summary>
    public sealed class Ac2WorldNotoriety
    {
        public ulong NotorietyWorld { get; init; }
        public float NotorietyValue { get; init; }
        public bool IsNotorious { get; init; }
    }

    /// <summary>PlayerProgSaveData and SnapshotPlayerProgSaveData (same layout).</summary>
    public sealed class Ac2PlayerProgression
    {
        /// <summary>IDs of every inventory item gained (hashes of AC2 inventory item names).</summary>
        public List<uint> GainedInventoryItems { get; init; } = new();
        public List<Ac2PlayerCharacterData> Characters { get; init; } = new();
        public List<Ac2PlayerCharacterData> CharacterSnapshots { get; init; } = new();
        public Ac2CollectedAmounts CollectedAmounts { get; init; } = new();
        public bool UplayUnlocked { get; init; }
        public bool DataInitialized { get; init; }
        public List<Ac2WorldNotoriety> WorldNotorieties { get; init; } = new();
        public ulong OverridePlayerEntityBuilder { get; init; }

        /// <summary>LIKELY: CurrentItemCount of the first container of the first character.</summary>
        public uint Money => Characters.Count > 0 ? Characters[0].Money : 0;

        public static Ac2PlayerProgression From(Props p)
        {
            var d = new Ac2PlayerProgression
            {
                GainedInventoryItems = p.PackedU32("GainedInventoryItems"),
                CollectedAmounts = new Ac2CollectedAmounts { Values = p.PackedU32("QuestItemsCollectedAmounts") },
                UplayUnlocked = p.Bool("UplayUnlocked"), DataInitialized = p.Bool("DataInitialized"),
                OverridePlayerEntityBuilder = p.U64("OverridePlayerEntityBuilder"),
            };
            foreach (var e in p.Elements("PlayerCharacterData")) d.Characters.Add(Ac2PlayerCharacterData.From(e));
            foreach (var e in p.Elements("PlayerCharacterDataSnapshots")) d.CharacterSnapshots.Add(Ac2PlayerCharacterData.From(e));
            foreach (var e in p.Elements("WorldNotorietyList"))
                d.WorldNotorieties.Add(new Ac2WorldNotoriety { NotorietyWorld = e.U64("NotorietyWorld"), NotorietyValue = e.Float("NotorietyValue"), IsNotorious = e.Bool("IsNotorious") });
            return d;
        }
    }

    /// <summary>PlayerCharacterItemsDelta: the items of one character that differ from the snapshot. Empty (ItemsDelta has no entries) in every sample.</summary>
    public sealed class Ac2CharacterItemsDelta
    {
        public ulong PlayerCharacter { get; init; }
        public List<ulong> ItemsDelta { get; init; } = new();
    }

    /// <summary>SnapshotPlayerProgressionDelta. Names confirmed in ACViewer.</summary>
    public sealed class Ac2ProgressionDelta
    {
        public List<ulong> GainedInventoryItemsDelta { get; init; } = new();
        public List<Ac2CharacterItemsDelta> PlayerCharacterItemsDelta { get; init; } = new();
        public List<bool> CodexPagesDecodedStatus { get; init; } = new();
        /// <summary>s16 x 30; the values seen are -90, 90 and 180.</summary>
        public List<short> CodexPagesCurrentRotation { get; init; } = new();

        public static Ac2ProgressionDelta From(Props p)
        {
            var d = new Ac2ProgressionDelta
            {
                GainedInventoryItemsDelta = p.Packed("GainedInventoryItemsDelta"), CodexPagesDecodedStatus = p.PackedBool("CodexPagesDecodedStatus"),
                CodexPagesCurrentRotation = p.Packed("CodexPagesCurrentRotation").Select(v => unchecked((short)(ushort)v)).ToList(),
            };
            foreach (var e in p.Elements("PlayerCharacterItemsDelta"))
                d.PlayerCharacterItemsDelta.Add(new Ac2CharacterItemsDelta { PlayerCharacter = e.U64("PlayerCharacter"), ItemsDelta = e.Packed("ItemsDelta") });
            return d;
        }
    }
}
