using System;
using System.Collections.Generic;
using Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Interfaces;
using Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Relievos.Terrain;
using Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Utils;
using Assets.Scripts.MapGenerator.MapGeneratorCore;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.MapGeneratorBehaviours
{
	[ExecuteInEditMode]
	public class UltraMapGenerator : MapGenerator
	{
		[Range(16, 512)]
		public int SizeX = 64;
		[Range(16, 512)]
		public int SizeZ = 64;

		[Range(16, 2048)]
		public int BlockRazerSize = 64;

		[HideInInspector]
		public int MeshSizeQuadHorizontal = 4;
		[HideInInspector]
		public int MeshSizeQuadVertical = 4;
		[HideInInspector]
		public int TerrainThickness = 10;

		[HideInInspector]
		public Vector3 LightDirection = new Vector3(0.4f, -0.7f, 0.6f);
		[HideInInspector]
		public float AmbientLightIntensity = 0.425f;
		[HideInInspector]
		public Color NoiseTextureColorMax = new Color(25 / 255f, 25 / 255f, 25 / 255f);
		[HideInInspector]
		public Vector3 NoiseTextureColorShift = new Vector3(0f, 0f, 0f);

		[HideInInspector]
		public bool MakeMeshMergeVertices;
		[HideInInspector]
		public bool GenerateVoxelSideX1 = true;
		[HideInInspector]
		public bool GenerateVoxelSideZ0 = true;

		private int _sizeX;
		private int _sizeZ;

		public void Update()
		{
			transform.localPosition = Vector3.zero;
			transform.localEulerAngles = Vector3.zero;
			transform.localScale = Vector3.one;

			bool sizeChanged = _sizeX != SizeX || _sizeZ != SizeZ;
			_sizeX = SizeX;
			_sizeZ = SizeZ;
			if (sizeChanged)
			{
				var needTerrainSizeObjects = MapGeneratorUtils.GetComponents<MapComponentWithTerrainSize>(gameObject, recursive: true);
				foreach (var needTerrainSizeObject in needTerrainSizeObjects)
				{
					needTerrainSizeObject.TerrainSizeX = _sizeX;
					needTerrainSizeObject.TerrainSizeZ = _sizeZ;
					needTerrainSizeObject.OnTerrainSizeChanged();
				}
			}
		}

		private IRelief GetLandscape()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i);
				var landscapeComponent = child.gameObject.GetComponent<ILandscape>();
				if (landscapeComponent != null)
					return landscapeComponent.GetRelief();
			}

			return AddRandomTerrain().GetRelief();
		}

		private List<TItem> GetMapComponents<TItem, TSearchComponent>(GameObject owner, Func<TSearchComponent, TItem> converter, bool recursive = false)
			where TSearchComponent : MonoBehaviour
		{
			var result = new List<TItem>();

			for (int i = 0; i < owner.transform.childCount; i++)
			{
				var child = owner.transform.GetChild(i);
				if (!child.gameObject.activeInHierarchy)
					continue;

				var component = child.gameObject.GetComponent<TSearchComponent>();
				if (component != null)
					result.Add(converter(component));

				if (recursive)
				{
					result.AddRange(GetMapComponents(child.gameObject, converter, recursive: true));
				}
			}

			return result;
		}

		public ILandscape AddRandomTerrain()
		{
			var result = new GameObject(RandomTerrainDiamondSquare.DefaultName);
			result.transform.parent = transform;
			var terrain = result.AddComponent<RandomTerrainDiamondSquare>();
			terrain.TerrainSizeX = SizeX;
			terrain.TerrainSizeZ = SizeZ;
			return terrain;
		}

		public override Location MakeLocation()
		{
			var location = new Location();
			location.sizeX = SizeX;
			location.sizeZ = SizeZ;
			location.sizeRazer = BlockRazerSize;
			location.sizeQuad = MeshSizeQuadHorizontal;
			location.sizeQuadVertical = MeshSizeQuadVertical;
			location.NoiseTextureColorMax = NoiseTextureColorMax.ToMyColor();
			location.NoiseTextureColorShift = NoiseTextureColorShift.ToMyVectorFloat() / 255f;
			location.TerrainThickness = TerrainThickness;

			MyQuadMesh.AmbientLightIntensity = AmbientLightIntensity;
			MyQuadMesh.DirectionalLight = LightDirection;

			location.AddRelief(GetLandscape());
			return location;
		}

		public override IEnumerable<Progress> Generate()
		{
			ClearBlocks();

			Location location = MakeLocation();
			location.Relievos.RemoveAll(r => r == null);

			var lc = new LocationCreator(location);
			lc.GenerateReliev();
			lc.GenerateVoxelSideX1 = GenerateVoxelSideX1;
			lc.GenerateVoxelSideZ0 = GenerateVoxelSideZ0;

			lc.BloorVoxels();

			if (MakeMeshMergeVertices)
			{
				MyMesh.bOptimize = true;
				MyQuadMesh.bOptimizeEd = true;
				MyQuadMesh.bOptimize2 = false;
				MyQuadMesh.bOptimize3 = false;
				MyMesh.bOptimizeIgnoreColor = true;
			}
			else
			{
				MyMesh.bOptimize = true;
				MyQuadMesh.bOptimizeEd = false;
				MyQuadMesh.bOptimize2 = true;
				MyQuadMesh.bOptimize3 = true;
				MyMesh.bOptimizeIgnoreColor = false;
			}

			foreach (var s in lc.MakeBlocks())
				yield return s;
			
			level = lc.MakeLevelInfo();

			foreach (var s in OnGetLevelData())
				yield return s;

			MakeHeightColorMaps(lc);

			yield return new Progress(1);
		}
	}
}
