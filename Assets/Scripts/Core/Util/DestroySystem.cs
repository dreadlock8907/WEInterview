using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using WE.Core.Extensions;
using WE.Core.Util.Components;

namespace WE.Core.Util
{
  public class DestroySystem : IEcsRunSystem
  {
    private readonly EcsPoolInject<DestroyComponent> destroyComponentPool;

    public void Run(IEcsSystems systems)
    {
      var world = systems.GetWorld();
      var filter = world.Filter<DestroyComponent>().End();

      foreach(var entity in filter)
        world.DelEntity(entity);
    }

    public bool IsOnDestroy(int entity)
    {
      return destroyComponentPool.Value.Has(entity);
    }

    public void DestroyEntity(int entity)
    {
      destroyComponentPool.Value.GetOrCreate(entity);
    }
  }
}
