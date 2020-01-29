using Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Interfaces;
using Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Utils;
using Assets.Scripts.MapGenerator.MapGeneratorCore;
using Assets.Scripts.MapGenerator.MapGeneratorCore.Relievos.Terrain;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Relievos.Terrain
{
	[ExecuteInEditMode]
	public class RandomTerrainDiamondSquare : MapComponentWithTerrainSize, ILandscape
	{
		[Range(0, 8)]
		public int SmoothCount = 2;
		public int SizeBlock = 64;

		public Gradient LandscapeGradient = new Gradient();

		public const string DefaultName = "RandomTerrainDiamondSquare";
		private string repPointsName = "repPoints";

		public IRelief GetRelief()
		{
			var diamondSquare = new DiamondSquare
			{
				sizeBlock = SizeBlock,
				MainBiome = new Biome
				{
					Colors = LandscapeGradient.ToListOfMyColor()
				},
				SmoothCount = SmoothCount,
				minp = GetRepPoints(-1),
				maxp = GetRepPoints(+1),
			};

			return diamondSquare;
		}

		public GameObject AddRepPoints()
		{
			var result = new GameObject(repPointsName);
			result.transform.parent = transform;
			var points = result.AddComponent<RepPoints>();
			points.TerrainSizeX = TerrainSizeX;
			points.TerrainSizeZ = TerrainSizeZ;
			points.OnTerrainSizeChanged();
			return result;
		}

		private float[,] GetRepPoints(int m)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i);
				var repPoints = child.gameObject.GetComponent<RepPoints>();
				if (repPoints != null)
					return repPoints.GetRefPoints(m);
			}

			return null;
		}

		public void Update()
		{
			if (!transform.Find(repPointsName))
				AddRepPoints();
		}
	}
}
