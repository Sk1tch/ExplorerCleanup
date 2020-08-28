using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace ExplorerCleanup
{

    enum BroadcastError
    {
        Ok,
        NotEnoughBlocks,
        NotEnoughFatBlocks,
        TooCloseToPlayers,
        TooFarFromPlayers,
    }

    static class BroadcastManager
    {
        public static List<BroadcastInfo> BroadcastingGrids = new List<BroadcastInfo>();
        public static ModConfig _ModConfig { get; set; }

        public static BroadcastError AddBroadcastInfo(BroadcastInfo broadcastInfo, bool broadcast = false)
        {
            HashSet<long> playersToNotify;
            BroadcastError err = PlayersWithinSafeRange(broadcastInfo.Location, out playersToNotify);

            if (err == BroadcastError.TooCloseToPlayers || err == BroadcastError.TooFarFromPlayers)
            {
                return err;
            }
            else
            {
                err = ValidateBlockRequirements(broadcastInfo);

                if(err != BroadcastError.Ok)
                {
                    return err;
                }

                if (err == BroadcastError.Ok && broadcast == true)
                {
                    foreach (long identityId in playersToNotify)
                    {
                        // If player near owned Antenna consider adding more detils to grid description and increasing range
                        // e.g. Antenna Size (big/small), Est. Grid Power, Grid Name, Ship/Station
                        IMyGps myGPS = AddGPSToPlayer(identityId, broadcastInfo);

                        if (myGPS != null)
                        {
                            broadcastInfo.MyGps.Add(myGPS);
                        }
                    }
                    BroadcastingGrids.Add(broadcastInfo);
                    MyVisualScriptLogicProvider.SendChatMessage($"Broadcasting {BroadcastingGrids.Count} offending grids", "SERVER", 0, "Red");
                }

                return BroadcastError.Ok;
            }
        }

        public static bool ContainsEntityId(long entityId)
        {
            return BroadcastingGrids.FirstOrDefault(i => i.EntityId == entityId) != default(BroadcastInfo);
        }

        public static int TrashBroadcasted()
        {
            List<long> gridsToTrash = new List<long>();

            foreach (BroadcastInfo bci in BroadcastingGrids)
            {
                gridsToTrash.Add(bci.EntityId);
            }

            Util.TrashGrids(gridsToTrash);
            BroadcastingGrids.Clear();
            return gridsToTrash.Count();
        }

        public static IMyGps AddGPSToPlayer(long IdentityId, BroadcastInfo broadcastInfo)
        {
            int sosCount = Util.CountPlayerSOSGPS(IdentityId, _ModConfig.SignalText);

            if (sosCount > _ModConfig.MaxSignals)
            {
                return null;
            }

            IMyGps gridGPS = MyAPIGateway.Session.GPS.Create(_ModConfig.SignalText, $"{broadcastInfo.EntityId}", broadcastInfo.Location, true, true);
            gridGPS.GPSColor = Color.OrangeRed;
            gridGPS.DiscardAt = TimeSpan.FromSeconds(MyAPIGateway.Session.ElapsedPlayTime.TotalSeconds + (double)_ModConfig.GPSDiscardTime);

            MyAPIGateway.Session.GPS.AddGps(IdentityId, gridGPS);
            return gridGPS;
        }

        public static BroadcastError PlayersWithinSafeRange(Vector3D gridLocation, out HashSet<long> playersToNotify)
        {
            playersToNotify = new HashSet<long>();

            List<IMyPlayer> playerList = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(playerList);

            foreach (IMyPlayer player in playerList)
            {
                double distance = Vector3D.Distance(player.GetPosition(), gridLocation);

                // Exit if any player within min range to avoid possibly disclosing their/faction base
                if (distance < _ModConfig.MinPlayerRange)
                {
                    return BroadcastError.TooCloseToPlayers;
                }
                else if (distance < _ModConfig.MaxDistance)
                {
                    playersToNotify.Add(player.IdentityId);
                }
            }

            if(playersToNotify.Count == 0)
            {
                return BroadcastError.TooFarFromPlayers;
            }

            return BroadcastError.Ok;
        }

        public static BroadcastError ValidateBlockRequirements(BroadcastInfo broadcastInfo)
        {
            if (_ModConfig.MinBlockCount > 0 && broadcastInfo.BlockCount < _ModConfig.MinBlockCount)
            {
                return BroadcastError.NotEnoughBlocks;
            }

            MyCubeGrid cubeGrid = MyVisualScriptLogicProvider.GetEntityById(broadcastInfo.EntityId) as MyCubeGrid;

            if (cubeGrid != null && cubeGrid.GetFatBlocks().Count < _ModConfig.MinFatBlockCount)
            {
                // Probably not an interesting grid
                return BroadcastError.NotEnoughFatBlocks;
            }

            return BroadcastError.Ok;
        }
    }
}
