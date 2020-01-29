using System;
using Assets.Scripts.MapGenerator.MapGeneratorCore.Algorithms;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Utils
{
	//Матрица опорных точек
	[ExecuteInEditMode]
	public class RepPoints : MapComponentWithTerrainSize
	{
		private const int RepPointMax = 32;

		[Range(2, RepPointMax)]
		public int RepPoint = 2;

		[SerializeField]
		[HideInInspector]
		private int RepPoint_;

		[Range(1, 16)]
		public int RepPointSpline = 4;

		public bool DrawTopGrid;
		public bool DrawBottomGrid;

		[SerializeField]
		[HideInInspector]
		private int _sizeX;

		[SerializeField]
		[HideInInspector]
		private int _sizeZ;

		public override void OnTerrainSizeChanged()
		{
			LateUpdate();
		}

		public void LateUpdate()
		{
			bool sizeChanged = _sizeX != TerrainSizeX || _sizeZ != TerrainSizeZ;
			_sizeX = TerrainSizeX;
			_sizeZ = TerrainSizeZ;

			if (RepPoint_ != RepPoint)
			{
				Clear();

				for (int i = 0; i < RepPoint; i++)
					for (int j = 0; j < RepPoint; j++)
					{
						float x = (float)i * _sizeX * Voxels.VoxelSize / (RepPoint - 1);
						float z = (float)j * _sizeZ * Voxels.VoxelSize / (RepPoint - 1);
						int y = 32 / 8;
						int h = 64 / 8;

						var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
						cube.transform.position = new Vector3(x, y, z);
						cube.transform.localScale = new Vector3(1, h, 1);
						cube.transform.parent = transform;
						UpdateLockTransform(cube, x, z);
					}

				RepPoint_ = RepPoint;
				return;
			}

			if (sizeChanged)
			{
				for (int i = 0; i < RepPoint; i++)
				{
					for (int j = 0; j < RepPoint; j++)
					{
						float x = (float)i * _sizeX * Voxels.VoxelSize / (RepPoint - 1);
						float z = (float)j * _sizeZ * Voxels.VoxelSize / (RepPoint - 1);

						var cube = transform.GetChild(i * RepPoint + j);
						cube.position = new Vector3(x, cube.position.y, z);
						UpdateLockTransform(cube.gameObject, x, z);
					}
				}
			}
		}

		private void UpdateLockTransform(GameObject cube, float x, float z)
		{
			foreach (var component in cube.GetComponents<LockTransform>())
			{
				DestroyImmediate(component);
			}

			var lockTransform = cube.AddComponent<LockTransform>();
			lockTransform.PositionMin = new Vector3(x, 0 * Voxels.VoxelSize, z);
			lockTransform.PositionMax = new Vector3(x, 256 * Voxels.VoxelSize, z);
			lockTransform.RotationMin = new Vector3(0, 0, 0);
			lockTransform.RotationMax = new Vector3(0, 0, 0);
			lockTransform.ScaleMin = new Vector3(1, Voxels.VoxelSize, 1);
			lockTransform.ScaleMax = new Vector3(1, 256 * Voxels.VoxelSize, 1);
		}

		private void Clear()
		{
			while (transform.childCount > 0)
			{
				var child = transform.GetChild(0).gameObject;
				DestroyImmediate(child);
			}
		}

		public float[,] GetRefPoints(int m)
		{
			var reppoints = new float[RepPoint, RepPoint];

			for (int i = 0; i < RepPoint; i++)
				for (int j = 0; j < RepPoint; j++)
				{
					var child = transform.GetChild(i * RepPoint + j);
					reppoints[i, j] = (child.position.y + m * child.localScale.y / 2) / Voxels.VoxelSize;
					reppoints[i, j] = Math.Min(Math.Max(0, reppoints[i, j]), 255);
				}

			return Spline.MakeSpline(reppoints, RepPointSpline);
		}

		public void DoubleRepPointsCount()
		{
			if (RepPoint * 2 > RepPointMax)
				return;

			Undo.RecordObject(gameObject, "RepPoints change points count");

			var oldTransforms = new Vector4[RepPoint, RepPoint];
			for (int i = 0; i < RepPoint; i++)
			{
				for (int j = 0; j < RepPoint; j++)
				{
					var cube = transform.GetChild(i * RepPoint + j);
					oldTransforms[i, j] = new Vector4(cube.transform.position.x, cube.transform.position.y, cube.transform.position.z,
						cube.transform.localScale.y);
				}
			}
			Clear();
			RepPoint_ = RepPoint = RepPoint * 2 - 1;

			for (int i = 0; i < RepPoint; i++)
			{
				for (int j = 0; j < RepPoint; j++)
				{
					float x, z, y, h;
					x = (float)i * _sizeX * Voxels.VoxelSize / (RepPoint - 1);
					z = (float)j * _sizeZ * Voxels.VoxelSize / (RepPoint - 1);
					if (i % 2 == 0 && j % 2 == 0)
					{
						var tran = oldTransforms[i / 2, j / 2];
						//x = tran.x;
						y = tran.y;
						//z = tran.z;
						h = tran.w;
					}
					else
					{
						y = 32 / 8f;
						h = 64 / 8f;
					}

					var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.transform.position = new Vector3(x, y, z);
					cube.transform.localScale = new Vector3(1, h, 1);
					cube.transform.parent = transform;
					UpdateLockTransform(cube, x, z);
				}
			}
		}
	}
}
