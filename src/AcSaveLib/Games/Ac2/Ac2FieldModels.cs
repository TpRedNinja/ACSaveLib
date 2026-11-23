using System.Collections.Generic;
using System.Linq;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac2
{
    public sealed class Ac2AccomplishmentEntry
    {
        public ulong ObjectId { get; init; }
        public bool Inactive { get; init; }
        public bool Completed { get; init; }
        public uint CurrentCount { get; init; }
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
        /// <summary>CodexPagesCurrentRotation (s16 x 30). Meaning not known.</summary>
        public List<short> CodexPagesCurrentRotation { get; init; } = new();

        public static Ac2Villa From(Props p) => new Ac2Villa
        {
            StrongholdValue = p.U32("StrongholdValue"), CurrentGoldIncome = p.U32("CurrentGoldIncome"), CurrentIncomeDelay = p.U32("CurrentIncomeDelay"),
            PendingIncomeValueForAfs = p.U32("PendingIncomeValueForAfs"), PendingStrValueForAfs = p.U32("PendingStrValueForAfs"),
            BuildingsLevels = p.PackedU8("BuildingsLevels"), CodexPagesDecodedStatus = p.PackedBool("CodexPagesDecodedStatus"),
            CodexPagesCurrentRotation = p.Packed("CodexPagesCurrentRotation").Select(v => unchecked((short)(ushort)v)).ToList(),
        };
    }

    /// <summary>AnimusDatabaseEntrySaveGame (elements of ADBEntries, 124 in a full save). Names confirmed in ACViewer.</summary>
    public sealed class Ac2AnimusEntry
    {
        public byte EntryFoundOrderNumber { get; init; }
        public byte GlyphFoundOrderNumber { get; init; }
        public byte AfsDisplayedCount { get; init; }
        public bool IsLgsSolved { get; init; }
    }

    /// <summary>AnimusDatabaseGlyphSaveGame (elements of ADBGlyphs, 20 in a full save). Names confirmed in ACViewer.</summary>
    public sealed class Ac2AnimusGlyph
    {
        public bool IsMiniGameSolved { get; init; }
        public ulong GlyphEntity { get; init; }
    }

    /// <summary>AnimusDatabaseData. Names confirmed in ACViewer.</summary>
    public sealed class Ac2AnimusDatabase
    {
        public List<Ac2AnimusEntry> Entries { get; init; } = new();
        public List<bool> CodexFound { get; init; } = new();
        public List<sbyte> LetterFoundCountLeft { get; init; } = new();
        public List<Ac2AnimusGlyph> Glyphs { get; init; } = new();
        public bool EntryFoundSpeechPlayed { get; init; }
        public bool NextToGlyphSpeechPlayed { get; init; }
        public bool GlyphFoundSpeechPlayed { get; init; }
        public bool TheTruthSpeechPlayed { get; init; }
        public bool FirstGlyphSpeechPlayed { get; init; }
        public bool CodexesAreKnown { get; init; }
        public bool EntryTutorialShown { get; init; }
        public sbyte EyeIconTutorialShown { get; init; }
        public bool TombIconTutorialShown { get; init; }
        public bool LairIconTutorialShown { get; init; }
        public bool Lgs01PresentationShown { get; init; }
        public bool Lgs02PresentationShown { get; init; }
        public bool Lgs03PresentationShown { get; init; }
        public bool Lgs04PresentationShown { get; init; }
        public uint SaveGameDate { get; init; }

        public static Ac2AnimusDatabase From(Props p)
        {
            var d = new Ac2AnimusDatabase
            {
                CodexFound = p.PackedBool("CodexFound"),
                LetterFoundCountLeft = p.Packed("LetterFoundCountLeft").Select(v => unchecked((sbyte)(byte)v)).ToList(),
                EntryFoundSpeechPlayed = p.Bool("ADBEntryFoundSpeechPlayed"), NextToGlyphSpeechPlayed = p.Bool("ADBNextToGlyphSpeechPlayed"),
                GlyphFoundSpeechPlayed = p.Bool("ADBGlyphFoundSpeechPlayed"), TheTruthSpeechPlayed = p.Bool("ADBTheTruthSpeechPlayed"),
                FirstGlyphSpeechPlayed = p.Bool("ADBFirstGlyphSpeechPlayed"), CodexesAreKnown = p.Bool("ADBCodexesAreKnown"),
                EntryTutorialShown = p.Bool("EntryTutorialShown"), EyeIconTutorialShown = unchecked((sbyte)(byte)(p.Get("EyeIconTutorialShown")?.Value ?? 0)),
                TombIconTutorialShown = p.Bool("TombIconTutorialShown"), LairIconTutorialShown = p.Bool("LairIconTutorialShown"),
                Lgs01PresentationShown = p.Bool("LGS01PresentationShown"), Lgs02PresentationShown = p.Bool("LGS02PresentationShown"),
                Lgs03PresentationShown = p.Bool("LGS03PresentationShown"), Lgs04PresentationShown = p.Bool("LGS04PresentationShown"),
                SaveGameDate = p.U32("SaveGameDate"),
            };
            foreach (var e in p.Elements("ADBEntries"))
                d.Entries.Add(new Ac2AnimusEntry { EntryFoundOrderNumber = e.U8("EntryFoundOrderNumber"), GlyphFoundOrderNumber = e.U8("GlyphFoundOrderNumber"), AfsDisplayedCount = e.U8("AFSDisplayedCount"), IsLgsSolved = e.Bool("IsLGSSolved") });
            foreach (var e in p.Elements("ADBGlyphs"))
                d.Glyphs.Add(new Ac2AnimusGlyph { IsMiniGameSolved = e.Bool("IsMiniGameSolved"), GlyphEntity = e.U64("GlyphEntity") });
            return d;
        }
    }

    /// <summary>AFSTutorialInfo: Oasis localisation line IDs of one tutorial message already shown. Names confirmed in ACViewer.</summary>
    public readonly record struct Ac2TutorialInfo(uint OasisMessageLineId, uint OasisTitleLineId);

    /// <summary>UITutorialData. Names confirmed in ACViewer.</summary>
    public sealed class Ac2UITutorialData
    {
        public List<Ac2TutorialInfo> HudTutorials { get; init; } = new();
        public List<Ac2TutorialInfo> SystemTutorials { get; init; } = new();
        public List<Ac2TutorialInfo> ControlsTutorials { get; init; } = new();
        public List<Ac2TutorialInfo> FightTutorials { get; init; } = new();
        public List<Ac2TutorialInfo> TacticsTutorials { get; init; } = new();
        public List<Ac2TutorialInfo> WorldTutorials { get; init; } = new();
        public uint OasisCurrentMissionObjectiveLineId { get; init; }

        private static List<Ac2TutorialInfo> ReadList(Props p, string name)
        {
            var r = new List<Ac2TutorialInfo>();
            foreach (var e in p.Elements(name)) r.Add(new Ac2TutorialInfo(e.U32("OasisMessageLineID"), e.U32("OasisTitleLineID")));
            return r;
        }

        public static Ac2UITutorialData From(Props p) => new Ac2UITutorialData
        {
            HudTutorials = ReadList(p, "HUDTutorials"), SystemTutorials = ReadList(p, "SystemTutorials"), ControlsTutorials = ReadList(p, "ControlsTutorials"),
            FightTutorials = ReadList(p, "FightTutorials"), TacticsTutorials = ReadList(p, "TacticsTutorials"), WorldTutorials = ReadList(p, "WorldTutorials"),
            OasisCurrentMissionObjectiveLineId = p.U32("OasisCurrentMissionObjectiveLineID"),
        };
    }

    /// <summary>PSPState and SnapshotPSPState (class PSPTransferredStuff). Names confirmed in ACViewer.</summary>
    public sealed class Ac2PspState
    {
        public bool PspConnectionDone { get; init; }
        public uint PspGoldCoins { get; init; }
        public uint PspSilverCoins { get; init; }
        public uint PspBronzeCoins { get; init; }
        public List<byte> PspWeaponUnlocked { get; init; } = new();
        public bool HiddenBladeArmorCodex { get; init; }
        public bool HiddenBladeSlimBladeCodex { get; init; }
        public bool HiddenBladeGunCodex { get; init; }
        public uint LifeCodex { get; init; }

        public static Ac2PspState From(Props p) => new Ac2PspState
        {
            PspConnectionDone = p.Bool("PSPConnectionDone"), PspGoldCoins = p.U32("PSPGoldCoins"), PspSilverCoins = p.U32("PSPSilverCoins"), PspBronzeCoins = p.U32("PSPBronzeCoins"),
            PspWeaponUnlocked = p.PackedU8("PSPWeaponUnlocked"), HiddenBladeArmorCodex = p.Bool("HiddenBladeArmorCodex"),
            HiddenBladeSlimBladeCodex = p.Bool("HiddenBladeSlimBladeCodex"), HiddenBladeGunCodex = p.Bool("HiddenBladeGunCodex"), LifeCodex = p.U32("LifeCodex"),
        };
    }
}
