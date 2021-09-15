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
public class ShadowMappingShader : IFragmentShader
    {
        public Enviroment Enviroment { get; set; }
        public RasterizerService Rasterizer { get; set; }
        public Camera Camera { get; set; }
        public Pivot Pivot => Camera.Pivot;
        public float LightIntensivity { get; set; }
        
        public float[] ZBuffer { get; set; }

        public ShadowMappingShader(float lightIntensivity, Camera camera, Enviroment enviroment)
        {
            this.Enviroment = enviroment;
            this.Camera = camera;
            LightIntensivity = lightIntensivity;
            Camera.OnRotate += (an , ax) => this.UpdateVisible();
            Camera.OnMove += (v) => this.UpdateVisible();
            Enviroment.OnChange += this.UpdateVisible;
            this.Rasterizer = new RasterizerService(new List<IFragmentShader>());
            this.UpdateVisible();
        }
        public void UpdateVisible()
        {
            this.ZBuffer = Rasterizer.GetDepthMap(this.Enviroment, this.Camera);
        }

        public FragmentInfo[] ComputeFragments(FragmentInfo[] polysFragments, Camera camera)
        {
            foreach (var frag in polysFragments)
            {
                //вычисляем глобальные координаты вершины
                var gPos = camera.Pivot.ToGlobalCoords(frag.Coordinate);
                //дистанция до света
                var lghDir = Pivot.Center - gPos;
                var distance = lghDir.Length();
                var local = Pivot.ToLocalCoords(gPos);
                var proectToLight = Camera.ScreenProection(local).ToPoint();
                if (proectToLight.X >= 0 && proectToLight.X < Camera.ScreenWidth && proectToLight.Y >= 0
                    && proectToLight.Y < Camera.ScreenHeight)
                {
                    int index = proectToLight.Y * Camera.ScreenWidth + proectToLight.X;
                    var n = Vector3.Normalize(frag.Normal);
                    var ld = Vector3.Normalize(lghDir);
                    
                    //вычислем сдвиг глубины
                    float bias = (float)Math.Max(5 * (1.0 - VectorMath.Cross(n, ld)), 0.05);
                    if (ZBuffer[index] == 0 || ZBuffer[index] + bias >= local.Z)
                    {
                        frag.Color = (int) (frag.Color * this.LightIntensivity / 15);
                    }
                }
                else
                {
                    frag.Color = (int) (frag.Color * this.LightIntensivity / 15);
                }
            }

            return polysFragments;
        }
    }
}