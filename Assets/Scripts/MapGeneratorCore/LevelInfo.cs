using Assets.Scripts.MapGenerator.MapGeneratorCore;
using System;
using System.Collections.Generic;
using UtilsLib.Logic;

namespace Assets.MapGenerator.MapGeneratorCore
{
	[Serializable]
	public class BlockInfo
	{
		public string name;
		public MyMesh mesh;
		public MyVector offset = new MyVector();
	}

	[Serializable]
	public class LevelInfo
	{
		public List<BlockInfo> blocks;
		public MyColor NoiseTextureColors;
		public MyVectorFloat NoiseTextureColorsShift;
	}
}
