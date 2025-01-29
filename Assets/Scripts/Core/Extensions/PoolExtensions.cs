using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace WE.Core.Extensions
{
  public static class PoolExtensions
  {
    public static ref ComponentType GetOrCreate<ComponentType>(this EcsPool<ComponentType> pool, int entity) where ComponentType : struct
    {
      if(!pool.Has(entity))
        pool.Add(entity);

      ref var res = ref pool.Get(entity);
      return ref res;
    }

    public static ref ComponentType GetOrCreate<ComponentType>(this EcsPoolInject<ComponentType> pool, int entity) where ComponentType : struct
    {
      return ref !pool.Value.Has(entity) ? ref pool.Value.Add(entity) : ref pool.Value.Get(entity);
    }
  }
}
