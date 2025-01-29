using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using WE.Core.Mine.Component;
using WE.Core.Extensions;
using WE.Core.Util;
using Unity.Mathematics;

namespace WE.Core.Mine.System
{
  public class MineUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<MineComponent> minePool = default;
    private readonly EcsCustomInject<DestroySystem> destroySystem = default;
    
    public void Setup(int entity, float loadingMultiplier)
    {
      ref var mineComponent = ref minePool.Value.GetOrCreate(entity);
      mineComponent.loadingMultiplier = loadingMultiplier;
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

    public float GetLoadingMultiplier(int entity)
    {
      if (!minePool.Value.Has(entity))
        return 0;
      return math.abs(minePool.Value.Get(entity).loadingMultiplier);
    }
  }
}