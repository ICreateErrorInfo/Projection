using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Projection {

    class DrawingSurface
    {
        public DrawingSurface()
        {
            Surface                       = new Canvas();
            Surface.RenderTransformOrigin = new Point(0.5, 0.5);
            Surface.RenderTransform       = new ScaleTransform(1, 1);
            Surface.Background            = Brushes.Black   ;
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
                Fill   = brush
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
                Point p1 = new Point(tri.Tp1.X, tri.Tp1.Y);
                Point p2 = new Point(tri.Tp2.X, tri.Tp2.Y);
                Point p3 = new Point(tri.Tp3.X, tri.Tp3.Y);

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
                
                    Fill            = brush,
                    Stroke          = brush,
                    StrokeThickness = 0
                };

                Surface.Children.Add(p);

            }
        }
        Point MapPoint(Point p)
        {
            // Offset
            double height = Surface.ActualHeight / 2;
            double width  = Surface.ActualWidth  / 2;

            int indicador = 50; // Skalierung

            return new Point(p.X * indicador + width, p.Y * indicador + height);
        }
    }

}