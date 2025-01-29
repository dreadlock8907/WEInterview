using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using System.Collections.Generic;
using UnityEngine.Assertions;
using WE.Core.Util.Components;

namespace WE.Core.Util
{
  public class EntityRepositorySystem : IEcsRunSystem
  {
    private readonly HashSet<int> aliveEntities = new(1024);

    private readonly EcsCustomInject<IEcsSystems> ecsSystems;

    public void Run(IEcsSystems ecsSystems)
    {
      var world = ecsSystems.GetWorld();
      var filter = world.Filter<DestroyComponent>().End();

      foreach(var entity in filter)
        OnEntityDestroyed(entity);
    }

    public bool IsEntityAlive(int entity)
    {
      return aliveEntities.Contains(entity);
    }

    public int CreateNewEntity()
    {
      var world = ecsSystems.Value.GetWorld();
      var entity = world.NewEntity();
      world.GetPool<NewComponent>().Add(entity);

      Assert.IsFalse(aliveEntities.Contains(entity), $"Entity {entity} alreafy present in aliveEntities");
      aliveEntities.Add(entity);

      return entity;
    }

    public void OnEntityDestroyed(int entity)
    {
      Assert.IsTrue(aliveEntities.Contains(entity), $"Entity {entity} not found in aliveEntities");
      aliveEntities.Remove(entity);
    }
  }
}
