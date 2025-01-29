using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;
using WE.Core.Base.System;
using WE.Core.Cargo.System;
using WE.Core.Mine.System;
using WE.Core.Navigation.System;
using WE.Core.Railroad.Component;
using WE.Core.Railroad.System;
using WE.Core.Time;
using WE.Core.Train.Component;
using WE.Core.Train.System;
using WE.Core.Transform.Component;
using WE.Core.Transform.System;
using WE.Core.Util;
using WE.Core.Util.Components;

#if UNITY_EDITOR
using WE.Debug.Debugger;
#endif

namespace WE.App
{
  public sealed class EcsContainer
  {
    private EcsWorld world;
    private IEcsSystems systems;

    public EcsContainer()
    {
    }

    public void Init()
    {
      UnityEngine.Debug.Log("EcsContainer Init");

      world = new EcsWorld();
      systems = new EcsSystems(world);

      var timeInitSystem = new TimeInitSystem();
      var transformUtilsSystem = new TransformUtilsSystem();
      var entityRepositorySystem = new EntityRepositorySystem();
      var railroadUtilsSystem = new RailroadUtilsSystem();
      var navigationUtilsSystem = new NavigationUtilsSystem();
      var baseUtilsSystem = new BaseUtilsSystem();
      var mineUtilsSystem = new MineUtilsSystem();
      var cargoUtilsSystem = new CargoUtilsSystem();
      var trainUtilsSystem = new TrainUtilsSystem();
      var destroySystem = new DestroySystem();

      systems
        .Add(timeInitSystem)
        .Add(transformUtilsSystem)
        .Add(entityRepositorySystem)
        .Add(railroadUtilsSystem)
        .Add(navigationUtilsSystem)
        .Add(baseUtilsSystem)
        .Add(mineUtilsSystem)
        .Add(new MineProcessSystem())
        .Add(cargoUtilsSystem)
        .Add(trainUtilsSystem)
        .Add(new TrainMovementProcessSystem())

        // Time process system should be after all systems are updated
        .Add(new TimeProcessSystem())

        // Dispose systems down here
        .Add(new DisposeSystem<NodeComponent>())
        .Add(new DisposeSystem<TrainMovementComponent>())

        // And after all, destroy entities with DestroyComponent
        .Add(destroySystem);

#if UNITY_EDITOR
      if (UnityEngine.Application.isPlaying)
      {
        systems
          .Add(new DebuggerInitSystem());
      }
#endif

      systems
        .DelHere<TransformChangedComponent>(default)
        .DelHere<NewComponent>(default);

      systems.Inject(
        transformUtilsSystem,
        entityRepositorySystem,
        railroadUtilsSystem,
        navigationUtilsSystem,
        baseUtilsSystem,
        mineUtilsSystem,
        cargoUtilsSystem,
        trainUtilsSystem,
        destroySystem,
        systems);

      systems.Init();
    }

    public void Update()
    {
      systems?.Run();
    }

    public void Destroy()
    {
      UnityEngine.Debug.Log("Destroying EcsContainer");

      if (systems != null)
      {
        // list of custom worlds will be cleared
        // during IEcsSystems.Destroy(). so, you
        // need to save it here if you need.
        systems.Destroy();
        systems = null;
      }

      // cleanup custom worlds here.

      // cleanup default world.
      if (world != null)
      {
        world.Destroy();
        world = null;
      }
    }
  }
}
