using System;
using UnityEngine;
namespace SkyDome.Utilities
{

    public class MathUtility
	{
		public static float CalculateDistance(float x1, float y1, float x2, float y2)
		{
			float num = x1 - x2;
			float num2 = y1 - y2;
			return Mathf.Sqrt(num * num + num2 * num2);
		}
		public static float CalculateHorizontalSpeed(Vector3 velocity)
		{
			float x = velocity.x;
			float y = velocity.y;
			return Mathf.Sqrt(x * x + y * y);
		}
	}
}