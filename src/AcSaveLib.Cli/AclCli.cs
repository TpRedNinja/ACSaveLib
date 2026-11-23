using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using AcSaveLib.Games.Acl;
using AcSaveLib.Names;
using AcSaveLib.Stream;

/// <summary>CLI verbs for Liberation: acl &lt;file&gt; summary|flat|unknown|raw &lt;container index&gt; [bytes]</summary>
public static class AclCli
{
    public static int Run(string[] args)
    {
        var save = AclSaveFile.Load(args[1]);
        switch (args[2])
        {
            case "summary":
                Console.WriteLine($"file={save.Data.Length} bytes containers={save.Containers.Count} [{string.Join(", ", save.Containers.Select(c => c.Length))}]");
                break;
            case "raw":
            {
                int n = args.Length > 3 ? int.Parse(args[3]) : 0;
                int max = args.Length > 4 ? int.Parse(args[4]) : 256;
                var c = save.Containers[n];
                for (int i = 0; i < Math.Min(c.Length, max); i += 16)
                {
                    var row = c.Skip(i).Take(16).ToArray();
                    Console.WriteLine($"{i:x4} {BitConverter.ToString(row).Replace('-', ' '),-48} {new string(row.Select(b => b >= 32 && b < 127 ? (char)b : '.').ToArray())}");
                }
                break;
            }
            case "flat":
            {
                void Flat(IEnumerable<Node> ns, string path)
                {
                    foreach (var n in ns)
                    {
                        string p = path + "/" + n.Name;
                        switch (n.Kind)
                        {
                            case NodeKind.Value: Console.WriteLine($"{p} = {(n.Text != null ? "\"" + n.Text + "\"" : n.Value.ToString())}"); break;
                            case NodeKind.Raw: Console.WriteLine($"{p} RAW {n.Bytes.Length}"); break;
                            case NodeKind.Object: Console.WriteLine($"{p} {{}}"); Flat(n.Children, p); break;
                            case NodeKind.Array:
                                if (n.ElementSize > 0) Console.WriteLine($"{p} packed{n.ElementSize} [{string.Join(",", n.Packed)}]");
                                else
                                {
                                    Console.WriteLine($"{p} [{n.Elements.Count}]");
                                    for (int i = 0; i < n.Elements.Count; i++) { Console.WriteLine($"{p}[{i}] {n.Elements[i].Name}"); Flat(n.Elements[i].Children, $"{p}[{i}]"); }
                                }
                                break;
                        }
                    }
                }
                for (int i = 0; i < save.Items.Count; i++) Flat(save.Items[i], "C" + i);
                break;
            }
            case "unknown":
            {
                var unknown = new SortedDictionary<uint, int>();
                void Walk(IEnumerable<Node> ns)
                {
                    foreach (var n in ns)
                    {
                        if (!PropertyNames.IsKnown(n.Key)) { unknown.TryGetValue(n.Key, out var a); unknown[n.Key] = a + 1; }
                        Walk(n.Children);
                        foreach (var e in n.Elements) { if (!PropertyNames.IsKnown(e.Key)) { unknown.TryGetValue(e.Key, out var b); unknown[e.Key] = b + 1; } Walk(e.Children); }
                    }
                }
                foreach (var items in save.Items) Walk(items);
                foreach (var kv in unknown) Console.WriteLine($"{kv.Key:x8}  x{kv.Value}");
                Console.WriteLine($"{unknown.Count} unknown keys");
                break;
            }
            default:
                Console.WriteLine("usage: AcSaveLib.Cli acl <file> summary|flat|unknown|raw <container> [bytes]");
                return 1;
        }
        return 0;
    }
}
