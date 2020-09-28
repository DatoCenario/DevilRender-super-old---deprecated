using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DevilRender.Primitives
{
    public class Thor : Primitive
    {
        public Thor(Vector3 center, float radius1, float radius2, int frequency1, int frequency2) : base()
        {
            Pivot = Pivot.BasePivot(center);
            GlobalVertices = new Vector3[frequency1 * frequency2];
            Indexes = new int[frequency1 * frequency2 * 6];
            Normals = new Vector3[frequency1 * frequency2];
            NormalIndexes = new int[Indexes.Length];

            var step1 = (float)Math.PI * 2 / frequency1;
            var step2 = (float)Math.PI * 2 / frequency2;
            var rPivot1 = Pivot.BasePivot(center);

            for (int i = 0; i < frequency1; i++)
            {
                var rPivot2 = new Pivot(rPivot1.Center + rPivot1.XAxis * radius1 , rPivot1.XAxis , rPivot1.YAxis , rPivot1.ZAxis);
                for (int g = 0; g < frequency2; g++)
                {
                    var global = GlobalVertices[i * frequency2 + g] = rPivot2.ToGlobalCoords(Vector3.UnitX * radius2);
                    rPivot2.Rotate(step2 , Axis.Z);
                }
                rPivot1.Rotate(step1, Axis.Y);
            }

            for (int i = 0; i < frequency1 - 1; i++)
            {
                for (int g = 0; g < frequency2 - 1; g++)
                {
                    int k = (i * frequency2 + g) * 6;
                    int i1 = Indexes[k] = i * frequency2 + g;
                    int i2 = Indexes[k + 1] = i * frequency2 + g + 1;
                    int i3 = Indexes[k + 2] = (i + 1) * frequency2 + g;
                    int i4 = Indexes[k + 3] = i * frequency2 + g + 1;
                    int i5 = Indexes[k + 4] = (i + 1) * frequency2 + g + 1;
                    int i6 = Indexes[k + 5] = (i + 1) * frequency2 + g;


                    var normal = VectorMath.GetNormal(GlobalVertices[i4], GlobalVertices[i5], GlobalVertices[i6]);

                    Normals[i * frequency2 + g] = normal;

                    NormalIndexes[(i * frequency2 + g) * 6] = 
                    NormalIndexes[(i * frequency2 + g) * 6 + 1] = 
                    NormalIndexes[(i * frequency2 + g) * 6 + 2] = 
                    NormalIndexes[(i * frequency2 + g) * 6 + 3] = 
                    NormalIndexes[(i * frequency2 + g) * 6 + 4] = 
                    NormalIndexes[(i * frequency2 + g) * 6 + 5] = i * frequency2 + 1;
                }
            }
            LocalVertices = GlobalVertices.Select(v => Pivot.ToLocalCoords(v)).ToArray();

        }
    }
}
