using System;

namespace Assets.Scripts.MapGenerator.MapGeneratorCore.Algorithms
{
	//Алгоритм DiamondSquare
	public static class DiamondSquareAlgo
	{
		//Генерация карты размером w*h
		public static void Generate(int a, int w, int h, Random rand, bool[,] Init, int[,] Land, float H = 1f)
		{
			for (int p = a / 2; p > 0; p /= 2)
			{
				//Square
				for (int x = p; x < w; x += p * 2)
					for (int y = p; y < h; y += p * 2)
						if (!Init[x, y])
						{
							int p1 = Land[x - p, y - p];
							int p2 = Land[x - p, y + p];
							int p3 = Land[x + p, y - p];
							int p4 = Land[x + p, y + p];
							Land[x, y] = (p1 + p2 + p3 + p4) / 4 + (int)(rand.Next(-p * 2, p * 2) * H);
							Init[x, y] = true;
						}

				//Diamond 1
				for (int y = p; y < h; y += p * 2)
				{
					if (!Init[0, y])
					{
						int p2 = Land[0, y - p];
						int p3 = Land[0 + p, y];
						int p4 = Land[0, y + p];
						Land[0, y] = (p2 + p3 + p4) / 3 + (int)(rand.Next(-p, p) * H);
						Init[0, y] = true;
					}

					for (int x = p * 2; x < w; x += p * 2)
						if (!Init[x, y])
						{
							int p1 = Land[x - p, y];
							int p2 = Land[x, y - p];
							int p3 = Land[x + p, y];
							int p4 = Land[x, y + p];
							Land[x, y] = (p1 + p2 + p3 + p4) / 4 + (int)(rand.Next(-p, p) * H);
							Init[x, y] = true;
						}

					if (!Init[w, y])
					{
						int p1 = Land[w - p, y];
						int p2 = Land[w, y - p];
						int p4 = Land[w, y + p];
						Land[w, y] = (p1 + p2 + p4) / 3 + (int)(rand.Next(-p, p) * H);
						Init[w, y] = true;
					}
				}

				//Diamond 2
				for (int x = p; x < w; x += p * 2)
				{
					if (!Init[x, 0])
					{
						int p1 = Land[x - p, 0];
						int p3 = Land[x + p, 0];
						int p4 = Land[x, 0 + p];
						Land[x, 0] = (p1 + p3 + p4) / 3 + (int)(rand.Next(-p, p) * H);
						Init[x, 0] = true;
					}

					for (int y = p * 2; y < h; y += p * 2)
						if (!Init[x, y])
						{
							int p1 = Land[x - p, y];
							int p2 = Land[x, y - p];
							int p3 = Land[x + p, y];
							int p4 = Land[x, y + p];
							Land[x, y] = (p1 + p2 + p3 + p4) / 4 + (int)(rand.Next(-p, p) * H);
							Init[x, y] = true;
						}

					if (!Init[x, h])
					{
						int p1 = Land[x - p, h];
						int p2 = Land[x, h - p];
						int p3 = Land[x + p, h];
						Land[x, h] = (p1 + p2 + p3) / 3 + (int)(rand.Next(-p, p) * H);
						Init[x, h] = true;
					}
				}
			}
		}
	}
}
