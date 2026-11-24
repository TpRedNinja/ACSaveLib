using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acs
{
    /// <summary>Header container of a Syndicate save (same items as Unity).</summary>
    public sealed class AcsHeader
    {
        public uint Version { get; init; }
        public uint MagicNumber { get; init; }
        public uint DlcPackagesBits { get; init; }
        public uint CodeChangelist { get; init; }
        public uint SaveGameId { get; init; }
        public bool IsOptions { get; init; }
        /// <summary>Slot name, "Options" in an options save.</summary>
        public string SaveGameName { get; init; } = "";
        /// <summary>Unit not known.</summary>
        public uint TimePlayed { get; init; }
        public uint PlayerGlobalSynch { get; init; }
        /// <summary>Unix time in seconds (UTC) as stored.</summary>
        public uint SaveGameDate { get; init; }

        public static AcsHeader From(List<Node> nodes)
        {
            var p = new Props(nodes);
            return new AcsHeader
            {
                Version = p.U32("Version"), MagicNumber = p.U32("MagicNumber"), DlcPackagesBits = p.U32("DLCPackagesBits"),
                CodeChangelist = p.U32("CodeChangelist"), SaveGameId = p.U32("SaveGameID"), IsOptions = p.Bool("IsOptions"),
                SaveGameName = p.AnsiString("SaveGameName"),
                TimePlayed = p.U32("TimePlayed"), PlayerGlobalSynch = p.U32("PlayerGlobalSynch"), SaveGameDate = p.U32("SaveGameDate"),
            };
        }
    }
}
