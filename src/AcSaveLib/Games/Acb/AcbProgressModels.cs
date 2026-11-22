using System.Collections.Generic;
using System.Linq;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acb
{
    // PlayerProgSaveData (and its Snapshot copy), VillaSaveData, WorldUpgradeSaveData.

    public sealed class AcbInventoryItem
    {
        /// <summary>InventoryItemSettings handle (resolves to Name.InventoryItemSettings in the forge index).</summary>
        public ulong Item { get; init; }
        public ulong Entity { get; init; }
    }

    public sealed class AcbInventoryContainer
    {
        /// <summary>Class name of the element: InventoryRechargeableContainer or InventoryItemContainer.</summary>
        public string ClassName { get; init; } = "";
        /// <summary>Stored enum value of the category (low 32 bits = index; the high 32 bits are the CRC32 of the value name, e.g. InvItem_Money).</summary>
        public ulong Category { get; init; }
        public uint CategoryIndex => (uint)(Category & 0xFFFFFFFF);
        public uint CategoryNameHash => (uint)(Category >> 32);
        public uint CurrentContainerLevel { get; init; }
        public uint CurrentItemCount { get; init; }
        public uint MaximumItemCount { get; init; }
        public List<AcbInventoryItem> Items { get; init; } = new();
        public List<AcbInventoryItem> OverflowItems { get; init; } = new();
        public uint OverflowQuantity { get; init; }
    }

    public sealed class AcbArmorState
    {
        public ulong InventoryItemSettings { get; init; }
        public uint Resistance { get; init; }
    }

    /// <summary>PlayerAttributesBuilderData: upgrade levels of one entity builder (outfit).</summary>
    public sealed class AcbAttributesBuilder
    {
        public ulong PlayerEntityBuilder { get; init; }
        public byte AgeLevel { get; init; }
        public byte ChestArmorLevel { get; init; }
        public byte BootLevel { get; init; }
        public byte RightArmLevel { get; init; }
        public byte ShoulderLevel { get; init; }
        public byte LeftArmLevel { get; init; }
        public byte RightArmHbLevel { get; init; }
        public byte PoisonBladeLevel { get; init; }
        public byte MetalFistLevel { get; init; }
        public byte KnivesLevel { get; init; }
        public byte HealthObjectPouchLevel { get; init; }
        public byte SmokeBombPouchLevel { get; init; }
        public byte ParachutesPouchLevel { get; init; }
        public byte CapeLevel { get; init; }
        public byte ColorLevel { get; init; }
        public byte NarrativeLevel { get; init; }
        public byte AssassinMarkLevel { get; init; }
    }

    public sealed class AcbCharacter
    {
        /// <summary>Enum PlayerCharacter, low 32 bits (6 = PlayerCharacter_ACR_EzioUltimate in the 100% saves).</summary>
        public uint PlayerCharacterIndex { get; init; }
        public ulong PlayerCharacterRaw { get; init; }
        public List<AcbInventoryContainer> Containers { get; init; } = new();
        /// <summary>InventoryItemSettings handles of every item gained.</summary>
        public List<ulong> GainedInventoryItems { get; init; } = new();
        public List<AcbArmorState> ArmorStates { get; init; } = new();
        public uint Life { get; init; }
        public uint MaxLife { get; init; }
        public uint BrokenHealth { get; init; }
        public List<ulong> LockedInventoryItems { get; init; } = new();
        /// <summary>Enum EquipmentSlot values, low 32 bits, for the four quick slots.</summary>
        public List<uint> QuickSlotItems { get; init; } = new();
        public List<AcbAttributesBuilder> AttributeBuilders { get; init; } = new();

        /// <summary>LIKELY: CurrentItemCount of the first container (category InvItem_Money) is the florins in hand.</summary>
        public uint Money => Containers.Count > 0 ? Containers[0].CurrentItemCount : 0;

        public static AcbCharacter From(Props p)
        {
            var c = new AcbCharacter
            {
                PlayerCharacterRaw = p.U64("PlayerCharacter"), PlayerCharacterIndex = p.Enum("PlayerCharacter"),
                GainedInventoryItems = p.Packed("GainedInventoryItems"), Life = p.U32("Life"), MaxLife = p.U32("MaxLife"),
                BrokenHealth = p.U32("BrokenHealth"), LockedInventoryItems = p.Packed("LockedInventoryItems"),
                QuickSlotItems = p.Packed("QuickSlotItems").Select(v => (uint)(v & 0xFFFFFFFF)).ToList(),
            };
            var spec = p.Get("PlayerInventorySpecification");
            if (spec != null)
            {
                var list = spec.Find("InventoryContainerList");
                if (list != null)
                    foreach (var e in list.Elements)
                    {
                        var ep = new Props(e.Children);
                        c.Containers.Add(new AcbInventoryContainer
                        {
                            ClassName = e.Name, Category = ep.U64("Category"), CurrentContainerLevel = ep.U32("CurrentContainerLevel"),
                            CurrentItemCount = ep.U32("CurrentItemCount"), MaximumItemCount = ep.U32("MaximumItemCount"),
                            Items = ReadItems(ep, "ItemList"), OverflowItems = ReadItems(ep, "OverflowItemList"), OverflowQuantity = ep.U32("OverflowQuantity"),
                        });
                    }
            }
            foreach (var a in p.Elements("ArmorStates"))
                c.ArmorStates.Add(new AcbArmorState { InventoryItemSettings = a.U64("InventoryItemSettings"), Resistance = a.U32("Resistance") });
            foreach (var b in p.Elements("PlayerAttributesBuilderData"))
                c.AttributeBuilders.Add(new AcbAttributesBuilder
                {
                    PlayerEntityBuilder = b.U64("PlayerEntityBuilder"), AgeLevel = b.U8("AgeLevel"), ChestArmorLevel = b.U8("ChestArmorLevel"),
                    BootLevel = b.U8("BootLevel"), RightArmLevel = b.U8("RightArmLevel"), ShoulderLevel = b.U8("ShoulderLevel"),
                    LeftArmLevel = b.U8("LeftArmLevel"), RightArmHbLevel = b.U8("RightArmHBLevel"), PoisonBladeLevel = b.U8("PoisonBladeLevel"),
                    MetalFistLevel = b.U8("MetalFistLevel"), KnivesLevel = b.U8("KnivesLevel"), HealthObjectPouchLevel = b.U8("HealthObjectPouchLevel"),
                    SmokeBombPouchLevel = b.U8("SmokeBombPouchLevel"), ParachutesPouchLevel = b.U8("ParachutesPouchLevel"), CapeLevel = b.U8("CapeLevel"),
                    ColorLevel = b.U8("ColorLevel"), NarrativeLevel = b.U8("NarrativeLevel"), AssassinMarkLevel = b.U8("AssassinMarkLevel"),
                });
            return c;
        }

        private static List<AcbInventoryItem> ReadItems(Props p, string name)
        {
            var r = new List<AcbInventoryItem>();
            foreach (var i in p.Elements(name)) r.Add(new AcbInventoryItem { Item = i.U64("Item"), Entity = i.U64("Entity") });
            return r;
        }
    }

    public sealed class AcbNotoriety
    {
        /// <summary>World handle (ACR_Rome.World and so on).</summary>
        public ulong World { get; init; }
        /// <summary>Float, 0 to 1.</summary>
        public float Value { get; init; }
        public bool IsNotorious { get; init; }
    }

    public sealed class AcbSceneOutput
    {
        public ulong SceneEntity { get; init; }
        public ulong SceneComponent { get; init; }
        public ulong SceneOutput { get; init; }
        public bool DeactiveSceneOnSet { get; init; }
    }

    public sealed class AcbHorse
    {
        public bool Mounted { get; init; }
        public ulong EntityBuilder { get; init; }
        public uint SelectedColourRow { get; init; }
        public static AcbHorse From(Props p) => new() { Mounted = p.Bool("Mounted"), EntityBuilder = p.U64("EntityBuilder"), SelectedColourRow = p.U32("SelectedColourRow") };
    }

    public sealed class AcbPlayerProgression
    {
        public List<ulong> UncollectedInventoryItemsOnMap { get; init; } = new();
        public List<AcbCharacter> Characters { get; init; } = new();
        /// <summary>Snapshot characters (PlayerCharacterDataSnapshots, 12 entries in the samples).</summary>
        public List<AcbCharacter> CharacterSnapshots { get; init; } = new();
        /// <summary>Collectible counters (11 values). Meaning of each index is listed in docs/FIELDS_ACB.md.</summary>
        public List<uint> QuestItemsCollectedAmounts { get; init; } = new();
        public bool UplayUnlocked { get; init; }
        public bool DataInitialized { get; init; }
        public List<AcbNotoriety> Notoriety { get; init; } = new();
        public List<AcbSceneOutput> LeoShopPlayedScenes { get; init; } = new();
        public ulong OverridePlayerEntityBuilder { get; init; }
        public bool UnlockFacebook1 { get; init; }
        public bool UnlockFacebook2 { get; init; }
        public int CashFacebook { get; init; }
        public int CashFightClub { get; init; }
        public AcbHorse MainWorldHorse { get; init; } = new();
        public AcbHorse OtherWorldHorse { get; init; } = new();
        public AcbHorse ReplayOrVrHorse { get; init; } = new();

        /// <summary>The player's own character data (the first PlayerCharacterData entry).</summary>
        public AcbCharacter? Player => Characters.Count > 0 ? Characters[0] : null;

        public static AcbPlayerProgression From(Props p)
        {
            var r = new AcbPlayerProgression
            {
                UncollectedInventoryItemsOnMap = p.Packed("UncollectedInventoryItemsOnMap"),
                QuestItemsCollectedAmounts = p.PackedU32("QuestItemsCollectedAmounts"),
                UplayUnlocked = p.Bool("UplayUnlocked"), DataInitialized = p.Bool("DataInitialized"),
                OverridePlayerEntityBuilder = p.U64("OverridePlayerEntityBuilder"), UnlockFacebook1 = p.Bool("UnlockFacebook1"),
                UnlockFacebook2 = p.Bool("UnlockFacebook2"), CashFacebook = p.S32("CashFacebook"), CashFightClub = p.S32("CashFightClub"),
                MainWorldHorse = AcbHorse.From(p.Obj("MainWorldHorse")), OtherWorldHorse = AcbHorse.From(p.Obj("OtherWorldHorse")),
                ReplayOrVrHorse = AcbHorse.From(p.Obj("ReplayOrVRHorse")),
            };
            foreach (var c in p.Elements("PlayerCharacterData")) r.Characters.Add(AcbCharacter.From(c));
            foreach (var c in p.Elements("PlayerCharacterDataSnapshots")) r.CharacterSnapshots.Add(AcbCharacter.From(c));
            foreach (var n in p.Elements("WorldNotorietyList"))
                r.Notoriety.Add(new AcbNotoriety { World = n.U64("NotorietyWorld"), Value = n.Float("NotorietyValue"), IsNotorious = n.Bool("IsNotorious") });
            foreach (var s in p.Elements("LeoShopPlayedScenes"))
                r.LeoShopPlayedScenes.Add(new AcbSceneOutput
                {
                    SceneEntity = s.U64("SceneEntity"), SceneComponent = s.U64("SceneComponent"), SceneOutput = s.U64("SceneOutput"),
                    DeactiveSceneOnSet = s.Bool("DeactiveSceneOnSet"),
                });
            return r;
        }
    }

    /// <summary>SnapshotPlayerProgressionDelta.</summary>
    public sealed class AcbProgressionDelta
    {
        public int PlayerCharacterItemsDeltaCount { get; init; }
        public List<byte> CodexPagesDecodedStatus { get; init; } = new();
        public List<byte> CodexPagesCurrentRotation { get; init; } = new();
        public List<ulong> UncollectedInventoryItemsOnMapDelta { get; init; } = new();

        public static AcbProgressionDelta From(Props p) => new()
        {
            PlayerCharacterItemsDeltaCount = p.ElementCount("PlayerCharacterItemsDelta"),
            CodexPagesDecodedStatus = p.PackedU8("CodexPagesDecodedStatus"), CodexPagesCurrentRotation = p.PackedU8("CodexPagesCurrentRotation"),
            UncollectedInventoryItemsOnMapDelta = p.Packed("UncollectedInventoryItemsOnMapDelta"),
        };
    }

    /// <summary>VillaSaveData / SnapshotVillaSaveData.</summary>
    public sealed class AcbVilla
    {
        public uint StrongholdValue { get; init; }
        public uint PendingStrValueForAfs { get; init; }
        public List<byte> BuildingsLevels { get; init; } = new();
        public List<byte> CodexPagesDecodedStatus { get; init; } = new();
        public List<byte> CodexPagesCurrentRotation { get; init; } = new();

        public static AcbVilla From(Props p) => new()
        {
            StrongholdValue = p.U32("StrongholdValue"), PendingStrValueForAfs = p.U32("PendingStrValueForAfs"), BuildingsLevels = p.PackedU8("BuildingsLevels"),
            CodexPagesDecodedStatus = p.PackedU8("CodexPagesDecodedStatus"), CodexPagesCurrentRotation = p.PackedU8("CodexPagesCurrentRotation"),
        };
    }

    public sealed class AcbBuilding
    {
        /// <summary>WorldUpgradeSettings handle (does not resolve from the supplied forge index files).</summary>
        public ulong Building { get; init; }
        public bool IsNewlyGained { get; init; }
    }

    public sealed class AcbEconomyShop
    {
        /// <summary>WorldEconomyShopSettings handle (does not resolve from the supplied forge index files).</summary>
        public ulong Settings { get; init; }
        /// <summary>Unnamed key dfaa4e68 (U32). GUESS: earnings or investment total; 0 except in one sample save.</summary>
        public uint Dfaa4e68 { get; init; }
        /// <summary>Unnamed key 0900aa54 (U32). GUESS: earnings or investment total; 0 except in one sample save.</summary>
        public uint Field0900aa54 { get; init; }
    }

    /// <summary>WorldUpgradeSaveData / SnapshotWorldUpgradeSaveData. The Snapshot copy can differ from the main copy.</summary>
    public sealed class AcbWorldUpgrade
    {
        public List<ulong> GainedWorldUpgradeSettings { get; init; } = new();
        public List<AcbBuilding> Buildings { get; init; } = new();
        /// <summary>BuildingsRules: one byte per rule (3 in most saves, 2 in one early save). Meaning of the values is not known.</summary>
        public List<byte> BuildingRules { get; init; } = new();
        public List<AcbEconomyShop> Shops { get; init; } = new();
        /// <summary>Unnamed keys of WorldEconomySaveData after the shop array (U32 each): 9ed74287, d99bbfde, 55c432c6, 4f436d86.</summary>
        public uint Key9ed74287 { get; init; }
        public uint Keyd99bbfde { get; init; }
        public uint Key55c432c6 { get; init; }
        public uint Key4f436d86 { get; init; }
        /// <summary>LIKELY: florins waiting in the vault (66720 = 4 x 16680, the in-game income per 20 minutes).</summary>
        public uint CurrentGoldChestAmount { get; init; }
        /// <summary>At most 1200 in all samples. GUESS: seconds into the 20 minute income cycle.</summary>
        public uint CurrentIncomeDelay { get; init; }
        public uint PendingIncomeValueForAfs { get; init; }

        public static AcbWorldUpgrade From(Props p)
        {
            var buildings = p.Obj("BuildingsSaveData");
            var eco = p.Obj("WorldEconomySaveData");
            var w = new AcbWorldUpgrade
            {
                GainedWorldUpgradeSettings = p.Packed("GainedWorldUpgradeSettings"),
                BuildingRules = buildings.PackedU8("BuildingsRules"),
                Key9ed74287 = (uint)(eco.GetByKey(0x9ed74287)?.Value ?? 0), Keyd99bbfde = (uint)(eco.GetByKey(0xd99bbfde)?.Value ?? 0),
                Key55c432c6 = (uint)(eco.GetByKey(0x55c432c6)?.Value ?? 0), Key4f436d86 = (uint)(eco.GetByKey(0x4f436d86)?.Value ?? 0),
                CurrentGoldChestAmount = p.U32("CurrentGoldChestAmount"), CurrentIncomeDelay = p.U32("CurrentIncomeDelay"),
                PendingIncomeValueForAfs = p.U32("PendingIncomeValueForAfs"),
            };
            foreach (var b in buildings.Elements("Buildings")) w.Buildings.Add(new AcbBuilding { Building = b.U64("Building"), IsNewlyGained = b.Bool("IsNewlyGained") });
            var shops = eco.GetByKey(0xead92d83);
            if (shops != null)
                foreach (var e in shops.Elements)
                {
                    var sp = new Props(e.Children);
                    w.Shops.Add(new AcbEconomyShop
                    {
                        Settings = sp.U64("WorldEconomyShopSettings"), Dfaa4e68 = (uint)(sp.GetByKey(0xdfaa4e68)?.Value ?? 0),
                        Field0900aa54 = (uint)(sp.GetByKey(0x0900aa54)?.Value ?? 0),
                    });
                }
            return w;
        }
    }
}
