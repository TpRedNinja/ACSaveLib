using System;
using System.Collections.Generic;
using System.Linq;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acb
{
    // Typed models for Assassin's Creed Brotherhood saves. Field names follow the names shown by ACViewer (docs/FIELDS_ACB.md).
    // Handles (Mission, Group, Accomplishment, InventoryItemSettings ids, ...) are the 64-bit object ids as stored.
    // Enum values are the low 32 bits of the stored u64 (the high 32 bits hold the CRC32 of the value name).

    /// <summary>Header container (root SaveGame): the properties shown in the save slot list.</summary>
    public sealed class AcbHeader
    {
        public uint Version { get; init; }
        public uint MagicNumber { get; init; }
        public uint DlcPackagesBits { get; init; }
        public uint CodeChangelist { get; init; }
        public uint SaveGameId { get; init; }
        public bool IsOptions { get; init; }
        /// <summary>SaveGameName: "Desmond" in game saves, "Options" in options files.</summary>
        public string SaveGameName { get; init; } = "";
        /// <summary>World handle (ACR_Rome.World and so on).</summary>
        public ulong CurrentWorld { get; init; }
        public ulong CurrentMission { get; init; }
        /// <summary>Seconds (same unit as GameStatsSaveData.TimePlayed, confirmed in the in-game Stats menu).</summary>
        public uint TimePlayed { get; init; }
        public uint PlayerGlobalSynch { get; init; }
        /// <summary>Unix time in seconds as stored.</summary>
        public uint SaveGameDate { get; init; }

        public static AcbHeader From(List<Node> nodes)
        {
            var p = new Props(nodes);
            string name = "";
            var n = p.Get("SaveGameName");
            if (n != null && n.Bytes.Length >= 4)
            {
                int len = (int)BitConverter.ToUInt32(n.Bytes, 0);
                if (len >= 0 && len <= n.Bytes.Length - 4) name = System.Text.Encoding.ASCII.GetString(n.Bytes, 4, len);
            }
            return new AcbHeader
            {
                Version = p.U32("Version"), MagicNumber = p.U32("MagicNumber"), DlcPackagesBits = p.U32("DLCPackagesBits"),
                CodeChangelist = p.U32("CodeChangelist"), SaveGameId = p.U32("SaveGameID"), IsOptions = p.Bool("IsOptions"),
                SaveGameName = name, CurrentWorld = p.U64("CurrentWorld"), CurrentMission = p.U64("CurrentMission"),
                TimePlayed = p.U32("TimePlayed"), PlayerGlobalSynch = p.U32("PlayerGlobalSynch"), SaveGameDate = p.U32("SaveGameDate"),
            };
        }
    }

    // ---- Missions ----

    public sealed class AcbMissionStep
    {
        /// <summary>Step handle (does not resolve from the supplied forge index files).</summary>
        public ulong Id { get; init; }
        public bool IsCheckpoint { get; init; }
        public bool Succeeded { get; init; }
    }

    public sealed class AcbMissionCondition
    {
        public uint ConditionId { get; init; }
        public uint MetaValue { get; init; }
    }

    public sealed class AcbMissionConstraint
    {
        public uint ConstraintId { get; init; }
        public bool PlayerSelected { get; init; }
        public bool CurrentRunFailed { get; init; }
        public bool BestRunSucceeded { get; init; }
        public List<AcbMissionCondition> Conditions { get; init; } = new();
    }

    public sealed class AcbMission
    {
        /// <summary>Mission handle (resolves to Name.Mission in the forge index, e.g. E01M01).</summary>
        public ulong Id { get; init; }
        public int HighestSynchronization { get; init; }
        public bool BriefingDisplayedOnce { get; init; }
        public bool BriefingAccepted { get; init; }
        public List<AcbMissionStep> Steps { get; init; } = new();
        public List<AcbMissionConstraint> Constraints { get; init; } = new();
        /// <summary>LIKELY: HighestSynchronization is 100 for every finished mission and 0 for entries that only hold progress (6 sample saves: only 0 and 100 occur).</summary>
        public bool Completed => HighestSynchronization >= 100;
    }

    public sealed class AcbMissionGroup
    {
        /// <summary>MissionGroup handle (E01, SQ01, ...).</summary>
        public ulong Id { get; init; }
        public List<AcbMission> Missions { get; init; } = new();
    }

    /// <summary>NarrativeMissionHistory / SideMissionHistory.</summary>
    public sealed class AcbMissionHistory
    {
        /// <summary>Enum MissionHistoryTrackIdentifiers (0 = MissionHistoryTrack_Narrative).</summary>
        public uint Identifier { get; init; }
        public List<AcbMissionGroup> Groups { get; init; } = new();
        public IEnumerable<AcbMission> AllMissions { get { foreach (var g in Groups) foreach (var m in g.Missions) yield return m; } }

        public static AcbMissionHistory From(Props history)
        {
            var h = new AcbMissionHistory { Identifier = history.Enum("Identifier") };
            foreach (var g in history.Elements("GroupSaveData"))
            {
                var group = new AcbMissionGroup { Id = g.U64("Group") };
                foreach (var m in g.Elements("MissionSaveData"))
                {
                    var mission = new AcbMission
                    {
                        Id = m.U64("Mission"), HighestSynchronization = m.S32("HighestSynchronization"),
                        BriefingDisplayedOnce = m.Bool("BriefingDisplayedOnce"), BriefingAccepted = m.Bool("BriefingAccepted"),
                    };
                    foreach (var s in m.Elements("MissionStepSaveData"))
                        mission.Steps.Add(new AcbMissionStep { Id = s.U64("Step"), IsCheckpoint = s.Bool("IsCheckpoint"), Succeeded = s.Bool("Succeeded") });
                    foreach (var c in m.Elements("MissionConstraintSaveData"))
                    {
                        var con = new AcbMissionConstraint
                        {
                            ConstraintId = c.U32("ConstraintID"), PlayerSelected = c.Bool("PlayerSelected"),
                            CurrentRunFailed = c.Bool("CurrentRunFailed"), BestRunSucceeded = c.Bool("BestRunSucceeded"),
                        };
                        foreach (var cd in c.Elements("ConditionData"))
                            con.Conditions.Add(new AcbMissionCondition { ConditionId = cd.U32("ConditionID"), MetaValue = cd.U32("MetaValue") });
                        mission.Constraints.Add(con);
                    }
                    group.Missions.Add(mission);
                }
                h.Groups.Add(group);
            }
            return h;
        }
    }

    // ---- Accomplishments, stats ----

    public sealed class AcbAccomplishment
    {
        /// <summary>Accomplishment object id, decimal as stored (resolves to Name.Accomplishment in the forge index).</summary>
        public uint ObjectId { get; init; }
        public bool Inactive { get; init; }
        public bool Completed { get; init; }
        public uint CurrentCount { get; init; }
    }

    public sealed class AcbWeaponUsed
    {
        /// <summary>InventoryItemSettings id of the weapon.</summary>
        public uint WeaponItemId { get; init; }
        public uint UsedTime { get; init; }
    }

    /// <summary>GameStatsSaveData. Units confirmed against the in-game Stats menu: times in seconds, distances in metres.</summary>
    public sealed class AcbGameStats
    {
        public List<AcbWeaponUsed> WeaponsUsed { get; init; } = new();
        public uint TimePlayed { get; init; }
        public uint TotalTimeBlended { get; init; }
        public uint DeathOccurrences { get; init; }
        public uint Kills { get; init; }
        public uint KillsCrossbow { get; init; }
        public uint KillsGun { get; init; }
        public uint HiddenBladeAssassinations { get; init; }
        public uint AirAssassinations { get; init; }
        public uint DoubleAirAssassinations { get; init; }
        public uint FromHorseAssassinations { get; init; }
        public uint HorseRiderKills { get; init; }
        public uint PoisonedEnemies { get; init; }
        public uint LongestComboKillStreak { get; init; }
        public uint Disarms { get; init; }
        public uint TotalMoneyEarned { get; init; }
        public uint TotalMoneySpent { get; init; }
        public uint NotoriousOccurrences { get; init; }
        public uint BorgiaCourrierKills { get; init; }
        public uint CourtesansHired { get; init; }
        public uint MercenariesHired { get; init; }
        public uint ThievesHired { get; init; }
        public uint MemoryReplayed { get; init; }
        public uint OverallDistanceCovered { get; init; }
        public uint HeightClimbed { get; init; }
        public uint DistanceOnGround { get; init; }
        public uint DistanceSwam { get; init; }
        public uint DistanceHorse { get; init; }
        public uint DistanceGondolas { get; init; }
        public uint DistanceFlown { get; init; }
        public uint DistanceParachute { get; init; }
        public uint MasterAssassins { get; init; }
        public uint ContractsCompleted { get; init; }
        public uint DeadAssassins { get; init; }
        public uint RecruitedAssassins { get; init; }
        public uint KillsUsingBAM { get; init; }
        public uint BAMUses { get; init; }
        public uint TotalMoneyWonFightClub { get; init; }
        public uint TotalMoneyLostFightClub { get; init; }
        public uint FightClubLoses { get; init; }
        public uint FightClubWins { get; init; }

        public static AcbGameStats From(Props p)
        {
            var s = new AcbGameStats
            {
                TimePlayed = p.U32("TimePlayed"), TotalTimeBlended = p.U32("TotalTimeBlended"), DeathOccurrences = p.U32("DeathOccurrences"),
                Kills = p.U32("Kills"), KillsCrossbow = p.U32("KillsCrossbow"), KillsGun = p.U32("KillsGun"),
                HiddenBladeAssassinations = p.U32("HiddenBladeAssassinations"), AirAssassinations = p.U32("AirAssassinations"),
                DoubleAirAssassinations = p.U32("DoubleAirAssassinations"), FromHorseAssassinations = p.U32("FromHorseAssassinations"),
                HorseRiderKills = p.U32("HorseRiderKills"), PoisonedEnemies = p.U32("PoisonedEnemies"),
                LongestComboKillStreak = p.U32("LongestComboKillStreak"), Disarms = p.U32("Disarms"),
                TotalMoneyEarned = p.U32("TotalMoneyEarned"), TotalMoneySpent = p.U32("TotalMoneySpent"),
                NotoriousOccurrences = p.U32("NotoriousOccurrences"), BorgiaCourrierKills = p.U32("BorgiaCourrierKills"),
                CourtesansHired = p.U32("CourtesansHired"), MercenariesHired = p.U32("MercenariesHired"), ThievesHired = p.U32("ThievesHired"),
                MemoryReplayed = p.U32("MemoryReplayed"), OverallDistanceCovered = p.U32("OverallDistanceCovered"),
                HeightClimbed = p.U32("HeightClimbed"), DistanceOnGround = p.U32("DistanceOnGround"), DistanceSwam = p.U32("DistanceSwam"),
                DistanceHorse = p.U32("DistanceHorse"), DistanceGondolas = p.U32("DistanceGondolas"), DistanceFlown = p.U32("DistanceFlown"),
                DistanceParachute = p.U32("DistanceParachute"), MasterAssassins = p.U32("MasterAssassins"),
                ContractsCompleted = p.U32("ContractsCompleted"), DeadAssassins = p.U32("DeadAssassins"),
                RecruitedAssassins = p.U32("RecruitedAssassins"), KillsUsingBAM = p.U32("KillsUsingBAM"), BAMUses = p.U32("BAMUses"),
                TotalMoneyWonFightClub = p.U32("TotalMoneyWonFightClub"), TotalMoneyLostFightClub = p.U32("TotalMoneyLostFightClub"),
                FightClubLoses = p.U32("FightClubLoses"), FightClubWins = p.U32("FightClubWins"),
            };
            foreach (var w in p.Elements("WeaponUsedSavedInfo"))
                s.WeaponsUsed.Add(new AcbWeaponUsed { WeaponItemId = w.U32("WeaponInventoryItemObjectID"), UsedTime = w.U32("UsedTime") });
            return s;
        }
    }

    // ---- Smaller top-level objects ----

    public sealed class AcbAnimusEntry
    {
        public uint EntryFoundOrderNumber { get; init; }
        public uint GlyphFoundOrderNumber { get; init; }
        public uint AfsDisplayedCount { get; init; }
        public bool IsLgsSolved { get; init; }
    }

    public sealed class AcbPspState
    {
        public bool ConnectionDone { get; init; }
        public uint GoldCoins { get; init; }
        public uint SilverCoins { get; init; }
        public uint BronzeCoins { get; init; }
        public List<byte> WeaponUnlocked { get; init; } = new();
        public bool HiddenBladeArmorCodex { get; init; }
        public bool HiddenBladeSlimBladeCodex { get; init; }
        public bool HiddenBladeGunCodex { get; init; }
        public bool LifeCodex { get; init; }

        public static AcbPspState From(Props p) => new()
        {
            ConnectionDone = p.Bool("PSPConnectionDone"), GoldCoins = p.U32("PSPGoldCoins"), SilverCoins = p.U32("PSPSilverCoins"),
            BronzeCoins = p.U32("PSPBronzeCoins"), WeaponUnlocked = p.PackedU8("PSPWeaponUnlocked"),
            HiddenBladeArmorCodex = p.Bool("HiddenBladeArmorCodex"), HiddenBladeSlimBladeCodex = p.Bool("HiddenBladeSlimBladeCodex"),
            HiddenBladeGunCodex = p.Bool("HiddenBladeGunCodex"), LifeCodex = p.Bool("LifeCodex"),
        };
    }

    public sealed class AcbUiTutorial
    {
        public uint OasisCurrentMissionObjectiveLineId { get; init; }
        /// <summary>50 entries, 0xFFFFFFFF = no time recorded.</summary>
        public List<uint> VRMissionBestTimes { get; init; } = new();
        public List<uint> VRMissionBestCounts { get; init; } = new();
        public List<uint> VRMissionLoaded { get; init; } = new();
        public List<uint> FightClubUnlockedLevels { get; init; } = new();
        /// <summary>Tutorial lists (HUD, System, Controls, Fight, Tactics, World): number of entries each.</summary>
        public int[] TutorialListCounts { get; init; } = new int[6];

        public static AcbUiTutorial From(Props p) => new()
        {
            OasisCurrentMissionObjectiveLineId = p.U32("OasisCurrentMissionObjectiveLineID"),
            VRMissionBestTimes = p.PackedU32("VRMissionBestTimes"), VRMissionBestCounts = p.PackedU32("VRMissionBestCounts"),
            VRMissionLoaded = p.PackedU32("VRMissionLoaded"), FightClubUnlockedLevels = p.PackedU32("FightClubUnlockedLevels"),
            TutorialListCounts = new[]
            {
                p.ElementCount("HUDTutorials"), p.ElementCount("SystemTutorials"), p.ElementCount("ControlsTutorials"),
                p.ElementCount("FightTutorials"), p.ElementCount("TacticsTutorials"), p.ElementCount("WorldTutorials"),
            },
        };
    }

    public sealed class AcbEmails
    {
        public bool HasUnlockedAllPassword { get; init; }
        public bool HasNewEmail { get; init; }
        public List<bool> IsEmailDeleted { get; init; } = new();
        public List<bool> EmailGroupUnlocked { get; init; } = new();
        public List<bool> IsEmailNew { get; init; } = new();

        public static AcbEmails From(Props p) => new()
        {
            HasUnlockedAllPassword = p.Bool("HasUnlockedAllPassword"), HasNewEmail = p.Bool("HasNewEmail"),
            IsEmailDeleted = p.PackedBool("IsEmailDeleted"), EmailGroupUnlocked = p.PackedBool("EmailGroupUnlocked"), IsEmailNew = p.PackedBool("IsEmailNew"),
        };
    }

    public sealed class AcbFacebookData
    {
        public ulong FacebookUid { get; init; }
        public uint Collections { get; init; }
        public uint Landmarks { get; init; }
        public uint Contracts { get; init; }
        public uint Cash { get; init; }
        public int AssassinCount { get; init; }

        public static AcbFacebookData From(Props p) => new()
        {
            FacebookUid = p.U64("FacebookUID"), Collections = p.U32("Collections"), Landmarks = p.U32("Landmarks"),
            Contracts = p.U32("Contracts"), Cash = p.U32("Cash"), AssassinCount = p.ElementCount("Assassins"),
        };
    }

    /// <summary>PlayerOptionsSaveData of the options file. The keys with no known name are kept in <see cref="UnnamedValues"/>.</summary>
    public sealed class AcbOptions
    {
        public bool DisplaySubtitles { get; init; }
        /// <summary>Enum values (low 32 bits).</summary>
        public uint LastConsoleLanguage { get; init; }
        public uint SoundLanguage { get; init; }
        public uint MusicVolume { get; init; }
        public uint VoiceVolume { get; init; }
        public uint SoundFxVolume { get; init; }
        public bool Vibration { get; init; }
        public float CameraHorizontalAxisSensitivity { get; init; }
        public float CameraVerticalAxisSensitivity { get; init; }
        public bool InvertCameraHorizontalAxis { get; init; }
        public bool InvertCameraVerticalAxis { get; init; }
        public bool InvertFirstPersonCameraHorizontalAxis { get; init; }
        public bool InvertFirstPersonCameraVerticalAxis { get; init; }
        public uint CameraInserts { get; init; }
        public uint Brightness { get; init; }
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
        public bool Lgs01Unlocked { get; init; }
        public bool Lgs02Unlocked { get; init; }
        public bool Lgs03Unlocked { get; init; }
        public bool Lgs04Unlocked { get; init; }
        public bool Skin01Unlocked { get; init; }
        public bool Skin02Unlocked { get; init; }
        public bool Skin03Unlocked { get; init; }
        public bool Skin04Unlocked { get; init; }
        public bool FanPackUnlocked { get; init; }
        public bool SharpestBladesUnlocked { get; init; }
        public bool EziosDisguiseUnlocked { get; init; }
        public bool MansionUnlocked { get; init; }
        /// <summary>Values of keys that have no name yet (key to raw value).</summary>
        public Dictionary<uint, ulong> UnnamedValues { get; init; } = new();

        public static AcbOptions From(Node? obj)
        {
            var p = new Props(obj?.Children);
            var o = new AcbOptions
            {
                DisplaySubtitles = p.Bool("DisplaySubtitles"), LastConsoleLanguage = p.Enum("LastConsoleLanguage"), SoundLanguage = p.Enum("SoundLanguage"),
                MusicVolume = p.U32("MusicVolume"), VoiceVolume = p.U32("VoiceVolume"), SoundFxVolume = p.U32("SoundFXVolume"),
                Vibration = p.Bool("Vibration"), CameraHorizontalAxisSensitivity = p.Float("CameraHorizontalAxisSensitivity"),
                CameraVerticalAxisSensitivity = p.Float("CameraVerticalAxisSensitivity"),
                InvertCameraHorizontalAxis = p.Bool("InvertCameraHorizontalAxis"), InvertCameraVerticalAxis = p.Bool("InvertCameraVerticalAxis"),
                InvertFirstPersonCameraHorizontalAxis = p.Bool("InvertFirstPersonCameraHorizontalAxis"),
                InvertFirstPersonCameraVerticalAxis = p.Bool("InvertFirstPersonCameraVerticalAxis"),
                CameraInserts = p.U32("CameraInserts"), Brightness = p.U32("Brightness"), Blood = p.Bool("Blood"),
                FlyingMachineControls = p.Bool("FlyingMachineControls"), Health = p.Bool("Health"), Controls = p.Bool("Controls"),
                MissionState = p.Bool("MissionState"), WeaponSelect = p.Bool("WeaponSelect"), MiniMap = p.Bool("MiniMap"),
                Money = p.Bool("Money"), Factions = p.Bool("Factions"), SSI = p.Bool("SSI"),
                Lgs01Unlocked = p.Bool("LGS01Unlocked"), Lgs02Unlocked = p.Bool("LGS02Unlocked"), Lgs03Unlocked = p.Bool("LGS03Unlocked"),
                Lgs04Unlocked = p.Bool("LGS04Unlocked"), Skin01Unlocked = p.Bool("Skin01Unlocked"), Skin02Unlocked = p.Bool("Skin02Unlocked"),
                Skin03Unlocked = p.Bool("Skin03Unlocked"), Skin04Unlocked = p.Bool("Skin04Unlocked"), FanPackUnlocked = p.Bool("FanPackUnlocked"),
                SharpestBladesUnlocked = p.Bool("SharpestBladesUnlocked"), EziosDisguiseUnlocked = p.Bool("EziosDisguiseUnlocked"),
                MansionUnlocked = p.Bool("MansionUnlocked"),
            };
            if (obj != null)
                foreach (var c in obj.Children)
                    if (c.Kind == NodeKind.Value && !Names.PropertyNames.IsKnown(c.Key)) o.UnnamedValues[c.Key] = c.Value;
            return o;
        }
    }
}
