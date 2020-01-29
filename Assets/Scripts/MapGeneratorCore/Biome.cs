using System;
using System.Collections.Generic;
using UnityEngine;
using UtilsLib.Logic;

//Настройки биома
[Serializable]
public class Biome
{
    [HideInInspector]
    public List<MyColor> Colors = new List<MyColor>(); //Цвет биома по высоте

    public MyColor GetColor(int y)
    {
        if (Colors.Count > 0)
            return Colors[Math.Min(Math.Max(0, y), Colors.Count - 1)];

        return new MyColor();
    }
}