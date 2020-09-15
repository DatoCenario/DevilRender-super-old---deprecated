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
public class LightShader : IShader
    {

        public Enviroment Enviroment;
        public List<Light> Lights;
        public LightShader(Enviroment enviroment, params Light[] lights)
        {
            Enviroment = enviroment;
            Lights = lights.ToList();
        }
        public void ComputeShader(Vertex vertex, Camera camera)
        {
            var globalPos = camera.Pivot.ToGlobalCoords(vertex.Position);
            foreach (var light in Lights)
            {
                var distance = (light.Pos - globalPos).Length();
                bool intersects = false;
                foreach (var poly in Enviroment.Primitives.SelectMany(p => p.GetPolys()))
                {
                    Vector3 i;
                    intersects = VectorMath.AreIntersecting(poly.Item1.Position, poly.Item2.Position, poly.Item3.Position, globalPos, light.Pos, out i)
                        && (i - globalPos).Length() > 1;
                    if (intersects) break;
                }
                if (!intersects)
                {
                    vertex.Color = Color.FromArgb(vertex.Color.A,
                         (int)Math.Min(255, vertex.Color.R + light.Intensivity / distance),
                         (int)Math.Min(255, vertex.Color.G + light.Intensivity / distance),
                         (int)Math.Min(255, vertex.Color.B + light.Intensivity / distance)
                         );
                }
                else
                {
                    vertex.Color = Color.FromArgb(vertex.Color.A,
                        (int)Math.Min(255, vertex.Color.R + light.Intensivity / distance / 2),
                        (int)Math.Min(255, vertex.Color.G + light.Intensivity / distance / 2),
                        (int)Math.Min(255, vertex.Color.B + light.Intensivity / distance / 2)
                        );
                }
            }
        }
    }

}