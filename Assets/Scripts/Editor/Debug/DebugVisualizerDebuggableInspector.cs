using UnityEditor;
using WE.Debug.Debugger;

namespace Mart.Server.Ecs.Editor.Inspectors
{
  [CustomEditor(typeof(DebuggerComponent))]
  public sealed class DebugVisualizerDebuggableInspector : UnityEditor.Editor
  {
    public override void OnInspectorGUI()
    {
      var debugger = (DebuggerComponent)target;
      debugger.OnInspectorGUI();
      EditorUtility.SetDirty(target);
    }
  }
}
