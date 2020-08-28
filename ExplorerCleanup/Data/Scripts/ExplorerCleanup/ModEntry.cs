using System;
using System.Collections.Generic;

using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.SessionComponents;
using VRage.Game.Entity;
using Sandbox.Game.Entities;
using System.Linq;
using VRageMath;

namespace ExplorerCleanup
{
    // Should use IMyParallelTask for heavy lifting?

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class ModEntry : MySessionComponentBase
    {
        public static ModConfig Config = new ModConfig();

        private static HashSet<long> trackedGrids = new HashSet<long>();

        private static readonly MyDefinitionId beaconId = new MyDefinitionId(typeof(MyObjectBuilder_Beacon));
        private static HashSet<IMyEntity> entityList = new HashSet<IMyEntity>();

        
        private static int broadcastTimer = -1;
        private static int gpsTimer = -1;
        private static int graceTimer = 0;
        private static int scanTimer = 0;
        private static int tickCounter = 0;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            Config.LoadSettings();
            BroadcastManager._ModConfig = Config;

            graceTimer = Config.GracePeriod;
            scanTimer = Config.ScanInterval;

            // Todo - How to proper undload? Too many of the same mod DLL in long debug sessions
        }

        public override void BeforeStart()
        {
            // Disable script if not server
            if (!MyAPIGateway.Multiplayer.IsServer)
            {
                MyAPIGateway.Utilities.InvokeOnGameThread(() => SetUpdateOrder(MyUpdateOrder.NoUpdate));
                return;
            }

            MyVisualScriptLogicProvider.SendChatMessage("Explorer Cleanup v0.1a Loaded", "SERVER", 0, "Red");
        }

        public override void UpdateAfterSimulation()
        {
            tickCounter += 1;

            if (tickCounter < 60)
            {
                return;
            }
            else
            {
                tickCounter = 0;
            }

            // Todo - Implement proper timers and callbacks, this is all gross
            if (scanTimer <= 0)
            {
                if (graceTimer == Config.GracePeriod - 1)
                {
                    InitialGridScan();
                }
                else if (graceTimer <= 0)
                {
                    TrackedGridScan();

                    broadcastTimer = Config.BroadcastTime;
                    gpsTimer = Config.GPSDiscardTime;
                    graceTimer = Config.GracePeriod;
                    scanTimer = Config.ScanInterval;
                }

                graceTimer -= 1;
            }
            else
            {
                scanTimer -= 1;
            }

            if(broadcastTimer > 0)
            {
                broadcastTimer -= 1;
                gpsTimer -= 1;
                if(gpsTimer == 0)
                {
                    // Built in GPS DiscardAt is broken? 
                    MyVisualScriptLogicProvider.RemoveGPSForAll(Config.SignalText);
                    gpsTimer = -1;
                }
            }
            else if(broadcastTimer == 0)
            {
                BroadcastManager.TrashBroadcasted();
                broadcastTimer = -1;
            }
        }

        void InitialGridScan()
        {
            // Save before?

            MyVisualScriptLogicProvider.SendChatMessage("Scanning for offending grids..", "SERVER", 0, "Red");
            MyAPIGateway.Entities.GetEntities(entityList);

            foreach (var entity in entityList)
            {
                IMyCubeGrid grid = entity as IMyCubeGrid;

                if (grid == null)
                    continue;

                if (BroadcastManager.ContainsEntityId(grid.EntityId))
                {
                    continue;
                }

                GridValidator gridValidator = new GridValidator(grid.EntityId);
                GridStatus gridStatus = gridValidator.Validate(Config);

                if (gridStatus > GridStatus.Marked)
                {
                    MyVisualScriptLogicProvider.SendChatMessage("Tracking [" + grid.CustomName + "] - " + Enum.GetName(typeof(GridStatus), gridStatus), "SERVER", 0);

                    if (Config.AlertOwner)
                    {
                        BroadcastInfo broadcastInfo = gridValidator.GridToBroadcastInfo();
                        if(broadcastInfo.Location == Vector3D.Zero)
                        {
                            continue;
                        }

                        BroadcastError err = BroadcastManager.AddBroadcastInfo(broadcastInfo, false);

                        if (err == BroadcastError.Ok)
                        {
                            double timeInMinutes = (double)Config.GracePeriod / 60.0;

                            MyVisualScriptLogicProvider.SendChatMessage(
                                $"Warning [{grid.CustomName}] may be BROADCAST in {timeInMinutes} minutes." +
                                $" If this is not intentional please review the grid policy and correct immediately.",
                                "SERVER",
                                gridValidator.ownerId,
                                "Red");
                        }
                    }

                    trackedGrids.Add(grid.EntityId);
                }
            }
            MyVisualScriptLogicProvider.SendChatMessage($"Tracking {trackedGrids.Count} grids for possible broadcast", "SERVER", 0, "Red");
            entityList.Clear();
        }


        static void TrackedGridScan()
        {
            int count = 0;
            MyVisualScriptLogicProvider.SendChatMessage("Checking for grids to broadcast..", "SERVER", 0, "Red");
            List<long> gridsToTrash = new List<long>();

            foreach (long entityId in trackedGrids)
            {
                // Validating twice, it maybe better to instead listen for events on the grid
                GridValidator gridValidator = new GridValidator(entityId);
                GridStatus gridStatus = gridValidator.Validate(Config);

                if (gridStatus > GridStatus.Marked)
                {
                    BroadcastInfo broadcastInfo = gridValidator.GridToBroadcastInfo();
                    BroadcastError err = BroadcastManager.AddBroadcastInfo(broadcastInfo, true);

                    switch (err)
                    {
                        case BroadcastError.NotEnoughBlocks:
                        case BroadcastError.NotEnoughFatBlocks:
                        case BroadcastError.TooFarFromPlayers:
                            MyVisualScriptLogicProvider.SendChatMessage($"Trashing {broadcastInfo} {Enum.GetName(typeof(BroadcastError), err)}", "SERVER", 0, "Red");
                            gridsToTrash.Add(entityId);
                            break;
                        case BroadcastError.TooCloseToPlayers:
                            MyVisualScriptLogicProvider.SendChatMessage($"Ignoring {broadcastInfo} {Enum.GetName(typeof(BroadcastError), err)}", "SERVER", 0, "Red");
                            break;
                        default:
                            MyVisualScriptLogicProvider.SendChatMessage($"Broadcasting {broadcastInfo}", "SERVER", 0, "Red");
                            count++;
                            break;
                    }
                }
            }

            Util.TrashGrids(gridsToTrash);
            trackedGrids.Clear();
        }
    }
}
