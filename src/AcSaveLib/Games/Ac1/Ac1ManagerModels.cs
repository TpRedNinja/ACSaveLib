using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AcSaveLib.Games.Ac1
{
    /// <summary>Class IDs of the save sections (manager objects) of an AC1 save.</summary>
    public static class Ac1SectionIds
    {
        public const uint MapManager = 2092;
        public const uint DnaMemoryManager = 2093;
        public const uint SaveGameDataManager = 3003;
        public const uint HorseData = 3004;
        public const uint Persistence = 3008;
        public const uint TutorialManager = 3009;
        public const uint AccomplishmentManager = 3010;
        /// <summary>Class ID 674798380, 695892940, 731185190 and 806401044 are the four map objects (class name Menu_Ingame_001).</summary>
        public const string MapObjectClassName = "Menu_Ingame_001";
    }

    /// <summary>World IDs (AC1_notes.md 6.2, from AC Viewer's WorldTransitionManager and observed live). Used by the SaveGameDataManager world fields.</summary>
    public static class Ac1Worlds
    {
        private static readonly Dictionary<uint, string> Names = new()
        {
            { 0x1A06968B, "LoadingRoom" }, { 0x208EEB47, "WhiteRoom" }, { 0x05DCF562, "PresentRoom" }, { 0x1E3889FE, "MapMenu" },
            { 0x377AC858, "ParadiseFlashback" }, { 0x1BCB96C4, "Kingdom" }, { 0x8E2947C8, "MemoryCorridor" }, { 0x18E63FF1, "Masyaf" },
            { 0x2051F9F1, "SolomonTemple" }, { 0x1A0D49D6, "Damascus" }, { 0x1A394504, "Acre" }, { 0x1AB6F345, "Jerusalem" },
        };
        public static string? Name(uint id) => Names.TryGetValue(id, out var n) ? n : null;
    }

    /// <summary>Shared field readers for the manager models. Paths are the dotted path text (names from the property table, hex for unnamed hashes).</summary>
    internal static class Ac1Read
    {
        public static bool Bool(Ac1Object o, string path) => o.Find(path)?.AsBool() ?? false;
        public static bool? BoolOrNull(Ac1Object o, string path) => o.Find(path)?.AsBool();
        public static uint UInt(Ac1Object o, string path) => o.Find(path)?.AsUInt32() ?? 0;
        public static uint? UIntOrNull(Ac1Object o, string path) => o.Find(path)?.AsUInt32();
        public static float[] Floats(Ac1Object o, string path)
        {
            var f = o.Find(path);
            if (f == null) return Array.Empty<float>();
            var r = new float[f.Raw.Length / 4];
            for (int i = 0; i < r.Length; i++)
                r[i] = BitConverter.Int32BitsToSingle((f.Raw[i * 4] << 24) | (f.Raw[i * 4 + 1] << 16) | (f.Raw[i * 4 + 2] << 8) | f.Raw[i * 4 + 3]);
            return r;
        }
    }

    /// <summary>A MapMarker struct as saved in MapManager (only SetByPlayerMarker has more than IsKnown).</summary>
    public sealed class Ac1MapMarker
    {
        public bool IsKnown { get; init; }
        /// <summary>Hash 0xDE936275, saved only on SetByPlayerMarker. The name IsMissionMarker is a CRC32 dictionary match (likely, not seen in a viewer under that name).</summary>
        public bool? IsMissionMarker { get; init; }
        /// <summary>MapMarker::IconType enum value (enum type 0x7E394986), only on SetByPlayerMarker. The meaning of the values is not decoded.</summary>
        public uint? MarkerType { get; init; }
        public uint? TextId { get; init; }
        /// <summary>Two floats (MarkerPosition), only on SetByPlayerMarker; the coordinate meaning is not checked.</summary>
        public float[] MarkerPosition { get; init; } = Array.Empty<float>();
    }

    /// <summary>MapManager (section 2092): which map markers the player knows.</summary>
    public sealed class Ac1MapData
    {
        /// <summary>City names in the order AC Viewer lists the five CitiesInKingdom entries (AC1_notes.md 8: Acre, Damascus, Masyaf, Jerusalem, Arsuf). Index i of CityKnown is CitiesInKingdom[i].</summary>
        public static readonly IReadOnlyList<string> CityNames = new[] { "Acre", "Damascus", "Masyaf", "Jerusalem", "Arsuf" };
        public IReadOnlyList<bool> CityKnown { get; init; } = new List<bool>();
        public Ac1MapMarker DefaultCompassMarker { get; init; } = new();
        public Ac1MapMarker AltairMarker { get; init; } = new();
        public Ac1MapMarker SetByPlayerMarker { get; init; } = new();

        public static Ac1MapData? From(Ac1SaveFile s)
        {
            var o = s.Find(Ac1SectionIds.MapManager);
            if (o == null) return null;
            var cities = new List<bool>();
            for (int i = 0; ; i++)
            {
                var f = o.Find($"CitiesInKingdom[{i}].CityMarker.IsKnown");
                if (f == null) break;
                cities.Add(f.AsBool());
            }
            return new Ac1MapData
            {
                CityKnown = cities,
                DefaultCompassMarker = new Ac1MapMarker { IsKnown = Ac1Read.Bool(o, "DefaultCompassMarker.IsKnown") },
                AltairMarker = new Ac1MapMarker { IsKnown = Ac1Read.Bool(o, "AltairMarker.IsKnown") },
                SetByPlayerMarker = new Ac1MapMarker
                {
                    IsKnown = Ac1Read.Bool(o, "SetByPlayerMarker.IsKnown"),
                    IsMissionMarker = Ac1Read.BoolOrNull(o, "SetByPlayerMarker.de936275"),
                    MarkerType = Ac1Read.UIntOrNull(o, "SetByPlayerMarker.MarkerType"),
                    TextId = Ac1Read.UIntOrNull(o, "SetByPlayerMarker.TextId"),
                    MarkerPosition = Ac1Read.Floats(o, "SetByPlayerMarker.MarkerPosition"),
                },
            };
        }
    }

    /// <summary>DNAMemoryManager (section 2093): the sync bar (the game's health manager). AC1_notes.md 5.2.</summary>
    public sealed class Ac1DnaData
    {
        /// <summary>Current sync blocks (NumSynchBlocks).</summary>
        public uint NumSynchBlocks { get; init; }
        /// <summary>Total sync blocks (TotalNumSynchBlocks).</summary>
        public uint TotalNumSynchBlocks { get; init; }
        public byte UnlockedBlocksCount { get; init; }
        /// <summary>When set the game treats the bar as full (state selector 0xB78910 in the notes). [likely]</summary>
        public bool NothingIsTrueEverythingIsPermitted { get; init; }
        public byte OptionalObjectivesCount { get; init; }

        public static Ac1DnaData? From(Ac1SaveFile s)
        {
            var o = s.Find(Ac1SectionIds.DnaMemoryManager);
            if (o == null) return null;
            return new Ac1DnaData
            {
                NumSynchBlocks = Ac1Read.UInt(o, "NumSynchBlocks"),
                TotalNumSynchBlocks = Ac1Read.UInt(o, "TotalNumSynchBlocks"),
                UnlockedBlocksCount = (byte)Ac1Read.UInt(o, "UnlockedBlocksCount"),
                NothingIsTrueEverythingIsPermitted = Ac1Read.Bool(o, "NothingIsTrueEverythingIsPermitted"),
                OptionalObjectivesCount = (byte)Ac1Read.UInt(o, "OptionalObjectivesCount"),
            };
        }
    }

    /// <summary>SaveGameDataManager (section 3003): what the game restores on load. All 23 saved fields; values are raw (meanings of the rank, weapon and dagger values are not decoded).</summary>
    public sealed class Ac1SaveGameData
    {
        public uint NoSaveSemaphore { get; init; }
        public uint CheckPointOnlySemaphore { get; init; }
        /// <summary>World IDs (see <see cref="Ac1Worlds"/>). The notes record 0x1BCB96C4 (Kingdom) for Current and Main on a mid-game save.</summary>
        public uint CurrentSaveWorldId { get; init; }
        public uint MainSaveWorldId { get; init; }
        public uint AlwaysSaveWorldId { get; init; }
        public string? CurrentSaveWorldName => Ac1Worlds.Name(CurrentSaveWorldId);
        public string? MainSaveWorldName => Ac1Worlds.Name(MainSaveWorldId);
        public string? AlwaysSaveWorldName => Ac1Worlds.Name(AlwaysSaveWorldId);
        public uint CurrentMemoryBlock { get; init; }
        public bool MissionReplay { get; init; }
        public bool ToleranceSaved { get; init; }
        /// <summary>ToleranceManager::GlobalToleranceMode enum value (enum type 0xF09BB115).</summary>
        public uint GlobalTolerance { get; init; }
        /// <summary>ToleranceManager::HighToleranceMode enum value (enum type 0xF9688796).</summary>
        public uint HighTolerance { get; init; }
        public bool AltairRankSaved { get; init; }
        public uint AltairCurrentRank { get; init; }
        public uint AltairNextRank { get; init; }
        public bool AssassinSaved { get; init; }
        public bool AssassinOnMount { get; init; }
        /// <summary>4x4 float matrix as stored (16 values). The last four values hold w = 1.0 in the saves seen, so they look like the translation row (position). [likely]</summary>
        public float[] AssassinMatrix { get; init; } = Array.Empty<float>();
        public uint DaggerBeltCount { get; init; }
        public uint DaggerShoulderCount { get; init; }
        public uint DaggerLegCount { get; init; }
        public uint SelectedWeapon { get; init; }
        public uint MainDaggerBeltCount { get; init; }
        public uint MainDaggerShoulderCount { get; init; }
        public uint MainDaggerLegCount { get; init; }

        public static Ac1SaveGameData? From(Ac1SaveFile s)
        {
            var o = s.Find(Ac1SectionIds.SaveGameDataManager);
            if (o == null) return null;
            return new Ac1SaveGameData
            {
                NoSaveSemaphore = Ac1Read.UInt(o, "NoSaveSemaphore"),
                CheckPointOnlySemaphore = Ac1Read.UInt(o, "CheckPointOnlySemaphore"),
                CurrentSaveWorldId = Ac1Read.UInt(o, "CurrentSaveWorldID"),
                MainSaveWorldId = Ac1Read.UInt(o, "MainSaveWorldID"),
                AlwaysSaveWorldId = Ac1Read.UInt(o, "AlwaysSaveWorldID"),
                CurrentMemoryBlock = Ac1Read.UInt(o, "CurrentMemoryBlock"),
                MissionReplay = Ac1Read.Bool(o, "MissionReplay"),
                ToleranceSaved = Ac1Read.Bool(o, "ToleranceSaved"),
                GlobalTolerance = Ac1Read.UInt(o, "GlobalTolerance"),
                HighTolerance = Ac1Read.UInt(o, "HighTolerance"),
                AltairRankSaved = Ac1Read.Bool(o, "AltairRankSaved"),
                AltairCurrentRank = Ac1Read.UInt(o, "AltairCurrentRank"),
                AltairNextRank = Ac1Read.UInt(o, "AltairNextRank"),
                AssassinSaved = Ac1Read.Bool(o, "AssassinSaved"),
                AssassinOnMount = Ac1Read.Bool(o, "AssassinOnMount"),
                AssassinMatrix = Ac1Read.Floats(o, "AssassinMatrix"),
                DaggerBeltCount = Ac1Read.UInt(o, "DaggerBeltCount"),
                DaggerShoulderCount = Ac1Read.UInt(o, "DaggerShoulderCount"),
                DaggerLegCount = Ac1Read.UInt(o, "DaggerLegCount"),
                SelectedWeapon = Ac1Read.UInt(o, "SelectedWeapon"),
                MainDaggerBeltCount = Ac1Read.UInt(o, "MainDaggerBeltCount"),
                MainDaggerShoulderCount = Ac1Read.UInt(o, "MainDaggerShoulderCount"),
                MainDaggerLegCount = Ac1Read.UInt(o, "MainDaggerLegCount"),
            };
        }
    }

    /// <summary>HorseData (section 3004, the HorseMonitor's saved data). AC1_notes.md 16.5.</summary>
    public sealed class Ac1HorseData
    {
        public bool MountIsChosen { get; init; }
        public bool MountIsInWorld { get; init; }
        public uint SpawnMode { get; init; }
        public uint MountIndex { get; init; }
        /// <summary>Four floats.</summary>
        public float[] MobileSpawnerPos { get; init; } = Array.Empty<float>();
        /// <summary>Four floats.</summary>
        public float[] MobileSpawnerDir { get; init; } = Array.Empty<float>();
        public bool AssassinOnMount { get; init; }

        public static Ac1HorseData? From(Ac1SaveFile s)
        {
            var o = s.Find(Ac1SectionIds.HorseData);
            if (o == null) return null;
            const string P = "HorseMonitorData.";
            return new Ac1HorseData
            {
                MountIsChosen = Ac1Read.Bool(o, P + "MountIsChosen"),
                MountIsInWorld = Ac1Read.Bool(o, P + "MountIsInWorld"),
                SpawnMode = Ac1Read.UInt(o, P + "SpawnMode"),
                MountIndex = Ac1Read.UInt(o, P + "MountIndex"),
                MobileSpawnerPos = Ac1Read.Floats(o, P + "MobileSpawnerPos"),
                MobileSpawnerDir = Ac1Read.Floats(o, P + "MobileSpawnerDir"),
                AssassinOnMount = Ac1Read.Bool(o, P + "AssassinOnMount"),
            };
        }
    }

    /// <summary>TutorialManager (section 3009): the three free-mission counters it saves.</summary>
    public sealed class Ac1TutorialData
    {
        public uint FreeMissionInformerCount { get; init; }
        public uint FreeMissionScholarsCount { get; init; }
        public uint FreeMissionVigilantesCount { get; init; }

        public static Ac1TutorialData? From(Ac1SaveFile s)
        {
            var o = s.Find(Ac1SectionIds.TutorialManager);
            if (o == null) return null;
            return new Ac1TutorialData
            {
                FreeMissionInformerCount = Ac1Read.UInt(o, "FreeMissionInformerCount"),
                FreeMissionScholarsCount = Ac1Read.UInt(o, "FreeMissionScholarsCount"),
                FreeMissionVigilantesCount = Ac1Read.UInt(o, "FreeMissionVigilantesCount"),
            };
        }
    }

    /// <summary>
    /// One of the four map objects (class name Menu_Ingame_001): per map component, the known flags of its markers and automatic markers and the fog cells.
    /// Which world or city each of the four belongs to is not decoded.
    /// </summary>
    public sealed class Ac1MapObject
    {
        public uint ClassId { get; init; }
        public List<Ac1MapComponent> Components { get; init; } = new();
    }

    public sealed class Ac1MapComponent
    {
        public int Index { get; init; }
        /// <summary>MapWorldInfo.Markers[i].IsKnown.</summary>
        public List<bool> Markers { get; init; } = new();
        /// <summary>MapWorldInfo.AutomaticMarkers[i].IsKnown.</summary>
        public List<bool> AutomaticMarkers { get; init; } = new();
        /// <summary>MapWorldInfo.FogCells byte array (2048 elements in the saves seen; the cell layout is not decoded).</summary>
        public byte[] FogCells { get; init; } = Array.Empty<byte>();
        public bool IsAllUnfogged { get; init; }
    }

    public static class Ac1ManagerExtensions
    {
        public static Ac1MapData? MapData(this Ac1SaveFile s) => Ac1MapData.From(s);
        public static Ac1DnaData? DnaData(this Ac1SaveFile s) => Ac1DnaData.From(s);
        public static Ac1SaveGameData? SaveGameData(this Ac1SaveFile s) => Ac1SaveGameData.From(s);
        public static Ac1HorseData? HorseData(this Ac1SaveFile s) => Ac1HorseData.From(s);
        public static Ac1TutorialData? TutorialData(this Ac1SaveFile s) => Ac1TutorialData.From(s);

        /// <summary>The map objects (every object with a Components[].MapWorldInfo field), in file order.</summary>
        public static List<Ac1MapObject> MapObjects(this Ac1SaveFile s)
        {
            var r = new List<Ac1MapObject>();
            uint hComp = AcSaveLib.Names.PropertyNames.Crc32("Components");
            foreach (var o in s.Objects)
            {
                if (!o.Fields.Any(f => f.Path.Count > 0 && f.Path[0].NameHash == hComp)) continue;
                var comps = new SortedDictionary<int, (List<bool> m, List<bool> a, byte[] fog, bool all)>();
                foreach (var f in o.Fields)
                {
                    var p = f.Path;
                    if (p.Count < 3 || p[0].NameHash != hComp) continue;
                    int ci = p[0].Index;
                    if (!comps.TryGetValue(ci, out var e)) e = (new List<bool>(), new List<bool>(), Array.Empty<byte>(), false);
                    string second = p[2].Name;
                    if (second == "Markers" && p.Count == 4) Set(e.m, p[2].Index, f.AsBool());
                    else if (second == "AutomaticMarkers" && p.Count == 4) Set(e.a, p[2].Index, f.AsBool());
                    else if (second == "FogCells") e.fog = f.Elements().Select(v => (byte)v).ToArray();
                    else if (second == "IsAllUnfogged") e.all = f.AsBool();
                    comps[ci] = e;
                }
                r.Add(new Ac1MapObject
                {
                    ClassId = o.ClassId,
                    Components = comps.Select(k => new Ac1MapComponent { Index = k.Key, Markers = k.Value.m, AutomaticMarkers = k.Value.a, FogCells = k.Value.fog, IsAllUnfogged = k.Value.all }).ToList(),
                });
            }
            return r;
        }

        private static void Set(List<bool> l, int index, bool v)
        {
            if (index == PathStep.NoIndex) return;
            while (l.Count <= index) l.Add(false);
            l[index] = v;
        }
    }
}
