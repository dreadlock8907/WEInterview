using UnityEditor;
using WE.Debug.Debugger;

namespace WE.Editor.Debug
{
  [CustomEditor(typeof(DebuggerComponent))]
  public sealed class DebuggerComponentInspector : UnityEditor.Editor
  {
    public override void OnInspectorGUI()
    {
      var debugger = (DebuggerComponent)target;
      debugger.OnInspectorGUI();
      EditorUtility.SetDirty(target);
    }
  }
}
