using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using WE.Core.Player.Component;
using WE.Core.Util;

namespace WE.Core.Player.System
{
  public class PlayerInitSystem : IEcsInitSystem
  {
    private readonly EcsCustomInject<EntityRepositorySystem> entityRepository = default;
    private readonly EcsCustomInject<PlayerUtilsSystem> playerUtils = default;

    public void Init(IEcsSystems systems)
    {
      var playerEntity = entityRepository.Value.CreateNewEntity();
      playerUtils.Value.Setup(playerEntity);
    }
  }
} 