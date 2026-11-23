using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac2
{
    /// <summary>
    /// Assassin's Creed II (.save). Layout is documented in docs/FORMAT_AC2.md.
    /// Plain file (no encryption, no compression): u32 headerLength | header | CRC32-keyed property stream.
    /// </summary>
    public sealed class Ac2SaveFile
    {
        /// <summary>Offset of the root object's first item, relative to the start of the property stream.</summary>
        public const int RootItemsOffset = 36;

        /// <summary>First u32 of the file; the property stream starts at HeaderLength + 4.</summary>
        public uint HeaderLength { get; private set; }
        /// <summary>Offset of the property stream in the file.</summary>
        public int StreamStart => (int)HeaderLength + 4;
        /// <summary>Offset of the property stream actually used (equals StreamStart unless the header length is wrong).</summary>
        public int StreamOffset { get; private set; }
        /// <summary>UTF-16 title in the header (e.g. "ACII 0").</summary>
        public string Title { get; private set; } = "";
        /// <summary>32-character ASCII id in the header of some saves (empty otherwise; meaning not known).</summary>
        public string HeaderId { get; private set; } = "";
        public uint StreamVersion { get; private set; }
        public uint StreamMagic { get; private set; }
        public byte[] Data { get; private set; } = Array.Empty<byte>();
        /// <summary>Top-level items of the root object (SaveGame): header properties plus SaveGameObjects / SaveGameDataObjects.</summary>
        public List<Node> Root { get; private set; } = new();

        /// <summary>Elements of SaveGameDataObjects: AssassinSaveGameData (the game) and/or AssassinGlobalProfileData (options). Saves of 2 KB hold only the profile.</summary>
        public List<ArrayElement> DataObjects { get; private set; } = new();
        /// <summary>True if the save holds a game (AssassinSaveGameData), false for a profile-only save.</summary>
        public bool HasGameData { get; private set; }

        /// <summary>Header properties (version, name, date, current world, flags).</summary>
        public Ac2Header Header { get; private set; } = new();
        /// <summary>Present when the save holds AssassinGlobalProfileData (options/bonus saves such as 2.save).</summary>
        public Ac2Options? Options { get; private set; }
        public Ac2Profile? Profile { get; private set; }
        /// <summary>All typed models of the game data (AssassinSaveGameData). Empty models for a profile-only save.</summary>
        public Ac2GameData Game { get; private set; } = new();

        public bool IsValid => Header.IsValid;
        /// <summary>Header flag IsCorrupt (name confirmed in ACViewer; 0 in all samples). Can be checked before overwriting a slot.</summary>
        public bool IsCorrupt => Header.IsCorrupt;
        public bool LoadOrSaveOptionsFailed => Header.LoadOrSaveOptionsFailed;
        public bool IsOptions => Header.IsOptions;
        /// <summary>SaveGameName: "Desmond" in game saves, "Options" in options saves.</summary>
        public string SaveGameName => Header.SaveGameName;
        public uint Version => Header.Version;
        public uint DlcPackagesBits => Header.DlcPackagesBits;
        public uint CodeChangelist => Header.CodeChangelist;
        /// <summary>Seconds played, from the save header (owner).</summary>
        public uint TimePlayed => Header.TimePlayed;
        /// <summary>PlayerGlobalSynch from the header (0x64 = 100 in the 100% save sample).</summary>
        public uint PlayerGlobalSynch => Header.PlayerGlobalSynch;
        /// <summary>SaveGameDate from the header: Unix time (seconds) as stored.</summary>
        public uint SaveGameDate => Header.SaveGameDate;
        public ulong CurrentWorld => Header.CurrentWorld;
        public ulong CurrentMission => Header.CurrentMission;

        public ulong PlayerPortal => Game.PlayerPortal;
        public Ac2MissionHistory Narrative => Game.Narrative;
        public Ac2MissionHistory Side => Game.Side;
        public Ac2GameStats GameStats => Game.GameStats;
        public Ac2Villa Villa => Game.Villa;
        public List<Ac2AccomplishmentEntry> Accomplishments => Game.Accomplishments;
        /// <summary>IDs of every inventory item gained (chests, feathers, codex pages, maps, ... : hashes of AC2_InventoryItems).</summary>
        public List<uint> GainedInventoryItems => Game.Progression.GainedInventoryItems;
        public Ac2CollectedAmounts CollectedAmounts => Game.Progression.CollectedAmounts;
        /// <summary>LIKELY: CurrentItemCount of the first inventory container of the first character (category InvItem_Money).</summary>
        public uint Money => Game.Progression.Money;

        public static Ac2SaveFile Load(string path) => Parse(File.ReadAllBytes(path));

        public static Ac2SaveFile Parse(byte[] file)
        {
            if (file.Length < 64) throw new InvalidDataException("File too small to be an AC2 save.");
            var r = new Ac2SaveFile { Data = file };
            r.HeaderLength = BitConverter.ToUInt32(file, 0);
            int s = r.StreamStart;
            // Some saves (one SKIDROW-patched sample) have a header length that is off by a few bytes: search for the magic instead.
            if (s + 8 > file.Length || BitConverter.ToUInt32(file, s + 4) != 0x00FEDBAC)
            {
                int m = IndexOfMagic(file);
                if (m < 4) throw new InvalidDataException("Property stream magic 0x00FEDBAC not found.");
                s = m - 4;
            }
            r.StreamOffset = s;
            if (s + RootItemsOffset > file.Length) throw new InvalidDataException("Stream starts too close to the end of the file.");
            r.StreamVersion = BitConverter.ToUInt32(file, s);
            r.StreamMagic = BitConverter.ToUInt32(file, s + 4);
            if (r.StreamMagic != 0x00FEDBAC) throw new InvalidDataException("Property stream magic 0x00FEDBAC not found.");
            r.Title = ReadTitle(file, r.HeaderLength, out var headerId);
            r.HeaderId = headerId;
            r.Root = PropertyReader.Parse(file, s + RootItemsOffset, file.Length, true);
            r.Fill();
            return r;
        }

        private void Fill()
        {
            Header = Ac2Header.From(Root);
            var objs = Root.FirstOrDefault(n => n.Name == "SaveGameDataObjects");
            if (objs != null) DataObjects = objs.Elements;
            var profile = DataObjects.FirstOrDefault(e => e.Name == "AssassinGlobalProfileData");
            if (profile != null)
            {
                var pp = new Props(profile.Children);
                Options = Ac2Options.From(pp.Obj("PlayerOptionsSaveData"));
                Profile = Ac2Profile.From(pp);
            }
            var game = DataObjects.FirstOrDefault(e => e.Name == "AssassinSaveGameData");
            if (game == null) return;
            HasGameData = true;
            Game = Ac2GameData.From(game);
        }

        private static int IndexOfMagic(byte[] f)
        {
            int limit = Math.Min(f.Length - 4, 0x400);
            for (int i = 0; i < limit; i++)
                if (f[i] == 0xAC && f[i + 1] == 0xDB && f[i + 2] == 0xFE && f[i + 3] == 0x00) return i;
            return -1;
        }

        private static string ReadTitle(byte[] f, uint headerLength, out string headerId)
        {
            // Header: u32 headerLength | 4 bytes | then either a 32-char ASCII id followed by the UTF-16 title (title at 0x28),
            // or zero padding / the title directly (title at 0x08).
            headerId = "";
            int end = (int)Math.Min(headerLength + 4, (uint)f.Length);
            int i = 8;
            if (end > 0x28 && f[9] != 0)
            {
                headerId = System.Text.Encoding.ASCII.GetString(f, 8, 32);
                i = 0x28;
            }
            else
                while (i + 1 < end && f[i] == 0 && f[i + 1] == 0) i += 2;
            var sb = new System.Text.StringBuilder();
            for (; i + 1 < end; i += 2)
            {
                char c = (char)(f[i] | f[i + 1] << 8);
                if (c == 0) break;
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
