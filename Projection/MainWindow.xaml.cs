using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Projection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        double _currentAngle;
        double _scale            = 45;
        double _distanceToObject = 10;

        private readonly DispatcherTimer _timer;
        private readonly DrawingSurface _drawingSurface;

        public MainWindow()
        {
            InitializeComponent();

            _drawingSurface = new DrawingSurface();
            MainGrid.Children.Add(_drawingSurface.Surface);

            _timer          =  new DispatcherTimer();
            _timer.Tick     += OnTick;
            _timer.Interval =  TimeSpan.FromMilliseconds(5);
            _timer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e) 
        {
            if (e.Key == Key.P) 
            {
                _timer.IsEnabled ^= true;
                e.Handled        =  true;
            }
            base.OnKeyDown(e);
        }
        private void OnTick(object sender, EventArgs e)
        {
            _drawingSurface.Clear();
            Project(++_currentAngle);
        }
        void Project(double angle)
        {
            Point3D[] points = new Point3D[8];
            
            double[,] rotationZ =
            {
            {Math.Round(Math.Cos(ToRad(angle)), 3), Math.Round(Math.Sin(ToRad(angle) * -1), 3), 0},
            {Math.Round(Math.Sin(ToRad(angle)), 3), Math.Round(Math.Cos(ToRad(angle)), 3), 0},
                {0,0,1 }
            };

            double[,] rotationX =
            {
                {1,0,0 },
            {0, Math.Round(Math.Cos(ToRad(angle)), 3), Math.Round(Math.Sin(ToRad(angle) * -1), 3)},
            {0, Math.Round(Math.Sin(ToRad(angle)), 3), Math.Round(Math.Cos(ToRad(angle)), 3)},
            };
            double[,] rotationY =
            {
                {Math.Round(Math.Cos(ToRad(angle)), 3),0, Math.Round(Math.Sin(ToRad(angle) * -1), 3)},
            {0, 1, 0},
            {Math.Round(Math.Sin(ToRad(angle)), 3),0, Math.Round(Math.Cos(ToRad(angle)), 3)},
            };

            points[0] = new Point3D(-1,-1,-1);
            points[1] = new Point3D(1, -1, -1);
            points[2] = new Point3D(1, 1, -1);
            points[3] = new Point3D(-1, 1, -1);
            points[4] = new Point3D(-1, -1, 1);
            points[5] = new Point3D(1, -1, 1);
            points[6] = new Point3D(1, 1, 1);
            points[7] = new Point3D(-1, 1, 1);

            Point[] projectedPoints = new Point[8];

            int i = 0;
            /*foreach (Point3D element in points)
            {
                Point3D rotated = Matrix.Mul3(rotationY, element);
                rotated = Matrix.Mul3(rotationX, rotated);
                rotated = Matrix.Mul3(rotationZ, rotated);

                double z = 1 / (_distanceToObject - rotated.Z);

                double[,] projection = { 

                    { z, 0, 0 },
                    { 0, z, 0 }
                    
                };

                projectedPoints[i] = Matrix.Mul2(projection, rotated);
                projectedPoints[i].X *= _scale;
                projectedPoints[i].Y *= _scale;
                i++;
            }*/

            Point3D cameraRotation = new Point3D(62, 0, -206);
            Point3D cameraPos = new Point3D(2, 4, 2);

            Point3D vecplane = new Point3D(6, 6, 6);

            foreach (Point3D element in points)
            {
                //projectedPoints[i] = PerspectiveProjection(element, cameraPos, cameraRotation, vecplane);
                projectedPoints[i] = PerspectiveProjectionMatrix(element, cameraPos, cameraRotation, vecplane);
                i++;

            }

            _drawingSurface.Line(projectedPoints[0], projectedPoints[1]);
            _drawingSurface.Line(projectedPoints[2], projectedPoints[1]);
            _drawingSurface.Line(projectedPoints[2], projectedPoints[3]);
            _drawingSurface.Line(projectedPoints[0], projectedPoints[3]);

            _drawingSurface.Line(projectedPoints[4], projectedPoints[5]);
            _drawingSurface.Line(projectedPoints[5], projectedPoints[6]);
            _drawingSurface.Line(projectedPoints[6], projectedPoints[7]);
            _drawingSurface.Line(projectedPoints[4], projectedPoints[7]);

            _drawingSurface.Line(projectedPoints[4], projectedPoints[0]);
            _drawingSurface.Line(projectedPoints[5], projectedPoints[1]);
            _drawingSurface.Line(projectedPoints[3], projectedPoints[7]);
            _drawingSurface.Line(projectedPoints[6], projectedPoints[2]);

            _drawingSurface.Rectangle(projectedPoints[4], projectedPoints[5], projectedPoints[6], projectedPoints[7]);

        }
        private Point PerspectiveProjection(Point3D PosProjPoint, Point3D PosCamera, Point3D RotationCamera, Point3D DispSurfPos)
        {
            double cx = Calpha(RotationCamera.X);
            double cy = Calpha(RotationCamera.Y);
            double cz = Calpha(RotationCamera.Z);

            double sx = Salpha(RotationCamera.X);
            double sy = Salpha(RotationCamera.Y);
            double sz = Salpha(RotationCamera.Z);

            double x = xyz(PosProjPoint.X, PosCamera.X);
            double y = xyz(PosProjPoint.Y, PosCamera.Y);
            double z = xyz(PosProjPoint.Z, PosCamera.Z);

            double dx = cy * (sz * y + cz * x) - sy * z;
            double dy = sx * (cy * z + sy * (sz * y + cz * x)) + cx * (cz * y - sz * x);
            double dz = cx * (cy * z + sy * (sz * y + cz * x)) - sx * (cz * y - sz * x);

            double bx = (DispSurfPos.Z / dz) * dx + DispSurfPos.X;
            double by = (DispSurfPos.Z / dz) * dy + DispSurfPos.Y;

            Point b = new Point(bx, by);
            return b;

        }
        private Point PerspectiveProjectionMatrix(Point3D PosProjPoint, Point3D PosCamera, Point3D RotationCamera, Point3D DispSurfPos)
        {
            double cx = Calpha(RotationCamera.X);
            double cy = Calpha(RotationCamera.Y);
            double cz = Calpha(RotationCamera.Z);

            double sx = Salpha(RotationCamera.X);
            double sy = Salpha(RotationCamera.Y);
            double sz = Salpha(RotationCamera.Z);

            double x = xyz(PosProjPoint.X, PosCamera.X);
            double y = xyz(PosProjPoint.Y, PosCamera.Y);
            double z = xyz(PosProjPoint.Z, PosCamera.Z);

            double[,] mat1 =
            {
                { 1, 0, 0},
                { 0, cx, sx},
                { 0, -sx, cx}
            };
            double[,] mat2 =
            {
                { cy, 0, -sy},
                { 0, 1, 0},
                { sy, 0, cy}
            };
            double[,] mat3 =
            {
                { cz, sz, 0},
                { -sz, cz, 0},
                { 0, 0, 1}
            };
            double[,] mat4 =
            {
                {x },
                {y },
                {z }
            };

            double[,] d = Matrix.MultiplyMatrix(mat1, mat2);
            d = Matrix.MultiplyMatrix(d, mat3);
            d = Matrix.MultiplyMatrix(d, mat4);

            double bx = (DispSurfPos.Z / d[2, 0]) * d[0, 0] + DispSurfPos.X;
            double by = (DispSurfPos.Z / d[2, 0]) * d[1, 0] + DispSurfPos.Y;

            Point b = new Point(bx, by);
            return b;

        }
        public double Calpha(double alpha)
        {
            return Math.Round(Math.Cos(ToRad(alpha)), 3);
        }
        public double Salpha(double alpha)
        {
            return Math.Round(Math.Sin(ToRad(alpha)), 3);
        }
        public double xyz(double xyz, double xyz1)
        {
            return xyz - xyz1;
        }
        public static double ToRad(double deg) => deg * Math.PI / 180;
    }
}
class Point3D
{
    public  Point3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Point3D(Point3D a)
    {
        X = a.X;
        Y = a.Y;
        Z = a.Z;
    }

    public double X;
    public double Y;
    public double Z;
}
class DrawingSurface
{

    public DrawingSurface() 
    {
        Surface                       = new Canvas();
        Surface.RenderTransformOrigin = new Point(0.5, 0.5);
        Surface.RenderTransform       = new ScaleTransform(1, 1);
    }

    public Canvas Surface { get; }

    public void Clear()
    {
        Surface.Children.Clear();
    }
    public  void Line(Point x, Point y)
    {
        Line objLine = new Line();

        objLine.Stroke = Brushes.Black;
        objLine.Fill = Brushes.Black;

        // Offset in x und y?
        double height = Surface.ActualHeight / 2;
        double width  = Surface.ActualWidth / 2;

        int indicador = 50; // Skalierung?

        objLine.X1 = (x.X * indicador) + width;
        objLine.Y1 = (x.Y * indicador) + height;

        objLine.X2 = (y.X * indicador) + width;
        objLine.Y2 = (y.Y * indicador) + height;


        Surface.Children.Add(objLine);
    }
    public void Rectangle(Point p1, Point p2, Point p3, Point p4)
    {
       
    }
}
class Matrix
{
    public static double[,] MultiplyMatrix(double[,] A, double[,] B)
    {
        int rA = A.GetLength(0);
        int cA = A.GetLength(1);
        int rB = B.GetLength(0);
        int cB = B.GetLength(1);
        double temp = 0;
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
                        temp += A[i, k] * B[k, j];
                    }
                    kHasil[i, j] = temp;
                }
            }
            return kHasil;
        }
        return null;
    }
}

