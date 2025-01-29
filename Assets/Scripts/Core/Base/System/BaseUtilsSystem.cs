using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Unity.Mathematics;

using WE.Core.Base.Component;
using WE.Core.Cargo.System;
using WE.Core.Extensions;
using WE.Core.Util;

namespace WE.Core.Base.System
{
  public class BaseUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<BaseComponent> basePool = default;
    private readonly EcsCustomInject<DestroySystem> destroySystem = default;
    private readonly EcsCustomInject<CargoUtilsSystem> cargoUtils = default;

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

    public void Unload(int entity, int baseEntity)
    {
      if (!IsBase(baseEntity))
        return;
    
      var currentResource = cargoUtils.Value.GetResource(entity);
      cargoUtils.Value.Unload(entity, currentResource);

      ref var baseComponent = ref basePool.Value.Get(baseEntity);
      var score = currentResource * baseComponent.resourceMultiplier;

      UnityEngine.Debug.Log($"Unload {currentResource} from {entity} to {baseEntity} with score {score}. TODO: Add score to player");
    }
  }
}