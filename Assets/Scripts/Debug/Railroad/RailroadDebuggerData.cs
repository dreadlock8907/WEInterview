using UnityEngine;
using System;
using System.Collections.Generic;

namespace WE.Debug.Railroad
{
  [Serializable]
  public class NodeData
  {
    public int id;
    public Vector3 position;
    public NodeType type;
    public float multiplier;
  }

  [Serializable]
  public class ConnectionData
  {
    public int fromNode;
    public int toNode;
  }

  [Serializable]
  public class RailroadSaveData
  {
    public List<NodeData> nodes = new();
    public List<ConnectionData> connections = new();
  }

  public enum NodeType
  {
    Default,
    Mine,
    Base
  }
} 