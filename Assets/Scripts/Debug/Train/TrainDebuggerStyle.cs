using UnityEngine;

namespace WE.Debug.Train
{
  public static class TrainDebuggerStyle
  {
    public static class UI
    {
      public static readonly Color SelectedBackground = new Color(0.7f, 0.2f, 0.2f);
    }

    public static class Train
    {
      public static readonly Color DefaultColor = Color.gray;
      public static readonly Color SelectedColor = new Color(0.8f, 0.2f, 0.2f);
      public static readonly Vector3 Size = Vector3.one * 1f; // tweaks by multiplier
      public static readonly Color LabelColor = Color.white;
      public static readonly Vector3 LabelOffset = Vector3.up * 0.5f; // tweaks by multiplier
      public static readonly int LabelSize = 12;
      public static readonly TextAnchor LabelAlignment = TextAnchor.MiddleCenter;
    }
  }
}