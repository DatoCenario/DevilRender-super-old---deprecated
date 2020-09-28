using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace DevilRender
{
    class Function : Primitive
    {
        public Function(Vector3 center, Func<float, float, float> f, float xstart, float ystart, float xend, float yend, float step)
        {
            Pivot = Pivot.BasePivot(center);
            int width = (int)((xend - xstart) / step);
            int height = (int)((yend - ystart) / step);
            int len = width * height;
            LocalVertices = new Vector3[len];
            GlobalVertices = new Vector3[len];
            Indexes = new int[width * height * 6];
            NormalIndexes = new int[width * height * 6];
            Normals = new Vector3[width * height];
            float x = xstart, y = ystart;
            for (int i = 0; i < width; i++)
            {
                y = ystart;
                for (int g = 0; g < height; g++)
                {
                    int ind = g * width + i;
                    var local = LocalVertices[ind] = new Vector3(x, y, f.Invoke(x,y));
                    GlobalVertices[ind] = Pivot.ToGlobalCoords(local);
                    y += step;
                }
                x += step;
            }


            for (int i = 0; i < width - 1; i++)
            {
                for (int g = 0; g < height - 1; g++)
                {
                    int k = (i * height + g) * 6;
                    int i1 = Indexes[k] = i * height + g;
                    int i2 = Indexes[k + 1] = i * height + g + 1;
                    int i3 = Indexes[k + 2] = (i + 1) * height + g;
                    int i4 = Indexes[k + 3] = i * height + g + 1;
                    int i5 = Indexes[k + 4] = (i + 1) * height + g + 1;
                    int i6 = Indexes[k + 5] = (i + 1) * height + g;


                    var normal = VectorMath.GetNormal(GlobalVertices[i4], GlobalVertices[i5], GlobalVertices[i6]);

                    Normals[i * height + g] = normal;

                    NormalIndexes[(i * height + g) * 6] =
                    NormalIndexes[(i * height + g) * 6 + 1] =
                    NormalIndexes[(i * height + g) * 6 + 2] =
                    NormalIndexes[(i * height + g) * 6 + 3] =
                    NormalIndexes[(i * height + g) * 6 + 4] =
                    NormalIndexes[(i * height + g) * 6 + 5] = i * height + 1;
                }
            }
        }
    }
}
