using System;
using UtilsLib.Logic;

namespace Assets.Scripts.MapGenerator.MapGeneratorCore
{
    //Воксельная структура
    public class BlockSource
    {
        public int H = 4; //толшина структуры
        public int OffsetX, OffsetZ; //смещение от начала координат
        public int SizeX, SizeZ; //размер структуры
        public int[,] Land; //карта высот
        public MyColor[,] Colors; //цвет вокселей

        public BlockSource(int offsetX, int offsetZ, int sizex, int sizez)
        {
            OffsetX = offsetX;
            OffsetZ = offsetZ;
            SizeX = sizex;
            SizeZ = sizez;

            Land = new int[SizeX + 1, SizeZ + 1];

            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeZ; j++)
                    Land[i, j] = int.MinValue;

            Colors = new MyColor[SizeX + 1, SizeZ + 1];
        }

        //Узнать высоту, в локальных координатах
        public int GetBlockLocal(int x, int z)
        {
            if (x < 0 || x >= SizeX || z < 0 || z >= SizeZ)
                return int.MinValue;

            return Land[x, z];
        }

        //Узнать высоту, в глобальных координатах
        public int GetBlockGlobal(int x, int z)
        {
            x -= OffsetX;
            z -= OffsetZ;

            if (x < 0 || x >= SizeX || z < 0 || z >= SizeZ)
                return int.MinValue;

            return Land[x, z];
        }

        //Проверить воксель в точке, в локальных координатах
        public bool HaveBlockLocal(int x, int y, int z)
        {
            if (x < 0 || x >= SizeX || z < 0 || z >= SizeZ)
                return false;

            return Land[x, z] >= y && Land[x, z] - H < y;
        }

        //Проверить воксель в точке, в глобальных координатах
        public bool HaveBlockGlobal(int x, int y, int z)
        {
            x -= OffsetX;
            z -= OffsetZ;

            if (x < 0 || x >= SizeX || z < 0 || z >= SizeZ)
                return false;

            return Land[x, z] >= y && Land[x, z] - H < y;
        }

        //Сгенерировать меш
        public MyQuadMesh MakeGroundMesh(LocationCreator lc, MyVector offset, int h)
        {
            var quadMesh = new MyQuadMesh();

            //Проходим по всем клеткам блока
            for (int x = 0; x < SizeX; x++)
                for (int z = 0; z < SizeZ; z++)
                {
                    int totalX = OffsetX + x;
                    int totalZ = OffsetZ + z;
                    int currentH = h;

                    //Проверяем цвет
                    var color = Colors[x, z];
                    if (color == null)
                        continue;

                    var y = Land[x, z];

                    //Строим верхнюю грань
                    {
                        if (lc.Land[x, z] - 1 != y)
                        {
                            var colorUp = color;

                            int v = y;
                            var color1 = lc.GetColor(colorUp, x, z, v + 1);
                            var color2 = lc.GetColor(colorUp, x + 1, z, v + 1);
                            var color3 = lc.GetColor(colorUp, x, z + 1, v + 1);
                            var color4 = lc.GetColor(colorUp, x + 1, z + 1, v + 1);
                            quadMesh.CreateY1(x + offset.X, v + offset.Y, z + offset.Z, new[] { color1, color2, color3, color4 });
                        }
                    }

                    //Строим боковые грани X0
                    if (totalX > 0)
                    {
                        var v0 = lc.Land[totalX - 1, totalZ];

                        for (int v = Math.Max(v0 + 1, y - currentH); v <= y; v++)
                        {
                            var color1 = lc.GetColor(color, x, z, v);
                            var color2 = lc.GetColor(color, x, z, v + 1);
                            var color3 = lc.GetColor(color, x, z + 1, v);
                            var color4 = lc.GetColor(color, x, z + 1, v + 1);
                            quadMesh.CreateX0(x + offset.X, v + offset.Y, z + offset.Z, new[] { color1, color2, color3, color4 });
                        }

                    }

                    //Строим боковые грани X1
                    if (lc.GenerateVoxelSideX1 && totalX < SizeX - 1)
                    {
                        var v0 = lc.Land[totalX + 1, totalZ];

                        for (int v = Math.Max(v0 + 1, y - currentH); v <= y; v++)
                        {
                            var color1 = lc.GetColor(color, x + 1, z, v);
                            var color2 = lc.GetColor(color, x + 1, z + 1, v);
                            var color3 = lc.GetColor(color, x + 1, z, v + 1);
                            var color4 = lc.GetColor(color, x + 1, z + 1, v + 1);
                            quadMesh.CreateX1(x + offset.X, v + offset.Y, z + offset.Z, new[] { color1, color2, color3, color4 });
                        }
                    }

                    //Строим боковые грани Z0
                    if (lc.GenerateVoxelSideZ0 && totalZ > 0)
                    {
                        var v0 = lc.Land[totalX, totalZ - 1];

                        for (int v = Math.Max(v0 + 1, y - currentH); v <= y; v++)
                        {
                            var color1 = lc.GetColor(color, x, z, v);
                            var color2 = lc.GetColor(color, x + 1, z, v);
                            var color3 = lc.GetColor(color, x, z, v + 1);
                            var color4 = lc.GetColor(color, x + 1, z, v + 1);
                            quadMesh.CreateZ0(x + offset.X, v + offset.Y, z + offset.Z, new[] { color1, color2, color3, color4 });
                        }
                    }

                    //Строим боковые грани Z1
                    if (totalZ < SizeZ - 1)
                    {
                        var v0 = lc.Land[totalX, totalZ + 1];

                        for (int v = Math.Max(v0 + 1, y - currentH); v <= y; v++)
                        {
                            var color1 = lc.GetColor(color, x, z + 1, v);
                            var color2 = lc.GetColor(color, x, z + 1, v + 1);
                            var color3 = lc.GetColor(color, x + 1, z + 1, v);
                            var color4 = lc.GetColor(color, x + 1, z + 1, v + 1);
                            quadMesh.CreateZ1(x + offset.X, v + offset.Y, z + offset.Z, new[] { color1, color2, color3, color4 });
                        }
                    }
                }

            return quadMesh;
        }
    }
}
