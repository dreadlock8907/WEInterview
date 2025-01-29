using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Unity.Mathematics;
using WE.Core.Base.Component;
using WE.Core.Extensions;
using WE.Core.Util;

namespace WE.Core.Base.System
{
  public class BaseUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<BaseComponent> basePool = default;
    private readonly EcsCustomInject<DestroySystem> destroySystem = default;
    
    public void Setup(int entity, float resourceMultiplier)
    {
      ref var baseComponent = ref basePool.Value.GetOrCreate(entity);
      baseComponent.resourceMultiplier = resourceMultiplier;
    }

    public bool IsBase(int entity)
    {
      return !destroySystem.Value.IsOnDestroy(entity) && basePool.Value.Has(entity);
    }

    public void RemoveBase(int entity)
    {
      if (basePool.Value.Has(entity))
        basePool.Value.Del(entity);
    }

    public float GetResourceMultiplier(int entity)
    {
      if (!basePool.Value.Has(entity))
        return 0;
      return math.abs(basePool.Value.Get(entity).resourceMultiplier);
    }
  }
}