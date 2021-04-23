using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Projection {

    class Load
    {
        public readonly List<Vektor>   Verts             = new List<Vektor>();
        public readonly List<Triangle> ImportedTriangles = new List<Triangle>();

        readonly        List<string>   _stringList        = new List<string>();

        public static Load Obj(string filename) {

            var load = new Load();

            foreach (var myString in File.ReadAllLines(filename))
            {
                load._stringList.Add(myString);
            }

            for (int i = 0; i < load._stringList.Count; i++)
            {
                if (i > 1)
                {
                    string[] zeile = load._stringList[i].Split(' ');

                    if (zeile[0] == "v")
                    {
                        NumberFormatInfo provider = new NumberFormatInfo();
                        provider.NumberDecimalSeparator = ".";
                        load.Verts.Add(new Vektor(Convert.ToDouble(zeile[1], provider), Convert.ToDouble(zeile[2], provider), Convert.ToDouble(zeile[3], provider)));
                    }
                }
            }

            return load;
        }

        public void CreateTriangles(List<Vektor> vertsImp)
        {
            for (int i = 0; i < _stringList.Count; i++)
            {
                if (i > 1)
                {
                    string[] zeile = _stringList[i].Split(' ');

                    if (zeile[0] == "f")
                    {
                        ImportedTriangles.Add(new Triangle(vertsImp[Convert.ToInt32(zeile[1]) - 1], vertsImp[Convert.ToInt32(zeile[2]) - 1], vertsImp[Convert.ToInt32(zeile[3]) - 1]));
                    }
                }
            }
        }
    }

}