using Assets.MapGenerator.MapGeneratorCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilsLib.Logic;
using Random = System.Random;

namespace Assets.Scripts.MapGenerator.MapGeneratorCore
{
	//Класс для генерации карты
	public class LocationCreator
	{
		public readonly Location loc;
		public readonly int sizeX, sizeZ;
		public readonly int sizeRazer;
		public readonly int sizeQuad, sizeQuadVertical;
		public readonly int sizeQuadWater = 64;
		public bool GenerateVoxelSideX1 = true;
		public bool GenerateVoxelSideZ0 = true;
		public Random rand;
		public BlockSource landscapeBlock;
		public int[,] Land; //карта высот
		public MyColor[,] Colors; //цвет вокселей
		public MyColor[,] UpColors; //цвет верхних граней вокселей
		public List<BlockInfo> blocks;

		public LocationCreator(Location loc)
		{
			this.loc = loc;

			sizeX = loc.sizeX;
			sizeZ = loc.sizeZ;
			sizeRazer = loc.sizeRazer;
			sizeQuad = loc.sizeQuad;
			sizeQuadVertical = loc.sizeQuadVertical;
		}

		//Сгенерировать рельеф
		public void GenerateReliev()
		{
			rand = new Random();

			landscapeBlock = new BlockSource(0, 0, sizeX, sizeZ) { H = loc.TerrainThickness };
			Land = landscapeBlock.Land;
			Colors = landscapeBlock.Colors;
			UpColors = new MyColor[sizeX, sizeZ];

			var Relievos = loc.Relievos;

			foreach (var relievo in Relievos)
			{
				relievo.Init(this);
			}

			foreach (var relievo in Relievos)
			{
				relievo.AfterInit(this);
			}

			for (int i = 0; i < sizeX; i++)
				for (int j = 0; j < sizeZ; j++)
					if (UpColors[i, j] == null)
						UpColors[i, j] = Colors[i, j];
		}

		//Размыть цвет вокселей
		public void BloorVoxels()
		{
			var newColors = new MyColor[sizeX, sizeZ];
			for (int x = 0; x < sizeX; x++)
				for (int z = 0; z < sizeZ; z++)
				{
					bool haveOther = false;
					MyColor m = UpColors[x, z];
					if (m == null)
						continue;

					int c = 2;
					float r = m.R * c, g = m.G * c, b = m.B * c;

					for (int x2 = Math.Max(0, x - 1); x2 <= x + 1 && x2 < sizeX; x2++)
						for (int z2 = Math.Max(0, z - 1); z2 <= z + 1 && z2 < sizeZ; z2++)
						{
							if (x2 != x && z2 != z && Math.Abs(Land[x, z] - Land[x2, z2]) < 3)
							{
								c++;
								MyColor m2 = UpColors[x2, z2];
								r += m2.R;
								g += m2.G;
								b += m2.B;
							}
						}

					if (haveOther)
						newColors[x, z] = MyColor.FromArgb((byte)(r / c), (byte)(g / c), (byte)(b / c));
					else
						newColors[x, z] = m;
				}

			UpColors = newColors;
		}

		//Сделать блоки и сгенерить меши
		public IEnumerable<Progress> MakeBlocks()
		{
			blocks = new List<BlockInfo>();

			var qmeshstructures = new List<MyQuadMesh>();

			//Создадим сразу все блоки
			int blocksSizeX = (int)Math.Ceiling(loc.sizeX / (float)sizeRazer);
			int blocksSizeZ = (int)Math.Ceiling(loc.sizeZ / (float)sizeRazer);
			var landscapeBlocks = new BlockInfo[blocksSizeX, blocksSizeZ];
			for (int i = 0; i < blocksSizeX; i++)
				for (int j = 0; j < blocksSizeZ; j++)
					landscapeBlocks[i, j] = new BlockInfo()
					{
						name = "block_" + i + "_" + j,
						offset = new MyVector(i * sizeRazer, 0, j * sizeRazer),
					};

			//Генерация основного ландшафта
			float prog = 0;
			float maxprog = (sizeX / sizeRazer + 1) * (sizeZ / sizeRazer + 1);
			var landscapeMeshes = new MyQuadMesh[blocksSizeX, blocksSizeZ];
			var offsetmesh = new MyVector(0, 0, 0);
			foreach (var qmesh in landscapeBlock.MakeGroundMesh(this, offsetmesh, landscapeBlock.H - 1).Razer(sizeRazer))
			{
				prog++;
				yield return new Progress(prog / maxprog);
				landscapeMeshes[qmesh.RazerI, qmesh.RazerJ] = qmesh;
			}

			//Прибавление дополнительных блоков и финальная генерация ландшафта
			prog = 0;
			maxprog = blocksSizeX * blocksSizeZ;
			for (int i = 0; i < blocksSizeX; i++)
				for (int j = 0; j < blocksSizeZ; j++)
				{
					prog++;
					yield return new Progress(prog / maxprog);

					var landscapeMeshe = landscapeMeshes[i, j] ?? new MyQuadMesh {RazerI = i, RazerJ = j};
					landscapeMeshe.AddStructure(qmeshstructures, i, j, sizeRazer);
					landscapeBlocks[i, j].mesh = landscapeMeshe.Build(this, sizeQuad, sizeQuadVertical);
				}

			for (int i = 0; i < blocksSizeX; i++)
				for (int j = 0; j < blocksSizeZ; j++)
				{
					var block = landscapeBlocks[i, j];
					blocks.Add(block);
				}

			blocks.RemoveAll(b => b.mesh == null);
		}

		//Узнать цвет вершины, учитываю затемненость от соседних блоков
		public MyColor GetColor(MyColor origColor, int x, int z, int y)
		{
			//return color;
			int c = -4;
			bool[] occupiedBlocks = new bool[8];

			if (landscapeBlock.HaveBlockLocal(x, y, z)) occupiedBlocks[0] = true;
			if (landscapeBlock.HaveBlockLocal(x - 1, y, z)) occupiedBlocks[1] = true;
			if (landscapeBlock.HaveBlockLocal(x, y, z - 1)) occupiedBlocks[2] = true;
			if (landscapeBlock.HaveBlockLocal(x - 1, y, z - 1)) occupiedBlocks[3] = true;

			if (landscapeBlock.HaveBlockLocal(x, y - 1, z)) occupiedBlocks[4] = true;
			if (landscapeBlock.HaveBlockLocal(x - 1, y - 1, z)) occupiedBlocks[5] = true;
			if (landscapeBlock.HaveBlockLocal(x, y - 1, z - 1)) occupiedBlocks[6] = true;
			if (landscapeBlock.HaveBlockLocal(x - 1, y - 1, z - 1)) occupiedBlocks[7] = true;

			c += occupiedBlocks.Count(o => o);

			if (c <= 0)
				return origColor;

			float H, S, V;
			var MyUnityColor = new Color(origColor.R / 255.0f, origColor.G / 255.0f, origColor.B / 255.0f);
			Color.RGBToHSV(MyUnityColor, out H, out S, out V);

			//S = S+(1.0f - S)/(4-c);
			//S = 0.0f;
			V = V * (4 - c) / 4;

			MyUnityColor = Color.HSVToRGB(H, S, V);

			return MyColor.FromArgb(
				(int)(MyUnityColor.r * 255 + 0.5),
				(int)(MyUnityColor.g * 255 + 0.5),
				(int)(MyUnityColor.b * 255 + 0.5)
			);
		}

		public LevelInfo MakeLevelInfo()
		{
			var result = new LevelInfo();
			result.blocks = blocks;
			result.NoiseTextureColors = loc.NoiseTextureColorMax;
			result.NoiseTextureColorsShift = loc.NoiseTextureColorShift;
			return result;
		}
	}
}
