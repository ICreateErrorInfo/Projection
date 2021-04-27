namespace Projection {

    class Triangle
    {
        public Triangle(Vektor p1, Vektor p2, Vektor p3)
        {
            Tp1 = p1;
            Tp2 = p2;
            Tp3 = p3;
        }
        public Triangle()
        {

        }

        public Vektor Tp1 { get; set; }
        public Vektor Tp2 { get; set; }
        public Vektor Tp3 { get; set; }
    }

}