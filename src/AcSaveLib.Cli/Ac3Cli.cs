using System;
using System.IO;
using AcSaveLib.Games.Ac3;

/// <summary>CLI verbs for AC3 (original): ac3 &lt;file&gt; summary|flat|unknown|raw &lt;container&gt; [bytes]</summary>
public static class Ac3Cli
{
    public static int Run(string[] args)
    {
        var save = Ac3SaveFile.Load(args[1]);
        if (args[2] == "summary") Console.WriteLine($"title=\"{save.Title}\"");
        return AclCli.RunSave(save.Stream, args);
    }
}
