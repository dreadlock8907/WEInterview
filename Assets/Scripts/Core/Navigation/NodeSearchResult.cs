using System.Collections.Generic;
using Unity.Collections;

namespace WE.Core.Navigation
{
  public struct NodeSearchResult
  {
    public int node;
    public float score;
    public float pathDistance;
    public NativeArray<int> path;

    public static NodeSearchResult Empty => new NodeSearchResult { node = -1, score = 0, pathDistance = 0, path = new NativeArray<int>(0, Allocator.Persistent) };

    public void Dispose()
    {
      if (path.IsCreated)
        path.Dispose();
    }

    public bool IsEmpty => node == -1;
  }

  public struct NodeSearchResultComparer : IComparer<NodeSearchResult>
  {
    public int Compare(NodeSearchResult x, NodeSearchResult y)
    {
      return y.score.CompareTo(x.score);
    }
  }
}