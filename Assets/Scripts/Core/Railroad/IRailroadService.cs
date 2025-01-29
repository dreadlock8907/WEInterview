using UnityEngine;
using System.Collections.Generic;

namespace WE.Core.Railroad
{
    public interface IRailroadService
    {
        IEnumerable<(Vector3 position, int entity)> GetAllNodes();
        IEnumerable<int> GetNodeConnections(int nodeEntity);
        Vector3 GetNodePosition(int nodeEntity);
    }
} 