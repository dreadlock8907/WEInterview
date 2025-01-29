using Leopotam.EcsLite;
using UnityEngine;
using WE.Core.Util.Components;

namespace WE.Core.Util
{
  public sealed class DisposeSystem<T> : IEcsRunSystem, IEcsDestroySystem
    where T : struct, IDisposableComponent
  {

    public DisposeSystem()
    {
    }

    public void Run(IEcsSystems systems)
    {
      DisposeAll(systems, force: false);
    }

    private void DisposeAll(IEcsSystems systems, bool force)
    {
      var world = systems.GetWorld();
      var filterMask = world.Filter<T>();
      if (!force)
        filterMask = filterMask.Inc<DestroyComponent>();
      var filter = filterMask.End();

      var componentPool = world.GetPool<T>();

      foreach (var entity in filter)
        componentPool.Get(entity).Dispose();
    }

    public void Destroy(IEcsSystems systems)
    {
      Debug.Log($"Disposing {typeof(T).FullName}");

      DisposeAll(systems, force: true);
    }
  }
}