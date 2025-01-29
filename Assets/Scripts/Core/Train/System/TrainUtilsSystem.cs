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

namespace WE.Core.Train.System
{
  public class TrainUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<TrainComponent> trainPool = default;
    private readonly EcsPoolInject<TrainBindComponent> trainBindPool = default;
    private readonly EcsPoolInject<TrainStateComponent> trainStatePool = default;
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
      train.loadingSpeed = loadingSpeed;
      train.maxResource = maxResource;

      ref var trainState = ref trainStatePool.Value.Add(entity);
      trainState.state = TrainState.Idle;

      ref var trainBind = ref trainBindPool.Value.Add(entity);
      trainBind.currentNode = startNode;

      transformUtils.Value.UpdatePosition(entity, transformUtils.Value.GetPosition(startNode));
      cargoUtils.Value.Setup(entity, maxResource, train.loadingSpeed);

      return entity;
    }

    public void Move(int trainEntity, NativeArray<int> route)
    {
      if (!IsTrain(trainEntity))
        return;

      if (IsMoving(trainEntity))
        Stop(trainEntity);

      ref var movement = ref movementPool.Value.GetOrCreate(trainEntity);
      movement.route.CopyFrom(route);
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
  }
}
