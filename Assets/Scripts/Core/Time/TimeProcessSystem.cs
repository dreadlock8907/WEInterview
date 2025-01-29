using System;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;


namespace WE.Core.Time
{
  public sealed class TimeProcessSystem : IEcsRunSystem
  {
    private readonly EcsCustomInject<TimeService> timeService = default;

    public void Run(IEcsSystems systems)
    {
      timeService.Value.tickTime = UnityEngine.Time.unscaledDeltaTime;
      timeService.Value.timeSinceServerStart = UnityEngine.Time.unscaledTime;
      timeService.Value.unixTimeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
  }
}
