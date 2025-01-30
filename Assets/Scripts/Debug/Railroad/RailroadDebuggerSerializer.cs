using UnityEngine;
using System.Collections.Generic;
using WE.Core.Railroad.System;
using WE.Core.Transform.System;
using WE.Core.Mine.System;
using WE.Core.Base.System;
using WE.Core.Extensions;
using Unity.Collections;

namespace WE.Debug.Railroad
{
  public class RailroadDebuggerSerializer
  {
    private readonly RailroadUtilsSystem railroadUtils;
    private readonly TransformUtilsSystem transformUtils;
    private readonly MineUtilsSystem mineUtils;
    private readonly BaseUtilsSystem baseUtils;

    public RailroadDebuggerSerializer(
      RailroadUtilsSystem railroadUtils,
      TransformUtilsSystem transformUtils,
      MineUtilsSystem mineUtils,
      BaseUtilsSystem baseUtils)
    {
      this.railroadUtils = railroadUtils;
      this.transformUtils = transformUtils;
      this.mineUtils = mineUtils;
      this.baseUtils = baseUtils;
    }

    public void WriteTo(string path)
    {
      if (string.IsNullOrEmpty(path))
        return;

      var data = new RailroadSaveData();
      using var nodes = railroadUtils.GetAllNodes(Allocator.Temp);

      foreach (var node in nodes)
      {
        var nodeData = new NodeData
        {
          id = node,
          position = transformUtils.GetPosition(node).ToVector3(),
          type = GetNodeType(node),
          multiplier = GetNodeMultiplier(node)
        };
        data.nodes.Add(nodeData);

        foreach (var connectedNode in railroadUtils.GetNodeConnections(node))
        {
          if (node < connectedNode)
            data.connections.Add(new ConnectionData { fromNode = node, toNode = connectedNode });
        }
      }

      try
      {
        var json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(path, json);
      }
      catch (System.Exception e)
      {
        UnityEngine.Debug.LogError(e.Message);
      }
    }

    public void ReadFrom(string path)
    {
      if (string.IsNullOrEmpty(path))
        return;

      var data = new RailroadSaveData();
      try
      {
        var json = System.IO.File.ReadAllText(path);
        data = JsonUtility.FromJson<RailroadSaveData>(json);
      }
      catch (System.Exception e)
      {
        UnityEngine.Debug.LogError(e.Message);
        return;
      }

      using var existingNodes = railroadUtils.GetAllNodes(Allocator.Temp);
      foreach (var node in existingNodes)
        railroadUtils.DestroyNode(node);

      var idMap = new Dictionary<int, int>();
      foreach (var nodeData in data.nodes)
      {
        var newNode = railroadUtils.CreateNode(nodeData.position.ToFloat3());
        idMap[nodeData.id] = newNode;

        switch (nodeData.type)
        {
          case NodeType.Mine:
            mineUtils.Setup(newNode, nodeData.multiplier);
            break;
          case NodeType.Base:
            baseUtils.Setup(newNode, nodeData.multiplier);
            break;
        }
      }

      foreach (var connection in data.connections)
      {
        railroadUtils.LinkNodes(
          idMap[connection.fromNode],
          idMap[connection.toNode]
        );
      }
    }

    private NodeType GetNodeType(int node)
    {
      if (mineUtils.IsMine(node))
        return NodeType.Mine;
      if (baseUtils.IsBase(node))
        return NodeType.Base;
      return NodeType.Default;
    }

    private float GetNodeMultiplier(int node)
    {
      if (mineUtils.IsMine(node))
        return mineUtils.GetMiningMultiplier(node);
      if (baseUtils.IsBase(node))
        return baseUtils.GetResourceMultiplier(node);
      return 1f;
    }
  }
}