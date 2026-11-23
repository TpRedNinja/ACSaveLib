using System;
using System.IO;
using AcSaveLib.Games.Ac3;

/// <summary>CLI verbs for AC3 (original): ac3 &lt;file&gt; summary|flat|unknown|raw &lt;container&gt; [bytes]; "ac3r" is an alias for Remastered saves</summary>
public static class Ac3Cli
{
    public static int Run(string[] args)
    {
        var save = Ac3SaveFile.Load(args[1]);
        if (args[2] == "summary") Console.WriteLine($"header=0x{save.HeaderLength:X} title=\"{save.Title}\" remastered={save.IsRemastered} id={save.HeaderId} key={Convert.ToHexString(save.Key).ToLowerInvariant()}");
        return AclCli.RunSave(save.Stream, args);
    }
}
