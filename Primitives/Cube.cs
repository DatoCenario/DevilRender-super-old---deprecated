using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace DevilRender.Primitives
{
    public class Cube : Primitive
    {
        public Cube(Vector3 center, float sideLen)
        {
            Pivot = Pivot.BasePivot(center);
            var delta = new float[] { -sideLen / 2, sideLen / 2 };
            GlobalVertices = delta.SelectMany(n => delta.SelectMany(n1 => delta.Select(n2 => center + new Vector3(n, n1, n2)))).ToArray();
            LocalVertices = GlobalVertices.Select(v => Pivot.ToLocalCoords(v)).ToArray();
            Indexes = new int[]
                {
                    1,3,2 , //Poly1
                    1,0,2 , //Poly2
                    5,7,6 ,
                    5,4,6 ,
                    1,0,4 ,
                    1,5,4 ,
                    3,2,6 ,
                    3,7,6 ,
                    1,3,7 ,
                    1,5,7 ,
                    0,2,6 ,
                    0,4,6
                };
            Texture = null;
            TextureCoords = null;
            TextureCoordsIndexes = null;
            Normals = new Vector3[]
                { 
                new Vector3(-1 , 0 , 0),//0
                new Vector3(1 , 0 , 0),//1
                new Vector3(0 , 1 , 0),//2
                new Vector3(0 , -1 , 0),//3
                new Vector3(0 , 0 , 1),//4
                new Vector3(0 , 0 , -1),//5
                };
            NormalIndexes = new int[]
                {
                    0,0,0 ,
                    0,0,0 ,
                    1,1,1 ,
                    1,1,1 ,
                    3,3,3 ,
                    3,3,3 ,
                    2,2,2 ,
                    2,2,2 ,
                    4,4,4 ,
                    4,4,4 ,
                    5,5,5 ,
                    5,5,5
                };
        }
    }
}
