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
        
        public void ComputePolyPart(Vector3 start, Vector3 deltaUp, Vector3 deltaDown,
            int xSteps, int xDir, Vector2 pixelStart, float deltaUpPixel, float deltaDownPixel,
            Vector2 textureStart, Vector2 deltaUpTexture, Vector2 deltaDownTexture,
            Vector3 normalStart, Vector3 deltaUpNormal, Vector3 deltaDownNormal, Primitive primitive)
        {
            int pixelStartX = (int)pixelStart.X;
            Vector3 up = start, down = start;
            Vector2 textureUp = textureStart, textureDown = textureStart;
            Vector3 normalUp = normalStart, normalDown = normalStart;
            float pixelUp = pixelStart.Y, pixelDown = pixelStart.Y;
            for (int i = 0; i <= xSteps; i++)
            {
                int steps = ((int)pixelUp - (int)pixelDown);
                Vector3 delta, deltaNormal; Vector2 deltaTexture;
                if (steps != 0)
                {
                    delta = (up - down) / steps;
                    deltaTexture = (textureUp - textureDown) / steps;
                    deltaNormal = (normalUp - normalDown) / steps;
                }
                else
                {
                    delta = Vector3.Zero; deltaTexture = Vector2.Zero; deltaNormal = Vector3.Zero;
                }
                int pixDown = (int)pixelDown;
                Vector3 position = down;
                Vector2 texCoord = textureDown;
                Vector3 normal = normalDown;
                for (int g = 0; g <= steps; g++)
                {
                    var proection = new Point(pixelStartX + i * xDir, pixDown + g);
                    if (proection.X >= 0 && proection.X < Width && proection.Y >= 0 && proection.Y < Height)
                    {
                        int index = proection.Y * Width + proection.X;
                        if (ZBuffer[index] == null)
                        {
                            ZBuffer[index] = new Vertex(position, Color.Black, texCoord, normal) { Primitive = primitive };
                            VisibleIndexes[VisibleCount] = index;
                            VisibleCount++;
                        }
                        else if (ZBuffer[index].Position.Z > position.Z)
                        {
                            ZBuffer[index] = new Vertex(position, Color.Black, texCoord, normal) { Primitive = primitive };
                        }
                    }

                    position += delta;
                    texCoord += deltaTexture;
                    normal += deltaNormal;
                }

                up += deltaUp; 
                pixelUp += deltaUpPixel; 
                textureUp += deltaUpTexture;
                normalUp += deltaUpNormal;
                down += deltaDown; 
                pixelDown += deltaDownPixel;
                textureDown += deltaDownTexture; 
                normalDown += deltaDownNormal;
            }
        }
        
    }
}
