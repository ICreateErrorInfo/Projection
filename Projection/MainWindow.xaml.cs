using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

using Microsoft.Win32;

using Path = System.IO.Path;

namespace Projection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double abstand = 5;
        Vektor Camera = new Vektor(0, 0, 0);
        string loadPath = "C:/Users/Moritz/source/repos/Projection/Testfiles/test.obj";

        double _currentAngle = 1;
        private readonly DispatcherTimer _timer;
        private readonly DrawingSurface _drawingSurface;

        public MainWindow()
        {
            //Initialize
            InitializeComponent();

            _drawingSurface = new DrawingSurface();
            MainGrid.Children.Add(_drawingSurface.Surface);
            
            ShowOpenFile();

            _timer = new DispatcherTimer();
            _timer.Tick += Update;
            _timer.Interval = TimeSpan.FromMilliseconds(5);
            _timer.Start();
        }

        void ShowOpenFile() 
        {
            var ofn = new OpenFileDialog 
            {
                Filter           = "Object files (*.obj)|*.obj",
                InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ""
            };
            if (ofn.ShowDialog() == true) {
                Load.obj(ofn.FileName);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                _timer.IsEnabled ^= true;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
        private void Update(object sender, EventArgs e)
        {
            //update
            _drawingSurface.Clear();
            Project(++_currentAngle);
        }
        void Project(double angle)
        {
            Load.importedTriangles.Clear();
            List<Vektor> translatedVerts = new List<Vektor>();

            for (int j = 0; j < Load.verts.Count; j++)
            {
                Vektor element = Load.verts[j];

                Vektor erg = Matrix.MultiplyVektor(Matrix.WorldMatrix(angle, abstand), element);

                translatedVerts.Add(erg);
            }

            Load.createTriangles(translatedVerts);

            Triangle[] projectedTriangles = new Triangle[Load.importedTriangles.Count];
            Color col;

            for (int i = 0; i < Load.importedTriangles.Count; i++)
            {
                Vektor line1 = new Vektor(Load.importedTriangles[i].tp2, Load.importedTriangles[i].tp1);
                Vektor line2 = new Vektor(Load.importedTriangles[i].tp2, Load.importedTriangles[i].tp3);

                Vektor normal = new Vektor();
                normal.CalcNormals(line1, line2);

                Vektor pointToCamera = Load.importedTriangles[i].tp1 - Camera;

                if (Vektor.DotProduct(normal, pointToCamera) > 0)
                {
                    Vektor lightDirection = new Vektor(0,0,-1);
                    lightDirection = Vektor.Normalise(lightDirection);

                    double dp = Vektor.DotProduct(normal, lightDirection);

                    col = Color.FromArgb(250, Convert.ToByte(dp * -250), Convert.ToByte(dp * -250), Convert.ToByte(dp * -250));

                    Vektor projectedPoint1 = PerspectiveProjectionMatrix(Load.importedTriangles[i].tp1);
                    Vektor projectedPoint2 = PerspectiveProjectionMatrix(Load.importedTriangles[i].tp2);
                    Vektor projectedPoint3 = PerspectiveProjectionMatrix(Load.importedTriangles[i].tp3);

                    _drawingSurface.Triangle(new Triangle(projectedPoint1, projectedPoint2, projectedPoint3), col);
                }
            }
        }
        private Vektor PerspectiveProjectionMatrix(Vektor input)
        {
            double Fov = 90;
            double near = .1;
            double far = 100;

            double aspectRation = 1920 / 1080; // auflösung Bildschirm

            Vektor ergebnis = Matrix.MultiplyVektor(Matrix.Projecton(Fov, aspectRation, far, near), input);
            double bx = ergebnis.X;
            double by = ergebnis.Y;

            if (ergebnis.W != 0)
            {
                bx = ergebnis.X / ergebnis.W;
                by = ergebnis.Y / ergebnis.W;
            }
            return new Vektor(bx, by, 0);
        }
        public static double ToRad(double deg) => deg * Math.PI / 180;
    }
}
class Load
{
    public static List<Vektor> verts = new List<Vektor>();
    public static List<Triangle> importedTriangles = new List<Triangle>();
    static List<string> stringList = new List<string>();

    public static void obj(string filename)
    {
        importedTriangles.Clear();
        verts.Clear();

        foreach (var myString in File.ReadAllLines(filename))
        {
            stringList.Add(myString);
        }

        for (int i = 0; i < stringList.Count; i++)
        {
            if (i > 1)
            {
                string[] zeile = stringList[i].Split(' ');

                if (zeile[0] == "v")
                {
                    NumberFormatInfo provider = new NumberFormatInfo();
                    provider.NumberDecimalSeparator = ".";
                    verts.Add(new Vektor(Convert.ToDouble(zeile[1], provider), Convert.ToDouble(zeile[2], provider), Convert.ToDouble(zeile[3], provider)));
                }
            }
        }
    }
    public static void createTriangles(List<Vektor> vertsImp)
    {
        for (int i = 0; i < stringList.Count; i++)
        {
            if (i > 1)
            {
                string[] zeile = stringList[i].Split(' ');

                if (zeile[0] == "f")
                {
                    importedTriangles.Add(new Triangle(vertsImp[Convert.ToInt32(zeile[1]) - 1], vertsImp[Convert.ToInt32(zeile[2]) - 1], vertsImp[Convert.ToInt32(zeile[3]) - 1]));
                }
            }
        }
    }
}
class Vektor
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
    public Vektor()
    {
        X = 0;
        Y = 0;
        Z = 0;
        W = 1;
    }
    public static double length(Vektor v)
    {
        return Math.Sqrt(DotProduct(v, v));
    }
    public static double DotProduct(Vektor v1, Vektor v2)
    {
        return v1.X * v2.X +
               v1.Y * v2.Y +
               v1.Z * v2.Z;
    }
    public static Vektor Normalise(Vektor v)
    {
        double l = length(v);
        return new Vektor(v.X / l, v.Y / l, v.Z / l);
    }
    public Vektor CrossProduct(Vektor v1, Vektor v2)
    {
        Vektor v = new Vektor();
        v.X = v1.Y * v2.Z - v1.Z * v2.Y;
        v.Y = v1.Z * v2.X - v1.X * v2.Z;
        v.Z = v1.X * v2.Y - v1.Y * v2.X;
        
        return v;
    }
    public void CalcNormals(Vektor v1, Vektor v2)
    {
        Vektor v = CrossProduct(v1, v2);
        v = Normalise(v);

        X = v.X;
        Y = v.Y;
        Z = v.Z;
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

    public double X;
    public double Y;
    public double Z;
    public double W;
}
class Triangle
{
    public Triangle(Vektor p1, Vektor p2, Vektor p3)
    {
        tp1 = p1;
        tp2 = p2;
        tp3 = p3;
    }

    public Vektor tp1;
    public Vektor tp2;
    public Vektor tp3;
}
class DrawingSurface
{
    public DrawingSurface()
    {
        Surface = new Canvas();
        Surface.RenderTransformOrigin = new Point(0.5, 0.5);
        Surface.RenderTransform = new ScaleTransform(1, 1);
        Surface.Background = Brushes.DarkGray;
    }
    public Canvas Surface { get; }
    public void Clear()
    {
        Surface.Children.Clear();
    }
    public void Line(Point x, Point y, SolidColorBrush brush)
    {
        var objLine = new Line
        {
            Stroke = brush,
            Fill = brush
        };

        var p1 = MapPoint(x);
        objLine.X1 = p1.X;
        objLine.Y1 = p1.Y;

        var p2 = MapPoint(y);
        objLine.X2 = p2.X;
        objLine.Y2 = p2.Y;

        objLine.StrokeThickness = 2; 

        Surface.Children.Add(objLine);
    }
    public void Triangle(Triangle tri, Color col)
    {
        if (tri != null)
        {
            Point p1 = new Point(tri.tp1.X, tri.tp1.Y);
            Point p2 = new Point(tri.tp2.X, tri.tp2.Y);
            Point p3 = new Point(tri.tp3.X, tri.tp3.Y);

            SolidColorBrush brush = new SolidColorBrush(col);
            Line(p1, p2, brush);
            Line(p2, p3, brush);
            Line(p1, p3, brush);

            var p = new Polygon
            {
                Points =
                {
                    MapPoint(p1),
                    MapPoint(p2),
                    MapPoint(p3)
                },
                
                Fill = brush,
                Stroke = Brushes.Black,
                StrokeThickness = 0
            };

            Surface.Children.Add(p);

        }
    }
    Point MapPoint(Point p)
    {
        // Offset
        double height = Surface.ActualHeight / 2;
        double width = Surface.ActualWidth / 2;

        int indicador = 50; // Skalierung

        return new Point(p.X * indicador + width, p.Y * indicador + height);
    }
}
class Matrix
{
    public static double[,] MultiplyMatrix(double[,] a, double[,] b)
    {
        int rA = a.GetLength(0);
        int cA = a.GetLength(1);
        int rB = b.GetLength(0);
        int cB = b.GetLength(1);
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

        return toVektor(MultiplyMatrix(a, b));
    }
    public static double[,] RotateX(double angle)
    {
        double[,] rotationX =
            {
            {1,0,0, 0},
            {0, Math.Cos(Projection.MainWindow.ToRad(angle)), Math.Sin(Projection.MainWindow.ToRad(angle) * -1),0},
            {0, Math.Sin(Projection.MainWindow.ToRad(angle)), Math.Cos(Projection.MainWindow.ToRad(angle)),0},
            {0,0,0,1 }
            };

        return rotationX;
    }
    public static double[,] RotateY(double angle)
    {
        double[,] rotationY =
            {
            {Math.Cos(Projection.MainWindow.ToRad(angle)),0 ,Math.Sin(Projection.MainWindow.ToRad(angle) * -1),0},
            {0, 1, 0,0},
            {Math.Sin(Projection.MainWindow.ToRad(angle)),0 ,Math.Cos(Projection.MainWindow.ToRad(angle)),0},
            {0,0,0,1 }
            };

        return rotationY;
    }
    public static double[,] RotateZ(double angle)
    {
        double[,] rotationZ =
            {
            {Math.Cos(Projection.MainWindow.ToRad(angle)), Math.Sin(Projection.MainWindow.ToRad(angle) * -1), 0 ,0},
            {Math.Sin(Projection.MainWindow.ToRad(angle)), Math.Cos(Projection.MainWindow.ToRad(angle)), 0 ,0},
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
    public static double[,] Projecton(double Fov, double aspectRatio, double far, double near)
    {
        double Rechnung2 = 1 / Math.Round(Math.Tan(Projection.MainWindow.ToRad(Fov / 2)), 3);
        double Rechnung1 = aspectRatio * (Rechnung2);
        double Rechnung3 = far / (far - near);
        double Rechnung4 = (-far * near) / (far - near);

        double[,] matrix =
        {
                { Rechnung1, 0, 0, 0 },
                { 0, Rechnung2, 0, 0 },
                { 0, 0, Rechnung3, 1 },
                { 0, 0, Rechnung4, 0 }
        };

        return matrix;
    }
    public static double[,] WorldMatrix(double angle, double abstand)
    {
        double[,] rotateX = Matrix.RotateX(angle);
        double[,] rotateY = Matrix.RotateY(angle);
        double[,] rotateZ = Matrix.RotateZ(angle);

        double[,] translate = Matrix.Translation(0, 0, abstand);

        double[,] worldMatrix = Matrix.Identety();
        worldMatrix = Matrix.MultiplyMatrix(worldMatrix, translate);
        worldMatrix = Matrix.MultiplyMatrix(worldMatrix, rotateX);
        worldMatrix = Matrix.MultiplyMatrix(worldMatrix, rotateY);
        //worldMatrix = Matrix.MultiplyMatrix(worldMatrix, rotateZ);

        return worldMatrix;
    }
    public static Vektor toVektor(double[,] a)
    {
        return new Vektor(a[0, 0], a[1, 0], a[2, 0], a[3, 0]);
    }
}

