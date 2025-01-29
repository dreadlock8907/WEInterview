using UnityEditor;

namespace WE.Debug.Debugger
{
  public interface IDebugger
  {
    string Name { get; }
    void DebugOnScene(SceneView sceneView);
    void DebugOnGUI();
    void DebugOnGizmos();
  }
}