using System;
using System.Collections.Generic;
using System.Text;

namespace DevilRender
{
    public struct Poly
    {
        public Vertex v1;
        public Vertex v2;
        public Vertex v3;

        public Poly(Vertex v1, Vertex v2, Vertex v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }
}
