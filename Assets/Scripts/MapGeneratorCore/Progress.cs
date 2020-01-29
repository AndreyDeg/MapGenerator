namespace Assets.Scripts.MapGenerator.MapGeneratorCore
{
	//Информация о прогрессе мап-генератора
	public struct Progress
	{
		public string content; //Что сейчас генерится
		public float time; //На сколько уже готово. От 0 до 1

		public Progress(float time, string content = "")
		{
			this.time = time;
			this.content = content + (time * 100).ToString("0.00") + "%";
		}
	}
}
