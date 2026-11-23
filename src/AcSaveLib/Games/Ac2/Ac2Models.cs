using System.Collections.Generic;
using System.Linq;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac2
{
    /// <summary>Header properties at the top of the root object (SaveGame). Names confirmed in ACViewer.</summary>
    public sealed class Ac2Header
    {
        public uint Version { get; init; }
        public uint MagicNumber { get; init; }
        public uint DlcPackagesBits { get; init; }
        public uint CodeChangelist { get; init; }
        public bool IsValid { get; init; }
        public uint SaveGameId { get; init; }
        public bool IsOptions { get; init; }
        public bool LoadOrSaveOptionsFailed { get; init; }
        public bool IsCorrupt { get; init; }
        /// <summary>"Desmond" in game saves, "Options" in options saves (stored as a 32-bit length followed by the characters).</summary>
        public string SaveGameName { get; init; } = "";
        /// <summary>Raw TimePlayed (unit not confirmed).</summary>
        public uint TimePlayed { get; init; }
        public uint PlayerGlobalSynch { get; init; }
        /// <summary>Unix time in seconds as stored.</summary>
        public uint SaveGameDate { get; init; }
        public ulong CurrentWorld { get; init; }
        public ulong CurrentMission { get; init; }

        public static Ac2Header From(List<Node> root)
        {
            var p = new Props(root);
            string name = "";
            var nameNode = p.Get("SaveGameName");
            if (nameNode != null && nameNode.Bytes.Length >= 4)
            {
                int len = (int)System.BitConverter.ToUInt32(nameNode.Bytes, 0);
                if (len >= 0 && len <= nameNode.Bytes.Length - 4) name = System.Text.Encoding.ASCII.GetString(nameNode.Bytes, 4, len);
            }
            return new Ac2Header
            {
                Version = p.U32("Version"), MagicNumber = p.U32("MagicNumber"), DlcPackagesBits = p.U32("DLCPackagesBits"), CodeChangelist = p.U32("CodeChangelist"),
                IsValid = p.Bool("IsValid"), SaveGameId = p.U32("SaveGameID"), IsOptions = p.Bool("IsOptions"),
                LoadOrSaveOptionsFailed = p.Bool("LoadOrSaveOptionsFailed"), IsCorrupt = p.Bool("IsCorrupt"), SaveGameName = name,
                TimePlayed = p.U32("TimePlayed"), PlayerGlobalSynch = p.U32("PlayerGlobalSynch"), SaveGameDate = p.U32("SaveGameDate"),
                CurrentWorld = p.U64("CurrentWorld"), CurrentMission = p.U64("CurrentMission"),
            };
        }
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
