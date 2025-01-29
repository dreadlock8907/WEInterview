using UnityEngine;
using Unity.Mathematics;

namespace WE.Core.Extensions
{
  public static class MathExtensions
  {
    public static float3 ToFloat3(this Vector3 vector)
    {
      return new float3(vector.x, vector.y, vector.z);
    }

    public static Vector3 ToVector3(this float3 vector)
    {
      return new Vector3(vector.x, vector.y, vector.z);
    }
  }
}
