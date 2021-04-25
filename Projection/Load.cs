using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Projection {

    class Import
    {
        readonly IReadOnlyList<string> _stringList;

        public Import(IReadOnlyList<String> stringList, IReadOnlyList<Vektor> verts) {
            _stringList = stringList;
            Verts=verts;
        }

        public IReadOnlyList<Vektor> Verts {get;}

        public static Import Obj(string filename) {

            var stringList = File.ReadAllLines(filename);
            var verts      = new List<Vektor>();

            var provider = new NumberFormatInfo 
            {
                NumberDecimalSeparator = "."
            };

            for (int i = 0; i < stringList.Length; i++)
            {
                if (i > 1)
                {
                    string[] zeile = stringList[i].Split(' ');

                    if (zeile[0] == "v")
                    {
                        verts.Add(new Vektor(Convert.ToDouble(zeile[1], provider), Convert.ToDouble(zeile[2], provider), Convert.ToDouble(zeile[3], provider)));
                    }
                }
            }

            return new Import(stringList, verts);
        }

        public List<Triangle> CreateTriangles(List<Vektor> vertsImp) 
        {
            var triangles = new List<Triangle>();
            for (int i = 0; i < _stringList.Count; i++)
            {
                if (i > 1)
                {
                    string[] zeile = _stringList[i].Split(' ');

                    if (zeile[0] == "f")
                    {
                        triangles.Add(new Triangle(vertsImp[Convert.ToInt32(zeile[1]) - 1], vertsImp[Convert.ToInt32(zeile[2]) - 1], vertsImp[Convert.ToInt32(zeile[3]) - 1]));
                    }
                }
            }

            return triangles;
        }
    }

}