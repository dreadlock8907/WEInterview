using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Unity.Mathematics;

using WE.Core.Base.Component;
using WE.Core.Cargo.System;
using WE.Core.Extensions;
using WE.Core.Util;
using WE.Core.Player.System;
using WE.Core.Score.System;

namespace WE.Core.Base.System
{
  public class BaseUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<BaseComponent> basePool = default;
    private readonly EcsCustomInject<DestroySystem> destroySystem = default;
    private readonly EcsCustomInject<CargoUtilsSystem> cargoUtils = default;
    private readonly EcsCustomInject<PlayerUtilsSystem> playerUtils = default;
    private readonly EcsCustomInject<ScoreVaultUtilsSystem> scoreVaultUtils = default;

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

    public void Unload(int cargoEntity, int baseEntity)
    {
      if (!IsBase(baseEntity))
        return;

      var resources = cargoUtils.Value.GetCurrentResource(cargoEntity);
      cargoUtils.Value.Unload(cargoEntity, resources);

      ref var baseComponent = ref basePool.Value.Get(baseEntity);
      var score = resources * baseComponent.resourceMultiplier;

      var playerEntity = playerUtils.Value.GetPlayerEntity();
      scoreVaultUtils.Value.AddScore(playerEntity, score);
    }
  }
}