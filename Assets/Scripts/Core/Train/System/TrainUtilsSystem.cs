using System;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Unity.Collections;
using WE.Core.Cargo.System;
using WE.Core.Extensions;
using WE.Core.Train.Component;
using WE.Core.Train.State;
using WE.Core.Transform.Component;
using WE.Core.Transform.System;
using WE.Core.Util;
using WE.Core.Util.Components;
using UnityEngine;

namespace WE.Core.Train.System
{
  public class TrainUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<TrainComponent> trainPool = default;
    private readonly EcsPoolInject<TrainStateComponent> trainStatePool = default;
    private readonly EcsFilterInject<Inc<TrainComponent>, Exc<DestroyComponent>> trainFilter = default;
    private readonly EcsPoolInject<TrainBindComponent> trainBindPool = default;
    private readonly EcsPoolInject<TrainMovementComponent> movementPool = default;
    private readonly EcsPoolInject<PositionComponent> positionPool = default;
    private readonly EcsCustomInject<DestroySystem> destroySystem = default;
    private readonly EcsCustomInject<EntityRepositorySystem> entityRepository = default;
    private readonly EcsCustomInject<TransformUtilsSystem> transformUtils = default;
    private readonly EcsCustomInject<CargoUtilsSystem> cargoUtils = default;

    public int CreateTrain(int startNode, int maxResource, float moveSpeed = 1f, float loadingSpeed = 1f)
    {
      var entity = entityRepository.Value.CreateNewEntity();

      ref var train = ref trainPool.Value.Add(entity);
      train.maxSpeed = moveSpeed;

      ref var trainState = ref trainStatePool.Value.Add(entity);
      trainState.state = TrainState.Idle;

      ref var trainBind = ref trainBindPool.Value.Add(entity);
      trainBind.currentNode = startNode;

      transformUtils.Value.UpdatePosition(entity, transformUtils.Value.GetPosition(startNode));
      cargoUtils.Value.Setup(entity, maxResource, loadingSpeed);

      SetState(entity, TrainState.Idle);

      return entity;  
    }

    public void SetState(int trainEntity, TrainState state)
    {
      ref var trainState = ref trainStatePool.Value.GetOrCreate(trainEntity);
      trainState.state = state;
    }

    public void Move(int trainEntity, NativeArray<int> route)
    {
      if (!IsTrain(trainEntity))
        return;

      if (route.Length < 2)
      {
        Debug.LogWarning($"Cannot move train {trainEntity}: route must contain at least 2 nodes");
        return;
      }

      if (IsMoving(trainEntity))
        Stop(trainEntity);

      ref var movement = ref movementPool.Value.GetOrCreate(trainEntity);
      
      if (movement.route.IsCreated)
        movement.route.Dispose();
      
      movement.route = new NativeArray<int>(route, Allocator.Persistent);
      movement.routeIndex = 0;
      movement.currentNode = route[0];
      movement.nextNode = route[1];
      movement.progress = 0f;
    }

    public void Stop(int trainEntity)
    {
      if (movementPool.Value.Has(trainEntity))
      {
        ref var movement = ref movementPool.Value.Get(trainEntity);
        if (movement.route.IsCreated)
          movement.route.Dispose();
        movementPool.Value.Del(trainEntity);
      }
    }

    public NativeArray<int> GetAllTrains(Allocator allocator)
    {
      var count = trainFilter.Value.GetEntitiesCount();
      var result = new NativeArray<int>(count, allocator);
      
      var idx = 0;
      foreach (var entity in trainFilter.Value)
        result[idx++] = entity;
      
      return result;
    }

    public bool IsTrain(int entity)
    {
      return !destroySystem.Value.IsOnDestroy(entity) && trainPool.Value.Has(entity);
    }

    public float GetMoveSpeed(int entity)
    {
      if (!IsTrain(entity))
        return 0f;
      return trainPool.Value.Get(entity).maxSpeed;
    }

    public bool IsMoving(int entity)
    {
      return IsTrain(entity) && movementPool.Value.Has(entity);
    }

    public int GetCurrentNode(int entity)
    {
      if (!IsTrain(entity))
        return -1;
      return trainBindPool.Value.Get(entity).currentNode;
    }

    public void DestroyTrain(int entity)
    {
      if (!IsTrain(entity))
        return;
        
      if (IsMoving(entity))
        Stop(entity);
        
      destroySystem.Value.DestroyEntity(entity);
    }

    public ref TrainComponent GetTrainComponent(int entity)
    {
      return ref trainPool.Value.Get(entity);
    }

    public TrainState GetTrainState(int entity)
    {
      if (!trainStatePool.Value.Has(entity))
        return TrainState.Idle;
      return trainStatePool.Value.Get(entity).state;
    }

    public void UpdateTrainParameters(int entity, int maxResource, float moveSpeed, float loadingSpeed)
    {
      if (!IsTrain(entity))
        return;
        
      ref var train = ref trainPool.Value.Get(entity);
      train.maxSpeed = moveSpeed;
      
      cargoUtils.Value.Setup(entity, maxResource, loadingSpeed);
    }

    public NativeArray<int>.ReadOnly GetTrainRoute(int entity)
    {
      if (!IsMoving(entity))
        return new NativeArray<int>(0, Allocator.Temp).AsReadOnly();
        
      ref var movement = ref movementPool.Value.Get(entity);
      return movement.route.AsReadOnly();
    }
  }
}
