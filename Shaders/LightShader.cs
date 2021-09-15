using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DevilRender
{
    public class LightShader : IFragmentShader
    {
        public Enviroment Enviroment;
        public List<Light> Lights;

        public LightShader(Enviroment enviroment, params Light[] lights)
        {
            Enviroment = enviroment;
            Lights = lights.ToList();
        }

        public FragmentInfo[] ComputeFragments(FragmentInfo[] polysFragments, Camera camera)
        {
            foreach (var frag in polysFragments)
            {
                var globalPos = camera.Pivot.ToGlobalCoords(frag.Coordinate);
                foreach (var light in Lights)
                {
                    var distance = (light.Pos - globalPos).Length();
                    bool intersects = false;
                    foreach (var poly in Enviroment.GetPrimitives().SelectMany(p => p.GetLocalPolys(camera)))
                    {
                        Vector3 i;
                        intersects = VectorMath.AreIntersecting(poly.v1.Position, poly.v2.Position, poly.v3.Position,
                                         globalPos, light.Pos, out i)
                                     && (i - globalPos).Length() > 1;
                        if (intersects) break;
                    }

                    if (!intersects)
                    {
                        frag.Color *= light.Intensivity;
                    }
                }
            }

            return polysFragments;
        }
    }
}