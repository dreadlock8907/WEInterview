using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using WE.Core.Extensions;
using WE.Core.Score.Component;
using WE.Core.Util;

namespace WE.Core.Score.System
{
  public class ScoreVaultUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<ScoreVaultComponent> vaultPool = default;
    private readonly EcsCustomInject<DestroySystem> destroySystem = default;

    public void Setup(int entity)
    {
      vaultPool.Value.GetOrCreate(entity);
    }

    public bool IsValidVauld(int entity)
    {
      return !destroySystem.Value.IsOnDestroy(entity) && vaultPool.Value.Has(entity);
    }

    public void AddScore(int entity, float income)
    {
      if (!IsValidVauld(entity))
        return;

      ref var vault = ref vaultPool.Value.Get(entity);
      vault.totalScore += income;
    }

    public void ResetScore(int entity)
    {
      if (!IsValidVauld(entity))
        return;

      ref var vault = ref vaultPool.Value.Get(entity);
      vault.totalScore = 0;
    }

    public float GetTotalScore(int entity)
    {
      if (!IsValidVauld(entity))
        return 0;

      return vaultPool.Value.Get(entity).totalScore;
    }
  }
} 