using System;
using System.Linq;
using System.Text;
using AcSaveLib.Games.Ac1;
using AcSaveLib.Games.Ac4;
using AcSaveLib.Names;
using AcSaveLib.Stream;

// Usage:
//   AcSaveLib.Cli ac4 <file.save> summary
//   AcSaveLib.Cli ac4 <file.save> missions
//   AcSaveLib.Cli ac4 <file.save> tree [maxDepth] [filterName]
//   AcSaveLib.Cli ac4 <file.save> unknown        (lists property keys that have no name yet)
//   AcSaveLib.Cli crc <name>
if (args.Length >= 2 && args[0] == "crc") { Console.WriteLine(PropertyNames.Crc32(args[1]).ToString("x8")); return 0; }
if (args.Length >= 3 && args[0] == "ac1") return Ac1Cli.Run(args);
if (args.Length < 3 || args[0] != "ac4") { Console.WriteLine("usage: AcSaveLib.Cli ac4 <file.save> summary|missions|tree [depth] [name]|unknown"); return 1; }

var save = Ac4SaveFile.Load(args[1]);
switch (args[2])
{
    case "summary":
        Console.WriteLine($"title: {save.Title}");
        Console.WriteLine($"key:   {BitConverter.ToString(save.Key)}");
        Console.WriteLine($"stream: {save.Stream.Length} bytes, {save.Root.Count} top-level items");
        foreach (var h in new[] { ("Narrative", save.Narrative), ("Side", save.Side) })
        {
            var ms = h.Item2.AllMissions.ToList();
            Console.WriteLine($"{h.Item1}: {h.Item2.Groups.Count} groups, {ms.Count} missions, {ms.Count(m => m.Completed)} completed");
        }
        Console.WriteLine($"CurrentMission=0x{save.CurrentMission:X} World=0x{save.World:X} PlayerPortal=0x{save.PlayerPortal:X}");
        break;
    case "named":
    {
        // usage: ac4 <file> named <forge-index-folder>
        var fi = ForgeIndex.LoadFolder(args[3]);
        Console.WriteLine($"index: {fi.Count} ids");
        Console.WriteLine($"CurrentMission={fi.Describe(save.CurrentMission)} World={fi.Describe(save.World)} PlayerPortal={fi.Describe(save.PlayerPortal)}");
        foreach (var (label, h) in new[] { ("Narrative", save.Narrative), ("Side", save.Side) })
            foreach (var g in h.Groups)
            {
                Console.WriteLine($"{label} {fi.Describe(g.Id)}");
                foreach (var m in g.Missions)
                    Console.WriteLine($"    {fi.Describe(m.Id)} sync={m.HighestSynchronization}");
            }
        break;
    }
    case "models":
    {
        var pp = save.PlayerProgression; var pl = pp.Player;
        Console.WriteLine($"ContextId={save.ContextId}");
        Console.WriteLine($"Characters={pp.Characters.Count} snapshots={pp.CharacterSnapshots.Count} player containers={pl?.Containers.Count} money={pl?.Money} life={pl?.Life}/{pl?.MaxLife} shipLife={pl?.ShipLife} weapon={pl?.SelectedWeapon} equip={pl?.SelectedEquipment}");
        Console.WriteLine($"Notorieties={pp.WorldNotorieties.Count} wanted={pp.WantedLevel} convoys={pp.NavalConvoys.Count} weaponKills={pp.WeaponKills.Count} toolKills={pp.ToolKills.Count} presentDay={pp.PresentDayEntityStates.Count} ammo={pp.RangedWeaponLoadedAmmo.Count}");
        Console.WriteLine($"Accomplishments={save.Accomplishments.Entries.Count} completed={save.Accomplishments.Entries.Count(e => e.Completed)}");
        Console.WriteLine($"Stats={save.GameStats.StatEntries.Count} toPost={save.GameStats.StatEntriesToPost.Count}");
        Console.WriteLine($"Animus unlocked={save.AnimusDatabase.UnlockedEntries.Count} data={save.AnimusDatabase.EntriesData.Count}");
        Console.WriteLine($"MapIcon worlds={save.MapIcons.SavedIcons.Count} icons={save.MapIcons.SavedIcons.Sum(w => w.Icons.Count)}");
        Console.WriteLine($"Messages={save.Messaging.Messages.Count} emails del/group/new={save.PresentEmails.IsEmailDeleted.Count}/{save.PresentEmails.EmailGroupUnlocked.Count}/{save.PresentEmails.IsEmailNew.Count}");
        Console.WriteLine($"Rewards={save.PresentRewards.RewardItems.Count} Fleet boats={save.Fleet.CapturedBoats.Count} missionIds={save.Fleet.OnlineMissionIds.Count}");
        Console.WriteLine($"UITutorial vrTimes={save.UITutorial.VRMissionBestTimes.Count} delta chars={save.SnapshotDelta.PlayerCharacterItemsDelta.Count}");
        break;
    }
    case "missions":
        foreach (var (label, h) in new[] { ("Narrative", save.Narrative), ("Side", save.Side) })
            foreach (var g in h.Groups)
                foreach (var m in g.Missions)
                    Console.WriteLine($"{label} group 0x{g.Id:X} mission 0x{m.Id:X} sync={m.HighestSynchronization} time={m.TimePlayed} steps={m.Steps.Count} constraints=[{string.Join(",", m.Constraints.Select(c => (int)c.State))}]");
        break;
    case "tree":
        int depth = args.Length > 3 ? int.Parse(args[3]) : 2;
        string? filter = args.Length > 4 ? args[4] : null;
        foreach (var n in save.Root) if (filter == null || n.Name == filter) Dump(n, 0, depth);
        break;
    case "unknown":
        var unknown = new System.Collections.Generic.SortedDictionary<uint, int>();
        void Walk(System.Collections.Generic.IEnumerable<Node> ns)
        {
            foreach (var n in ns)
            {
                if (!PropertyNames.IsKnown(n.Key)) { unknown.TryGetValue(n.Key, out var c0); unknown[n.Key] = c0 + 1; }
                Walk(n.Children);
                foreach (var e in n.Elements) { if (!PropertyNames.IsKnown(e.Key)) { unknown.TryGetValue(e.Key, out var c1); unknown[e.Key] = c1 + 1; } Walk(e.Children); }
            }
        }
        Walk(save.Root); Walk(save.HeaderProperties);
        foreach (var kv in unknown) Console.WriteLine($"{kv.Key:x8}  x{kv.Value}");
        Console.WriteLine($"{unknown.Count} unknown keys");
        break;
}
return 0;

static void Dump(Node n, int depth, int maxDepth)
{
    string pad = new string(' ', depth * 2);
    switch (n.Kind)
    {
        case NodeKind.Value when n.Text != null: Console.WriteLine($"{pad}{n.Name} = \"{n.Text}\""); break;
        case NodeKind.Value: Console.WriteLine($"{pad}{n.Name} = {n.Value}{(n.Value > 9999 ? $" (0x{n.Value:X})" : "")} [t{n.TypeId:x4}]"); break;
        case NodeKind.Raw: Console.WriteLine($"{pad}{n.Name} RAW {n.Bytes.Length}B t{n.TypeId:x4}"); break;
        case NodeKind.Object:
            Console.WriteLine($"{pad}{n.Name} {{object}}");
            if (depth < maxDepth) foreach (var c in n.Children) Dump(c, depth + 1, maxDepth);
            break;
        case NodeKind.Array:
            if (n.ElementSize > 0)
            {
                Console.WriteLine($"{pad}{n.Name}[{n.Packed.Count}] packed u{n.ElementSize * 8}: {string.Join(" ", n.Packed.Take(12).Select(v => v.ToString("X")))}{(n.Packed.Count > 12 ? " ..." : "")}");
                break;
            }
            Console.WriteLine($"{pad}{n.Name}[{n.Elements.Count}]");
            if (depth < maxDepth)
                foreach (var e in n.Elements) { Console.WriteLine($"{pad}  - {e.Name}"); foreach (var c in e.Children) Dump(c, depth + 2, maxDepth); }
            break;
    }
}
