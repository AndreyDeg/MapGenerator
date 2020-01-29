using System;
using System.Xml.Serialization;
using UnityEngine;

namespace UtilsLib.Logic
{
	[Serializable]
	public class MyVector
	{
		[XmlAttribute]
		public int X, Y, Z;

		public MyVector()
		{
		}

		public MyVector(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static MyVector operator +(MyVector lhs, MyVector rhs)
		{
			return new MyVector(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
		}

		public static MyVector operator -(MyVector lhs, MyVector rhs)
		{
			return new MyVector(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
		}

		public static MyVector operator /(MyVector lhs, int rhs)
		{
			return new MyVector(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
		}
	}

	[Serializable]
	public class MyVectorFloat
	{
		[XmlAttribute]
		public float x, y, z;

		public MyVectorFloat()
		{
		}

		public MyVectorFloat(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static MyVectorFloat operator +(MyVectorFloat lhs, MyVectorFloat rhs)
		{
			return new MyVectorFloat(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
		}

		public static MyVectorFloat operator -(MyVectorFloat lhs, MyVectorFloat rhs)
		{
			return new MyVectorFloat(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
		}

		public static MyVectorFloat operator *(MyVectorFloat lhs, float rhs)
		{
			return new MyVectorFloat(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
		}

		public static MyVectorFloat operator /(MyVectorFloat lhs, float rhs)
		{
			return new MyVectorFloat(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
		}
	}

	[Serializable]
	public class MyColor
	{
		[XmlAttribute]
		public int R, G, B, A = 255;

		public static MyColor FromUnityColor(Color color)
		{
			return new MyColor { R = (int)(color.r * 255), G = (int)(color.g * 255), B = (int)(color.b * 255), A = (int)(color.a * 255) };
		}

		public static MyColor FromArgb(byte r, byte g, byte b, byte a = 255)
		{
			return new MyColor { R = r, G = g, B = b, A = a };
		}

		public static MyColor FromArgb(int r, int g, int b, int a = 255)
		{
			return new MyColor { R = r, G = g, B = b, A = a };
		}

		public static bool operator ==(MyColor lhs, MyColor rhs)
		{
			if ((object)lhs == null)
				return (object)rhs == null;
			if ((object)rhs == null)
				return false;
			return lhs.R == rhs.R && lhs.G == rhs.G && lhs.B == rhs.B && lhs.A == rhs.A;
		}

		public static bool operator !=(MyColor lhs, MyColor rhs)
		{
			return !(lhs == rhs);
		}

		public static MyColor operator +(MyColor lhs, MyColor rhs)
		{
			return FromArgb(lhs.R + rhs.R, lhs.G + rhs.G, lhs.B + rhs.B, lhs.A + rhs.A);
		}

		public static MyColor operator -(MyColor lhs, MyColor rhs)
		{
			return FromArgb(lhs.R - rhs.R, lhs.G - rhs.G, lhs.B - rhs.B, lhs.A - rhs.A);
		}

		public static MyColor operator *(MyColor lhs, int rhs)
		{
			return FromArgb(lhs.R * rhs, lhs.G * rhs, lhs.B * rhs, lhs.A * rhs);
		}

		public static MyColor operator *(MyColor lhs, float rhs)
		{
			return FromArgb((int)(lhs.R * rhs), (int)(lhs.G * rhs), (int)(lhs.B * rhs), (int)(lhs.A * rhs));
		}

		public static MyColor operator /(MyColor lhs, int rhs)
		{
			return FromArgb(lhs.R / rhs, lhs.G / rhs, lhs.B / rhs, lhs.A / rhs);
		}

		public override string ToString()
		{
			return string.Format("MyColor({0},{1},{2},{3})", R, G, B, A);
		}

		public bool Same(MyColor color, int delta = 0)
		{
			if (Math.Abs(R - color.R) > delta)
				return false;
			if (Math.Abs(G - color.G) > delta)
				return false;
			if (Math.Abs(B - color.B) > delta)
				return false;

			return true;
		}
	}
}
