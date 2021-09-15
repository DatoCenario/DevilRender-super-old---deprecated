using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Poly = System.Tuple<DevilRender.Vertex, DevilRender.Vertex, DevilRender.Vertex>;

namespace DevilRender
{
    public interface IShader
    {
        void ComputeShader(ref Vertex vertex, Camera camera);
    }


    public struct Light
    {
        public Vector3 Pos;
        public int Intensivity;
        public Light(Vector3 pos , int intensivity)
        {
            Pos = pos;
            Intensivity = intensivity;
        }
    }
}









