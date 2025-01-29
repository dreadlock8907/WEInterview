using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using WE.Core.Cargo.System;
using WE.Core.Mine.Component;
using WE.Core.Time;
using WE.Core.Util.Components;

namespace WE.Core.Mine.System
{
  public class MineProcessSystem : IEcsRunSystem
  {
    private readonly EcsFilterInject<Inc<MiningComponent>, Exc<DestroyComponent>> miningFilter = default;
    private readonly EcsPoolInject<MiningComponent> miningPool = default;
    private readonly EcsCustomInject<MineUtilsSystem> mineUtils = default;
    private readonly EcsCustomInject<TimeService> timeService = default;
    private readonly EcsCustomInject<CargoUtilsSystem> cargoUtils = default;
    
    public void Run(IEcsSystems systems)
    {
      ProcessMining();
    }

    private void ProcessMining()
    {
      foreach (var entity in miningFilter.Value)
      {
        if (cargoUtils.Value.IsFull(entity))
        {
          mineUtils.Value.StopMine(entity);
          continue;
        }

        ref var mining = ref miningPool.Value.Get(entity);
        mining.progress += mining.miningSpeed * timeService.Value.tickTime;
        
        if (mining.progress >= 1f)
        {
          cargoUtils.Value.Load(entity, 1);
          mining.progress = 0f;
        }
      }
    }
  }
}