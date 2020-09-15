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
        public Thor(Vector3 center, float radius1, float radius2, int frequency1, int frequency2)
        {
            Pivot = Pivot.BasePivot(center);
            GlobalVertices = new Vector3[frequency1 * frequency2];
            Indexes = new int[frequency1 * frequency2 * 6];
            Vector3 pointer = new Vector3(radius1, 0, 0);
            float delta1 = (float)Math.PI * 2 / frequency1;
            float delta2 = (float)Math.PI * 2 / frequency2;
            ///We nned basis that we'll rotate
            ///Every new cylinder will be obtained by transforming local rotating vector to world coords
            Pivot rotorBasis = Pivot.BasePivot(center);
            int k = 0;
            for (int i = 0; i < frequency1; i++)
            {
                ///Moving basis center 
                rotorBasis = new Pivot(center + pointer, rotorBasis.XAxis, rotorBasis.YAxis, rotorBasis.ZAxis);
                ///pointer for making cylinder
                Vector3 pointer2 = new Vector3(radius2, 0, 0);
                for (int g = 0; g < frequency2; g++)
                {
                    int ind = i * frequency2 + g;
                    ///Transforming to world coords
                    GlobalVertices[ind] = pointer2.Transform(rotorBasis.GlobalCoordsMatrix) + rotorBasis.Center;
                    pointer2 = pointer2.Rotate(delta2, Axis.Z);
                    ///Setting indexes
                    ///Every part of cylinder divided to parrallelograms , each of them are represented by two polys
                    if (i < frequency1 - 1 && g < frequency2 - 1)
                    {
                        Indexes[k] = i * frequency2 + g; Indexes[k + 1] = (i + 1) * frequency2 + g; Indexes[k + 2] = (i + 1) * frequency2 + g + 1; ///first poly
                        Indexes[k + 3] = i * frequency2 + g; Indexes[k + 4] = i * frequency2 + g + 1; Indexes[k + 5] = (i + 1) * frequency2 + g + 1; ///second poly
                        k += 6;
                    }
                }
                pointer = pointer.Rotate(delta1, Axis.Y);
                rotorBasis.Rotate(delta1, Axis.Y);
            }

            ///Closing thor
            int l = frequency1 - 1;
            for (int i = 0; i < frequency2 - 1; i++)
            {
                Indexes[k] = l * frequency2 + i; Indexes[k + 1] = i; Indexes[i + 2] = i + 1; ///first poly
                Indexes[k + 3] = l * frequency2 + i; Indexes[k + 4] = l * frequency2 + i + 1; Indexes[k + 5] = i + 1; ///second poly
                k+=6;
            }

            LocalVertices = GlobalVertices.Select(v => Pivot.ToLocalCoords(v)).ToArray();
            TextureCoords = new Vector2[GlobalVertices.Length];
        }
    }
}
