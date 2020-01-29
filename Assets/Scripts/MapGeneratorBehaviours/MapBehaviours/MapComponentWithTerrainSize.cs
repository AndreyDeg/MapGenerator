using UnityEngine;

namespace Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Utils
{
	//Компонент, следит за изменением размера карты
	public class MapComponentWithTerrainSize : MonoBehaviour
	{
		[HideInInspector]
		public int TerrainSizeX;
		[HideInInspector]
		public int TerrainSizeZ;

		public virtual void OnTerrainSizeChanged() { }
	}
}
