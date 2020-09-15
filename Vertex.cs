using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevilRender
{
    public class Vertex
    {
        public FastBitmap Texture { get; set; }
        public Vector3 Position { get; set; }
        public Color Color { get; set; }
        public Vector2 TextureCoord { get; set; }
        public Vector3 Normal { get; set; }

        public Vertex(Vector3 pos , Color color , Vector2 texCoord , Vector3 normal)
        {
            Position = pos;
            Color = color;
            TextureCoord = texCoord;
            Normal = normal;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
