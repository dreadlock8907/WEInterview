using UnityEngine;
using UnityEditor;
using WE.Debug.Debugger;
using WE.Core.Base.System;
using WE.Core.Score.System;
using WE.Core.Player.System;
using WE.Debug.Score;

namespace WE.Debug.Stats
{
  public class ScoreDebugger : IDebugger
  {
    public string Name => "Score";

    private readonly ScoreVaultUtilsSystem scoreVaultUtils;
    private readonly PlayerUtilsSystem playerUtils;

    public ScoreDebugger(ScoreVaultUtilsSystem scoreVaultUtils, PlayerUtilsSystem playerUtils)
    {
      this.scoreVaultUtils = scoreVaultUtils;
      this.playerUtils = playerUtils;
    }

    public void DebugOnScene(SceneView sceneView)
    {
      var camera = sceneView.camera;
      if (camera == null) return;
      
      var sceneViewCenterX = sceneView.position.width / 2;
      var panelPosX = sceneViewCenterX - ScoreDebuggerStyle.Panel.CenterX;
      
      Handles.BeginGUI();
      
      var rect = new Rect(panelPosX, ScoreDebuggerStyle.Panel.TopOffset, 
        ScoreDebuggerStyle.Panel.Size.x, ScoreDebuggerStyle.Panel.Size.y);
      EditorGUI.DrawRect(rect, ScoreDebuggerStyle.Panel.BackgroundColor);
      
      var style = new GUIStyle(EditorStyles.label)
      {
        normal = { textColor = ScoreDebuggerStyle.Text.Color },
        fontSize = ScoreDebuggerStyle.Text.FontSize,
        alignment = ScoreDebuggerStyle.Text.Alignment,
        fontStyle = ScoreDebuggerStyle.Text.Style
      };
      
      var playerEntity = playerUtils.GetPlayerEntity();
      var totalScore = scoreVaultUtils.GetTotalScore(playerEntity);
      var content = new GUIContent($"Total Score:\n{totalScore}");
      EditorGUI.LabelField(rect, content, style);
      
      Handles.EndGUI();
    }

    public void DebugOnGUI() { }
    public void DebugOnGizmos() { }
    public void OnDeselect() { }
  }
} 