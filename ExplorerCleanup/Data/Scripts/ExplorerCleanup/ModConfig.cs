using System;
using System.Xml.Serialization;

using Sandbox.ModAPI;

namespace ExplorerCleanup
{
    [XmlRoot("ExplorerClean")]
    public class ModConfig
    {
        public bool AlertOwner { get; set; }
        public bool SkipNPCGrids { get; set; }
        public int BroadcastTime { get; set; }
        public bool CheckHasOwner { get; set; }
        public bool CheckHasPower { get; set; }
        public bool CheckBeacon { get; set; }
        public int GPSDiscardTime { get; set; }
        public int GracePeriod { get; set; }
        public string CheckDefaultName { get; set; }
        public int GridTimeout { get; set; }
        public int MaxSignals { get; set; }
        public int MaxDistance { get; set; }
        public int MinBlockCount { get; set; }
        public int MinFatBlockCount { get; set; }
        public int MinPlayerRange { get; set; }
        public int ScanInterval { get; set; }
        public string SignalText { get; set; }

        private string ConfigName = "Explorer-Cleanup-Settings.xml";

        public ModConfig()
        {
            AlertOwner = true;
            GPSDiscardTime = 15; // seconds until grid GPS is discarded
            GracePeriod = 30; // peroid until checked again, otherwise broadcast
            GridTimeout = 30; // time before grid will be deleted
            MaxSignals = 5; // max number of signals broadcast to each player
            MaxDistance = 150 * 1000; // max distance (meters) a player can be notified of a abandoned site from
            MinFatBlockCount = 5; // min number of fat (functional) blocks for grid to be broadcast
            MinPlayerRange = 1000; // Minimum range from player for broadcast - this should be large to avoid accidentally disclosing player bases
            ScanInterval = 30; // time between abandoned grid scans
            SignalText = "[S.O.S Signal]"; // GPS text for signal
            SkipNPCGrids = true; // Skip marking NPC grids

            CheckHasOwner = true; // Ensure gird has an owner
            CheckHasPower = true; // Ensure grid has power
            CheckBeacon = true; // Check for beacon on grid
            MinBlockCount = 10; // Min count of blocks in grid
            CheckDefaultName = "Grid"; // Grid custom name contains

            BroadcastTime = 25; // time to broadcast until delete

            // TODO - Abandoned by login time
            // TODO - Beacon check
        }

        public ModConfig LoadSettings()
        {
            if (MyAPIGateway.Utilities.FileExistsInLocalStorage(ConfigName, typeof(ModConfig)) == true)
            {
                try
                {
                    ModConfig config = null;
                    var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(ConfigName, typeof(ModConfig));
                    string configcontents = reader.ReadToEnd();
                    config = MyAPIGateway.Utilities.SerializeFromXML<ModConfig>(configcontents);
                    return config;
                }
                catch (Exception exc)
                {
                    var defaultSettings = new ModConfig();
                    return defaultSettings;
                }
            }

            var settings = new ModConfig();

            try
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(ConfigName, typeof(ModConfig)))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML<ModConfig>(settings));
                }

            }
            catch (Exception exc)
            {
            }

            return settings;
        }

        public bool SaveSettings(ModConfig settings)
        {
            try
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(ConfigName, typeof(ModConfig)))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML<ModConfig>(settings));
                }
                return true;
            }
            catch (Exception exc)
            {
            }

            return false;
        }
    }
}
