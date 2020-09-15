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
    public interface IShader
    {
        void ComputeShader(Vertex vertex, Camera camera);
    }


    public struct Light
    {
        public Vector3 Pos;
        public float Intensivity;
        public Light(Vector3 pos , float intensivity)
        {
            Pos = pos;
            Intensivity = intensivity;
        }
    }
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
    public class PhongModelShader : IShader
    {
        public static float DiffuseCoef = 0.1f;
        public static float ReflectCoef = 0.2f;
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
                var diffuseVal = Math.Max(VectorMath.Cross(ldir, -vertex.Normal), 0) * light.Intensivity;
                var eye = Vector3.Normalize(-vertex.Position);
                var reflectVal = Math.Max(VectorMath.Cross(reflect, eye), 0) * light.Intensivity;
                var total = diffuseVal * DiffuseCoef + reflectVal * ReflectCoef;
                vertex.Color = Color.FromArgb(vertex.Color.A,
                    (int)Math.Min(255, vertex.Color.R * total),
                    (int)Math.Min(255, vertex.Color.G * total),
                    (int)Math.Min(255, vertex.Color.B * total));
            }
        }
    }

    public class ShadowMappingShader : IShader
    {
        public Enviroment Enviroment { get; set; }
        public Rasterizer Rasterizer { get; set; }
        public Camera Camera => Rasterizer.Camera;
        public Pivot Pivot => Camera.Pivot;
        public Vertex[] ZBuffer => Rasterizer.ZBuffer;
        public float LightIntensivity { get; set; }

        public ShadowMappingShader(Enviroment enviroment , Rasterizer rasterizer, float lightIntensivity)
        {
            Enviroment = enviroment;
            LightIntensivity = lightIntensivity;
            Rasterizer = rasterizer;
            Camera.OnRotate += () => UpdateVisible(Enviroment.Primitives);
            Camera.OnMove += () => UpdateVisible(Enviroment.Primitives);
            UpdateVisible(Enviroment.Primitives);
        }
        public void ComputeShader(Vertex vertex, Camera camera)
        {
            //вычисляем глобальные координаты вершины
            var gPos = camera.Pivot.ToGlobalCoords(vertex.Position);
            //дистанция до света
            var lghDir = Pivot.Center - gPos;
            var distance = lghDir.Length();
            var local = Pivot.ToLocalCoords(gPos);
            var proectToLight = Camera.ScreenProection(local).ToPoint();
            if (proectToLight.X >= 0 && proectToLight.X < Camera.ScreenWidth && proectToLight.Y >= 0
                && proectToLight.Y < Camera.ScreenHeight)
            {
                int index = proectToLight.Y * Camera.ScreenWidth + proectToLight.X;
                var n = Vector3.Normalize(vertex.Normal);
                var ld = Vector3.Normalize(lghDir);
                //вычислем сдвиг глубины
                float bias = (float)Math.Max(10 * (1.0 - VectorMath.Cross(n, ld)), 0.05);
                if (ZBuffer[index] == null || ZBuffer[index].Position.Z + bias >= local.Z)
                {
                    vertex.Color = Color.FromArgb(vertex.Color.A,
                        (int)Math.Min(255, vertex.Color.R + LightIntensivity / distance),
                        (int)Math.Min(255, vertex.Color.G + LightIntensivity / distance),
                        (int)Math.Min(255, vertex.Color.B + LightIntensivity / distance));
                }
            }
            else
            {
                vertex.Color = Color.FromArgb(vertex.Color.A,
                        (int)Math.Min(255, vertex.Color.R + (LightIntensivity / distance) / 15),
                        (int)Math.Min(255, vertex.Color.G + (LightIntensivity / distance) / 15),
                        (int)Math.Min(255, vertex.Color.B + (LightIntensivity / distance) / 15));
            }
        }
        public void UpdateVisible(IEnumerable<Primitive> primitives)
        {
            Rasterizer.ComputeVisibleVertices(primitives);
        }
    }
}
