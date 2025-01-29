using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using WE.Core.Mine.Component;
using WE.Core.Extensions;
using WE.Core.Util;
using Unity.Mathematics;
using WE.Core.Cargo.System;

namespace WE.Core.Mine.System
{
  public class MineUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<MineComponent> minePool = default;
    private readonly EcsPoolInject<MiningComponent> miningPool = default;
    private readonly EcsCustomInject<DestroySystem> destroySystem = default;
    private readonly EcsCustomInject<CargoUtilsSystem> cargoUtils = default;
    public void Setup(int entity, float loadingMultiplier)
    {
      ref var mineComponent = ref minePool.Value.GetOrCreate(entity);
      mineComponent.miningMulitiplier = loadingMultiplier;
    }

    public void Mine(int cargoEntity, int mineEntity)
    {
      if (!IsMine(mineEntity))
        return;

      ref var mine = ref minePool.Value.Get(mineEntity);
      ref var mining = ref miningPool.Value.GetOrCreate(cargoEntity);
      mining.miningSpeed = cargoUtils.Value.GetLoadingSpeed(cargoEntity) * mine.miningMulitiplier;
      mining.progress = 0f;
    }

    public void StopMine(int entity)
    {
      if (!IsMining(entity))
        return;

      miningPool.Value.Del(entity);
    }

    public float GetMiningProgress(int entity)
    {
      if (!IsMining(entity))
        return 0f;
      return miningPool.Value.Get(entity).progress;
    }

    public bool IsMining(int entity)
    {
      return !destroySystem.Value.IsOnDestroy(entity) && miningPool.Value.Has(entity);
    }

    public bool IsMine(int entity)
    {
      return !destroySystem.Value.IsOnDestroy(entity) && minePool.Value.Has(entity);
    }

    public void RemoveMine(int entity)
    {
      if (minePool.Value.Has(entity))
        minePool.Value.Del(entity);
    }

    public float GetMiningMultiplier(int entity)
    {
      if(!IsMine(entity))
        return 0;
      return math.abs(minePool.Value.Get(entity).miningMulitiplier);
    }
  }
}