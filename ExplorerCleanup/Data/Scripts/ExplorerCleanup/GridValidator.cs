using System;
using System.Collections.Generic;
using System.Text;

using Sandbox.Game;
using Sandbox.Game.Contracts;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

namespace ExplorerCleanup
{
    class GridValidator
    {
        public List<IMySlimBlock> blocks = new List<IMySlimBlock>();
        public long ownerId = 0;
        public GridStatus gridStatus;

        int blockCount = 0;
        MyEntity entity = null;
        long entityId = -1;
        private string entityName;

        public GridValidator(long gridEntityId)
        {
            entityId = gridEntityId;
            entityName = MyVisualScriptLogicProvider.GetEntityName(entityId);

            entity = MyVisualScriptLogicProvider.GetEntityByName(entityName);

            if (string.IsNullOrEmpty(entityName) == true)
            {
                entityName = entityId.ToString();
                MyVisualScriptLogicProvider.SetName(entityId, entityName);
            }

            gridStatus = GridStatus.Ok;
            ownerId = MyVisualScriptLogicProvider.GetOwner(entityName);
        }

        public GridStatus Validate(ModConfig config)
        {
            if (config.SkipNPCGrids && IsNPCGrid())
            {
                gridStatus = GridStatus.NPC;
                return gridStatus;
            }

            if(config.CheckHasOwner && !HasOwner())
            {
                gridStatus = GridStatus.NoOwner;
                return gridStatus;
            }

            if (config.CheckHasPower && !HasPower())
            {
                gridStatus = GridStatus.NoPower;
                return gridStatus;
            }

            if (config.MinBlockCount != 0 && !HasMinBlocks(config.MinBlockCount))
            {
                gridStatus = GridStatus.NotEnoughBlocks;
                return gridStatus;
            }

            if (!string.IsNullOrEmpty(config.CheckDefaultName) && !HasCustomName(config.CheckDefaultName))
            {
                gridStatus = GridStatus.DefaultName;
                return gridStatus;
            }

            return gridStatus;
        }

        public BroadcastInfo GridToBroadcastInfo()
        {
            IMyCubeGrid cubeGrid = FetchCurrentCubeGrid(entityName);
            string ownerName = MyVisualScriptLogicProvider.GetPlayersName(ownerId);

            if (cubeGrid != null)
            {
                return new BroadcastInfo(blockCount, entityId, cubeGrid.CustomName, cubeGrid.IsStatic, cubeGrid.GetPosition(), ownerId, ownerName, gridStatus);
            } 
            else
            {
                // dead grid, return Vector3D, ModEntry will trash
                return new BroadcastInfo(blockCount, entityId, "cubeGrid null", false, Vector3D.Zero, ownerId, ownerName, gridStatus);
            }
        }

        public bool HasOwner()
        {
            // 0 = no owner
            if (ownerId == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool IsNPCGrid()
        {
            long ownerId = MyVisualScriptLogicProvider.GetOwner(entityName);

            // Check pirate owner
            if (ownerId == MyVisualScriptLogicProvider.GetPirateId())
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        public bool HasPower()
        {
            // Check if grid powered
            if (MyVisualScriptLogicProvider.HasPower(entityName) == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool HasMinBlocks(int minBlocks)
        {
            IMyCubeGrid cubeGrid = FetchCurrentCubeGrid(entityName);

            cubeGrid.GetBlocks(blocks);
            blockCount = blocks.Count;

            if (blockCount < minBlocks)
            {
                return false;
            }

            return true;
        }

        public bool HasCustomName(string defaultName)
        {
            IMyCubeGrid cubeGrid = FetchCurrentCubeGrid(entityName);

            if (cubeGrid.CustomName.Contains(defaultName))
            {
                return false;
            }
            return true;
        }

        private static IMyCubeGrid FetchCurrentCubeGrid(string entityName)
        {
            // Workaround for CubeGrid references that seem to get nulled

            MyEntity entity = MyVisualScriptLogicProvider.GetEntityByName(entityName);
            return entity as IMyCubeGrid;
        }
    }
}
