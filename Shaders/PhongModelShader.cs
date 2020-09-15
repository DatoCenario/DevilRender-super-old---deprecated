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
public class PhongModelShader : IShader
    {
        public static float DiffuseCoef = 0.05f;
        public static float ReflectCoef = 0.1f;
        public Light[] Lights { get; set; }

        public PhongModelShader(params Light[] lights)
        {
            Lights = lights;
        }
        public void ComputeShader(Vertex vertex, Camera camera)
        {
            if (vertex.Normal.X == 0 && vertex.Normal.Y == 0 && vertex.Normal.Z == 0)
            {
                return;
            }
            var gPos = camera.Pivot.ToGlobalCoords(vertex.Position);
            foreach (var light in Lights)
            {
                var ldir = Vector3.Normalize(light.Pos - gPos);
                //Следующие три строчки нужны чтобы найти отраженный от поверхности луч
                var proection = VectorMath.Proection(ldir, -vertex.Normal);
                var d = ldir - proection;
                var reflect = proection - d;
                var diffuseVal = Math.Max(VectorMath.Cross(ldir, vertex.Normal), 0) * light.Intensivity;
                //луч от наблюдателя
                var eye = Vector3.Normalize(vertex.Position);
                var reflectVal = Math.Max(VectorMath.Cross(reflect, eye), 0) * light.Intensivity;
                var total = diffuseVal * DiffuseCoef + reflectVal * ReflectCoef;
                vertex.Color = Color.FromArgb(vertex.Color.A,
                    (int)Math.Min(255, vertex.Color.R * total),
                    (int)Math.Min(255, vertex.Color.G * total),
                    (int)Math.Min(255, vertex.Color.B * total));
            }
        }
    }
}