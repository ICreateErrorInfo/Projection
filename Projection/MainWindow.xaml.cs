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
        double                  _currentAngle;
        double _scale = 45;
        double _distanceToObject = 10;
        private DispatcherTimer _timer;
        public MainWindow()
        {
             _timer = new DispatcherTimer();
            _timer.Tick     += OnTick;
            _timer.Interval =  TimeSpan.FromMilliseconds(5);
            _timer.Start();

            InitializeComponent();
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
            MainGrid.Children.Clear();
            Draw.Clear();
            Project(_currentAngle);
            MainGrid.Children.Add(Draw.MainGrid1);
            _currentAngle += 0.1;
        }
        public void Project(double angle)
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
                projectedPoints[i] = PerspectiveProjection(element, cameraPos, cameraRotation, vecplane);
                i++;

            }

            Draw.Line(projectedPoints[0], projectedPoints[1]);
            Draw.Line(projectedPoints[2], projectedPoints[1]);
            Draw.Line(projectedPoints[2], projectedPoints[3]);
            Draw.Line(projectedPoints[0], projectedPoints[3]);

            Draw.Line(projectedPoints[4], projectedPoints[5]);
            Draw.Line(projectedPoints[5], projectedPoints[6]);
            Draw.Line(projectedPoints[6], projectedPoints[7]);
            Draw.Line(projectedPoints[4], projectedPoints[7]);

            Draw.Line(projectedPoints[4], projectedPoints[0]);
            Draw.Line(projectedPoints[5], projectedPoints[1]);
            Draw.Line(projectedPoints[3], projectedPoints[7]);
            Draw.Line(projectedPoints[6], projectedPoints[2]);

            Draw.Rectangle(projectedPoints[4], projectedPoints[5], projectedPoints[6], projectedPoints[7]);

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
class Draw
{
    public static Grid MainGrid1 = new Grid();
    public static void Clear()
    {

        MainGrid1 = new Grid();
    }
    public static void Line(Point x, Point y)
    {
        Line objLine = new Line();

        objLine.Stroke = System.Windows.Media.Brushes.Black;
        objLine.Fill = System.Windows.Media.Brushes.Black;

        double height = SystemParameters.PrimaryScreenHeight / 2;
        double width = SystemParameters.PrimaryScreenWidth / 2;

        int indicador = 50;

        objLine.X1 = (x.X * indicador) + width;
        objLine.Y1 = (x.Y * indicador) + height;

        objLine.X2 = (y.X * indicador) + width;
        objLine.Y2 = (y.Y * indicador) + height;


        MainGrid1.Children.Add(objLine);
    }
    public static void Rectangle(Point p1, Point p2, Point p3, Point p4)
    {
       
    }
}
class Matrix
{
    public static Point Mul2(double[,] a, Point3D b)
    {
        double ergebnis0 = a[0, 0] * b.X;
        ergebnis0 += a[0, 1] * b.Y;
        ergebnis0 += a[0, 2] * b.Z;

        double ergebnis1 = a[1, 0] * b.X;
        ergebnis1 += a[1, 1] * b.Y;
        ergebnis1 += a[1, 2] * b.Z;

        Point end = new Point(ergebnis0, ergebnis1);
        return end;
    }
    public static Point3D Mul3(double[,] a, Point3D b)
    {
        double ergebnis0 = a[0, 0] * b.X;
        ergebnis0 += a[0, 1] * b.Y;
        ergebnis0 += a[0, 2] * b.Z;

        double ergebnis1 = a[1, 0] * b.X;
        ergebnis1 += a[1, 1] * b.Y;
        ergebnis1 += a[1, 2] * b.Z;

        double ergebnis2 = a[2, 0] * b.X;
        ergebnis2 += a[2, 1] * b.Y;
        ergebnis2 += a[2, 2] * b.Z;

        Point3D end = new Point3D(ergebnis0, ergebnis1, ergebnis2);
        return end;
    }
}

