using System;
using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac3
{
    /// <summary>Header container (root SaveGame): the properties shown in the save slot list.</summary>
    public sealed class Ac3Header
    {
        public uint Version { get; init; }
        public uint MagicNumber { get; init; }
        public uint DlcPackagesBits { get; init; }
        public uint CodeChangelist { get; init; }
        public uint SaveGameId { get; init; }
        public bool IsOptions { get; init; }
        /// <summary>SaveGameName as stored (a number in Remastered saves, text in Liberation).</summary>
        public string SaveGameName { get; init; } = "";
        /// <summary>TimePlayed as stored (unit not confirmed).</summary>
        public uint TimePlayed { get; init; }
        public uint PlayerGlobalSynch { get; init; }
        /// <summary>Unix time in seconds as stored.</summary>
        public uint SaveGameDate { get; init; }

        public static Ac3Header From(List<Node> nodes)
        {
            var p = new Props(nodes);
            return new Ac3Header
            {
                Version = p.U32("Version"), MagicNumber = p.U32("MagicNumber"), DlcPackagesBits = p.U32("DLCPackagesBits"),
                CodeChangelist = p.U32("CodeChangelist"), SaveGameId = p.U32("SaveGameID"), IsOptions = p.Bool("IsOptions"),
                SaveGameName = p.Text("SaveGameName"),
                TimePlayed = p.U32("TimePlayed"), PlayerGlobalSynch = p.U32("PlayerGlobalSynch"), SaveGameDate = p.U32("SaveGameDate"),
            };
        }
    }

    /// <summary>PlayerOptionsSaveData of the options file. The keys with no known name are kept in <see cref="UnnamedValues"/>.</summary>
    public sealed class Ac3Options
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

        public static Ac3Options From(Node? obj)
        {
            var p = new Props(obj?.Children);
            var o = new Ac3Options
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
