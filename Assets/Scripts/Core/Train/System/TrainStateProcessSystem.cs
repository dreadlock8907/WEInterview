using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using WE.Core.Train.Component;
using WE.Core.Base.System;
using WE.Core.Mine.System;
using WE.Core.Cargo.System;
using WE.Core.Util.Components;
using WE.Core.Navigation.System;
using WE.Core.Base.Component;
using WE.Core.Mine.Component;
using WE.Core.Train.State;
using WE.Core.Extensions;

namespace WE.Core.Train.System
{
  public class TrainStateProcessSystem : IEcsInitSystem, IEcsRunSystem
  {
    private readonly EcsFilterInject<Inc<TrainStateComponent>, Exc<DestroyComponent>> trainStateFilter = default;
    private readonly EcsPoolInject<TrainStateComponent> trainStatePool = default;
    private readonly EcsCustomInject<TrainUtilsSystem> trainUtils = default;
    private readonly EcsCustomInject<BaseUtilsSystem> baseUtils = default;
    private readonly EcsCustomInject<MineUtilsSystem> mineUtils = default;
    private readonly EcsCustomInject<CargoUtilsSystem> cargoUtils = default;
    private readonly EcsCustomInject<NavigationUtilsSystem> navigationUtils = default;

    private ITrainState[] states;

    public void Init(IEcsSystems systems)
    {
      var context = new TrainStateContext(
        trainUtils.Value,
        baseUtils.Value,
        mineUtils.Value,
        cargoUtils.Value,
        navigationUtils.Value
      );

      states = new ITrainState[4];
      states[(int)TrainState.Idle] = new IdleState(context);
      states[(int)TrainState.Moving] = new MovingState(context);
      states[(int)TrainState.Loading] = new LoadingState(context);
      states[(int)TrainState.Unloading] = new UnloadingState(context);
    }

    public void Run(IEcsSystems systems)
    {
      foreach (var entity in trainStateFilter.Value)
      {
        ref var trainState = ref trainStatePool.Value.Get(entity);
        states[(int)trainState.state].Process(entity, ref trainState);
      }
    }
  }
}
