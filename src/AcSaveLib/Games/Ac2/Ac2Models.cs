using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac2
{
    public sealed class Ac2MissionStep
    {
        public ulong Id { get; init; }
        public bool IsCheckpoint { get; init; }
        public bool Succeeded { get; init; }
    }

    public sealed class Ac2Mission
    {
        /// <summary>Mission hash ID (matches AC2_HashNames.txt, e.g. 0x385B98EB Boys Will Be Boys).</summary>
        public ulong Id { get; init; }
        public int HighestSynchronization { get; init; }
        public bool BriefingDisplayedOnce { get; init; }
        public bool BriefingAccepted { get; init; }
        public List<Ac2MissionStep> Steps { get; init; } = new();
        public int ConstraintCount { get; init; }
        /// <summary>LIKELY: HighestSynchronization is 100 for every finished mission and 0 for entries that only hold progress (steps).</summary>
        public bool Completed => HighestSynchronization >= 100;
    }

    public sealed class Ac2MissionGroup
    {
        public ulong Id { get; init; }
        public List<Ac2Mission> Missions { get; init; } = new();
    }

    /// <summary>NarrativeMissionHistory / SideMissionHistory.</summary>
    public sealed class Ac2MissionHistory
    {
        public List<Ac2MissionGroup> Groups { get; init; } = new();
        public IEnumerable<Ac2Mission> AllMissions { get { foreach (var g in Groups) foreach (var m in g.Missions) yield return m; } }

        public static Ac2MissionHistory From(Node? history)
        {
            var h = new Ac2MissionHistory();
            var groups = history?.Find("GroupSaveData");
            if (groups == null) return h;
            foreach (var ge in groups.Elements)
            {
                var g = new Ac2MissionGroup { Id = ge.Find("Group")?.Value ?? 0 };
                foreach (var me in ge.Find("MissionSaveData")?.Elements ?? new())
                {
                    var steps = new List<Ac2MissionStep>();
                    foreach (var se in me.Find("MissionStepSaveData")?.Elements ?? new())
                        steps.Add(new Ac2MissionStep
                        {
                            Id = se.Find("Step")?.Value ?? 0,
                            IsCheckpoint = se.Find("IsCheckpoint")?.AsBool ?? false,
                            Succeeded = se.Find("Succeeded")?.AsBool ?? false,
                        });
                    g.Missions.Add(new Ac2Mission
                    {
                        Id = me.Find("Mission")?.Value ?? 0,
                        HighestSynchronization = (int)(me.Find("HighestSynchronization")?.Value ?? 0),
                        BriefingDisplayedOnce = me.Find("BriefingDisplayedOnce")?.AsBool ?? false,
                        BriefingAccepted = me.Find("BriefingAccepted")?.AsBool ?? false,
                        Steps = steps,
                        ConstraintCount = me.Find("MissionConstraintSaveData")?.Elements.Count ?? 0,
                    });
                }
                h.Groups.Add(g);
            }
            return h;
        }
    }

    public sealed class Ac2AccomplishmentEntry
    {
        public ulong ObjectId { get; init; }
        public bool Inactive { get; init; }
        public bool Completed { get; init; }
        public uint CurrentCount { get; init; }
    }

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

    /// <summary>GameStatsSaveData. Names confirmed in ACViewer; units are not confirmed.</summary>
    public sealed class Ac2GameStats
    {
        public uint TimePlayed { get; init; }
        public uint TotalTimeBlended { get; init; }
        public uint DeathOccurrences { get; init; }
        public uint Kills { get; init; }
        public uint HiddenBladeAssassinations { get; init; }
        public uint AirAssassinations { get; init; }
        public uint DoubleAirAssassinations { get; init; }
        public uint Disarms { get; init; }
        public uint TotalMoneyEarned { get; init; }
        public uint TotalMoneySpent { get; init; }
        public uint NotoriousOccurrences { get; init; }
        public uint BorgiaCourrierKills { get; init; }
        public uint CourtesansHired { get; init; }
        public uint MercenariesHired { get; init; }
        public uint ThievesHired { get; init; }
        public uint OverallDistanceCovered { get; init; }
        public uint HeightClimbed { get; init; }
        public uint DistanceOnGround { get; init; }
        public uint DistanceSwam { get; init; }
        public uint DistanceHorse { get; init; }
        public uint DistanceGondolas { get; init; }
        public uint DistanceFlown { get; init; }

        public static Ac2GameStats From(Props p) => new Ac2GameStats
        {
            TimePlayed = p.U32("TimePlayed"), TotalTimeBlended = p.U32("TotalTimeBlended"), DeathOccurrences = p.U32("DeathOccurrences"),
            Kills = p.U32("Kills"), HiddenBladeAssassinations = p.U32("HiddenBladeAssassinations"), AirAssassinations = p.U32("AirAssassinations"),
            DoubleAirAssassinations = p.U32("DoubleAirAssassinations"), Disarms = p.U32("Disarms"), TotalMoneyEarned = p.U32("TotalMoneyEarned"),
            TotalMoneySpent = p.U32("TotalMoneySpent"), NotoriousOccurrences = p.U32("NotoriousOccurrences"), BorgiaCourrierKills = p.U32("BorgiaCourrierKills"),
            CourtesansHired = p.U32("CourtesansHired"), MercenariesHired = p.U32("MercenariesHired"), ThievesHired = p.U32("ThievesHired"),
            OverallDistanceCovered = p.U32("OverallDistanceCovered"), HeightClimbed = p.U32("HeightClimbed"), DistanceOnGround = p.U32("DistanceOnGround"),
            DistanceSwam = p.U32("DistanceSwam"), DistanceHorse = p.U32("DistanceHorse"), DistanceGondolas = p.U32("DistanceGondolas"), DistanceFlown = p.U32("DistanceFlown"),
        };
    }

    /// <summary>VillaSaveData (Monteriggioni). Names confirmed in ACViewer.</summary>
    public sealed class Ac2Villa
    {
        public uint StrongholdValue { get; init; }
        public uint CurrentGoldIncome { get; init; }
        public uint CurrentIncomeDelay { get; init; }
        public uint PendingIncomeValueForAfs { get; init; }
        public uint PendingStrValueForAfs { get; init; }
        public List<byte> BuildingsLevels { get; init; } = new();
        public List<bool> CodexPagesDecodedStatus { get; init; } = new();

        public static Ac2Villa From(Props p) => new Ac2Villa
        {
            StrongholdValue = p.U32("StrongholdValue"), CurrentGoldIncome = p.U32("CurrentGoldIncome"), CurrentIncomeDelay = p.U32("CurrentIncomeDelay"),
            PendingIncomeValueForAfs = p.U32("PendingIncomeValueForAfs"), PendingStrValueForAfs = p.U32("PendingStrValueForAfs"),
            BuildingsLevels = p.PackedU8("BuildingsLevels"), CodexPagesDecodedStatus = p.PackedBool("CodexPagesDecodedStatus"),
        };
    }

    /// <summary>PlayerOptionsSaveData (in AssassinGlobalProfileData, the options-only save). Names confirmed in ACViewer.</summary>
    public sealed class Ac2Options
    {
        public bool DisplaySubtitles { get; init; }
        public uint LastConsoleLanguage { get; init; }
        public uint SoundLanguage { get; init; }
        public float MusicVolume { get; init; }
        public float VoiceVolume { get; init; }
        public float SoundFXVolume { get; init; }
        public bool Vibration { get; init; }
        public float CameraHorizontalAxisSensitivity { get; init; }
        public float CameraVerticalAxisSensitivity { get; init; }
        public bool InvertCameraHorizontalAxis { get; init; }
        public bool InvertCameraVerticalAxis { get; init; }
        public bool InvertFirstPersonCameraHorizontalAxis { get; init; }
        public bool InvertFirstPersonCameraVerticalAxis { get; init; }
        public byte CameraInserts { get; init; }
        public byte Brightness { get; init; }
        public bool Blood { get; init; }
        public bool FlyingMachineControls { get; init; }
        public bool Health { get; init; }
        public bool Controls { get; init; }
        public bool MissionState { get; init; }
        public bool WeaponSelect { get; init; }
        public bool MiniMap { get; init; }
        public bool Money { get; init; }
        public bool Factions { get; init; }
        public bool SSI { get; init; }
        /// <summary>Bonus content unlock flags: LGS01..04, Skin01..04, FanPack, SharpestBlades, EziosDisguise, Mansion.</summary>
        public bool LGS01Unlocked { get; init; }
        public bool LGS02Unlocked { get; init; }
        public bool LGS03Unlocked { get; init; }
        public bool LGS04Unlocked { get; init; }
        public bool Skin01Unlocked { get; init; }
        public bool Skin02Unlocked { get; init; }
        public bool Skin03Unlocked { get; init; }
        public bool Skin04Unlocked { get; init; }
        public bool FanPackUnlocked { get; init; }
        public bool SharpestBladesUnlocked { get; init; }
        public bool EziosDisguiseUnlocked { get; init; }
        public bool MansionUnlocked { get; init; }

        public static Ac2Options From(Props p) => new Ac2Options
        {
            DisplaySubtitles = p.Bool("DisplaySubtitles"), LastConsoleLanguage = p.Enum("LastConsoleLanguage"), SoundLanguage = p.Enum("SoundLanguage"),
            MusicVolume = p.Float("MusicVolume"), VoiceVolume = p.Float("VoiceVolume"), SoundFXVolume = p.Float("SoundFXVolume"), Vibration = p.Bool("Vibration"),
            CameraHorizontalAxisSensitivity = p.Float("CameraHorizontalAxisSensitivity"), CameraVerticalAxisSensitivity = p.Float("CameraVerticalAxisSensitivity"),
            InvertCameraHorizontalAxis = p.Bool("InvertCameraHorizontalAxis"), InvertCameraVerticalAxis = p.Bool("InvertCameraVerticalAxis"),
            InvertFirstPersonCameraHorizontalAxis = p.Bool("InvertFirstPersonCameraHorizontalAxis"), InvertFirstPersonCameraVerticalAxis = p.Bool("InvertFirstPersonCameraVerticalAxis"),
            CameraInserts = p.U8("CameraInserts"), Brightness = p.U8("Brightness"), Blood = p.Bool("Blood"), FlyingMachineControls = p.Bool("FlyingMachineControls"),
            Health = p.Bool("Health"), Controls = p.Bool("Controls"), MissionState = p.Bool("MissionState"), WeaponSelect = p.Bool("WeaponSelect"),
            MiniMap = p.Bool("MiniMap"), Money = p.Bool("Money"), Factions = p.Bool("Factions"), SSI = p.Bool("SSI"),
            LGS01Unlocked = p.Bool("LGS01Unlocked"), LGS02Unlocked = p.Bool("LGS02Unlocked"), LGS03Unlocked = p.Bool("LGS03Unlocked"), LGS04Unlocked = p.Bool("LGS04Unlocked"),
            Skin01Unlocked = p.Bool("Skin01Unlocked"), Skin02Unlocked = p.Bool("Skin02Unlocked"), Skin03Unlocked = p.Bool("Skin03Unlocked"), Skin04Unlocked = p.Bool("Skin04Unlocked"),
            FanPackUnlocked = p.Bool("FanPackUnlocked"), SharpestBladesUnlocked = p.Bool("SharpestBladesUnlocked"), EziosDisguiseUnlocked = p.Bool("EziosDisguiseUnlocked"), MansionUnlocked = p.Bool("MansionUnlocked"),
        };
    }

    /// <summary>Profile-level unlocks stored next to the options (AssassinGlobalProfileData). Names confirmed in ACViewer.</summary>
    public sealed class Ac2Profile
    {
        public bool UPlayMapUnlocked { get; init; }
        public bool UPlaySkinUnlocked { get; init; }
        public bool UniqueKeyMap1Unlocked { get; init; }
        public bool UniqueKeyMap2Unlocked { get; init; }
        public bool UniqueKeyMap3Unlocked { get; init; }
        public bool UniqueKeyMap4Unlocked { get; init; }
        public List<bool> CompletedUplayActions { get; init; } = new();

        public static Ac2Profile From(Props p) => new Ac2Profile
        {
            UPlayMapUnlocked = p.Bool("UPlayMapUnlocked"), UPlaySkinUnlocked = p.Bool("UPlaySkinUnlocked"),
            UniqueKeyMap1Unlocked = p.Bool("UniqueKeyMap1Unlocked"), UniqueKeyMap2Unlocked = p.Bool("UniqueKeyMap2Unlocked"),
            UniqueKeyMap3Unlocked = p.Bool("UniqueKeyMap3Unlocked"), UniqueKeyMap4Unlocked = p.Bool("UniqueKeyMap4Unlocked"),
            CompletedUplayActions = p.PackedBool("CompletedUplayActions"),
        };
    }
}
