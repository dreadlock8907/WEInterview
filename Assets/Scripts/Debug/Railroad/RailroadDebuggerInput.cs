using UnityEngine;
using UnityEditor;
using WE.Core.Extensions;
using Unity.Mathematics;

namespace WE.Debug.Railroad
{
  public static class RailroadDebuggerInput
  {
    public class Create
    {
      public float3 Position = float3.zero;
    }

    public class Edit
    {
      public int SelectedNodeEntity = -1;
      public int LinkTargetEntity = -1;
    }
  }
}