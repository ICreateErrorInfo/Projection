using System;
using System.Collections.Generic;
using System.Threading;
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
        List<Triangle> triangles = new List<Triangle>();

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

            Point3D[] points = new Point3D[8];

            points[0] = new Point3D(-1, -1, -1);  
            points[1] = new Point3D(1, -1, -1);
            points[2] = new Point3D(1, -1, 1);
            points[3] = new Point3D(-1, -1, 1);
            points[4] = new Point3D(-1, 1, -1);
            points[5] = new Point3D(1, 1, -1);
            points[6] = new Point3D(1, 1, 1);   
            points[7] = new Point3D(-1, 1, 1);

            int z = 0;
            foreach(Point3D element in points)
            {
                double[,] inputAsMatrix=
                {
                    {element.X },
                    {element.Y },
                    {element.Z }
                };
                double[,] MatrixErgebnis = Matrix.MultiplyMatrix(rotationX, inputAsMatrix);
                MatrixErgebnis = Matrix.MultiplyMatrix(rotationY, MatrixErgebnis);

                Point3D erg = new Point3D(MatrixErgebnis[0,0], MatrixErgebnis[1,0], MatrixErgebnis[2,0]);


                points[z] = erg;
                
                z++;
            }
            z = 0;
            foreach(Point3D element in points)
            {
                element.Z -= 4;
                points[z] = element;
                z++;
            }

            triangles.Clear();

            InitTriangles(points);

            Triangle[] projectedTriangles = new Triangle[triangles.Count];

            int i = 0;
            foreach (Triangle element in triangles)
            {
                Point3D line1 = new Point3D();
                line1.X = element.tp3.X - element.tp2.X;
                line1.Y = element.tp3.Y - element.tp2.Y;
                line1.Z = element.tp3.Z - element.tp2.Z;

                Point3D line2 = new Point3D();
                line2.X = element.tp1.X - element.tp2.X;    
                line2.Y = element.tp1.Y - element.tp2.Y;
                line2.Z = element.tp1.Z - element.tp2.Z;

                Point3D normal = new Point3D();
                normal.X = line2.Y * line1.Z - line2.Z * line1.Y;
                normal.Y = line2.Z * line1.X - line2.X * line1.Z;
                normal.Z = line2.X * line1.Y - line2.Y * line1.X;

                double l = Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z);

                normal.X /= l;
                normal.Y /= l;
                normal.Z /= l;

                if(normal.Z > 0)
                {
                        projectedTriangles[i] = PerspectiveProjectionMatrix(element);

                }
                i++;
            }
            viewTriangles(projectedTriangles);

        }
        private Triangle PerspectiveProjectionMatrix(Triangle input)
        {
            List<Point3D> pointList = new List<Point3D>();

            List<Point3D> inputPointList = new List<Point3D>();
            inputPointList.Add(input.tp1);
            inputPointList.Add(input.tp2);
            inputPointList.Add(input.tp3);

            double Fov = 90;
            double near = .1;
            double far = 100;
            double w = 1;

            double aspectRation = 1920 / 1080; // auflösung Bildschirm
            for(int i = 0; i < 3; i++)
            {
                double Rechnung2 = 1 / Math.Round(Math.Tan(ToRad(Fov / 2)), 3);
                double Rechnung1 = aspectRation * (Rechnung2);
                double Rechnung3 = far / (far - near);
                double Rechnung4 = (-far * near) / (far - near);

                double[,] matrix =
                {
                {Rechnung1, 0, 0, 0},
                {0, Rechnung2, 0, 0},
                {0, 0, Rechnung3, 1},
                {0, 0, Rechnung4, 0}
            };

                double[,] inputMatrix =
                {
                {inputPointList[i].X },
                {inputPointList[i].Y },
                {inputPointList[i].Z },
                {w },
            };

                double[,] ergebnis = Matrix.MultiplyMatrix(matrix, inputMatrix);
                double bx = ergebnis[0, 0];
                double by = ergebnis[1, 0];

                if (w != 0)
                {
                    bx = ergebnis[0, 0] / ergebnis[3, 0];
                    by = ergebnis[1, 0] / ergebnis[3, 0];
                }
                pointList.Add(new Point3D(bx, by , 0));
            }
            return new Triangle(pointList[0], pointList[1], pointList[2]);

        }
        private void InitTriangles(Point3D[] projectedPoints)
        {
            Triangle t1 = new Triangle(projectedPoints[0], projectedPoints[3], projectedPoints[2]);
            Triangle t2 = new Triangle(projectedPoints[2], projectedPoints[1], projectedPoints[0]);

            Triangle t3 = new Triangle(projectedPoints[4], projectedPoints[5], projectedPoints[6]);
            Triangle t4 = new Triangle(projectedPoints[6], projectedPoints[7], projectedPoints[4]);

            Triangle t5 = new Triangle(projectedPoints[3], projectedPoints[7], projectedPoints[6]);
            Triangle t6 = new Triangle(projectedPoints[6], projectedPoints[2], projectedPoints[3]);

            Triangle t7 = new Triangle(projectedPoints[0], projectedPoints[1], projectedPoints[5]);
            Triangle t8 = new Triangle(projectedPoints[5], projectedPoints[4], projectedPoints[0]);

            Triangle t9 = new Triangle(projectedPoints[5], projectedPoints[1], projectedPoints[2]);
            Triangle t10 = new Triangle(projectedPoints[2], projectedPoints[6], projectedPoints[5]);

            Triangle t11 = new Triangle(projectedPoints[0], projectedPoints[4], projectedPoints[7]);
            Triangle t12 = new Triangle(projectedPoints[7], projectedPoints[3], projectedPoints[0]);

            triangles.Add(t1);
            triangles.Add(t2);
            triangles.Add(t3);
            triangles.Add(t4);
            triangles.Add(t5);
            triangles.Add(t6);
            triangles.Add(t7);
            triangles.Add(t8);
            triangles.Add(t9);
            triangles.Add(t10);
            triangles.Add(t11);
            triangles.Add(t12);
        }
        private void viewTriangles(Triangle[] triarr)
        {
            foreach(Triangle tri in triarr)
            {
                _drawingSurface.Triangle(tri);
            }
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
    public Point3D()
    {
    }

    public double X;
    public double Y;
    public double Z;
}
class Triangle
{
    public Triangle(Point3D p1, Point3D p2, Point3D p3)
    {
        tp1 = p1;
        tp2 = p2;
        tp3 = p3;
    }

    public Point3D tp1;
    public Point3D tp2;
    public Point3D tp3;
}
class DrawingSurface
{

    public DrawingSurface() 
    {
        Surface                       = new Canvas();
        Surface.RenderTransformOrigin = new Point(0.5, 0.5);
        Surface.RenderTransform       = new ScaleTransform(1, 1);
;
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

        double height = Surface.ActualHeight / 2;
        double width  = Surface.ActualWidth / 2;

        int indicador = 50; // Skalierung

        objLine.X1 = (x.X * indicador) + width;
        objLine.Y1 = (x.Y * indicador) + height;

        objLine.X2 = (y.X * indicador) + width;
        objLine.Y2 = (y.Y * indicador) + height;


        Surface.Children.Add(objLine);
    }
    public void Triangle(Triangle tri)
    {
        if(tri != null)
        {
            Point p1 = new Point(tri.tp1.X, tri.tp1.Y);
            Point p2 = new Point(tri.tp2.X, tri.tp2.Y);
            Point p3 = new Point(tri.tp3.X, tri.tp3.Y);

            Line(p1, p2);
            Line(p2, p3);
            //Line(p1, p3);
        }
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

