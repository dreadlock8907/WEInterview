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
        ProcessMovement(entity, ref movement);
        ProcessUpdatePosition(entity, ref movement);
        ProcessStopMovement(entity, ref movement);
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
      if (movement.progress >= 1f && movement.currentNode == movement.nextNode)
        trainUtilsSystem.Value.Stop(entity);
    }

    private void ProcessMovement(int entity, ref TrainMovementComponent movement)
    {
      var currentPos = transformUtilsSystem.Value.GetPosition(movement.currentNode);
      var nextPos = transformUtilsSystem.Value.GetPosition(movement.nextNode);
      var distance = GetDistanceBetweenNodes(currentPos, nextPos);

      if (distance <= float.Epsilon)
        return;

      var maxSpeed = trainUtilsSystem.Value.GetMaxSpeed(entity);
      var progressDelta = maxSpeed * timeService.Value.tickTime / distance;
      movement.progress = math.clamp(movement.progress + progressDelta, 0f, 1f);

      if (movement.progress >= 1f)
      {
        var newRouteIndex = math.min(movement.routeIndex + 1, movement.route.Length - 1);
        var nextRouteIndex = math.min(newRouteIndex + 1, movement.route.Length - 1);
        movement.routeIndex = newRouteIndex;
        movement.currentNode = movement.nextNode;
        movement.nextNode = movement.route[nextRouteIndex];
        movement.progress = movement.currentNode == movement.nextNode ? 1f : 0f;

        ref var trainBind = ref trainBindPool.Value.Get(entity);
        trainBind.currentNode = movement.currentNode;
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