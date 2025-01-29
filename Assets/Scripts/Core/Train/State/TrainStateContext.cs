using WE.Core.Base.System;
using WE.Core.Cargo.System;
using WE.Core.Mine.System;
using WE.Core.Navigation.System;
using WE.Core.Train.System;

namespace WE.Core.Train.State
{
  public class TrainStateContext
  {
    public readonly TrainUtilsSystem TrainUtils;
    public readonly BaseUtilsSystem BaseUtils;
    public readonly MineUtilsSystem MineUtils;
    public readonly CargoUtilsSystem CargoUtils;
    public readonly NavigationUtilsSystem NavigationUtils;

    public TrainStateContext(
      TrainUtilsSystem trainUtils,
      BaseUtilsSystem baseUtils,
      MineUtilsSystem mineUtils,
      CargoUtilsSystem cargoUtils,
      NavigationUtilsSystem navigationUtils)
    {
      TrainUtils = trainUtils;
      BaseUtils = baseUtils;
      MineUtils = mineUtils;
      CargoUtils = cargoUtils;
      NavigationUtils = navigationUtils;
    }
  }
}