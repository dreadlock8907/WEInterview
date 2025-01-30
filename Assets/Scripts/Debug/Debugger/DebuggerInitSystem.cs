using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using WE.Core.Railroad.System;
using WE.Core.Transform.System;
using WE.Core.Mine.System;
using WE.Core.Base.System;

using WE.Debug.Railroad;
using WE.Core.Train.System;
using WE.Debug.Train;
using WE.Core.Cargo.System;
using WE.Debug.Stats;
using WE.Core.Score.System;
using WE.Core.Player.System;

namespace WE.Debug.Debugger
{
  public class DebuggerInitSystem : IEcsPreInitSystem, IEcsInitSystem
  {
    private IEcsSystems ecsSystems;
    private readonly EcsCustomInject<RailroadUtilsSystem> railroadUtils = default;
    private readonly EcsCustomInject<TransformUtilsSystem> transformUtils = default;
    private readonly EcsCustomInject<MineUtilsSystem> mineUtils = default;
    private readonly EcsCustomInject<BaseUtilsSystem> baseUtils = default;
    private readonly EcsCustomInject<TrainUtilsSystem> trainUtils = default;
    private readonly EcsCustomInject<CargoUtilsSystem> cargoUtils = default;
    private readonly EcsCustomInject<ScoreVaultUtilsSystem> scoreVaultUtils = default;
    private readonly EcsCustomInject<PlayerUtilsSystem> playerUtils = default;
    public DebuggerInitSystem()
    {
    }

    public void PreInit(IEcsSystems ecsSystems)
    {
      this.ecsSystems = ecsSystems;
    }

    private void CreateDebugVisualizingDebuggable(IDebugger debugger)
    {
      var debuggerRoot = new GameObject($"Debugger_{debugger.Name}");
      var debuggerComponent = debuggerRoot.AddComponent<DebuggerComponent>();
      debuggerComponent.Init(debugger);
    }

    public void Init(IEcsSystems systems)
    {
      CreateDebugVisualizingDebuggable(new RailroadDebugger(railroadUtils.Value, transformUtils.Value, mineUtils.Value, baseUtils.Value));
      CreateDebugVisualizingDebuggable(new TrainDebugger(trainUtils.Value, railroadUtils.Value, transformUtils.Value, cargoUtils.Value, mineUtils.Value));
      CreateDebugVisualizingDebuggable(new ScoreDebugger(scoreVaultUtils.Value, playerUtils.Value));
      // Add new debuggers down here..
    }
  }
}
