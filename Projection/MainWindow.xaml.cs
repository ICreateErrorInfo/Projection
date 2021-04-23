using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Microsoft.Win32;

namespace Projection {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double          abstand        = 6;
        Vektor          _camera        = new Vektor(0,  0, 0);
        readonly Vektor _lookDirCamera = new Vektor(0,  0, 1);
        readonly Vektor _up            = new Vektor( 0, 1, 0 );
        readonly Vektor _target;

        double                           _currentAngle = 1;
        private readonly DispatcherTimer _timer;
        private readonly DrawingSurface  _drawingSurface;

        readonly List<Load> _loads;

        public MainWindow() {
            _loads = new List<Load>();
            //Initialize
            InitializeComponent();
            _target = _camera + _lookDirCamera;

            _drawingSurface = new DrawingSurface();
            MainGrid.Children.Add(_drawingSurface.Surface);
            
            ShowOpenFile();

            _timer          =  new DispatcherTimer();
            _timer.Tick     += Update;
            _timer.Interval =  TimeSpan.FromMilliseconds(5);
            _timer.Start();
        }

        void ShowOpenFile() 
        {
            var ofn = new OpenFileDialog 
            {
                Filter = "Object files (*.obj)|*.obj",
            };
            if (ofn.ShowDialog() == true) {
                _loads.Add(Load.Obj(ofn.FileName));
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                _timer.IsEnabled ^= true;
                e.Handled        =  true;
            }
            base.OnKeyDown(e);

            if (e.Key == Key.Space)
            {
                _camera   = new Vektor(x: _camera.X, y: _camera.Y + .01, z: _camera.Z);
                e.Handled = true;
            }
            base.OnKeyDown(e);

            if (e.Key == Key.LeftCtrl)
            {
                _camera   = new Vektor(x: _camera.X, y: _camera.Y - .01, z: _camera.Z);
                e.Handled = true;
            }
            base.OnKeyDown(e);


            if (e.Key == Key.A)
            {
                _camera   = new Vektor(x: _camera.X + .01, y: _camera.Y, z: _camera.Z);
                e.Handled = true;
            }
            base.OnKeyDown(e);

            if (e.Key == Key.D)
            {
                _camera   = new Vektor(x: _camera.X - .01, y: _camera.Y, z: _camera.Z);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
        private void Update(object sender, EventArgs e)
        {
            //update
            _drawingSurface.Clear();

            foreach (var load in _loads) {
                Project(load, _currentAngle);
            }
            
        }

        void Project(Load load, double angle)
        {
            List<Vektor> translatedVerts = new List<Vektor>();

            //double[,] matCamera = Matrix.PointAt(Camera, Target, up);
            double[,] matView = Matrix.LookAt(_camera, _target, _up);


            for (int j = 0; j < load.Verts.Count; j++)
            {
                Vektor element = load.Verts[j];

                Vektor erg = Matrix.MultiplyVektor(Matrix.WorldMatrix(angle, abstand), element);

                translatedVerts.Add(erg);
            }

            load.CreateTriangles(translatedVerts);

            for (int i = 0; i < load.ImportedTriangles.Count; i++)
            {
                
                Vektor line1 = new Vektor(load.ImportedTriangles[i].Tp2, load.ImportedTriangles[i].Tp1);
                Vektor line2 = new Vektor(load.ImportedTriangles[i].Tp2, load.ImportedTriangles[i].Tp3);

                Vektor normal =Vektor.CalcNormals(line1, line2);

                Vektor pointToCamera = load.ImportedTriangles[i].Tp1 - _camera;

                if (Vektor.DotProduct(normal, pointToCamera) > 0)
                {
                    Vektor lightDirection = new Vektor(0, 0, -1);
                    lightDirection = Vektor.Normalise(lightDirection);

                    double dp        = Vektor.DotProduct(normal, lightDirection);
                    var    grayValue = Convert.ToByte(Math.Abs(dp * Byte.MaxValue));
                    var    col       = Color.FromArgb(250, grayValue, grayValue, grayValue);

                    var tp1 = Matrix.ToVektor(Matrix.MultiplyMatrix(matView, Vektor.ToMatrix(load.ImportedTriangles[i].Tp1)));
                    var tp2 = Matrix.ToVektor(Matrix.MultiplyMatrix(matView, Vektor.ToMatrix(load.ImportedTriangles[i].Tp2)));
                    var tp3 = Matrix.ToVektor(Matrix.MultiplyMatrix(matView, Vektor.ToMatrix(load.ImportedTriangles[i].Tp3)));

                    Triangle triViewed = new Triangle(tp1, tp2, tp3);

                    Vektor projectedPoint1 = PerspectiveProjectionMatrix(triViewed.Tp1);
                    Vektor projectedPoint2 = PerspectiveProjectionMatrix(triViewed.Tp2);
                    Vektor projectedPoint3 = PerspectiveProjectionMatrix(triViewed.Tp3);

                    _drawingSurface.Triangle(new Triangle(projectedPoint1, projectedPoint2, projectedPoint3), col);
                }
            }
        }
        private Vektor PerspectiveProjectionMatrix(Vektor input)
        {
            double fov  = 50;
            double near = .1;
            double far  = 100;

            // ReSharper disable once PossibleLossOfFraction
            double aspectRation = 1920 / 1080; // auflösung Bildschirm

            Vektor ergebnis = Matrix.MultiplyVektor(Matrix.Projecton(fov, aspectRation, far, near), input);
            double bx       = ergebnis.X;
            double by       = ergebnis.Y;

            if (ergebnis.W != 0)
            {
                bx = ergebnis.X / ergebnis.W;
                by = ergebnis.Y / ergebnis.W;
            }
            return new Vektor(bx, by, 0);
        }

    }

}