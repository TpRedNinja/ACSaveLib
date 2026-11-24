using System.Collections.Generic;
using AcSaveLib.Stream;

namespace AcSaveLib.Games.Acu
{
    public sealed class AcuAssassinsRank
    {
        public uint RankIndex { get; init; }
        public uint RankThreshold { get; init; }
        public ulong RankRewardId { get; init; }
    }

    /// <summary>Contents of a slot 3 (options) save: PlayerOptionsSaveData, AccomplishmentManagerData, OnlineProfileSaveData, RiftGhostData,
    /// AssassinsRankTableData (profile container) and SinglePlayerOptionsSaveData (last container). Unnamed keys stay in the node tree.</summary>
    public sealed class AcuOptionsData
    {
        public bool DisplaySubtitles { get; private set; }
        public ulong LastConsoleLanguage { get; private set; }
        public ulong SoundLanguage { get; private set; }
        public ulong SubtitlesLanguage { get; private set; }
        public float MusicVolume { get; private set; }
        public float VoiceVolume { get; private set; }
        public float SoundFxVolume { get; private set; }
        public bool Vibration { get; private set; }
        public float CameraXAxisSensitivity { get; private set; }
        public float CameraYAxisSensitivity { get; private set; }
        public bool InvertCameraHorizontalAxis { get; private set; }
        public bool InvertCameraVerticalAxis { get; private set; }
        public bool InvertAimingCameraHorizontalAxis { get; private set; }
        public bool InvertAimingCameraVerticalAxis { get; private set; }
        public uint CameraInserts { get; private set; }
        public uint Brightness { get; private set; }
        public bool Blood { get; private set; }
        public uint TimeOfDay { get; private set; }
        public bool JoinInProgressGhosts { get; private set; }
        public bool RiftGhosts { get; private set; }
        public bool Companion { get; private set; }
        public ulong HudAllHidden { get; private set; }
        public Dictionary<string, bool> HudFlags { get; private set; } = new();
        public uint PoppedEvents { get; private set; }
        public uint ExclusiveContents { get; private set; }
        public uint UplayRewards { get; private set; }
        public uint UplayActions { get; private set; }
        public uint RiftYear { get; private set; }
        public uint RiftWeek { get; private set; }
        public uint RiftMaxSeeds { get; private set; }
        public uint RiftBlackList { get; private set; }
        public int RiftScoreHistoryCount { get; private set; }
        public List<AcuAssassinsRank> AssassinsRanks { get; private set; } = new();
        public int AccomplishmentEntryCount { get; private set; }
        public bool FlyingMachineControls { get; private set; }
        public bool InvertCannonHorizontalOnlyCameraControls { get; private set; }
        public bool InvertCannonVerticalOnlyCameraControls { get; private set; }

        private static readonly string[] HudNames =
        {
            "HUDConflictHidden", "HUDPlayerWidgetHidden", "HUDFriendsWidgetHidden", "HUDMinimapHidden", "HUDWeaponIndicatorHidden", "HUDHighGroundHidden",
            "HUDTutorialsDisabled", "HUDUpdatesDisabled", "HUDPuppeteerHidden", "HighlightEffectsHidden", "HUDMQLHidden", "HUDAdditionnalHidden",
        };

        /// <param name="profileRoot">Items of the middle container.</param>
        /// <param name="lastRoot">Items of the last container.</param>
        public static AcuOptionsData From(List<Node> profileRoot, List<Node> lastRoot)
        {
            var r = new AcuOptionsData();
            foreach (var n in profileRoot)
            {
                var p = new Props(n.Children);
                switch (n.Name)
                {
                    case "PlayerOptionsSaveData":
                        r.DisplaySubtitles = p.Bool("DisplaySubtitles"); r.LastConsoleLanguage = p.U64("LastConsoleLanguage");
                        r.SoundLanguage = p.U64("SoundLanguage"); r.SubtitlesLanguage = p.U64("SubtitlesLanguage");
                        r.MusicVolume = p.Float("MusicVolume"); r.VoiceVolume = p.Float("VoiceVolume"); r.SoundFxVolume = p.Float("SoundFXVolume");
                        r.Vibration = p.Bool("Vibration"); r.CameraXAxisSensitivity = p.Float("CameraXAxisSensitivity"); r.CameraYAxisSensitivity = p.Float("CameraYAxisSensitivity");
                        r.InvertCameraHorizontalAxis = p.Bool("InvertCameraHorizontalAxis"); r.InvertCameraVerticalAxis = p.Bool("InvertCameraVerticalAxis");
                        r.InvertAimingCameraHorizontalAxis = p.Bool("InvertAimingCameraHorizontalAxis"); r.InvertAimingCameraVerticalAxis = p.Bool("InvertAimingCameraVerticalAxis");
                        r.CameraInserts = p.U32("CameraInserts"); r.Brightness = p.U32("Brightness"); r.Blood = p.Bool("Blood"); r.TimeOfDay = p.U32("TimeOfDay");
                        r.JoinInProgressGhosts = p.Bool("JoinInProgressGhosts"); r.RiftGhosts = p.Bool("RiftGhosts"); r.Companion = p.Bool("Companion");
                        r.HudAllHidden = p.U64("HUDAllHidden");
                        foreach (var name in HudNames) r.HudFlags[name] = p.Bool(name);
                        break;
                    case "AccomplishmentManagerData": r.AccomplishmentEntryCount = p.ElementCount("AccomplishmentEntries"); break;
                    case "OnlineProfileSaveData":
                        r.PoppedEvents = p.Obj("EventsSaveData").U32("PoppedEvents"); r.ExclusiveContents = p.U32("ExclusiveContents");
                        r.UplayRewards = p.U32("UplayRewards"); r.UplayActions = p.U32("UplayActions");
                        break;
                    case "RiftGhostData":
                        r.RiftYear = p.U32("Year"); r.RiftWeek = p.U32("Week"); r.RiftMaxSeeds = p.U32("MaxSeeds"); r.RiftBlackList = p.U32("BlackList");
                        r.RiftScoreHistoryCount = p.Packed("ScoreHistory").Count;
                        break;
                    case "AssassinsRankTableData":
                        foreach (var e in p.Elements("AssassinsRankTable"))
                            r.AssassinsRanks.Add(new AcuAssassinsRank { RankIndex = e.U32("RankIndex"), RankThreshold = e.U32("RankThreshold"), RankRewardId = e.U64("RankRewardID") });
                        break;
                }
            }
            foreach (var n in lastRoot)
            {
                if (n.Name != "SinglePlayerOptionsSaveData") continue;
                var p = new Props(n.Children);
                r.FlyingMachineControls = p.Bool("FlyingMachineControls");
                r.InvertCannonHorizontalOnlyCameraControls = p.Bool("InvertCannonHorizontalOnlyCameraControls");
                r.InvertCannonVerticalOnlyCameraControls = p.Bool("InvertCannonVerticalOnlyCameraControls");
            }
            return r;
        }
    }
}
