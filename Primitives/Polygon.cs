using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace DevilRender.Primitives
{
    class Polygon : Primitive
    {
        public Polygon(Vector3 v1, Vector3 v2 ,Vector3 v3)
        {
            Pivot = Pivot.BasePivot(v1);
            GlobalVertices = new[] { v1,v2,v3};
            LocalVertices = GlobalVertices.Select(v => Pivot.ToLocalCoords(v)).ToArray();
            Indexes = new[] { 0,1,2};
        }
    }

    class TexturedPolygon : Polygon
    {
        public TexturedPolygon(Vector3 v1, Vector3 v2, Vector3 v3 , FastBitmap texture) : base(v1, v2, v3)
        {
            Texture = texture;
            TextureCoords = new Vector2[]
                {
                    new Vector2(0 , 0) ,
                    new Vector2(0 , 0.9f) ,
                    new Vector2(0.9f , 0.9f)
                };
            TextureCoordsIndexes = new int[] { 0, 1, 2 }; 
        }
    }
}
