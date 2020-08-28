using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using VRage.Game.ModAPI;
using VRageMath;

namespace ExplorerCleanup
{
    class BroadcastInfo
    {
        public int BlockCount { get; set; }
        public long EntityId { get; set; }
        public string GridName { get; set; }
        public bool IsStatic { get; set; }
        public Vector3D Location { get; set; }
        public HashSet<IMyGps> MyGps { get; set; }
        public long OwnerId { get; set; }
        public string OwnerName { get; set; }
        public long TimeSinceLastPlayer { get; set; }
        public GridStatus Status { get; set; }

        public BroadcastInfo(int blockCount, long entityId, string gridname, bool isStatic, Vector3D location, long ownerId, string ownerName, GridStatus status)
        {
            BlockCount = blockCount;
            EntityId = entityId;
            GridName = gridname;
            IsStatic = isStatic;
            Location = location;
            MyGps = new HashSet<IMyGps>();
            OwnerId = ownerId;
            OwnerName = ownerName;
            TimeSinceLastPlayer = 0;
            Status = status;
        }

        public override string ToString()
        {
            return $"GridName: {GridName} OwnerName: {OwnerName} GridStatus: {Status} BlockCount: {BlockCount} IsStatic {IsStatic}";
        }
    }
}
