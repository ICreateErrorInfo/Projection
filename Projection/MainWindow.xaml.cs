using Projection;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
        double angle1 = 0;
        public MainWindow()
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Start();

            InitializeComponent();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            MainGrid.Children.Clear();
            Draw.Update();
            Project(angle1);
            MainGrid.Children.Add(Draw.MainGrid1);
            Thread.Sleep(5);
            angle1 += 0.1;
        }
        public void Project(double angle)
        {
            Point3D[] points = new Point3D[8];
            double[,] projection = { { 1, 0, 0 }, { 0, 1, 0 }};
            double[,] rotationZ =
            {
            {Math.Round(Math.Cos(ToRad(angle)), 3), Math.Round(Math.Sin(ToRad(angle) * -1), 3), 0},
            {Math.Round(Math.Sin(ToRad(angle)), 3), Math.Round(Math.Cos(ToRad(angle)), 3), 0},
                {0,0,0 }
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

            points[0] = new Point3D(-5,-5,-5);
            points[1] = new Point3D(5, -5, -5);
            points[2] = new Point3D(5, 5, -5);
            points[3] = new Point3D(-5, 5, -5);
            points[4] = new Point3D(-5, -5, 5);
            points[5] = new Point3D(5, -5, 5);
            points[6] = new Point3D(5, 5, 5);
            points[7] = new Point3D(-5, 5, 5);

            Point3D[] projectedPoints = new Point3D[8];

            int i = 0;
            foreach (Point3D element in points)
            {
                projectedPoints[i] = Matrix.mul2(projection, element);
                projectedPoints[i] = Matrix.mul3(rotationY, projectedPoints[i]);
                projectedPoints[i] = Matrix.mul3(rotationX, projectedPoints[i]);
                projectedPoints[i] = Matrix.mul3(rotationZ, projectedPoints[i]);
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

        }
        public static double ToRad(double deg) => deg * Math.PI / 180;
    }
}
class Point3D
{
    public Point3D(double x, double y, double z)
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

    public double X { get; }
    public double Y { get; }
    public double Z { get; }

}
class Draw
{
    public static Grid MainGrid1 = new Grid();
    public static void Update()
    {

        MainGrid1 = new Grid();
    }
    public static void Line(Point3D x, Point3D y)
    {
        Line objLine = new Line();

        objLine.Stroke = System.Windows.Media.Brushes.Black;
        objLine.Fill = System.Windows.Media.Brushes.Black;

        double height = System.Windows.SystemParameters.PrimaryScreenHeight / 2;
        double width = System.Windows.SystemParameters.PrimaryScreenWidth / 2;

        int indicador = 50;

        objLine.X1 = (x.X * indicador) + width;
        objLine.Y1 = (x.Y * indicador) + height;

        objLine.X2 = (y.X * indicador) + width;
        objLine.Y2 = (y.Y * indicador) + height;


        MainGrid1.Children.Add(objLine);
    }
}
class Matrix
{
    public static Point3D mul2(double[,] a, Point3D b)
    {
        double ergebnis0 = a[0, 0] * b.X;
        ergebnis0 += a[0, 1] * b.Y;
        ergebnis0 += a[0, 2] * b.Z;

        double ergebnis1 = a[1, 0] * b.X;
        ergebnis1 += a[1, 1] * b.Y;
        ergebnis1 += a[1, 2] * b.Z;

        Point3D end = new Point3D(ergebnis0, ergebnis1, b.Z);
        return end;
    }
    public static Point3D mul3(double[,] a, Point3D b)
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

