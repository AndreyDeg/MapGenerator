using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilsLib.Logic;

namespace Assets.Scripts.MapGenerator.MapGeneratorCore
{
	//Меш, который умеет хранить только квадраты
	public class MyQuadMesh
	{
		public static bool bOptimize2;
		public static bool bOptimize3;
		public static bool bOptimizeEd;

		public float offsetX; //Сдвиг по оси X
		public float offsetY; //Сдвиг по оси Y
		public float offsetZ; //Сдвиг по оси Z
		List<Quadrangle> quadranglesY1; //Список квадратов Y1
		List<Quadrangle> quadranglesX0; //Список квадратов X0
		List<Quadrangle> quadranglesX1; //Список квадратов X1
		List<Quadrangle> quadranglesZ0; //Список квадратов Z0
		List<Quadrangle> quadranglesZ1; //Список квадратов Z1

		public static Vector3 DirectionalLight = new Vector3(0.4f, -0.7f, 0.6f); //направление источника света
		public static float AmbientLightIntensity = 0.425f; //интенсивность окружающего света

		public MyQuadMesh()
		{
			quadranglesY1 = new List<Quadrangle>();
			quadranglesX0 = new List<Quadrangle>();
			quadranglesX1 = new List<Quadrangle>();
			quadranglesZ0 = new List<Quadrangle>();
			quadranglesZ1 = new List<Quadrangle>();
		}

		private enum QuadOrient { y1, x0, x1, z0, z1 };

		//Координаты векторов, четыре углы квадрата
		private readonly Dictionary<QuadOrient, float[]> orientVertices = new Dictionary<QuadOrient, float[]>
		{
			{QuadOrient.y1, new float[] {0, 1, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1}}, //Квадрат у которого y == 1
			{QuadOrient.x0, new float[] {0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 1}},
			{QuadOrient.x1, new float[] {1, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1}},
			{QuadOrient.z0, new float[] {0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 0}},
			{QuadOrient.z1, new float[] {0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1}}
		};

		private struct Quadrangle
		{
			public int x, y, z;
			public int dx, dy, dz;
			public MyColor[] color;

			public Quadrangle(int x, int y, int z, MyColor[] color)
			{
				this.x = x;
				this.y = y;
				this.z = z;
				this.color = color;
				dx = 1;
				dy = 1;
				dz = 1;
			}

			public bool SameColor(Quadrangle quad2)
			{
				for (int i = 0; i < 4; i++)
					if (!color[i].Same(quad2.color[i], 8))
						return false;

				return true;
			}
		}

		//Глобальное освещение
		private MyColor GlobalLight(MyColor color, float[] orient)
		{
			var ldirX = DirectionalLight.x;
			var ldirY = DirectionalLight.y;
			var ldirZ = DirectionalLight.z;

			var normX = -orient[0];
			var normY = -orient[1];
			var normZ = -orient[2];

			float d = Math.Max(AmbientLightIntensity, ldirX * normX + ldirY * normY + ldirZ * normZ);

			return color * d;
		}

		public void CreateY1(int x, int y, int z, MyColor[] color)
		{
			var newColor = new MyColor[color.Length];
			for (int i = 0; i < color.Length; i++)
				newColor[i] = GlobalLight(color[i], orientVertices[QuadOrient.y1]);

			quadranglesY1.Add(new Quadrangle(x, y, z, newColor));
		}

		public void CreateX0(int x, int y, int z, MyColor[] color)
		{
			var newColor = new MyColor[color.Length];
			for (int i = 0; i < color.Length; i++)
				newColor[i] = GlobalLight(color[i], orientVertices[QuadOrient.x0]);

			quadranglesX0.Add(new Quadrangle(x, y, z, newColor));
		}

		public void CreateX1(int x, int y, int z, MyColor[] color)
		{
			var newColor = new MyColor[color.Length];
			for (int i = 0; i < color.Length; i++)
				newColor[i] = GlobalLight(color[i], orientVertices[QuadOrient.x1]);

			quadranglesX1.Add(new Quadrangle(x, y, z, newColor));
		}

		public void CreateZ0(int x, int y, int z, MyColor[] color)
		{
			var newColor = new MyColor[color.Length];
			for (int i = 0; i < color.Length; i++)
				newColor[i] = GlobalLight(color[i], orientVertices[QuadOrient.z0]);

			quadranglesZ0.Add(new Quadrangle(x, y, z, newColor));
		}

		public void CreateZ1(int x, int y, int z, MyColor[] color)
		{
			var newColor = new MyColor[color.Length];
			for (int i = 0; i < color.Length; i++)
				newColor[i] = GlobalLight(color[i], orientVertices[QuadOrient.z1]);

			quadranglesZ1.Add(new Quadrangle(x, y, z, newColor));
		}

		//Добавляет квадраты из структуры
		public void AddStructure(List<MyQuadMesh> qmeshstructures, int x, int z, int size)
		{
			foreach (var quadMesh in qmeshstructures)
			{
				quadranglesY1.AddRange(quadMesh.quadranglesY1.Where(w => w.x / size == x && w.z / size == z));
				quadranglesX0.AddRange(quadMesh.quadranglesX0.Where(w => w.x / size == x && w.z / size == z));
				quadranglesX1.AddRange(quadMesh.quadranglesX1.Where(w => w.x / size == x && w.z / size == z));
				quadranglesZ0.AddRange(quadMesh.quadranglesZ0.Where(w => w.x / size == x && w.z / size == z));
				quadranglesZ1.AddRange(quadMesh.quadranglesZ1.Where(w => w.x / size == x && w.z / size == z));
			}
		}

		private IEnumerable<Quadrangle> QuadrangleOptimizeX(List<Quadrangle> quadrangles, int grid, int max)
		{
			quadrangles.Sort((a, b) => a.x - b.x);

			var quads = new Dictionary<int, List<Quadrangle>>();
			foreach (var quad in quadrangles)
			{
				int key = quad.y * 256 * 256 + quad.z;
				if (!quads.ContainsKey(key))
					quads[key] = new List<Quadrangle> { quad };
				else
				{
					bool find = false;
					var quadskey = quads[key];
					for (int i = quadskey.Count - 1; i < quadskey.Count; i++)
					{
						var quad2 = quadskey[i];
						if (quad.dy != quad2.dy || quad.dz != quad2.dz)
							continue;

						if (quad2.x + quad2.dx != quad.x)
							continue;

						if (quad2.dx + quad.dx > max)
							continue;

						if (quad2.x % grid > 0)
							continue;

						if (!quad.SameColor(quad2))
							continue;

						quad2.dx += quad.dx;
						quadskey[i] = quad2;
						find = true;
						break;
					}
					if (!find)
						quadskey.Add(quad);
				}
			}

			foreach (var quad in quads)
				foreach (var quadrangle in quad.Value)
					yield return quadrangle;
		}

		private IEnumerable<Quadrangle> QuadrangleOptimizeY(List<Quadrangle> quadrangles, int grid, int max)
		{
			quadrangles.Sort((a, b) => a.y - b.y);

			var quads = new Dictionary<int, List<Quadrangle>>();
			foreach (var quad in quadrangles)
			{
				int key = quad.x * 256 * 256 + quad.z;
				if (!quads.ContainsKey(key))
					quads[key] = new List<Quadrangle> { quad };
				else
				{
					bool find = false;
					var quadskey = quads[key];
					for (int i = quadskey.Count - 1; i < quadskey.Count; i++)
					{
						var quad2 = quadskey[i];
						if (quad.dz != quad2.dz || quad.dx != quad2.dx)
							continue;

						if (quad2.y + quad2.dy != quad.y)
							continue;

						if (quad2.dy + quad.dy > max)
							continue;

						if (quad2.y % grid > 0)
							continue;

						if (!quad.SameColor(quad2))
							continue;

						quad2.dy += quad.dy;
						quadskey[i] = quad2;
						find = true;
						break;
					}
					if (!find)
						quadskey.Add(quad);
				}
			}

			foreach (var quad in quads)
				foreach (var quadrangle in quad.Value)
					yield return quadrangle;
		}

		private IEnumerable<Quadrangle> QuadrangleOptimizeZ(List<Quadrangle> quadrangles, int grid, int max)
		{
			quadrangles.Sort((a, b) => a.z - b.z);

			var quads = new Dictionary<int, List<Quadrangle>>();
			foreach (var quad in quadrangles)
			{
				int key = quad.x * 256 * 256 + quad.y;
				if (!quads.ContainsKey(key))
					quads[key] = new List<Quadrangle> { quad };
				else
				{
					bool find = false;
					var quadskey = quads[key];
					for (int i = quadskey.Count - 1; i < quadskey.Count; i++)
					{
						var quad2 = quadskey[i];
						if (quad.dy != quad2.dy || quad.dx != quad2.dx)
							continue;

						if (quad2.z + quad2.dz != quad.z)
							continue;

						if (quad2.dz + quad.dz > max)
							continue;

						if (quad2.z % grid > 0)
							continue;

						if (!quad.SameColor(quad2))
							continue;

						quad2.dz += quad.dz;
						quadskey[i] = quad2;
						find = true;
						break;
					}
					if (!find)
						quadskey.Add(quad);
				}
			}

			foreach (var quad in quads)
				foreach (var quadrangle in quad.Value)
					yield return quadrangle;
		}

		//Сгруппировать квадраты по определенному признаку
		private IEnumerable<List<Quadrangle>> QuadGroup(List<Quadrangle> quadrangles, Func<Quadrangle, int> keyFunc)
		{
			var result = new Dictionary<int, List<Quadrangle>>();

			foreach (var quadrangle in quadrangles)
			{
				var key = keyFunc(quadrangle);
				if (!result.ContainsKey(key))
					result[key] = new List<Quadrangle>();

				result[key].Add(quadrangle);
			}

			return result.Select(x => x.Value);
		}

		public MyMesh Build(LocationCreator lc, int k = 4, int m = 4)
		{
			var qy1 = QuadGroup(quadranglesY1, q => q.y);
			var qx0 = QuadGroup(quadranglesX0, q => q.x);
			var qx1 = QuadGroup(quadranglesX1, q => q.x);
			var qz0 = QuadGroup(quadranglesZ0, q => q.z);
			var qz1 = QuadGroup(quadranglesZ1, q => q.z);

			if (bOptimizeEd)
			{
				if (k > 1)
				{
					qy1 = qy1.Select(x => QuadrangleOptimizeX(x, k, k).ToList());
					qy1 = qy1.Select(x => QuadrangleOptimizeZ(x, k, k).ToList());
				}
				if (m > 1)
				{
					qx0 = qx0.Select(x => QuadrangleOptimizeY(x, m, m).ToList());
					qx0 = qx0.Select(x => QuadrangleOptimizeZ(x, m, m).ToList());
					qx1 = qx1.Select(x => QuadrangleOptimizeY(x, m, m).ToList());
					qx1 = qx1.Select(x => QuadrangleOptimizeZ(x, m, m).ToList());

					qz0 = qz0.Select(x => QuadrangleOptimizeX(x, m, m).ToList());
					qz0 = qz0.Select(x => QuadrangleOptimizeY(x, m, m).ToList());
					qz1 = qz1.Select(x => QuadrangleOptimizeX(x, m, m).ToList());
					qz1 = qz1.Select(x => QuadrangleOptimizeY(x, m, m).ToList());
				}
			}

			if (bOptimize3)
			{
				qy1 = qy1.Select(x => QuadrangleOptimizeX(x, 4, 4).ToList());
				qy1 = qy1.Select(x => QuadrangleOptimizeZ(x, 4, 4).ToList());

				qx0 = qx0.Select(x => QuadrangleOptimizeY(x, 4, 4).ToList());
				qx0 = qx0.Select(x => QuadrangleOptimizeZ(x, 4, 4).ToList());
				qx1 = qx1.Select(x => QuadrangleOptimizeY(x, 4, 4).ToList());
				qx1 = qx1.Select(x => QuadrangleOptimizeZ(x, 4, 4).ToList());

				qz0 = qz0.Select(x => QuadrangleOptimizeX(x, 4, 4).ToList());
				qz0 = qz0.Select(x => QuadrangleOptimizeY(x, 4, 4).ToList());
				qz1 = qz1.Select(x => QuadrangleOptimizeX(x, 4, 4).ToList());
				qz1 = qz1.Select(x => QuadrangleOptimizeY(x, 4, 4).ToList());
			}

			if (k > 4)
			{
				if (bOptimize3)
				{
					qy1 = qy1.Select(x => QuadrangleOptimizeX(x, k, k).ToList());
					qy1 = qy1.Select(x => QuadrangleOptimizeZ(x, k, k).ToList());

					qx0 = qx0.Select(x => QuadrangleOptimizeY(x, k, k).ToList());
					qx0 = qx0.Select(x => QuadrangleOptimizeZ(x, k, k).ToList());
					qx1 = qx1.Select(x => QuadrangleOptimizeY(x, k, k).ToList());
					qx1 = qx1.Select(x => QuadrangleOptimizeZ(x, k, k).ToList());

					qz0 = qz0.Select(x => QuadrangleOptimizeX(x, k, k).ToList());
					qz0 = qz0.Select(x => QuadrangleOptimizeY(x, k, k).ToList());
					qz1 = qz1.Select(x => QuadrangleOptimizeX(x, k, k).ToList());
					qz1 = qz1.Select(x => QuadrangleOptimizeY(x, k, k).ToList());
				}
			}

			if (bOptimize2 && k > 1)
			{
				qy1 = qy1.Select(x => QuadrangleOptimizeX(x, 1, k).ToList());
				qy1 = qy1.Select(x => QuadrangleOptimizeZ(x, 1, k).ToList());

				qx0 = qx0.Select(x => QuadrangleOptimizeY(x, 1, k).ToList());
				qx0 = qx0.Select(x => QuadrangleOptimizeZ(x, 1, k).ToList());
				qx1 = qx1.Select(x => QuadrangleOptimizeY(x, 1, k).ToList());
				qx1 = qx1.Select(x => QuadrangleOptimizeZ(x, 1, k).ToList());

				qz0 = qz0.Select(x => QuadrangleOptimizeX(x, 1, k).ToList());
				qz0 = qz0.Select(x => QuadrangleOptimizeY(x, 1, k).ToList());
				qz1 = qz1.Select(x => QuadrangleOptimizeX(x, 1, k).ToList());
				qz1 = qz1.Select(x => QuadrangleOptimizeY(x, 1, k).ToList());
			}

			var _qy1 = qy1.ToList();
			var _qx0 = qx0.ToList();
			var _qx1 = qx1.ToList();
			var _qz0 = qz0.ToList();
			var _qz1 = qz1.ToList();

			var count = _qy1.Sum(x => x.Count) + _qx0.Sum(x => x.Count) + _qx1.Sum(x => x.Count) + _qz0.Sum(x => x.Count) + _qz1.Sum(x => x.Count);
			if (count == 0)
				return null;

			var mesh = new MyMesh(count * 4, count);
			foreach (var quads in _qy1) foreach (var quad in quads) Create(mesh, quad, orientVertices[QuadOrient.y1]);
			foreach (var quads in _qx0) foreach (var quad in quads) Create(mesh, quad, orientVertices[QuadOrient.x0]);
			foreach (var quads in _qx1) foreach (var quad in quads) Create(mesh, quad, orientVertices[QuadOrient.x1]);
			foreach (var quads in _qz0) foreach (var quad in quads) Create(mesh, quad, orientVertices[QuadOrient.z0]);
			foreach (var quads in _qz1) foreach (var quad in quads) Create(mesh, quad, orientVertices[QuadOrient.z1]);
			mesh = mesh.Optimize();
			return mesh;
		}

		private void Create(MyMesh mesh, Quadrangle quad, float[] points)
		{
			float x = quad.x + offsetX;
			float y = quad.y + offsetY - 1;
			float z = quad.z + offsetZ;
			MyColor[] color = quad.color;

			for (int j = 0; j < 4; j++)
			{
				var p = mesh.pcount++;

				var dx = points[j * 3 + 0] * quad.dx;
				var dy = points[j * 3 + 1] * quad.dy;
				var dz = points[j * 3 + 2] * quad.dz;

				mesh.vertices[p * 3 + 0] = (x + dx) * Voxels.VoxelSize; //x
				mesh.vertices[p * 3 + 1] = (y + dy) * Voxels.VoxelSize; //y
				mesh.vertices[p * 3 + 2] = (z + dz) * Voxels.VoxelSize; //z

				mesh.colors[p * 3 + 0] = color[j].R / 255f;
				mesh.colors[p * 3 + 1] = color[j].G / 255f;
				mesh.colors[p * 3 + 2] = color[j].B / 255f;
			}

			int ind = mesh.index++;
			if (color[1].Same(color[2], 8) || !color[0].Same(color[3], 8))
			{
				mesh.indices[ind * 6 + 0] = ind * 4 + 0;
				mesh.indices[ind * 6 + 1] = ind * 4 + 2;
				mesh.indices[ind * 6 + 2] = ind * 4 + 1;

				mesh.indices[ind * 6 + 3] = ind * 4 + 2;
				mesh.indices[ind * 6 + 4] = ind * 4 + 3;
				mesh.indices[ind * 6 + 5] = ind * 4 + 1;
			}
			else
			{
				mesh.indices[ind * 6 + 0] = ind * 4 + 0;
				mesh.indices[ind * 6 + 1] = ind * 4 + 3;
				mesh.indices[ind * 6 + 2] = ind * 4 + 1;

				mesh.indices[ind * 6 + 3] = ind * 4 + 2;
				mesh.indices[ind * 6 + 4] = ind * 4 + 3;
				mesh.indices[ind * 6 + 5] = ind * 4 + 0;
			}
		}

		public bool bNeedRazer = true;
		public int RazerI; //Номер разрезанной части по оси x
		public int RazerJ; //Номер разрезанной части по оси z

		//Разрезает меш на несколько квадратных частей размером не больше size
		public IEnumerable<MyQuadMesh> Razer(int size = 64)
		{
			if (!bNeedRazer)
			{
				yield return this;
				yield break;
			}

			while (quadranglesY1.Count > 0)
			{
				int x = quadranglesY1[0].x / size;
				int z = quadranglesY1[0].z / size;

				yield return new MyQuadMesh
				{
					offsetX = offsetX,
					offsetY = offsetY,
					offsetZ = offsetZ,
					RazerI = x,
					RazerJ = z,
					quadranglesY1 = quadranglesY1.Where(w => w.x/size == x && w.z/size == z).ToList(),
					quadranglesX0 = quadranglesX0.Where(w => w.x/size == x && w.z/size == z).ToList(),
					quadranglesX1 = quadranglesX1.Where(w => w.x/size == x && w.z/size == z).ToList(),
					quadranglesZ0 = quadranglesZ0.Where(w => w.x/size == x && w.z/size == z).ToList(),
					quadranglesZ1 = quadranglesZ1.Where(w => w.x/size == x && w.z/size == z).ToList()
				};

				quadranglesY1 = quadranglesY1.Where(w => !(w.x / size == x && w.z / size == z)).ToList();
				quadranglesX0 = quadranglesX0.Where(w => !(w.x / size == x && w.z / size == z)).ToList();
				quadranglesX1 = quadranglesX1.Where(w => !(w.x / size == x && w.z / size == z)).ToList();
				quadranglesZ0 = quadranglesZ0.Where(w => !(w.x / size == x && w.z / size == z)).ToList();
				quadranglesZ1 = quadranglesZ1.Where(w => !(w.x / size == x && w.z / size == z)).ToList();
			}
		}
	}
}
