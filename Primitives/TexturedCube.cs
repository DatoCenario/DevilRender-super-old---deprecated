using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DevilRender.Primitives
{
    class TexturedCube : Cube
    {
        public TexturedCube(Vector3 center, float sideLen, FastBitmap texture) : base(center, sideLen)
        {
            Texture = texture;
            TextureCoords = new Vector2[]
                {
                new Vector2(1f / 6f , 0.9f), //0
                new Vector2(2f / 6f , 0.9f),//1
                new Vector2(3f / 6f , 0.9f),//2
                new Vector2(4f / 6f , 0.9f),//3
                new Vector2(5f / 6f , 0.9f),//4
                new Vector2(0.9f , 0.9f),//5
                new Vector2(0 , 0.9f), //6
                new Vector2(1f/6f, 0),//7
                new Vector2(2f/6f, 0),//8
                new Vector2(3f/6f, 0),//9
                new Vector2(4f/6f, 0),//10
                new Vector2(5f/6f, 0),//11
                new Vector2(0.9f, 0),//12
                new Vector2(0, 0),//13
                };

            TextureCoordsIndexes = new int[]
                {
                    7 , 6 , 13 ,
                    7 , 6 ,  0,
                    8 , 2 , 9 ,
                    8 , 1 , 9 ,
                    7 , 0 , 1 ,
                    7 , 8 , 1 ,
                    10 , 3 , 9 ,
                    10 , 2 , 9 ,
                    5 , 12 , 11 ,
                    5 ,4 , 11,
                    3 , 10 , 11 ,
                    3 , 4 , 11
                };
        }
    }
}
