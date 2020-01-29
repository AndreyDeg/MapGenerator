using System.Collections.Generic;
using UtilsLib.Logic;

namespace Assets.Scripts.MapGenerator.MapGeneratorCore
{
    //Данные для генерации карты
    public class Location
	{
		public int sizeX, sizeZ; //Размер карты
		public int sizeRazer = 64; //Размер блока на которые разрезать карту
		public int sizeQuad = 4; //Оптимизировать квадраты не больше данного размера
		public int sizeQuadVertical = 4; //Оптимизировать квадраты не больше данного размера по вертикали
		public int TerrainThickness = 10; //Максимальное колличество вокселей по вертикали
		public MyColor NoiseTextureColorMax; //Сила шума на земле
		public MyVectorFloat NoiseTextureColorShift;
		public List<IRelief> Relievos = new List<IRelief>(); //Список рельефов на карте

		public void AddRelief(IRelief relief)
		{
			if (relief == null)
				return;

			Relievos.Add(relief);
		}
	}
}