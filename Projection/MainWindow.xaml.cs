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

            points[0] = new Point3D(-1, -1, -5);  
            points[1] = new Point3D(1, -1, -5);
            points[2] = new Point3D(1, 1, -5);
            points[3] = new Point3D(-1, 1, -5);
            points[4] = new Point3D(-1, -1, -7);
            points[5] = new Point3D(1, -1, -7);
            points[6] = new Point3D(1, 1, -7);
            points[7] = new Point3D(-1, 1, -7);

            Point[] projectedPoints = new Point[8];

            int i = 0;

            foreach (Point3D element in points)
            {
                projectedPoints[i] = PerspectiveProjectionMatrix1(element);
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
        private Point PerspectiveProjectionMatrix1(Point3D input)
        {
            double Fov = 90;
            double near = .1;
            double far = 100;
            double w = 1;

            double aspectRation = 1920 / 1080; // auflösung Bildschirm

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
                {input.X },
                {input.Y },
                {input.Z },
                {w },
            };

            double[,] ergebnis = Matrix.MultiplyMatrix(matrix, inputMatrix);
            double bx = ergebnis[0,0];
            double by = ergebnis[1,0];

            if (w != 0)
            {
                bx = ergebnis[0, 0] / ergebnis[3, 0];
                by = ergebnis[1, 0] / ergebnis[3, 0];
            }        

            return new Point(bx, by);

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

