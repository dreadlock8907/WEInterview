using UnityEditor;
using UnityEngine;

namespace WE.Debug.Debugger
{
  [ExecuteAlways]
  public class DebuggerComponent : MonoBehaviour
  {
    private IDebugger debugger;

    public void Init(IDebugger debugger)
    {
      this.debugger = debugger;
    }

    public void OnEnable()
    {
      SceneView.duringSceneGui += OnSceneView;
      SceneView.RepaintAll();
    }


    public void OnDisable()
    {
      SceneView.duringSceneGui -= OnSceneView;
      SceneView.RepaintAll();
    }

    private void OnSceneView(SceneView sceneView)
    {
      if (Event.current.type != EventType.Repaint)
        return;

      debugger.DebugOnScene(sceneView);
      SceneView.RepaintAll();
    }

    public void OnInspectorGUI()
    {
      debugger.DebugOnGUI();
    }

    public void OnDrawGizmos()
    {
      debugger.DebugOnGizmos();
    }
  }
}