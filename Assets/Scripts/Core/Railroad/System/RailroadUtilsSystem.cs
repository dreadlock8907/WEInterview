using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using WE.Core.Railroad.Component;
using WE.Core.Transform.Component;
using WE.Core.Transform.System;
using WE.Core.Util;
using Unity.Collections;
using WE.Core.Util.Components;
using WE.Core.Extensions;

namespace WE.Core.Railroad.System
{
  public class RailroadUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<NodeComponent> nodePool = default;
    private readonly EcsFilterInject<Inc<NodeComponent>, Exc<DestroyComponent>> nodeFilter = default;

    private readonly EcsCustomInject<DestroySystem> destroySystem = default;
    private readonly EcsCustomInject<EntityRepositorySystem> entityRepository = default;
    private readonly EcsCustomInject<TransformUtilsSystem> transformUtilsSystem = default;

    public int CreateNode(float3 position)
    {
      var nodeEntity = entityRepository.Value.CreateNewEntity();
      transformUtilsSystem.Value.UpdatePosition(nodeEntity, position);
      nodePool.Value.GetOrCreate(nodeEntity);

      return nodeEntity;
    }

    public void DestroyNode(int entity)
    {
      if(!IsValidNode(entity))
        return;

      ref var nodeComponent = ref nodePool.Value.Get(entity);
      foreach(var connectedNodeEntity in nodeComponent.ReadConnectedNodes())
      {
        Assert.IsTrue(connectedNodeEntity >= 0, "Connected node entity is invalid");
        ref var connectedNode = ref nodePool.Value.Get(connectedNodeEntity);
        connectedNode.RemoveConnectedNode(entity);
      }
      destroySystem.Value.DestroyEntity(entity);
    }

    public bool IsValidNode(int entity)
    {
      if(destroySystem.Value.IsOnDestroy(entity))
        return false;

      return nodePool.Value.Has(entity);
    }

    public void LinkNodes(int firstNodeEntity, int secondNodeEntity)
    {
      if (!IsValidNode(firstNodeEntity) || !IsValidNode(secondNodeEntity))
      {
        Debug.LogError("Cannot link nodes: one or both entities don't have NodeComponent");
        return;
      }

      ref var firstNode = ref nodePool.Value.Get(firstNodeEntity);
      ref var secondNode = ref nodePool.Value.Get(secondNodeEntity);

      firstNode.AddUniqueConnectedNode(secondNodeEntity);
      secondNode.AddUniqueConnectedNode(firstNodeEntity);
    }

    public void UnlinkNodes(int firstNodeEntity, int secondNodeEntity)
    {
      ref var firstNode = ref nodePool.Value.Get(firstNodeEntity);
      ref var secondNode = ref nodePool.Value.Get(secondNodeEntity);

      firstNode.RemoveConnectedNode(secondNodeEntity);
      secondNode.RemoveConnectedNode(firstNodeEntity);
    }

    public NativeArray<int> GetAllNodes(Allocator allocator)
    {
      var count = nodeFilter.Value.GetEntitiesCount();
      var result = new NativeArray<int>(count, allocator);
      
      var idx = 0;
      foreach (var entity in nodeFilter.Value)
        result[idx++] = entity;
      
      return result;
    }

    public NativeArray<int>.ReadOnly GetNodeConnections(int nodeEntity)
    {
      if (!IsValidNode(nodeEntity))
        return new NativeArray<int>(0, Allocator.Temp).AsReadOnly();
      return nodePool.Value.Get(nodeEntity).ReadConnectedNodes();
    }

    public int GetNodeConnectionsCount(int nodeEntity)
    {
      if (!IsValidNode(nodeEntity))
        return 0;
      return nodePool.Value.Get(nodeEntity).ConnectionsCount;
    }


  }
}