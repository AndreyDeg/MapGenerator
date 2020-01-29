using System;
using System.Collections.Generic;
using Assets.MapGenerator.MapGeneratorCore;
using Assets.Scripts.MapGenerator.MapGeneratorCore;
using UnityEngine;
using UtilsLib.Logic;

namespace Assets.Scripts.MapGenerator.MapGeneratorBehaviours
{
	[ExecuteInEditMode]
	public abstract class MapGenerator : MonoBehaviour
	{
		[HideInInspector, NonSerialized]
		public LevelInfo level;

		protected string blocksName = "blocks";

		public virtual Location MakeLocation()
		{
			return new Location();
		}

		public abstract IEnumerable<Progress> Generate();

		protected void ClearBlocks()
		{
			var blocks = transform.Find(blocksName);
			if (blocks != null)
			{
				while (blocks.childCount > 0)
					DestroyImmediate(blocks.GetChild(0).gameObject);
			}
			else
			{
				var go_blocks = new GameObject(blocksName);
				blocks = go_blocks.transform;
				blocks.transform.parent = transform;
			}

			blocks.transform.localPosition = Vector3.zero;
			blocks.transform.localEulerAngles = Vector3.zero;
		}

		protected IEnumerable<Progress> OnGetLevelData()
		{
			var blocks = transform.Find(blocksName);

			MakeNoiseTex();

			int prog = 0;
			float maxprog = level.blocks.Count;
			foreach (var block in level.blocks)
			{
				prog++;
				yield return new Progress(prog / maxprog);

				CreateBlocks(block, blocks);
			}

			blocks.localPosition = transform.localPosition;
			blocks.localEulerAngles = transform.localEulerAngles;
		}

		protected int pixWidth = 64;
		protected int pixHeight = 64;

		[HideInInspector]
		public Texture2D noiseTex;

		private void MakeNoiseTex()
		{
			noiseTex = new Texture2D(pixWidth/2, pixHeight/2);

			var pix = new Color[noiseTex.width * noiseTex.height];
			var noiseColors = level.NoiseTextureColors;

			for (int y = 0; y < noiseTex.height; y++)
			{
				for (int x = 0; x < noiseTex.width; x++)
				{
					Color c = new Color(UnityEngine.Random.Range(0, noiseColors.R), UnityEngine.Random.Range(0, noiseColors.G),
							UnityEngine.Random.Range(0, noiseColors.B)) / 255f;
					pix[y * noiseTex.width + x] = c / 2; //TODO_deg почему-то яркость в два раза больше, чем надо
				}
			}
			noiseTex.filterMode = FilterMode.Point;
			noiseTex.SetPixels(pix);
			noiseTex.Apply();
		}

		private void CreateBlocks(BlockInfo block, Transform parent)
		{
			var blockObj = new GameObject(block.name);
			blockObj.transform.parent = parent;
			CreateMesh(blockObj, block);

			MeshRenderer mr = blockObj.AddComponent<MeshRenderer>();
			mr.sharedMaterial = new Material(Shader.Find("Terrain"));
			mr.sharedMaterial.SetTexture("_NoiseTex", noiseTex);
			var noiseShift = level.NoiseTextureColorsShift;
			mr.sharedMaterial.SetVector("_NoiseColorShift", new Vector4(noiseShift.x, noiseShift.y, noiseShift.z));

			blockObj.AddComponent<MeshCollider>();
		}

		protected Mesh CreateMesh(GameObject blockObj, BlockInfo block)
		{
			if (block.mesh == null)
				return null;

			Mesh mesh = new Mesh();
			if (blockObj != null)
			{
				var meshFilter = blockObj.AddComponent<MeshFilter>();
				meshFilter.sharedMesh = mesh;
			}

			Vector3[] vertices = new Vector3[block.mesh.vertices.Length/3];
			Color[] colors = new Color[block.mesh.vertices.Length/3];
			for (int i = 0; i < block.mesh.vertices.Length; i+=3)
			{
				vertices[i/3].x = block.mesh.vertices[i];
				vertices[i/3].y = block.mesh.vertices[i+1];
				vertices[i/3].z = block.mesh.vertices[i+2];

				colors[i/3].r = block.mesh.colors[i];
				colors[i/3].g = block.mesh.colors[i+1];
				colors[i/3].b = block.mesh.colors[i+2];
			}

			int[] triangles = new int[block.mesh.indices.Length];
			for (int i = 0; i < block.mesh.indices.Length; i++)
			{
				triangles[i] = block.mesh.indices[i];
			}

			mesh.vertices = vertices;
			mesh.colors = colors;
			mesh.triangles = triangles;

			return mesh;
		}

		[HideInInspector]
		public int heightShift = 0;
		[HideInInspector]
		public int heightScale = 5;
		[HideInInspector, NonSerialized]
		public int[,] heightsMap; //карта высот
		[HideInInspector, NonSerialized]
		public Color[,] colorsMap; //карта цветов

		public void MakeHeightColorMaps(LocationCreator lc)
		{
			heightsMap = new int[lc.sizeX, lc.sizeZ];
			for (int i = 0; i < lc.sizeX; i++)
				for (int j = 0; j < lc.sizeZ; j++)
					heightsMap[i, j] = lc.Land[lc.sizeX - i - 1, lc.sizeZ - j - 1];

			var defaultColor = new MyColor();
			colorsMap = new Color[lc.sizeX, lc.sizeZ];
			for (int i = 0; i < lc.sizeX; i++)
				for (int j = 0; j < lc.sizeZ; j++)
				{
					var c = lc.UpColors[lc.sizeX - i - 1, lc.sizeZ - j - 1] ?? lc.Colors[lc.sizeX - i - 1, lc.sizeZ - j - 1] ?? defaultColor;
					colorsMap[i, j] = new Color(c.R / 255f, c.G / 255f, c.B / 255f);
				}
		}
	}
}
