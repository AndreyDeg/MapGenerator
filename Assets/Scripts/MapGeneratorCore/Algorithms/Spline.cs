using System;
using System.Collections.Generic;
using UtilsLib.Logic;

namespace Assets.Scripts.MapGenerator.MapGeneratorCore.Algorithms
{
	//Алгоритмы интерполяции
	public static class Spline
	{
		//Коэффицент многочлена
		private static float splineKoeff(int count, int i, float x)
		{
			float result = 1;
			for (int j = 0; j < count; j++)
				if (i != j)
					result *= (x - j) / (i - j);

			return result;
		}

		//Интерполяция листа чисел
		public static List<float> MakeSpline(List<float> points, int count)
		{
			int tmax = points.Count * count;
			var result = new List<float>();
			for (int t = 0; t <= tmax; t++)
			{
				var v = 0f;
				for (int i = 0; i < points.Count; i++)
					v += points[i] * splineKoeff(points.Count, i, (float)t / tmax * (points.Count - 1));

				result.Add(v);
			}

			return result;
		}

		//Интерполяция листа векторов
		public static List<MyVectorFloat> MakeSpline(List<MyVectorFloat> points, int count)
		{
			int tmax = points.Count * count;
			var result = new List<MyVectorFloat>();
			for (int t = 0; t <= tmax; t++)
			{
				var v = new MyVectorFloat();
				for (int i = 0; i < points.Count; i++)
					v += points[i] * splineKoeff(points.Count, i, (float)t / tmax * (points.Count - 1));

				result.Add(v);
			}

			return result;
		}

		//Двухмерная интерполяция матрицы чисел
		public static float[,] MakeSpline(float[,] points, int count)
		{
			var lehgth0 = points.GetLength(0);
			var lehgth1 = points.GetLength(1);

			int tmax0 = (lehgth0 - 1) * count + 1;
			int tmax1 = (lehgth1 - 1) * count + 1;
			var result = new float[tmax0, tmax1];
			for (int t0 = 0; t0 < tmax0; t0++)
				for (int t1 = 0; t1 < tmax1; t1++)
				{
					float v = 0;
					for (int i = 0; i < lehgth0; i++)

						for (int j = 0; j < lehgth1; j++)
						{
							v += points[i, j]
								* splineKoeff(lehgth0, i, (float)t0 / (tmax0 - 1) * (lehgth0 - 1))
								* splineKoeff(lehgth1, j, (float)t1 / (tmax1 - 1) * (lehgth1 - 1));
						}

					result[t0, t1] = v;
				}

			return result;
		}

		//Квадрат расстояние от точки (x0,y0) до точки (x1,y1)
		public static float PointToPointSquadDistance(float x0, float x1, float y0, float y1)
		{
			return (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0);
		}

		//Расстояние от точки (x,y) до линии (x0,x1,y0,y1)
		public static float PointToLineDistance(float x0, float x1, float y0, float y1, float x, float y)
		{
			var d = (float)Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0)); //длинны вектора
			var s = Math.Abs((y0 - y1) * x + (x1 - x0) * y + (x0 * y1 - x1 * y0)); //математическая магия
			return s / d;
		}

		//Расстояние от точки (x,y) до линии (x0,x1,y0,y1) со знаком (без модуля)
		public static float PointToLineDistanceSign(float x0, float x1, float y0, float y1, float x, float y)
		{
			var d = (float)Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0)); //длинны вектора
			var s = (y0 - y1) * x + (x1 - x0) * y + (x0 * y1 - x1 * y0); //математическая магия
			return s / d;
		}

		//Проеция точки (x,y) на линию (x0,x1,y0,y1)
		public static float PointToLineProjection(float x0, float x1, float y0, float y1, float x, float y)
		{
			var d = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0); //квадрат длинны вектора
			var s = (x1 - x0) * (x - x0) + (y1 - y0) * (y - y0); //скалярное произведение
			return s / d;
		}
	}
}
