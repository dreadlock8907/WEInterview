using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Unity.Mathematics;
using Unity.Collections;
using WE.Core.Train.Component;
using WE.Core.Transform.System;
using WE.Core.Util;
using WE.Core.Util.Components;
using WE.Core.Time;
using System;

namespace WE.Core.Train.System
{
  public class TrainMovementProcessSystem : IEcsRunSystem
  {
    private readonly EcsFilterInject<Inc<TrainMovementComponent>, Exc<DestroyComponent>> trainMovementFilter = default;
    private readonly EcsPoolInject<TrainMovementComponent> trainMovementPool = default;
    private readonly EcsPoolInject<TrainBindComponent> trainBindPool = default;
    private readonly EcsCustomInject<TrainUtilsSystem> trainUtilsSystem = default;
    private readonly EcsCustomInject<TransformUtilsSystem> transformUtilsSystem = default;
    private readonly EcsCustomInject<TimeService> timeService = default;

    public void Run(IEcsSystems systems)
    {
      foreach (var entity in trainMovementFilter.Value)
      {
        ref var movement = ref trainMovementPool.Value.Get(entity);
        ProcessStopMovement(entity, ref movement);
        ProcessMovement(entity, ref movement);
        ProcessUpdatePosition(entity, ref movement);
      }
    }

    private void ProcessUpdatePosition(int entity, ref TrainMovementComponent movement)
    {
      var currentPos = transformUtilsSystem.Value.GetPosition(movement.currentNode);
      var nextPos = transformUtilsSystem.Value.GetPosition(movement.nextNode);

      var pos = CalculateCurrentPosition(currentPos, nextPos, movement.progress);
      transformUtilsSystem.Value.UpdatePosition(entity, pos);
    }

    private void ProcessStopMovement(int entity, ref TrainMovementComponent movement)
    {
      if (movement.progress >= 1f || 
          movement.routeIndex >= movement.route.Length - 1 || 
          movement.currentNode == movement.nextNode)
      {
        trainUtilsSystem.Value.Stop(entity);
      }
    }

    private void ProcessMovement(int entity, ref TrainMovementComponent movement)
    {
      var currentPos = transformUtilsSystem.Value.GetPosition(movement.currentNode);
      var nextPos = transformUtilsSystem.Value.GetPosition(movement.nextNode);
      var distance = GetDistanceBetweenNodes(currentPos, nextPos);

      if (distance <= float.Epsilon)
        return;

      var moveSpeed = trainUtilsSystem.Value.GetMoveSpeed(entity);
      var normalizedSpeed = moveSpeed / distance;
      movement.progress += normalizedSpeed * timeService.Value.tickTime;

      if (movement.progress >= 1f)
      {
        movement.progress = 0f;
        movement.currentNode = movement.nextNode;
        movement.routeIndex = math.min(movement.routeIndex + 1, movement.route.Length - 1);
        movement.nextNode = movement.route[movement.routeIndex];

        ref var trainBind = ref trainBindPool.Value.Get(entity);
        trainBind.currentNode = movement.nextNode;
      }
    }

    private float GetDistanceBetweenNodes(float3 from, float3 to)
    {
      return math.length(to - from);
    }

    private float3 CalculateCurrentPosition(float3 from, float3 to, float progress)
    {
      return math.lerp(from, to, progress);
    }
  }
}