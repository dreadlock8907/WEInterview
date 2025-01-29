using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Unity.Mathematics;
using WE.Core.Extensions;
using WE.Core.Transform.Component;

namespace WE.Core.Transform.System
{
  public class TransformUtilsSystem : IEcsSystem
  {
    private EcsPoolInject<PositionComponent> positionComponentPool;
    private EcsPoolInject<TransformChangedComponent> transformChangedComponentPool;

    public void UpdatePosition(int entity, float3 position)
    {
      ref var positionComponent = ref positionComponentPool.Value.GetOrCreate(entity);
      var positionChanged = !positionComponent.position.Equals(position);

      if(positionChanged)
      {
        positionComponent.position = position;
        transformChangedComponentPool.Value.GetOrCreate(entity);
      }
    }

    public float3 GetPosition(int entity)
    {
      return positionComponentPool.Value.Get(entity).position;
    }

  }
}