using System;
using System.Collections.Generic;

namespace Assets.Scripts.MapGenerator.MapGeneratorCore
{
	//Меш, которых хранит любые треугольники
	public class MyMesh
	{
		public static bool bOptimize; //Объединяет вершины у которых одинаковая позиция и цвет
		public static bool bOptimizeIgnoreColor; //Объединяет вершины у которых одинаковая позиция, даже если цвет разный

		public int pcount; //количество вершин
		public float[] vertices; //координаты вершин
		public float[] colors; //цвет вершин

		public int index; //колличество треугольников
		public int[] indices; //номера вершин для треугольников

		public MyMesh(int pc, int tc)
		{
			vertices = new float[pc * 3];
			colors = new float[pc * 3];

			indices = new int[tc * 6];
		}

		//Создает новый меш, в котором объеденены одинаковые точки
		public MyMesh Optimize()
		{
			if (!bOptimize)
				return this;

			int pointn = 0;
			var points = new Dictionary<string, int>();

			var newMesh = new MyMesh(pcount, index);

			var newindices = new int[pcount];

			for (int i = 0; i < pcount; i++) //пробегаем по всем точкам
			{
				var x = (int)Math.Round(vertices[i * 3 + 0] / Voxels.VoxelSize);
				var y = (int)Math.Round(vertices[i * 3 + 1] / Voxels.VoxelSize);
				var z = (int)Math.Round(vertices[i * 3 + 2] / Voxels.VoxelSize);

				int r, g, b;

				if (bOptimizeIgnoreColor)
				{
					r = g = b = 0;
				}
				else
				{
					r = (int)Math.Round(colors[i * 3 + 0] * 255);
					g = (int)Math.Round(colors[i * 3 + 1] * 255);
					b = (int)Math.Round(colors[i * 3 + 2] * 255);
				}

				int p;
				var key = string.Format("{0} {1} {2} {3} {4} {5}", x, y, z, r, g, b);
				if (points.ContainsKey(key))
				{
					p = points[key];
				}
				else
				{
					int vert = newMesh.pcount++;
					newMesh.vertices[vert * 3 + 0] = vertices[i * 3 + 0];
					newMesh.vertices[vert * 3 + 1] = vertices[i * 3 + 1];
					newMesh.vertices[vert * 3 + 2] = vertices[i * 3 + 2];
					newMesh.colors[vert * 3 + 0] = colors[i * 3 + 0];
					newMesh.colors[vert * 3 + 1] = colors[i * 3 + 1];
					newMesh.colors[vert * 3 + 2] = colors[i * 3 + 2];

					points[key] = p = pointn++;
				}

				newindices[i] = p;
			}

			newMesh.index = index;

			for (int i = 0; i < index * 6; i++) //пробегаем по всем индексам
				newMesh.indices[i] = newindices[indices[i]];

			return newMesh;
		}
	}
}
