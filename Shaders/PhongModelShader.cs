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
    using System.Reflection.Metadata.Ecma335;

    public class PhongModelShader : IFragmentShader
    {
        public static float DiffuseCoef = 0.5f;
        public static float ReflectCoef = 0.05f;
        public Light[] Lights { get; set; }

        public PhongModelShader(params Light[] lights)
        {
            Lights = lights;
        }

        public FragmentInfo[] ComputeFragments(FragmentInfo[] polysFragments, Camera camera)
        {
            foreach (var frag in polysFragments)
            {
                if (frag.Normal.X == 0 && frag.Normal.Y == 0 && frag.Normal.Z == 0)
                {
                    continue;
                }
                var gPos = camera.Pivot.ToGlobalCoords(frag.Coordinate);
                foreach (var light in Lights)
                {
                    var ldir = Vector3.Normalize(light.Pos - gPos);
                    //Следующие три строчки нужны чтобы найти отраженный от поверхности луч
                    var proection = VectorMath.Proection(ldir, frag.Normal);
                    var d = ldir - proection;
                    var reflect = proection - d;
                    var diffuseVal = Math.Max(VectorMath.Cross(ldir, frag.Normal), 0) * light.Intensivity;
                    //луч от наблюдателя
                    var eye = Vector3.Normalize(frag.Coordinate);
                    var reflectVal = Math.Max(VectorMath.Cross(reflect, eye), 0) * light.Intensivity;
                    var total = diffuseVal * DiffuseCoef + reflectVal * ReflectCoef;
                    frag.Color = (int)(frag.Color * total);
                }
            }

            return polysFragments;
        }
    }
}