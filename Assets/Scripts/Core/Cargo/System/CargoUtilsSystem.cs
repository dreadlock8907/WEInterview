using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using Unity.Mathematics;

using WE.Core.Cargo.Component;
using WE.Core.Extensions;
using WE.Core.Util;

namespace WE.Core.Cargo.System
{
  public class CargoUtilsSystem : IEcsSystem
  {
    private readonly EcsPoolInject<CargoComponent> cargoPool = default;
    private readonly EcsCustomInject<DestroySystem> destroySystem = default;

    public void Setup(int entity, int maxResource, float loadingSpeed)
    {
      ref var cargo = ref cargoPool.Value.GetOrCreate(entity);
      cargo.maxResource = maxResource;
      cargo.loadingSpeed = loadingSpeed;
      cargo.resource = 0;
    }

    public void Load(int entity, int resource)
    {
      if (!IsCargo(entity))
        return;

      var maxAmount = cargoPool.Value.Get(entity).maxResource;
      var newAmount = cargoPool.Value.Get(entity).resource + resource;
      cargoPool.Value.Get(entity).resource = math.min(newAmount, maxAmount);
    }

    public void Unload(int entity, int resource)
    {
      if (!IsCargo(entity))
        return;

      var newAmount = cargoPool.Value.Get(entity).resource - resource;
      cargoPool.Value.Get(entity).resource = math.max(newAmount, 0);
    }

    public int GetResource(int entity)
    {
      if (!IsCargo(entity))
        return 0;
      return cargoPool.Value.Get(entity).resource;
    }

    public bool IsFull(int entity)
    {
      if (!IsCargo(entity))
        return false;
      return cargoPool.Value.Get(entity).resource == cargoPool.Value.Get(entity).maxResource;
    }

    public bool IsEmpty(int entity)
    {
      if (!IsCargo(entity))
        return false;
      return cargoPool.Value.Get(entity).resource == 0;
    }

    private bool IsCargo(int entity)
    {
      return !destroySystem.Value.IsOnDestroy(entity) && cargoPool.Value.Has(entity);
    }

    public float GetLoadingSpeed(int entity)
    {
      if (!IsCargo(entity))
        return 0f;
      return cargoPool.Value.Get(entity).loadingSpeed;
    }
  }
}