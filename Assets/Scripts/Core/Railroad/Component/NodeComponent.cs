using System;
using Unity.Collections;
using WE.Core.Util.Components;

namespace WE.Core.Railroad.Component
{
  public struct NodeComponent : IDisposableComponent
  {
    public int ConnectionsCount => IsEmpty() ? 0 : connectedNodes.Length;
    private NativeList<int> connectedNodes;


    public NativeArray<int>.ReadOnly ReadConnectedNodes()
    {
      if (IsEmpty())
        return new NativeArray<int>(0, Allocator.Temp).AsReadOnly();
      return connectedNodes.AsArray().AsReadOnly();
    }

    public void Dispose()
    {
      if(!IsEmpty())
      connectedNodes.Dispose();
    }

    public int GetConnectedNode(int index)
    {
      if (IsEmpty())
        return -1;

      return index >= 0 && index < ConnectionsCount ? connectedNodes[index] : -1;
    }

    public int FindConnectedNodeIdx(int node)
    {
      if (IsEmpty())
        return -1;

      return connectedNodes.BinarySearch(node);
    }

    public void AddConnectedNode(int node)
    {
      if (IsEmpty())
        connectedNodes = new NativeList<int>(Allocator.Persistent);

      connectedNodes.Add(node);
      connectedNodes.Sort();
    }

    public void AddUniqueConnectedNode(int node)
    {
      if (FindConnectedNodeIdx(node) < 0)
        AddConnectedNode(node);
    }

    public void Clear()
    {
      connectedNodes.Clear();
    }

    public void RemoveConnectedNode(int node)
    {
      var idx = FindConnectedNodeIdx(node);
      if (idx < 0)
        return;

      connectedNodes.RemoveAtSwapBack(idx);
    }

    public bool IsEmpty()
    {
      return connectedNodes.IsEmpty;
    }
  }
}

