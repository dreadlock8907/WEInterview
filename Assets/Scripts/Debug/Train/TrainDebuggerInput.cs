using UnityEngine;

namespace WE.Debug.Train
{
  public static class TrainDebuggerInput
  {
    public class Create
    {
      public int SelectedNode { get; set; } = -1;
      public float MoveSpeed { get; set; } = 1f;
      public float LoadingTime { get; set; } = 1f;
      public int MaxResource { get; set; } = 1;
    }

    public class Edit
    {
      public int SelectedTrain { get; set; } = -1;
      public float MoveSpeed { get; set; } = 1f;
      public float LoadingTime { get; set; } = 1f;
      public int MaxResource { get; set; } = 1;
    }
  }
} 