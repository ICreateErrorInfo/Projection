using System;
using System.Collections.Generic;
using System.Text;

namespace Projection
{
    class Clipping
    {
        public static Triangle outTri1;
        public static Triangle outTri2;
        static Vektor VektorIntersectPlane(Vektor planeP, Vektor planeN, Vektor lineStart, Vektor lineEnd)
        {
            planeN = planeN.Normalise();
            double planeD = -Vektor.DotProduct(planeN, planeP);
            double ad = Vektor.DotProduct(lineStart, planeN);
            double bd = Vektor.DotProduct(lineEnd, planeN);
            double t = (-planeD - ad) / (bd - ad);
            Vektor lineStartToEnd = lineEnd - lineStart;
            Vektor lineToIntersect = lineStartToEnd * new Vektor(t,t,t);

            return lineStart + lineToIntersect;
        }

        public static int Triangle_ClipAgainstPlane(Vektor planeP, Vektor planeN, Triangle inTri)
        {
            outTri1 = new Triangle();
            outTri2 = new Triangle();

            planeN = planeN.Normalise();

            Vektor[] insidePoints = new Vektor[3];
            Vektor[] outsidePoints = new Vektor[3];
            int InsidePointCount = 0;
            int OutsidePointCount = 0;

            double d0 = calcdis(inTri.Tp1, planeP, planeN);
            double d1 = calcdis(inTri.Tp2, planeP, planeN);
            double d2 = calcdis(inTri.Tp3, planeP, planeN);

            if(d0 > 0)
            {
                insidePoints[InsidePointCount++] = inTri.Tp1;
            }
            else
            {
                outsidePoints[OutsidePointCount++] = inTri.Tp1;
            }

            if (d1 > 0)
            {
                insidePoints[InsidePointCount++] = inTri.Tp2;
            }
            else
            {
                outsidePoints[OutsidePointCount++] = inTri.Tp2;
            }

            if (d2 > 0)
            {
                insidePoints[InsidePointCount++] = inTri.Tp3;
            }
            else
            {
                outsidePoints[OutsidePointCount++] = inTri.Tp3;
            }

            if(InsidePointCount == 0)
            {
                return 0;
            }

            if (InsidePointCount == 3)
            {
                outTri1 = inTri;
                return 1;
            }

            if(InsidePointCount == 1 && OutsidePointCount == 2)
            {
                outTri1.Tp1 = insidePoints[0];

                outTri1.Tp2 = VektorIntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[0]);
                outTri1.Tp3 = VektorIntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[1]);

                return 1;
            }

            if(InsidePointCount == 2 && OutsidePointCount == 1)
            {
                outTri1.Tp1 = insidePoints[0];
                outTri1.Tp2 = insidePoints[1];
                outTri1.Tp3 = VektorIntersectPlane(planeP, planeN, insidePoints[0], outsidePoints[0]);

                outTri2.Tp1 = insidePoints[1];
                outTri2.Tp2 = outTri1.Tp3;
                outTri2.Tp3 = VektorIntersectPlane(planeP, planeN, insidePoints[1], outsidePoints[0]);

                return 2;
            }

            return 1;
        }


        static double calcdis(Vektor p, Vektor planeP, Vektor planeN)
        {
            Vektor erg = planeN * (p - planeP);

            return erg.X + erg.Y + erg.Z;
        }

    }
}
