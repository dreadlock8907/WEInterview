using UnityEngine;

namespace WE.App
{
  public sealed class EcsStartup : MonoBehaviour
  {
    private EcsContainer ecsContainer;

    private void Start()
    {
      Application.targetFrameRate = 30;
      Time.fixedDeltaTime = 1.0f / Application.targetFrameRate;

      ecsContainer = new EcsContainer();
      ecsContainer.Init();
    }

    private void Update()
    {
      ecsContainer?.Update();
    }

    private void OnDestroy()
    {
      ecsContainer?.Destroy();
      ecsContainer = null;
    }

    private void OnApplicationQuit()
    {
      OnDestroy();
    }
  }
}
