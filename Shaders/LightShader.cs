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
        public void ComputeShader(ref Vertex vertex, Camera camera)
        {
            var globalPos = camera.Pivot.ToGlobalCoords(vertex.Position);
            foreach (var light in Lights)
            {
                var distance = (light.Pos - globalPos).Length();
                bool intersects = false;
                foreach (var poly in Enviroment.GetPrimitives().SelectMany(p => p.GetLocalPolys(camera)))
                {
                    Vector3 i;
                    intersects = VectorMath.AreIntersecting(poly.v1.Position, poly.v2.Position, poly.v3.Position, globalPos, light.Pos, out i)
                        && (i - globalPos).Length() > 1;
                    if (intersects) break;
                }
                if (!intersects)
                {
                    vertex.Color = new TGAColor(vertex.Color.a,
                        (byte)Math.Min(255, vertex.Color.r * light.Intensivity),
                        (byte)Math.Min(255, vertex.Color.g * light.Intensivity),
                        (byte)Math.Min(255, vertex.Color.b * light.Intensivity));
                }
                else
                {
                    vertex.Color = new TGAColor(vertex.Color.a,
                        (byte)Math.Min(255, vertex.Color.r * light.Intensivity),
                        (byte)Math.Min(255, vertex.Color.g * light.Intensivity),
                        (byte)Math.Min(255, vertex.Color.b * light.Intensivity));
                }
            }
        }
    }

}