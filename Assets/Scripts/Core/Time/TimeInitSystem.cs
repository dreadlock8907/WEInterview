using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


namespace WE.Core.Time
{
  public sealed class TimeInitSystem : IEcsInitSystem
  {
    private TimeService timeService;

    public void Init(IEcsSystems systems)
    {
      timeService = new TimeService();
      systems.Inject(timeService);
    }

  }
}
