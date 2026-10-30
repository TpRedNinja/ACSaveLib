using System;
using System.Collections.Generic;
using System.IO;
using AcSaveLib.Container;
using AcSaveLib.Crypto;
using AcSaveLib.Names;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Ac4
{
    /// <summary>
    /// Assassin's Creed IV Black Flag (.save). Layout is documented in docs/FORMAT_AC4.md.
    /// File -> XOR tail cipher (from byte 552) -> chunked LZO containers -> CRC32-keyed property stream.
    /// </summary>
    public sealed class Ac4SaveFile
    {
        public const int EncryptedStart = 552;
        public const int RootItemsOffset = 26;     // property stream: 14 zero bytes + root header (key u32, size u32, 0x01, size u32) then items
        private static readonly byte[] KnownPlaintext =
            { 0x23, 0, 0, 0, 0xAC, 0xDB, 0xFE, 0, 0, 0, 0, 0, 0, 0, 0, 0 };   // version 35, magic 0x00FEDBAC, 8 zero bytes

        /// <summary>UTF-16 save title from the plaintext header (e.g. "AC4BF[0]-tm_...").</summary>
        public string Title { get; private set; } = "";
        /// <summary>The 16-byte XOR key recovered from this file.</summary>
        public byte[] Key { get; private set; } = Array.Empty<byte>();
        /// <summary>Decrypted but still compressed file bytes.</summary>
        public byte[] Decrypted { get; private set; } = Array.Empty<byte>();
        /// <summary>Uncompressed header properties (container 1: Version, MagicNumber, SaveGameID, TimePlayed, ...).</summary>
        public byte[] HeaderStream { get; private set; } = Array.Empty<byte>();
        /// <summary>Uncompressed main property stream (last container).</summary>
        public byte[] Stream { get; private set; } = Array.Empty<byte>();
        /// <summary>Items of the header stream.</summary>
        public List<Node> HeaderProperties { get; private set; } = new();
        /// <summary>Top-level items of the main stream (NarrativeMissionHistory, SideMissionHistory, ...).</summary>
        public List<Node> Root { get; private set; } = new();

        public MissionHistory Narrative { get; private set; } = new();
        public MissionHistory Side { get; private set; } = new();
        public ulong CurrentMission { get; private set; }
        public ulong World { get; private set; }
        public ulong PlayerPortal { get; private set; }

        public uint ContextId { get; private set; }
        public AnimusDatabase AnimusDatabase { get; private set; } = new();
        public MapIconData MapIcons { get; private set; } = new();
        public MapFogData MapFog { get; private set; } = new();
        public MessagingDatabase Messaging { get; private set; } = new();
        public UITutorialData UITutorial { get; private set; } = new();
        public PresentEmails PresentEmails { get; private set; } = new();
        public PresentRewards PresentRewards { get; private set; } = new();
        public AccomplishmentManager Accomplishments { get; private set; } = new();
        public GameStats GameStats { get; private set; } = new();
        public CommunityChallenge CommunityChallenge { get; private set; } = new();
        public PlayerProgression PlayerProgression { get; private set; } = new();
        public PlayerProgression SnapshotPlayerProgression { get; private set; } = new();
        public SnapshotDelta SnapshotDelta { get; private set; } = new();
        public FleetData Fleet { get; private set; } = new();

        public static Ac4SaveFile Load(string path) => Parse(File.ReadAllBytes(path));

        public static Ac4SaveFile Parse(byte[] file)
        {
            if (file.Length < 600) throw new InvalidDataException("File too small to be an AC4 save.");
            var r = new Ac4SaveFile();
            r.Title = System.Text.Encoding.Unicode.GetString(file, 4, 0x200 - 4).Trim().TrimEnd((char)0).Trim();
            r.Key = XorTailCipher.RecoverKey(file, EncryptedStart, KnownPlaintext, 16);
            r.Decrypted = XorTailCipher.Apply(file, EncryptedStart, r.Key);

            var containers = ChunkContainer.ReadAll(r.Decrypted, 576);
            if (containers.Count < 2) throw new InvalidDataException("Expected a header container and a main container.");
            r.HeaderStream = containers[0];
            r.Stream = containers[containers.Count - 1];
            r.HeaderProperties = PropertyReader.Parse(r.HeaderStream, 14, r.HeaderStream.Length);
            r.Root = PropertyReader.Parse(r.Stream, RootItemsOffset, r.Stream.Length);

            foreach (var n in r.Root)
            {
                switch (n.Name)
                {
                    case "NarrativeMissionHistory": r.Narrative = MissionHistory.From(n); break;
                    case "SideMissionHistory": r.Side = MissionHistory.From(n); break;
                    case "CurrentMission": r.CurrentMission = n.Value; break;
                    case "World": r.World = n.Value; break;
                    case "PlayerPortal": r.PlayerPortal = n.Value; break;
                    case "ContextID": r.ContextId = (uint)n.Value; break;
                    case "AnimusDatabaseData": r.AnimusDatabase = AnimusDatabase.From(new Props(n.Children)); break;
                    case "MapIconData": r.MapIcons = MapIconData.From(new Props(n.Children)); break;
                    case "MapFogData": r.MapFog = MapFogData.From(new Props(n.Children)); break;
                    case "MessagingDatabaseData": r.Messaging = MessagingDatabase.From(new Props(n.Children)); break;
                    case "UITutorialData": r.UITutorial = UITutorialData.From(new Props(n.Children)); break;
                    case "PresentEmailsData": r.PresentEmails = PresentEmails.From(new Props(n.Children)); break;
                    case "PresentRewardSaveData": r.PresentRewards = PresentRewards.From(new Props(n.Children)); break;
                    case "AccomplishmentManagerData": r.Accomplishments = AccomplishmentManager.From(new Props(n.Children)); break;
                    case "GameStatsSaveData": r.GameStats = GameStats.From(new Props(n.Children)); break;
                    case "CommunityChallengeSaveData": r.CommunityChallenge = CommunityChallenge.From(new Props(n.Children)); break;
                    case "PlayerProgSaveData": r.PlayerProgression = PlayerProgression.From(new Props(n.Children)); break;
                    case "SnapshotPlayerProgSaveData": r.SnapshotPlayerProgression = PlayerProgression.From(new Props(n.Children)); break;
                    case "SnapshotPlayerProgressionDelta": r.SnapshotDelta = SnapshotDelta.From(new Props(n.Children)); break;
                    case "FleetSaveGameData": r.Fleet = FleetData.From(new Props(n.Children)); break;
                }
            }
            return r;
        }
    }
}
