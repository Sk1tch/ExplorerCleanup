using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Sandbox.Engine.Voxels;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using VRageMath;
using Sandbox.Game.Entities;

namespace ExplorerCleanup
{
    static class Util
    {
        public static int CountPlayerSOSGPS(long identityId, string gpsText)
        {
            int count = 0;
            List<IMyGps> playerGPS = MyAPIGateway.Session.GPS.GetGpsList(identityId);

            foreach (IMyGps gps in playerGPS)
            {
                if (gps.Description.Contains(gpsText))
                {
                    count++;
                }
            }
            return count;
        }

        public static void TrashGrids(List<long> gridsToTrash)
        {
            MyVisualScriptLogicProvider.SendChatMessage($"Trashing {gridsToTrash.Count} offending grids", "SERVER", 0, "Red");

            foreach (long removeGridId in gridsToTrash)
            {
                MyCubeGrid grid = MyVisualScriptLogicProvider.GetEntityById(removeGridId) as MyCubeGrid;

                // Possible trash collector gets it first
                if (grid != null)
                {
                    grid.MarkAsTrash();
                }
            }
        }
    }
}

