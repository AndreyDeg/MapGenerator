using Assets.Scripts.MapGenerator.MapGeneratorCore;

namespace Assets.Scripts.MapGenerator.MapGeneratorBehaviours.Interfaces
{
	//Компонент является ландшафтом
	public interface ILandscape
	{
		IRelief GetRelief();
	}
}
