using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using WE.Core.Extensions;
using WE.Core.Player.Component;
using WE.Core.Score.Component;
using WE.Core.Score.System;
using WE.Core.Util;

namespace WE.Core.Player.System
{
  public class PlayerUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<PlayerComponent> playerPool = default;
    private readonly EcsCustomInject<DestroySystem> destroySystem = default;
    private readonly EcsCustomInject<ScoreVaultUtilsSystem> vaultUtils = default;

    private int playerEntity = -1;

    public void Setup(int entity)
    {
      playerEntity = entity;
      playerPool.Value.GetOrCreate(entity);
      vaultUtils.Value.Setup(entity);
    }

    public int GetPlayerEntity()
    {
      return playerEntity;
    }

    public bool IsPlayer(int entity)
    {
      return !destroySystem.Value.IsOnDestroy(entity) && playerPool.Value.Has(entity);
    }
  }
} 