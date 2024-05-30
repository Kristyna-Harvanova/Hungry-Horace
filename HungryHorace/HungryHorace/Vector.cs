using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungryHorace
{
    /// <summary>
    /// Directions of movement, in order of codes of arrow keys.
    /// </summary>
    public enum Direction
    { 
        Left = 0,
        Up = 1,
        Right = 2,
        Down = 3
    }

    //TODO: (opakovani kodu opraveno) od .NET verze 7 jde dat "where T: INumber", pak pujdou nadefinovat i pocetni operace
    abstract class Vector<T>
    {
        public T x;
        public T y;

        public Vector(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector() { }

        public void Set(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public void Set(Vector<T> v)
        { Set(v.x, v.y); }

        // Necessary for operand '==' a '!='.
        public override bool Equals(object obj)
        {
            Vector<T> v = obj as Vector<T>;
            return v != null && this == v;
        }

        public override int GetHashCode()
        { return (x, y).GetHashCode(); }
    }

    class VectorInt : Vector<int> 
    {
        public static VectorInt One = new VectorInt(1, 1);
        public static VectorInt Zero = new VectorInt(0, 0);

        public static VectorInt Left = new VectorInt(-1, 0);
        public static VectorInt Up = new VectorInt(0, -1);
        public static VectorInt Right = new VectorInt(1, 0);
        public static VectorInt Down = new VectorInt(0, 1);

        public VectorInt(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public VectorInt() { }

        public static implicit operator VectorFloat(VectorInt v)
        { return new VectorFloat(v.x, v.y); }

        /// <summary>
        /// Returns VectorInt according to Direction.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static VectorInt FromDirection(Direction d)
        {
            switch (d)
            {
                case Direction.Left:
                    return Left;
                case Direction.Up:
                    return Up;
                case Direction.Right:
                    return Right;
                case Direction.Down:
                    return Down;
            }
            return null;
        }

        public float GetMagnitude()
        { return (float)Math.Sqrt(x * x + y * y); }

        public static VectorInt operator + (VectorInt a, VectorInt b)
        { return new VectorInt(a.x + b.x, a.y + b.y); }

        public static VectorInt operator - (VectorInt a, VectorInt b)
        { return new VectorInt(a.x - b.x, a.y - b.y); }

        public static VectorInt operator * (VectorInt v, int a)
        { return new VectorInt(v.x * a, v.y * a); }

        public static VectorInt operator * (int a, VectorInt v)
        { return v * a; }

        public static VectorInt operator / (VectorInt v, float a)
        { return new VectorInt((int)(v.x / a), (int)(v.y / a)); }

        public static VectorInt operator / (float a, VectorInt v)
        { return v / a; }

        public static bool operator == (VectorInt a, VectorInt b)
        { return a.x == b.x && a.y == b.y; }

        public static bool operator != (VectorInt a, VectorInt b)
        { return !(a == b); }

        public override bool Equals(object obj)
        {
            VectorInt v = obj as VectorInt;
            return v != null && this == v;
        }

        public override int GetHashCode()
        { return (x, y).GetHashCode(); }
    }

    class VectorFloat : Vector<float>
    {
        public static VectorFloat One = new VectorFloat(1, 1);
        public static VectorFloat Zero = new VectorFloat(0, 0);

        public static VectorFloat Left = new VectorFloat(-1, 0);
        public static VectorFloat Up = new VectorFloat(0, -1);
        public static VectorFloat Right = new VectorFloat(1, 0);
        public static VectorFloat Down = new VectorFloat(0, 1);

        public VectorFloat(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public VectorFloat() { }

        public static explicit operator VectorInt(VectorFloat v)
        { return new VectorInt((int)v.x, (int)v.y); }   

        public float GetMagnitude()
        { return (float)Math.Sqrt(x * x + y * y); }

        /// <summary>
        /// Adjusting VectorFloat to value a.
        /// </summary>
        /// <param name="a"></param>
        public void SetMagnitude(float a)
        {
            float m = GetMagnitude();
            x = x / m * a;
            y = y / m * a;
        }

        public static VectorFloat operator + (VectorFloat a, VectorFloat b)
        { return new VectorFloat(a.x + b.x, a.y + b.y); }

        public static VectorFloat operator - (VectorFloat a, VectorFloat b)
        { return new VectorFloat(a.x - b.x, a.y - b.y); }

        public static VectorFloat operator * (VectorFloat v, float a)
        { return new VectorFloat(v.x * a, v.y * a); }

        public static VectorFloat operator * (float a, VectorFloat v)
        { return v * a; }

        public static VectorFloat operator / (VectorFloat v, float a)
        { return new VectorFloat(v.x / a, v.y / a); }

        public static VectorFloat operator / (float a, VectorFloat v)
        { return v / a; }

        public static bool operator == (VectorFloat a, VectorFloat b)
        { return a.x == b.x && a.y == b.y; }

        public static bool operator != (VectorFloat a, VectorFloat b)
        { return !(a == b); }

        public override bool Equals(object obj)
        {
            VectorFloat v = obj as VectorFloat;
            return v != null && this == v;
        }

        public override int GetHashCode()
        { return (x, y).GetHashCode(); }

        public override string ToString()
        { return $"{x}, {y}"; }
    }
}
