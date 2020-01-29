namespace Assets.Scripts.MapGenerator.MapGeneratorCore
{
	//Изменяет рельеф поверхности
	public abstract class IRelief
	{
		//Вызывается при инициализации
		public abstract void Init(LocationCreator locationCreator);

		//Вызывается после инициализации
		public virtual void AfterInit(LocationCreator locationCreator)
		{
		}
	}
}
