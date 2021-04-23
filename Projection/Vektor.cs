using System;

namespace Projection {

    readonly struct Vektor
    {
        public Vektor(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            W = 1;
        }
        public Vektor(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public Vektor(Vektor anfang, Vektor ende)
        {
            X = ende.X - anfang.X;
            Y = ende.Y - anfang.Y;
            Z = ende.Z - anfang.Z;
            W = 1;
        }
   
        public double X {get;}
        public double Y {get;}
        public double Z {get;}
        public double W {get;}

        public static double Length(Vektor v)
        {
            return Math.Sqrt(DotProduct(v, v));
        }
        public static double DotProduct(Vektor v1, Vektor v2)
        {
            return v1.X * v2.X +
                   v1.Y * v2.Y +
                   v1.Z * v2.Z;
        }
        public static Vektor DotProductVek(Vektor v1, Vektor v2)
        {
            return v1 * v2;
        }
        public static Vektor Normalise(Vektor v)
        {
            double l = Length(v);
            return new Vektor(v.X / l, v.Y / l, v.Z / l);
        }
        public static Vektor CrossProduct(Vektor v1, Vektor v2)
        {
            Vektor v = new Vektor(
                x: v1.Y * v2.Z - v1.Z * v2.Y,
                y: v1.Z * v2.X - v1.X * v2.Z,
                z: v1.X * v2.Y - v1.Y * v2.X);
        
            return v;
        }
        public static Vektor CalcNormals(Vektor v1, Vektor v2)
        {
            Vektor v = CrossProduct(v1, v2);
            v = Normalise(v);
            return new Vektor(
                x: v.X,
                y: v.Y,
                z: v.Z);
        }
        public static double[,] ToMatrix(Vektor v)
        {
            double[,] matrix =
            {
                { v.X},
                { v.Y},
                { v.Z},
                { v.W}
            };
            return matrix;
        }
        public static Vektor operator +(Vektor v1, Vektor v2)
        {
            return new Vektor(v1.X + v2.X,
                              v1.Y + v2.Y,
                              v1.Z + v2.Z);
        }
        public static Vektor operator -(Vektor v1, Vektor v2)
        {
            return new Vektor(v1.X - v2.X,
                              v1.Y - v2.Y,
                              v1.Z - v2.Z);
        }
        public static Vektor operator *(Vektor v1, Vektor v2)
        {
            return new Vektor(v1.X * v2.X,
                              v1.Y * v2.Y,
                              v1.Z * v2.Z);
        }
        public static Vektor operator /(Vektor v1, Vektor v2)
        {
            return new Vektor(v1.X / v2.X,
                              v1.Y / v2.Y,
                              v1.Z / v2.Z);
        }

    
    }

}