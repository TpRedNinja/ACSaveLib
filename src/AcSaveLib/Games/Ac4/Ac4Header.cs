using System.Collections.Generic;
using System.IO;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac4
{
    /// <summary>The header container (first container) of an AC4 or Freedom Cry save.</summary>
    public sealed class Ac4Header
    {
        public uint Version { get; init; }
        public uint MagicNumber { get; init; }
        public uint DlcPackagesBits { get; init; }
        public uint CodeChangelist { get; init; }
        public uint SaveGameId { get; init; }
        public bool IsOptions { get; init; }
        /// <summary>Slot name typed by the player ("Test 100", "Abstergo") or "Options" in an options save. ACViewer shows the same text (type String).</summary>
        public string SaveGameName { get; init; } = "";
        public uint TimePlayed { get; init; }
        public uint PlayerGlobalSynch { get; init; }
        /// <summary>Unix time in seconds as stored.</summary>
        public uint SaveGameDate { get; init; }

        public static Ac4Header From(List<Node> nodes)
        {
            var p = new Props(nodes);
            return new Ac4Header
            {
                Version = p.U32("Version"), MagicNumber = p.U32("MagicNumber"), DlcPackagesBits = p.U32("DLCPackagesBits"),
                CodeChangelist = p.U32("CodeChangelist"), SaveGameId = p.U32("SaveGameID"), IsOptions = p.Bool("IsOptions"),
                SaveGameName = p.AnsiString("SaveGameName"),
                TimePlayed = p.U32("TimePlayed"), PlayerGlobalSynch = p.U32("PlayerGlobalSynch"), SaveGameDate = p.U32("SaveGameDate"),
            };
        }
    }
}
