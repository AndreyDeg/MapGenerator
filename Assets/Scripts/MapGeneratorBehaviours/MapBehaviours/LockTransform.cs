using UnityEngine;

namespace Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Utils
{
	//Компонент, блокирует позицию, поворот и размер у gameObject
	[ExecuteInEditMode]
	public class LockTransform : MonoBehaviour
	{
		public Vector3 PositionMin = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		public Vector3 PositionMax = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public Vector3 RotationMin = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		public Vector3 RotationMax = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public Vector3 ScaleMin = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		public Vector3 ScaleMax = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		public bool ScaleXZ;

		public void Update()
		{
			transform.position = Vector3.Min(Vector3.Max(PositionMin, transform.position), PositionMax);
			transform.eulerAngles = Vector3.Min(Vector3.Max(RotationMin, transform.eulerAngles), RotationMax);
			transform.localScale = Vector3.Min(Vector3.Max(ScaleMin, transform.localScale), ScaleMax);

			if (ScaleXZ)
				transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.x);
		}
	}
}
