using System;
using System.Collections.Generic;
using System.Linq;

namespace AcSaveLib.Games.Ac1
{
    /// <summary>MissionItemState enum (CRC32 0x44B57418). Values 0..3 confirmed in AC1_notes.md 4.7.</summary>
    public enum Ac1MissionState : uint { Locked = 0, Available = 1, Active = 2, Done = 3 }

    /// <summary>One mission item (every object with a hash class ID). The state is the field whose enum type is 0x44B57418.</summary>
    public sealed class Ac1MissionItem
    {
        public const uint StateEnumType = 0x44B57418;
        public uint ClassId { get; init; }
        public string Name => Ac1Names.Class(ClassId);
        public Ac1MissionState State { get; init; }
        public bool Done => State == Ac1MissionState.Done;
        /// <summary>IsCompleted field (hash 523ad0c1) every mission item has. Meaning vs State not yet checked.</summary>
        public bool IsCompleted { get; init; }
        /// <summary>Second bool every mission item has (hash 6dcf11f9); the owner's table lists it as "Unknown (mission)".</summary>
        public bool Unknown6DCF11F9 { get; init; }
    }

    /// <summary>Static info about one of the 44 achievements (config 1 slot order). Source: AC1_notes.md section 9.1.</summary>
    public sealed record Ac1AchievementInfo(int Slot, int XboxId, string Title, int Gamerscore, string Requirement);

    /// <summary>Saved state of one accomplishment slot of the Accomplishment Manager (object 3010).</summary>
    public sealed class Ac1Accomplishment
    {
        public int Config { get; init; }
        public int Slot { get; init; }
        /// <summary>Accomplishment::AccomplishmentState (enum 0xEACDC2B9). 1 = in progress, 2 = completed (config 1 verified on 3 saves; 2 matches NotifyAccomplishment in the notes).</summary>
        public uint State { get; init; }
        public bool Completed => State == 2;
        public bool IsResolved { get; init; }
        /// <summary>Monitor.Count for counter monitors, otherwise null.</summary>
        public uint? Count { get; init; }
        /// <summary>Monitor.UsedEvents: collected indices in collection order (flags/Templars), otherwise empty.</summary>
        public List<byte> UsedEvents { get; init; } = new();
        public Ac1AchievementInfo? Info => Config == 1 ? Ac1Achievements.BySlot(Slot) : null;
    }

    /// <summary>Collected count for one collectible slot. FlagHashes are the flag hash IDs in collection order (empty for Templars: index-to-hash list not imported yet).</summary>
    public sealed record Ac1Collectible(string Category, int Slot, int Needed, int Collected, IReadOnlyList<uint> FlagHashes);

    public static class Ac1Flags
    {
        private static readonly Dictionary<string, List<uint>> Lists = Load();
        private static Dictionary<string, List<uint>> Load()
        {
            var d = new Dictionary<string, List<uint>>();
            using var s = typeof(Ac1Flags).Assembly.GetManifestResourceStream("AcSaveLib.ac1_flags.tsv");
            if (s == null) return d;
            using var r = new System.IO.StreamReader(s);
            string? line;
            while ((line = r.ReadLine()) != null)
            {
                if (line.Length == 0 || line[0] == '#') continue;
                var p = line.Split('\t');
                if (!d.TryGetValue(p[0], out var l)) d[p[0]] = l = new List<uint>();
                int idx = int.Parse(p[1]);
                while (l.Count <= idx) l.Add(0);
                l[idx] = Convert.ToUInt32(p[2], 16);
            }
            return d;
        }
        /// <summary>Flag hash IDs of a list by category key (Kingdom, Masyaf, Jerusalem, Damascus, AcreTeutonic, AcreTemplars, AcreHospitallers), index = position in the list.</summary>
        public static IReadOnlyList<uint> List(string key) => Lists.TryGetValue(key, out var l) ? l : new List<uint>();
    }

    public static class Ac1Achievements
    {
        public static readonly IReadOnlyList<Ac1AchievementInfo> All = new List<Ac1AchievementInfo>
        {
            new(0, 1, "The Eagle and The Apple - 1191", 100, "Finish the Game Easy"),
            new(1, 2, "Visions of the Future", 50, "Get the secret ending"),
            new(2, 3, "Personal Vendetta", 40, "Kill All Templars"),
            new(3, 4, "Keeper of the Lions Passant", 25, "Find All Flags in Kingdom"),
            new(4, 5, "Keeper of the Creed", 10, "Find All Flags in Masyaf"),
            new(5, 6, "Keeper of the Four Gospels", 20, "Find All Flags in Jerusalem"),
            new(6, 7, "Keeper of the Crescent", 20, "Find All Flags in Damascus"),
            new(7, 9, "Absolute Symbiosis", 45, "Maximize your sync bar"),
            new(8, 11, "The Blood of a Corrupt Merchant", 25, "Kill Tamir"),
            new(9, 12, "The Blood Of A Doctor", 25, "Kill Garnier"),
            new(10, 13, "The Blood of a Slave Trader", 25, "Kill Talal"),
            new(11, 14, "The Blood of a Merchant King", 25, "Kill Vizier"),
            new(12, 15, "The Blood of a Regent", 25, "Kill Emir"),
            new(13, 16, "The Blood of a Liege-Lord", 25, "Kill Montferrat"),
            new(14, 17, "The Blood of a Teutonic Leader", 25, "Kill Sibrand"),
            new(15, 18, "The Blood of a Scribe", 25, "Kill Jubair"),
            new(16, 19, "The Blood of a Nemesis", 25, "Kill Robert"),
            new(17, 20, "Fearless", 25, "Complete all reach high points"),
            new(18, 21, "Hungerer of Knowledge", 20, "See 85% of the memory glitches"),
            new(19, 23, "Defender of the People: Acre", 20, "Rescue all free missions in Acre"),
            new(20, 24, "Defender of the People: Jerusalem", 20, "Rescue all free missions in Jerusalem"),
            new(21, 25, "Defender of the People: Damascus", 20, "Rescue all free missions in Damascus"),
            new(22, 28, "Conversationalist", 20, "Investigator"),
            new(23, 29, "Disciple of the Creed", 30, "Kill every target with a full DNA bar"),
            new(24, 30, "Eagle's Will", 20, "Defeat 100 opponents without dying"),
            new(25, 31, "Eagle's Flight", 20, "Last 10 minutes in open conflict"),
            new(26, 32, "Eagle's Prey", 20, "Assassinate 100 Guards"),
            new(27, 33, "Blade in the Crowd", 30, "Kill one of your nine targets in stealth"),
            new(28, 34, "Eagle's Challenge", 20, "Defeat 25 guards in a single fight"),
            new(29, 35, "Eagle's Swiftness", 20, "Perform 100 Counter Kill in Fights"),
            new(30, 36, "Eagle's Dive", 20, "Perform 50 Combo Kills in Fights"),
            new(31, 37, "Eagle's Talon", 15, "Perform 50 Stealth Assassinations"),
            new(32, 38, "Eagle's Dance", 10, "Perform 50 Leap of Faith"),
            new(33, 39, "The hands of a Thief", 15, "Pickpocket 200 throwing daggers"),
            new(34, 40, "March of the Pious", 5, "Use monk blending traps 20 times"),
            new(35, 41, "Eagle's Eye", 15, "Kill 75 guards by throwing daggers"),
            new(36, 42, "Enemy of the Poor", 5, "Grab and Throw 25 Harassers"),
            new(37, 43, "Gifted Escapist", 5, "Go through 20 merchant stands"),
            new(38, 45, "Keeper of the Black Cross", 10, "Find All Flags in Acre Teutonic"),
            new(39, 46, "Keeper of the Order", 10, "Find All Flags in Acre Templar"),
            new(40, 47, "Keeper of the 8 Virtues", 10, "Find All Flags in Acre Hospitalier"),
            new(41, 48, "Welcome to the Animus", 20, "Finish the Memory Corridor"),
            new(42, 49, "Hero of Masyaf", 20, "Defend Masyaf from the Templar"),
            new(43, 50, "The Punishment for Treason", 20, "Find the traitor in Masyaf"),
        };
        public static Ac1AchievementInfo? BySlot(int slot) => slot >= 0 && slot < All.Count ? All[slot] : null;

        /// <summary>Collectible slots in config 1 (all confirmed by collecting one item each, AC1_notes.md 8.3).</summary>
        public static readonly IReadOnlyList<(string Category, int Slot, int Needed, string Key)> CollectibleSlots = new List<(string, int, int, string)>
        {
            ("Kingdom flags", 3, 100, "Kingdom"), ("Masyaf flags", 4, 20, "Masyaf"), ("Jerusalem flags", 5, 100, "Jerusalem"), ("Damascus flags", 6, 100, "Damascus"),
            ("Acre Teutonic flags", 38, 33, "AcreTeutonic"), ("Acre Templar flags", 39, 34, "AcreTemplars"), ("Acre Hospitalier flags", 40, 33, "AcreHospitallers"), ("Templars killed", 2, 60, ""),
        };
    }

    public static class Ac1SaveExtensions
    {
        private static IEnumerable<Ac1Object> MissionObjects(Ac1SaveFile s) => s.Objects.Where(o => o.ClassId > 10000);

        /// <summary>All mission items with their state.</summary>
        public static List<Ac1MissionItem> MissionItems(this Ac1SaveFile s)
        {
            var r = new List<Ac1MissionItem>();
            foreach (var o in MissionObjects(s))
            {
                var st = o.Fields.FirstOrDefault(f => f.EnumType == Ac1MissionItem.StateEnumType);
                if (st == null) continue;
                bool B(uint h) => o.Fields.FirstOrDefault(f => f.Path.Count == 1 && f.Path[0].NameHash == h)?.AsBool() ?? false;
                r.Add(new Ac1MissionItem { ClassId = o.ClassId, State = (Ac1MissionState)st.AsUInt32(), IsCompleted = B(0x523ad0c1), Unknown6DCF11F9 = B(0x6dcf11f9) });
            }
            return r;
        }

        public static Ac1MissionItem? MissionItem(this Ac1SaveFile s, uint classId) => s.MissionItems().FirstOrDefault(m => m.ClassId == classId);

        /// <summary>Every accomplishment slot of every config in the Accomplishment Manager (3010).</summary>
        public static List<Ac1Accomplishment> Accomplishments(this Ac1SaveFile s)
        {
            var r = new List<Ac1Accomplishment>();
            var mgr = s.Find(3010);
            if (mgr == null) return r;
            uint hCfg = Names.Hash("Configs"), hAcc = Names.Hash("Accomplishments"), hMon = Names.Hash("Monitor"), hState = Names.Hash("State"),
                 hRes = Names.Hash("IsResolved"), hCount = Names.Hash("Count"), hUsed = Names.Hash("UsedEvents");
            var map = new Dictionary<(int, int), (uint state, bool res, uint? cnt, List<byte> used)>();
            foreach (var f in mgr.Fields)
            {
                var p = f.Path;
                if (p.Count < 3 || p[0].NameHash != hCfg || p[1].NameHash != hAcc) continue;
                var key = (c: (int)p[0].Index, a: (int)p[1].Index);
                map.TryGetValue(key, out var e); e.used ??= new List<byte>();
                if (p.Count == 3 && p[2].NameHash == hState) e.state = f.AsUInt32();
                else if (p.Count == 4 && p[2].NameHash == hMon && p[3].NameHash == hRes) e.res = f.AsBool();
                else if (p.Count == 4 && p[2].NameHash == hMon && p[3].NameHash == hCount) e.cnt = f.AsUInt32();
                else if (p.Count == 4 && p[2].NameHash == hMon && p[3].NameHash == hUsed) e.used = f.Elements().Select(v => (byte)v).ToList();
                map[key] = e;
            }
            foreach (var kv in map.OrderBy(k => k.Key.Item1).ThenBy(k => k.Key.Item2))
                r.Add(new Ac1Accomplishment { Config = kv.Key.Item1, Slot = kv.Key.Item2, State = kv.Value.state, IsResolved = kv.Value.res, Count = kv.Value.cnt, UsedEvents = kv.Value.used });
            return r;
        }

        /// <summary>Collected counts for flags and Templars (config 1 UsedEvents length).</summary>
        public static List<Ac1Collectible> Collectibles(this Ac1SaveFile s)
        {
            var acc = s.Accomplishments().Where(a => a.Config == 1).ToDictionary(a => a.Slot);
            return Ac1Achievements.CollectibleSlots.Select(c =>
            {
                var used = acc.TryGetValue(c.Slot, out var a) ? a.UsedEvents : new List<byte>();
                var list = c.Key.Length == 0 ? null : Ac1Flags.List(c.Key);
                var hashes = list == null ? new List<uint>() : used.Select(i => i < list.Count ? list[i] : 0u).ToList();
                return new Ac1Collectible(c.Category, c.Slot, c.Needed, used.Count, hashes);
            }).ToList();
        }

        /// <summary>Persistence.PersistedIDs (hash IDs).</summary>
        public static List<uint> PersistedIds(this Ac1SaveFile s) =>
            s.Find(3008)?.Find("PersistedIDs")?.Elements().Select(v => (uint)v).ToList() ?? new List<uint>();

        private static class Names { public static uint Hash(string n) => AcSaveLib.Names.PropertyNames.Crc32(n); }
    }
}
