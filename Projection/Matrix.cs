using System;

namespace Projection {

    class Matrix
    {
        public static double[,] MultiplyMatrix(double[,] a, double[,] b)
        {
            int       rA     = a.GetLength(0);
            int       cA     = a.GetLength(1);
            int       rB     = b.GetLength(0);
            int       cB     = b.GetLength(1);
            double    temp   = 0;
            double[,] kHasil = new double[rA, cB];
            if (cA != rB)
            {
                Console.WriteLine("matrik can't be multiplied !!");
            }
            else
            {
                for (int i = 0; i < rA; i++)
                {
                    for (int j = 0; j < cB; j++)
                    {
                        temp = 0;
                        for (int k = 0; k < cA; k++)
                        {
                            temp += a[i, k] * b[k, j];
                        }
                        kHasil[i, j] = temp;
                    }
                }
                return kHasil;
            }
            return null;
        }
        public static Vektor MultiplyVektor(double[,] a, Vektor v)
        {
            double[,] b =
            {
                { v.X},
                { v.Y},
                { v.Z},
                { v.W}
            };

            return ToVektor(MultiplyMatrix(a, b));
        }
        public static double[,] RotateX(double angle)
        {
            double[,] rotationX =
            {
                {1,0,0, 0},
                {0, Math.Cos(Mathe.ToRad(angle)), Math.Sin(Mathe.ToRad(angle) * -1),0},
                {0, Math.Sin(Mathe.ToRad(angle)), Math.Cos(Mathe.ToRad(angle)),0},
                {0,0,0,1 }
            };

            return rotationX;
        }
        public static double[,] RotateY(double angle)
        {
            double[,] rotationY =
            {
                {Math.Cos(Mathe.ToRad(angle)),0 ,Math.Sin(Mathe.ToRad(angle) * -1),0},
                {0, 1, 0,0},
                {Math.Sin(Mathe.ToRad(angle)),0 ,Math.Cos(Mathe.ToRad(angle)),0},
                {0,0,0,1 }
            };

            return rotationY;
        }
        public static double[,] RotateZ(double angle)
        {
            double[,] rotationZ =
            {
                {Math.Cos(Mathe.ToRad(angle)), Math.Sin(Mathe.ToRad(angle) * -1), 0 ,0},
                {Math.Sin(Mathe.ToRad(angle)), Math.Cos(Mathe.ToRad(angle)), 0 ,0},
                {0, 0, 1 , 0},
                {0, 0, 0 , 1},
            };

            return rotationZ;
        }
        public static double[,] Translation(double x, double y, double z)
        {
            double[,] matrix =
            {
                { 1, 0, 0, x },
                { 0, 1, 0, y },
                { 0, 0, 1, z },
                { 0, 0, 0, 1 }
            };
            return matrix;
        }
        public static double[,] Identety()
        {
            double[,] matrix =
            {
                {1,0,0,0 },
                {0,1,0,0 },
                {0,0,1,0 },
                {0,0,0,1 }
            };

            return matrix;
        }
        public static double[,] Projecton(double fov, double aspectRatio, double far, double near)
        {
            double rechnung2 = 1             / Math.Round(Math.Tan(Mathe.ToRad(fov / 2)), 3);
            double rechnung1 = aspectRatio   * (rechnung2);
            double rechnung3 = far           / (far - near);
            double rechnung4 = (-far * near) / (far - near);

            double[,] matrix =
            {
                { rechnung1, 0, 0, 0 },
                { 0, rechnung2, 0, 0 },
                { 0, 0, rechnung3, 1 },
                { 0, 0, rechnung4, 0 }
            };

            return matrix;
        }
        public static double[,] WorldMatrix(Vektor angle, Vektor abstand)
        {
            double[,] rotateX = RotateX(angle.X);
            double[,] rotateY = RotateY(angle.Y);
            //double[,] rotateZ = Matrix.RotateZ(angle);

            double[,] translate = Translation(abstand.X, abstand.Y, abstand.Z);

            double[,] worldMatrix = Identety();
            worldMatrix = MultiplyMatrix(worldMatrix, translate);
            worldMatrix = MultiplyMatrix(worldMatrix, rotateX);
            worldMatrix = MultiplyMatrix(worldMatrix, rotateY);
            //worldMatrix = Matrix.MultiplyMatrix(worldMatrix, rotateZ);

            return worldMatrix;
        }
        public static double[,] PointAt(Vektor pos, Vektor target, Vektor up)
        {
            Vektor zaxis = pos - target;
            zaxis = zaxis.Normalise();

            Vektor xaxis = Vektor.CrossProduct(up, zaxis);
            xaxis = xaxis.Normalise();

            Vektor yaxis = Vektor.CrossProduct(zaxis, xaxis);

            double[,] matrix =
            {
                {xaxis.X, xaxis.Y, xaxis.Z,pos.X},
                {yaxis.X, yaxis.Y, yaxis.Z,pos.Y},
                {zaxis.X, zaxis.Y, zaxis.Z,pos.Z},
                {0,0,0, 1}
            };

            return matrix;
        }
        public static double[,] lookAt(double[,] m)
        {
            Vektor eye = new Vektor(m[0,3], m[1,3], m[2,3]);
            Vektor xaxis = new Vektor(m[0,0], m[0,1], m[0,2]);
            Vektor yaxis = new Vektor(m[1,0], m[1,1], m[1,2]);
            Vektor zaxis = new Vektor(m[2,0], m[2,1], m[2,2]);

            double[,] matrix =
            {
                {xaxis.X, yaxis.X, zaxis.X, -Vektor.DotProduct(xaxis, eye)},
                {xaxis.Y, yaxis.Y, zaxis.Y, -Vektor.DotProduct(yaxis, eye)},
                {xaxis.Z, yaxis.Z, zaxis.Z, -Vektor.DotProduct(zaxis, eye)},
                {0,0,0,1}
            };

            return matrix;
        }
        public static Vektor ToVektor(double[,] a)
        {
            return new Vektor(a[0, 0], a[1, 0], a[2, 0], a[3, 0]);
        }
    }

}