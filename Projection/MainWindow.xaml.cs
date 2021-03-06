using System;
using System.Collections.Generic;
using System.Linq;
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
        double          abstand        = 5;
        Vektor          _camera        = new Vektor(0,0,0);
        Vektor _lookDirCamera = new Vektor(0,  0,  -1);
        readonly Vektor _up            = new Vektor( 0, 1,  0 );
        double Yaw;
        double Pitch;
        double fov = 90;
        double near = .1;
        double far = 10;

        double                           _currentAngle = 0;
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

            Vektor Forward = _lookDirCamera * new Vektor(.1,.1,-.1);

            if (e.Key == Key.Space)
            {
                _camera   = new Vektor(x: _camera.X, y: _camera.Y - .1, z: _camera.Z);
                e.Handled = true;
            }
            if (e.Key == Key.LeftCtrl)
            {
                _camera   = new Vektor(x: _camera.X, y: _camera.Y + .1, z: _camera.Z);
                e.Handled = true;
            }


            if (e.Key == Key.A)
            {
                _camera += Matrix.MultiplyVektor(Matrix.RotateY(90), _lookDirCamera * new Vektor(.1,.1,-.1));
                e.Handled = true;
            }
            if (e.Key == Key.D)
            {
                _camera -= Matrix.MultiplyVektor(Matrix.RotateY(90), _lookDirCamera * new Vektor(.1, .1, -.1));
                e.Handled = true;
            }

            if (e.Key == Key.W)
            {
                _camera -=  Forward;
                e.Handled = true;
            }
            if (e.Key == Key.S)
            {
                _camera +=  Forward;
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

        public delegate int Comparison<in T>(T x, T y);
        void Project(Import import, double angle)
        {
            List<Vektor> translatedVerts = new List<Vektor>();
            List<Triangle> projectedTriangles = new List<Triangle>();
            List<Color> projectedTriColor = new List<Color>();

            Vektor target = new Vektor(0, 0, 1);
            double[,] matCameraRotY = Matrix.RotateY(Yaw);
            double[,] matCameraRotX = Matrix.RotateX(Pitch);
            _lookDirCamera = Matrix.MultiplyVektor(matCameraRotY, target);
            target = _camera + _lookDirCamera;

            double[,] matView = Matrix.lookAt(_camera, target, _up);

            for (int j = 0; j < import.Verts.Count; j++)
            {
                Vektor element = import.Verts[j];

                Vektor erg = Matrix.MultiplyVektor(Matrix.WorldMatrix(new Vektor(angle, angle, angle), new Vektor(0,0,abstand)), element);

                translatedVerts.Add(erg);
            }

            List<Triangle> sortedTriangles = new List<Triangle>();
            List<Triangle> sortedTrianglesF = new List<Triangle>();
            Dictionary<Triangle, double> sort = new Dictionary<Triangle, double>();
            foreach (var triangle in import.CreateTriangles(translatedVerts))
            {
                sort.Add(triangle, triangle.Tp1.Z + triangle.Tp2.Z + triangle.Tp3.Z / 3);
            }

            foreach (KeyValuePair<Triangle, double> author in sort.OrderBy(key => key.Value))
            {
                sortedTriangles.Add(author.Key);
            }
            for(int t = 0; t < sortedTriangles.Count; t++)
            {

                sortedTrianglesF.Add(sortedTriangles[sortedTriangles.Count - 1 - t]);
            }

            foreach (var triangle in sortedTrianglesF) 
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

                    var tp1 = Matrix.MultiplyVektor(matView, triangle.Tp1);
                    var tp2 = Matrix.MultiplyVektor(matView, triangle.Tp2);
                    var tp3 = Matrix.MultiplyVektor(matView, triangle.Tp3);

                    Triangle triViewed = new Triangle(tp1 , tp2 , tp3 );

                    int clippedTriangles = 0;
                    Triangle[] clipped = new Triangle[4];

                    clippedTriangles += Clipping.Triangle_ClipAgainstPlane(new Vektor(0,0, -near), new Vektor(0,0,-1), triViewed);
                    clipped[0] = Clipping.outTri1;
                    clipped[1] = Clipping.outTri2;

                    for (int n = 0; n < clippedTriangles; n++)
                    {
                        Vektor projectedPoint1 = PerspectiveProjectionMatrix(clipped[n].Tp1);
                        Vektor projectedPoint2 = PerspectiveProjectionMatrix(clipped[n].Tp2);
                        Vektor projectedPoint3 = PerspectiveProjectionMatrix(clipped[n].Tp3);

                        projectedTriangles.Add(new Triangle(projectedPoint1, projectedPoint2, projectedPoint3));
                        projectedTriColor.Add(col);
                    }
                }
            }

            foreach (Triangle triToRaster in projectedTriangles)
            {
                int c = 0;
                Triangle[] clipped = new Triangle[2];
                List<Triangle> listTriangles = new List<Triangle>();
                Color col = projectedTriColor[c];
                listTriangles.Add(triToRaster);
                int newTriangles = 1;

                for (int p = 0; p < 4; p++)
                {
                    int TrisToAdd = 0;
                    while (newTriangles > 0)
                    {
                        Triangle test = listTriangles[0];
                        listTriangles.RemoveAt(0);
                        newTriangles--;

                        switch (p)
                        {
                            case 0:
                                TrisToAdd = Clipping.Triangle_ClipAgainstPlane(new Vektor(0, (-_drawingSurface.Surface.ActualHeight / 2)/50, 0), new Vektor(0, 1, 0), test);
                                clipped[0] = Clipping.outTri1;
                                clipped[1] = Clipping.outTri2;
                                break;
                            case 1:
                                TrisToAdd = Clipping.Triangle_ClipAgainstPlane(new Vektor(0, (_drawingSurface.Surface.ActualHeight / 2) / 50, 0), new Vektor(0, -1, 0), test);
                                clipped[0] = Clipping.outTri1;
                                clipped[1] = Clipping.outTri2;
                                break;
                            case 2:
                                TrisToAdd = Clipping.Triangle_ClipAgainstPlane(new Vektor((-_drawingSurface.Surface.ActualWidth / 2) / 50, 0, 0), new Vektor(1, 0, 0), test);
                                clipped[0] = Clipping.outTri1;
                                clipped[1] = Clipping.outTri2;
                                break;
                            case 3:
                                TrisToAdd = Clipping.Triangle_ClipAgainstPlane(new Vektor((_drawingSurface.Surface.ActualWidth / 2) / 50, 0, 0), new Vektor(-1, 0, 0), test);
                                clipped[0] = Clipping.outTri1;
                                clipped[1] = Clipping.outTri2;
                                break;
                        }

                        for (int w = 0; w < TrisToAdd; w++)
                        {
                            listTriangles.Add(clipped[w]);
                        }
                    }
                    newTriangles = listTriangles.Count;
                }

                for (int i = 0; i < listTriangles.Count; i++)
                {
                    _drawingSurface.Triangle(listTriangles[i], col);
                }
                c++;
            }
        }
        private Vektor PerspectiveProjectionMatrix(Vektor input)
        {
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