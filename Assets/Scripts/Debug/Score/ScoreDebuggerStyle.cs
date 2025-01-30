using UnityEditor;
using UnityEngine;

namespace WE.Debug.Score
{
  public static class ScoreDebuggerStyle
  {
    public static class Panel
    {
      public static readonly Vector2 Size = new(200, 50);
      public static readonly float CenterX = Size.x / 2;
      public static readonly float TopOffset = 20f;
      public static readonly Color BackgroundColor = new(0.2f, 0.2f, 0.2f, 0.8f);
    }

    public static class Text
    {
      public static readonly Color Color = Color.white;
      public static readonly int FontSize = 14;
      public static readonly TextAnchor Alignment = TextAnchor.MiddleCenter;
      public static readonly FontStyle Style = FontStyle.Bold;

      public static readonly GUIStyle LabelStyle = new GUIStyle(EditorStyles.label)
      {
        normal = { textColor = Color },
        fontSize = FontSize,
        alignment = Alignment,
        fontStyle = Style
      };
    }
  }
} 