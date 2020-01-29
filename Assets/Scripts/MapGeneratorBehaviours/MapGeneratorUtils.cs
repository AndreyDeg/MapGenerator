using System.Collections.Generic;
using UnityEngine;
using UtilsLib.Logic;

namespace Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Utils
{
	//Содержит вспомогательные функции
	public static class MapGeneratorUtils
	{
		public static List<T> GetComponents<T>(GameObject owner, bool recursive = false, bool includeInactive = true)
		{
			var result = new List<T>();

			if (recursive)
				result.AddRange(owner.GetComponentsInChildren<T>(includeInactive));
			else
				result.AddRange(owner.GetComponents<T>());

			return result;
		}

		public static MyColor ToMyColor(this Color unityColor)
		{
			return MyColor.FromArgb((int)(unityColor.r * 255), (int)(unityColor.g * 255), (int)(unityColor.b * 255), (int)(unityColor.a * 255));
		}

		public static List<MyColor> ToListOfMyColor(this Gradient gradient, int count = 255)
		{
			var colors = new List<MyColor>();
			for (float y = 0; y < count; y++)
			{
				var color = gradient.Evaluate(y / count);
				colors.Add(color.ToMyColor());
			}

			return colors;
		}

		public static MyVectorFloat ToMyVectorFloat(this Vector3 v)
		{
			return new MyVectorFloat(v.x, v.y, v.z);
		}
	}
}
