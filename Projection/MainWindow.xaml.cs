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
        Vektor          _camera        = new Vektor(0,0,0);
        Vektor _lookDirCamera = new Vektor(0,  0,  1);
        readonly Vektor _up            = new Vektor( 0, -1,  0 );
        double Yaw;
        double Pitch;

        double                           _currentAngle = 1;
        private readonly DispatcherTimer _timer;
        private readonly DrawingSurface  _drawingSurface;

        readonly List<Import> _loads;

        public MainWindow() {
            _loads = new List<Import>();
            //Initialize
            InitializeComponent();

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
                _loads.Add(Import.Obj(ofn.FileName));
            }
        }

        Point? _mouseStartPosition;
        protected override void OnMouseDown(MouseButtonEventArgs e) 
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                CaptureMouse();
                _mouseStartPosition = Mouse.GetPosition(this);
                e.Handled           = true;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) 
        {
            if (_mouseStartPosition != null)
            {
                var startMousePos   = _mouseStartPosition.Value;
                var currentMousePos = Mouse.GetPosition(this);

                // TODO hier evtl. einen RichtungsVektor basteln?
                e.Handled = true;
            }
            base.OnMouseMove(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e) 
        {
            _mouseStartPosition=null;
            base.OnLostMouseCapture(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) 
        {
            ReleaseMouseCapture();
            _mouseStartPosition = null;
            e.Handled           = true;
            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                _timer.IsEnabled ^= true;
                e.Handled        =  true;
            }

            Vektor Forward = _lookDirCamera * new Vektor(.1, .1, .1);

            if (e.Key == Key.Space)
            {
                _camera   = new Vektor(x: _camera.X, y: _camera.Y + .1, z: _camera.Z);
                e.Handled = true;
            }
            if (e.Key == Key.LeftCtrl)
            {
                _camera   = new Vektor(x: _camera.X, y: _camera.Y - .1, z: _camera.Z);
                e.Handled = true;
            }


            if (e.Key == Key.A)
            {
                _camera   += Matrix.MultiplyVektor(Matrix.RotateY(90), _lookDirCamera * new Vektor(.1, .1, .1));
                e.Handled = true;
            }
            if (e.Key == Key.D)
            {
                _camera -= Matrix.MultiplyVektor(Matrix.RotateY(90), _lookDirCamera * new Vektor(.1,.1,.1));
                e.Handled = true;
            }

            if (e.Key == Key.W)
            {
                _camera = _camera + Forward;
                e.Handled = true;
            }
            if (e.Key == Key.S)
            {
                _camera = _camera - Forward;
                e.Handled = true;
            }

            if (e.Key == Key.Left)
            {
                Yaw += 1;
                e.Handled = true;
            }
            if (e.Key == Key.Right)
            {
                Yaw -= 1;
                e.Handled = true;
            }

            if (e.Key == Key.Up)
            {
                Pitch += 1;
                e.Handled = true;
            }
            if (e.Key == Key.Down)
            {
                Pitch -= 1;
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

        void Project(Import import, double angle)
        {
            List<Vektor> translatedVerts = new List<Vektor>();
            var target = new Vektor(0,0,1);
            double[,] matCameraRotY = Matrix.RotateY(Yaw);
            double[,] matCameraRotX = Matrix.RotateX(Pitch);
            _lookDirCamera = Matrix.MultiplyVektor(matCameraRotY, target);
            var _lookDirCamera1 = Matrix.MultiplyVektor(matCameraRotX, target);
            target = _camera + _lookDirCamera + _lookDirCamera1;


            double[,] matPointAt = Matrix.PointAt(_camera, target, _up);
            double[,] matView = Matrix.lookAt(matPointAt);

            for (int j = 0; j < import.Verts.Count; j++)
            {
                Vektor element = import.Verts[j];

                Vektor erg = Matrix.MultiplyVektor(Matrix.WorldMatrix(new Vektor(angle, angle, angle), new Vektor(0,0,abstand)), element);

                translatedVerts.Add(erg);
            }

            foreach (var triangle in import.CreateTriangles(translatedVerts)) 
            {

                Vektor line1 = new Vektor(triangle.Tp2, triangle.Tp1);
                Vektor line2 = new Vektor(triangle.Tp2, triangle.Tp3);

                Vektor normal =Vektor.CalcNormals(line1, line2);

                Vektor pointToCamera = _camera - triangle.Tp1;

                if (Vektor.DotProduct(pointToCamera, normal) < 0)
                {
                    Vektor lightDirection = _camera + new Vektor(0,0,-1);
                    lightDirection = lightDirection.Normalise();

                    double dp        = Vektor.DotProduct(normal, lightDirection);
                    var    grayValue = Convert.ToByte(Math.Abs(dp * Byte.MaxValue));
                    var    col       = Color.FromArgb(250, grayValue, grayValue, grayValue);

                    //var tp1 = Matrix.ToVektor(Matrix.MultiplyMatrix(Matrix.WorldMatrix(new Vektor(0, 0, 0), _camera * new Vektor(-1,-1,-1)), Vektor.ToMatrix(triangle.Tp1)));
                    //var tp2 = Matrix.ToVektor(Matrix.MultiplyMatrix(Matrix.WorldMatrix(new Vektor(0, 0, 0), _camera * new Vektor(-1, -1, -1)), Vektor.ToMatrix(triangle.Tp2)));
                    //var tp3 = Matrix.ToVektor(Matrix.MultiplyMatrix(Matrix.WorldMatrix(new Vektor(0, 0, 0), _camera * new Vektor(-1, -1, -1)), Vektor.ToMatrix(triangle.Tp3)));

                    var tp1 = Matrix.MultiplyVektor(matView, triangle.Tp1);
                    var tp2 = Matrix.MultiplyVektor(matView, triangle.Tp2);
                    var tp3 = Matrix.MultiplyVektor(matView, triangle.Tp3);

                    Triangle triViewed = new Triangle(tp1 , tp2 , tp3 );

                    int clippedTriangles = 0;
                    Triangle[] clipped = new Triangle[2];

                    clippedTriangles = Clipping.Triangle_ClipAgainstPlane(new Vektor(0,0, -2.1), new Vektor(0,0,-1), triViewed);
                    clipped[0] = Clipping.outTri1;
                    clipped[1] = Clipping.outTri2;

                    for (int n = 0; n < clippedTriangles; n++)
                    {
                        Vektor projectedPoint1 = PerspectiveProjectionMatrix(clipped[n].Tp1);
                        Vektor projectedPoint2 = PerspectiveProjectionMatrix(clipped[n].Tp2);
                        Vektor projectedPoint3 = PerspectiveProjectionMatrix(clipped[n].Tp3);

                        _drawingSurface.Triangle(new Triangle(projectedPoint1, projectedPoint2, projectedPoint3), col);
                    }
                }
            }
        }
        private Vektor PerspectiveProjectionMatrix(Vektor input)
        {
            double fov  = 90;
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