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