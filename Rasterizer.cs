using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;
using Poly = System.Tuple<DevilRender.Vertex, DevilRender.Vertex, DevilRender.Vertex>;

namespace DevilRender
{
    public class Rasterizer
    {
        public Vertex[] ZBuffer;
        public int[] VisibleIndexes;
        public int VisibleCount;
        public FastBitmap PreviousBuffer;
        public int Width;
        public int Height;
        public Camera Camera;
        public IShader[] Shaders;
        public int DefaultShading = 1;

        public Rasterizer(Camera camera, params IShader[] shaders)
        {
            PreviousBuffer = new FastBitmap(1,1);
            Shaders = shaders;
            Width = camera.ScreenWidth;
            Height = camera.ScreenHeight;
            Camera = camera;
            if (Shaders.Length != 0)
            {
                DefaultShading = 3;
            }
        }
        public Bitmap Rasterize(IEnumerable<Primitive> primitives)
        {
            ComputeVisibleVertices(primitives);
            var buffer = new FastBitmap(Width, Height);
            for (int i = 0; i < VisibleCount; i++)
            {
                int index = VisibleIndexes[i];
                var vertex = ZBuffer[index];
                if (vertex.Texture != null)
                {
                    int texX = Math.Min((int)(vertex.TextureCoord.X * vertex.Texture.Width), vertex.Texture.Width - 1);
                    int texY = Math.Min(vertex.Texture.Height - (int)(vertex.TextureCoord.Y * vertex.Texture.Height), vertex.Texture.Height - 1);
                    var color = vertex.Texture.GetPixel(texX, texY);
                    vertex.Color = Color.FromArgb(color.A, color.R / DefaultShading, color.G / DefaultShading, color.B / DefaultShading);
                }
                else
                {
                    vertex.Color = Color.FromArgb(255, 30, 30, 30);
                }
                ComputeShaders(index, vertex, buffer);
            }
            PreviousBuffer.Dispose();
            PreviousBuffer = buffer;
            return buffer.Bitmap;
        }
        public void ComputeVisibleVertices(IEnumerable<Primitive> primitives)
        {
            VisibleCount = 0;
            VisibleIndexes = new int[Width * Height];
            ZBuffer = new Vertex[Width * Height];
            foreach (var prim in primitives)
            {
                foreach (var poly in prim.GetPolys())
                {
                    MakeLocal(poly);
                    if (Observed(poly))
                    {
                        ComputePoly(poly.Item1, poly.Item2, poly.Item3);
                    }
                }
            }
        }
        public void ComputeShaders(int index, Vertex vertex, FastBitmap buffer)
        {
            for (int i = 0; i < Shaders.Length; i++)
            {
                Shaders[i].ComputeShader(vertex, Camera);
            }
            buffer.SetPixel(index, vertex.Color);
        }
        public bool Observed(Poly p)
        {
            return Camera.InObserve(p.Item1.Position)
                || Camera.InObserve(p.Item2.Position) || Camera.InObserve(p.Item3.Position);
        }
        public void MakeLocal(Poly poly)
        {
            poly.Item1.Position = Camera.Pivot.ToLocalCoords(poly.Item1.Position);
            poly.Item2.Position = Camera.Pivot.ToLocalCoords(poly.Item2.Position);
            poly.Item3.Position = Camera.Pivot.ToLocalCoords(poly.Item3.Position);

        }
        public void ComputePoly(Vertex v1, Vertex v2, Vertex v3)
        {
            var v1p = Camera.ScreenProection(v1.Position);
            var v2p = Camera.ScreenProection(v2.Position);
            var v3p = Camera.ScreenProection(v3.Position);

            //сортируем треугольник по x координате
            if (v1p.X > v2p.X) { var w = v1p; v1p = v2p; v2p = w; var v = v1; v1 = v2; v2 = v; }
            if (v2p.X > v3p.X) { var w = v2p; v2p = v3p; v3p = w; var v = v2; v2 = v3; v3 = v; }
            if (v1p.X > v2p.X) { var w = v1p; v1p = v2p; v2p = w; var v = v1; v1 = v2; v2 = v; }

            //считаем количество шагов для построения линии алгоритмом Брезенхема
            int x12 = Math.Max((int)v2p.X - (int)v1p.X, 1);
            int x13 = Math.Max((int)v3p.X - (int)v1p.X, 1);

            float dy12 = (v2p.Y - v1p.Y) / x12; var dr12 = (v2.Position - v1.Position) / x12; var dt12 = (v2.TextureCoord - v1.TextureCoord) / x12; var dn12 = (v2.Normal - v1.Normal) / x12;
            float dy13 = (v3p.Y - v1p.Y) / x13; var dr13 = (v3.Position - v1.Position) / x13; var dt13 = (v3.TextureCoord - v1.TextureCoord) / x13; var dn13 = (v3.Normal - v1.Normal) / x13;

            Vector3 deltaUp, deltaDown, deltaUpNormal, deltaDownNormal; Vector2 deltaUpTexture, deltaDownTexture; float deltaUpY, deltaDownY;
            if (dy12 > dy13) { deltaUp = dr12; deltaDown = dr13; deltaUpY = dy12; deltaDownY = dy13; deltaUpTexture = dt12; deltaDownTexture = dt13; deltaUpNormal = dn12; deltaDownNormal = dn13; }
            else { deltaUp = dr13; deltaDown = dr12; deltaUpY = dy13; deltaDownY = dy12; deltaUpTexture = dt13; deltaDownTexture = dt12; deltaUpNormal = dn13; deltaDownNormal = dn12; }
            ComputePolyPart(v1.Position, deltaUp, deltaDown, x12, 1, v1p, deltaUpY, deltaDownY, v1.TextureCoord, deltaUpTexture, deltaDownTexture, v1.Normal, deltaUpNormal, deltaDownNormal, v1.Texture);

            //аналогично обрабатываем вторую часть треугольника
            int x32 = Math.Max(Math.Abs((int)v2p.X - (int)v3p.X), 1);
            int x31 = Math.Max(Math.Abs((int)v1p.X - (int)v3p.X), 1);

            float dy32 = (v2p.Y - v3p.Y) / x32; var dr32 = (v2.Position - v3.Position) / x32; var dt32 = (v2.TextureCoord - v3.TextureCoord) / x32; var dn32 = (v2.Normal - v3.Normal) / x32;
            float dy31 = (v1p.Y - v3p.Y) / x31; var dr31 = (v1.Position - v3.Position) / x31; var dt31 = (v1.TextureCoord - v3.TextureCoord) / x31; var dn31 = (v1.Normal - v3.Normal) / x31;

            if (dy32 > dy31) { deltaUp = dr32; deltaDown = dr31; deltaUpY = dy32; deltaDownY = dy31; deltaUpTexture = dt32; deltaDownTexture = dt31; deltaUpNormal = dn32; deltaDownNormal = dn31; }
            else { deltaUp = dr31; deltaDown = dr32; deltaUpY = dy31; deltaDownY = dy32; deltaUpTexture = dt31; deltaDownTexture = dt32; deltaUpNormal = dn31; deltaDownNormal = dn32; }
            ComputePolyPart(v3.Position, deltaUp, deltaDown, x32, -1, v3p, deltaUpY, deltaDownY, v3.TextureCoord, deltaUpTexture, deltaDownTexture, v3.Normal, deltaUpNormal, deltaDownNormal, v3.Texture);
        }
        public void ComputePolyPart(Vector3 start, Vector3 deltaUp, Vector3 deltaDown,
            int xSteps, int xDir, Vector2 pixelStart, float deltaUpPixel, float deltaDownPixel,
            Vector2 textureStart, Vector2 deltaUpTexture, Vector2 deltaDownTexture,
            Vector3 normalStart, Vector3 deltaUpNormal, Vector3 deltaDownNormal, FastBitmap texture)
        {
            int pixelStartX = (int)pixelStart.X;
            Vector3 up = start - deltaUp, down = start - deltaDown;
            Vector2 textureUp = textureStart - deltaUpTexture, textureDown = textureStart - deltaDownTexture;
            Vector3 normalUp = normalStart - deltaUpNormal, normalDown = normalStart - deltaDownNormal;
            float pixelUp = pixelStart.Y - deltaUpPixel, pixelDown = pixelStart.Y - deltaDownPixel;
            for (int i = 0; i <= xSteps; i++)
            {
                up += deltaUp; pixelUp += deltaUpPixel; textureUp += deltaUpTexture; normalUp += deltaUpNormal;
                down += deltaDown; pixelDown += deltaDownPixel; textureDown += deltaDownTexture; normalDown += deltaDownNormal;
                int steps = ((int)pixelUp - (int)pixelDown);
                var delta = steps == 0 ? Vector3.Zero : (up - down) / steps;
                var deltaTexture = steps == 0 ? Vector2.Zero : (textureUp - textureDown) / steps;
                var deltaNormal = steps == 0 ? Vector3.Zero : (normalUp - normalDown) / steps;
                Vector3 position = down - delta;
                Vector2 texCoord = textureDown - deltaTexture;
                Vector3 normal = normalDown - deltaNormal;
                for (int g = 0; g <= steps; g++)
                {
                    position += delta;
                    texCoord += deltaTexture;
                    normal += deltaNormal;
                    var proection = new Point(pixelStartX + i * xDir, (int)pixelDown + g);
                    if (proection.X >= 0 && proection.X < Width && proection.Y >= 0 && proection.Y < Height)
                    {
                        int index = proection.Y * Width + proection.X;
                        if (ZBuffer[index] == null)
                        {
                            ZBuffer[index] = new Vertex(position, Color.Black, texCoord, normal) { Texture = texture };
                            VisibleIndexes[VisibleCount] = index;
                            VisibleCount++;
                        }
                        else if (ZBuffer[index].Position.Z > position.Z)
                        {
                            ZBuffer[index] = new Vertex(position, Color.Black, texCoord, normal) { Texture = texture };
                        }
                    }
                }
            }
        }
    }
}
