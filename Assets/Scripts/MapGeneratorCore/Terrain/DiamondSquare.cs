using System;
using Assets.Scripts.MapGenerator.MapGeneratorCore.Algorithms;

namespace Assets.Scripts.MapGenerator.MapGeneratorCore.Relievos.Terrain
{
    //Генерация поверхности на основе алгоритма DiamondSquare
    public class DiamondSquare : IRelief
	{
		public int minH = -20, maxH = 20, delH = 1, sizeBlock = 64;

		public Biome MainBiome;
		public Biome MainBiomeUp;

		public float[,] minp;
		public float[,] maxp;

		public int SmoothCount = 2;

		public override void Init(LocationCreator lc)
		{
			if (MainBiomeUp == null)
				MainBiomeUp = MainBiome;

			Generate(lc);

			for (int i = 0; i < SmoothCount; i++)
				Smooth(lc);

			if (minp == null || maxp == null)
			{
				int minv = int.MaxValue;
				int maxv = int.MinValue;
				for (int x = 0; x < lc.sizeX; x++)
					for (int y = 0; y < lc.sizeZ; y++)
					{
						minv = Math.Min(minv, lc.Land[x, y]);
						maxv = Math.Max(maxv, lc.Land[x, y]);
					}

				for (int x = 0; x < lc.sizeX; x++)
					for (int y = 0; y < lc.sizeZ; y++)
						lc.Land[x, y] = (lc.Land[x, y] - minv) * (maxH - minH) / (maxv - minv) + minH;
			}
			else
			{
				for (int x = 0; x < lc.sizeX; x++)
					for (int y = 0; y < lc.sizeZ; y++)
					{
						try
						{
							var minv = GetMin(lc, x, y);
							var maxv = GetMax(lc, x, y);
							lc.Land[x, y] = (int)((lc.Land[x, y] + sizeBlock) / (2f * sizeBlock) * (maxv - minv) + minv);
						}
						catch (Exception)
						{
							lc.Land[x, y] = -10;
						}
					}
			}

			if (delH > 1)
				for (int x = 0; x < lc.sizeX; x++)
					for (int y = 0; y < lc.sizeZ; y++)
						lc.Land[x, y] = lc.Land[x / delH * delH, y / delH * delH] / delH * delH;

			for (int x = 0; x < lc.sizeX; x++)
				for (int z = 0; z < lc.sizeZ; z++)
				{
					var y = lc.Land[x, z];
					lc.Colors[x, z] = MainBiome.GetColor(y);
					lc.UpColors[x, z] = MainBiomeUp.GetColor(y);
				}
		}

		//Нижняя граница рандома
		private float GetMin(LocationCreator lc, int x, int z)
		{
			var dx = (float)x / lc.sizeX * (minp.GetLength(0) - 1);
			var dz = (float)z / lc.sizeZ * (minp.GetLength(1) - 1);
			int x1 = (int)dx;
			int z1 = (int)dz;
			dx -= x1;
			dz -= z1;
			var r1 = minp[x1, z1] * (1 - dx) + minp[x1 + 1, z1] * dx;
			var r2 = minp[x1, z1 + 1] * (1 - dx) + minp[x1 + 1, z1 + 1] * dx;
			return r1 * (1 - dz) + r2 * dz;
		}

		//Верхняя граница рандома
		private float GetMax(LocationCreator lc, int x, int z)
		{
			var dx = (float)x / lc.sizeX * (maxp.GetLength(0) - 1);
			var dz = (float)z / lc.sizeZ * (maxp.GetLength(1) - 1);
			int x1 = (int)dx;
			int z1 = (int)dz;
			dx -= x1;
			dz -= z1;
			var r1 = maxp[x1, z1] * (1 - dx) + maxp[x1 + 1, z1] * dx;
			var r2 = maxp[x1, z1 + 1] * (1 - dx) + maxp[x1 + 1, z1 + 1] * dx;
			return r1 * (1 - dz) + r2 * dz;
		}

		public void Generate(LocationCreator lc)
		{
			int sizeX = (int)Math.Ceiling((double)lc.sizeX / sizeBlock) * sizeBlock;
			int sizeZ = (int)Math.Ceiling((double)lc.sizeZ / sizeBlock) * sizeBlock;

			var Init = new bool[sizeX + 1, sizeZ + 1];
			var Land = new int[sizeX + 1, sizeZ + 1];
			for (int i = 0; i < sizeX; i += sizeBlock)
				for (int j = 0; j < sizeZ; j += sizeBlock)
				{
					Init[i, j] = true;
					Land[i, j] = lc.rand.Next(-sizeBlock, sizeBlock);
				}

			DiamondSquareAlgo.Generate(sizeBlock, sizeX, sizeZ, lc.rand, Init, Land);

			for (int i = 0; i <= lc.sizeX; i++)
				for (int j = 0; j <= lc.sizeZ; j++)
					lc.Land[i, j] = Land[i, j];
		}

		//Сгладить высоты
		public void Smooth(LocationCreator lc)
		{
			for (int x = 0; x < lc.sizeX; x++)
				for (int z = 0; z < lc.sizeZ; z++)
					lc.Land[x, z] = (lc.Land[x, z] + lc.Land[x + 1, z] + lc.Land[x, z + 1] + lc.Land[x + 1, z + 1]) / 4;
		}
	}
}
