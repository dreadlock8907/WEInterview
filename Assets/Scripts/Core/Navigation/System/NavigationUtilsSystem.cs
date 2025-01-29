using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Unity.Collections;
using Unity.Mathematics;
using WE.Core.Railroad.System;
using WE.Core.Transform.System;
using WE.Core.Util;

namespace WE.Core.Navigation.System
{
  public class NavigationUtilsSystem : IEcsSystem
  {
    private readonly EcsCustomInject<RailroadUtilsSystem> railroadUtils = default;
    private readonly EcsCustomInject<TransformUtilsSystem> transformUtils = default;
    private readonly EcsCustomInject<EntityRepositorySystem> entityRepository = default;

    public NativeArray<int> FindPath(int startNode, int endNode)
    {
      if (startNode == endNode)
        return new NativeArray<int>(new[] { startNode }, Allocator.Persistent);

      using var visited = new NativeHashSet<int>(64, Allocator.Temp);
      using var queue = new NativeQueue<PathNode>(Allocator.Temp);
      using var cameFrom = new NativeHashMap<int, int>(64, Allocator.Temp);

      queue.Enqueue(new PathNode(startNode, 0));
      visited.Add(startNode);

      while (!queue.IsEmpty())
      {
        var current = queue.Dequeue();

        if (current.node == endNode)
        {
          return ReconstructPath(startNode, endNode, cameFrom);
        }

        foreach (var neighbor in railroadUtils.Value.GetNodeConnections(current.node))
        {
          if (visited.Contains(neighbor))
            continue;

          visited.Add(neighbor);
          cameFrom.Add(neighbor, current.node);
          
          var cost = current.cost + GetDistance(current.node, neighbor);
          queue.Enqueue(new PathNode(neighbor, cost));
        }
      }

      // Путь не найден, возвращаем пустой массив
      return new NativeArray<int>(0, Allocator.Persistent);
    }

    public int FindClosestNode<T>(int entity) where T : struct
    {
      var entityPos = transformUtils.Value.GetPosition(entity);
      var closestNode = -1;
      var minDistance = float.MaxValue;

      using var nodes = railroadUtils.Value.GetAllNodes(Allocator.Temp);
      foreach (var node in nodes)
      {
        // Проверяем, что нода имеет нужный компонент
        if (!entityRepository.Value.HasComponent<T>(node))
          continue;

        var nodePos = transformUtils.Value.GetPosition(node);
        var distance = math.lengthsq(nodePos - entityPos); // Используем lengthsq для оптимизации

        if (distance < minDistance)
        {
          minDistance = distance;
          closestNode = node;
        }
      }

      return closestNode;
    }

    private float GetDistance(int fromNode, int toNode)
    {
      var fromPos = transformUtils.Value.GetPosition(fromNode);
      var toPos = transformUtils.Value.GetPosition(toNode);
      return math.length(toPos - fromPos);
    }

    private NativeArray<int> ReconstructPath(int startNode, int endNode, NativeHashMap<int, int> cameFrom)
    {
      using var path = new NativeList<int>(16, Allocator.Temp);
      var current = endNode;

      while (current != startNode)
      {
        path.Add(current);
        current = cameFrom[current];
      }
      path.Add(startNode);

      // Создаем результирующий массив и копируем в него путь в обратном порядке
      var result = new NativeArray<int>(path.Length, Allocator.Persistent);
      for (int i = 0; i < path.Length; i++)
      {
        result[i] = path[path.Length - 1 - i];
      }
      return result;
    }

    private readonly struct PathNode
    {
      public readonly int node;
      public readonly float cost;

      public PathNode(int node, float cost)
      {
        this.node = node;
        this.cost = cost;
      }
    }
  }
}