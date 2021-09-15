using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DevilRender
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 TextureCoord;
        public Vector3 Normal;

        public Vertex(
            Vector3 pos,
            Vector2 texCoord,
            Vector3 normal)
        {
            Position = pos;
            TextureCoord = texCoord;
            Normal = normal;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
