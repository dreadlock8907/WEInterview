using Unity.Collections;
using WE.Core.Util.Components;

namespace WE.Core.Train.Component
{
  public struct TrainMovementComponent : IDisposableComponent
  {
    public int currentNode;
    public int nextNode;
    public float progress;
    public NativeArray<int> route;
    public int routeIndex;

    public void Dispose()
    {
      if(route.IsCreated)
        route.Dispose();
    }
  }
}