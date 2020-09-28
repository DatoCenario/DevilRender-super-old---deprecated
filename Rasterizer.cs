using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace DevilRender
{
    public class Rasterizer
    {
        public Vertex[] ZBuffer;
        public int[] VisibleIndexes;
        public int VisibleCount;
        public FastBitmap PreviousBuffer;
        public int Width => Camera.ScreenWidth;
        public int Height => Camera.ScreenHeight;

        public Camera Camera;
        public int DefaultShading = 1;

        public Rasterizer(Camera camera)
        {
            VisibleIndexes = new int[camera.ScreenWidth * camera.ScreenHeight];
            PreviousBuffer = new FastBitmap(1, 1);
            Camera = camera;
        }
        public Bitmap Rasterize(IEnumerable<Primitive> primitives)
        {
            ComputeVisibleVertices(primitives);
            var buffer = new FastBitmap(Width, Height);
            for (int i = 0; i < VisibleCount; i++)
            {
                int index = VisibleIndexes[i];
                var vertex = ZBuffer[index];
                var tex = vertex.Primitive.Texture;
                if (tex != null)
                {
                    int texX = Math.Min((int)(vertex.TextureCoord.X * tex.Width), tex.Width - 1);
                    int texY = Math.Min(tex.Height - (int)(vertex.TextureCoord.Y * tex.Height), tex.Height - 1);
                    var color = tex.GetPixel(texX, texY);
                    vertex.Color = new TGAColor(color.A, color.R, color.G, color.B);
                }
                else
                {
                    vertex.Color = new TGAColor(255, 30, 30, 30);
                }
                //buffer.SetPixel(index,vertex.Color);
                ComputeShaders(index, ref vertex, buffer);
            }

            //while (!Parallel.For(0, VisibleCount, (i) =>
            //{
            //    int index = VisibleIndexes[i];
            //    var vertex = ZBuffer[index];
            //    var tex = vertex.Primitive.Texture;
            //    if (tex != null)
            //    {
            //        int texX = Math.Min((int)(vertex.TextureCoord.X * tex.Width), tex.Width - 1);
            //        int texY = Math.Min(tex.Height - (int)(vertex.TextureCoord.Y * tex.Height), tex.Height - 1);
            //        var color = tex.GetPixel(texX, texY);
            //        vertex.Color = Color.FromArgb(color.A, color.R / DefaultShading, color.G / DefaultShading, color.B / DefaultShading);
            //    }
            //    else
            //    {
            //        vertex.Color = Color.FromArgb(255, 30, 30, 30);
            //    }
            //    ComputeShaders(index, ref vertex, buffer);
            //}).IsCompleted) ;

            PreviousBuffer.Dispose();
            PreviousBuffer = buffer;
            return buffer.Bitmap;
        }
        public void ComputeVisibleVertices(IEnumerable<Primitive> primitives)
        {
            VisibleCount = 0;
            ZBuffer = new Vertex[Width * Height];
            foreach (var prim in primitives)
            {
                // while(!Parallel.ForEach(prim.GetPolys(), (p) =>
                //{
                //    MakeLocal(p);
                //    if (Observed(p))
                //    {
                //        ComputePoly(p.Item1 , p.Item2 , p.Item3 , prim);
                //    }
                //}).IsCompleted);
                for (int i = 0; i < prim.Indexes.Length; i += 3)
                {
                    var poly = prim.GetLocalPoly(Camera, i);
                    if (Observed(ref poly))
                    {
                        ComputePoly(ref poly);
                    }
                }
            }
        }
        public void ComputeShaders(int index, ref Vertex vertex, FastBitmap buffer)
        {
            for (int i = 0; i < vertex.Primitive.Shaders.Length; i++)
            {
                vertex.Primitive.Shaders[i].ComputeShader(ref vertex, Camera);
            }
            lock (buffer)
            {
                buffer.SetPixel(index, Color.FromArgb(vertex.Color.a, vertex.Color.r, vertex.Color.g, vertex.Color.b));
            }
        }
        public bool Observed(ref Poly p)
        {
            return Camera.InObserve(p.v1.Position)
                || Camera.InObserve(p.v2.Position) || Camera.InObserve(p.v3.Position);
        }
        public void ComputePoly(ref Poly poly)
        {
            var v1p = Camera.ScreenProection(poly.v1.Position);
            var v2p = Camera.ScreenProection(poly.v2.Position);
            var v3p = Camera.ScreenProection(poly.v3.Position);

            if (v1p.X > v2p.X) { var w = v1p; v1p = v2p; v2p = w; var v = poly.v1; poly.v1 = poly.v2; poly.v2 = v; }
            if (v2p.X > v3p.X) { var w = v2p; v2p = v3p; v3p = w; var v = poly.v2; poly.v2 = poly.v3; poly.v3 = v; }
            if (v1p.X > v2p.X) { var w = v1p; v1p = v2p; v2p = w; var v = poly.v1; poly.v1 = poly.v2; poly.v2 = v; }

            int x1 = (int)v1p.X;
            int x2 = (int)v2p.X;
            int x3 = (int)v3p.X;

            int x12 = x2 - x1;
            int x13 = x3 - x1;
            int x32 = x3 - x2;

            Vector3 u, d, du, dd, nu, nd, dnu, dnd;
            Vector2 tu, td, dtu, dtd;
            float pu, pd, dpu, dpd;
            int rDiff;

            if (x2 > 0 && x1 < Width)
            {
                u = poly.v1.Position;
                d = poly.v1.Position;
                tu = poly.v1.TextureCoord;
                td = poly.v1.TextureCoord;
                nu = poly.v1.Normal;
                nd = poly.v1.Normal;
                pu = v1p.Y;
                pd = v1p.Y;

                if (x12 != 0)
                {
                    dpu = (v2p.Y - v1p.Y) / x12;
                    dpd = (v3p.Y - v1p.Y) / x13;

                    if (dpu > dpd)
                    {
                        du = (poly.v2.Position - poly.v1.Position) / x12;
                        dtu = (poly.v2.TextureCoord - poly.v1.TextureCoord) / x12;
                        dnu = (poly.v2.Normal - poly.v1.Normal) / x12;

                        dd = (poly.v3.Position - poly.v1.Position) / x13;
                        dtd = (poly.v3.TextureCoord - poly.v1.TextureCoord) / x13;
                        dnd = (poly.v3.Normal - poly.v1.Normal) / x13;
                    }
                    else
                    {
                        var t = dpu; dpu = dpd; dpd = t;

                        du = (poly.v3.Position - poly.v1.Position) / x13;
                        dtu = (poly.v3.TextureCoord - poly.v1.TextureCoord) / x13;
                        dnu = (poly.v3.Normal - poly.v1.Normal) / x13;

                        dd = (poly.v2.Position - poly.v1.Position) / x12;
                        dtd = (poly.v2.TextureCoord - poly.v1.TextureCoord) / x12;
                        dnd = (poly.v2.Normal - poly.v1.Normal) / x12;
                    }
                }
                else
                {
                    dpu = 0;
                    dpd = 0;
                    dtu = Vector2.Zero;
                    dtd = Vector2.Zero;
                    dnu = Vector3.Zero;
                    dnd = Vector3.Zero;
                    du = Vector3.Zero;
                    dd = Vector3.Zero;
                }

                rDiff = Width - 1 - x2;

                if (rDiff < 0)
                {
                    x12 += rDiff;
                }

                if (x1 < 0)
                {
                    x12 += x1;
                    u -= du * x1;
                    d -= dd * x1;
                    tu -= dtu * x1;
                    td -= dtd * x1;
                    nu -= dnu * x1;
                    nd -= dnd * x1;
                    pu -= dpu * x1;
                    pd -= dpd * x1;
                    x1 = 0;
                }

                //method for computing part of poly
                for (int i = 0; i <= x12; i++)
                {
                    Vector3 pos = d;
                    Vector3 norm = nd;
                    Vector2 tex = td;
                    Vector3 delta, deltaNormal;
                    Vector2 deltaTexture;

                    if (pu > 0)
                    {
                        int steps;
                        int up = (int)pu, down = (int)pd;
                        int diffU = Height - 1 - up;

                        if (diffU < 0)
                        {
                            up = Height - 1;
                        }

                        steps = up - down;
                        if (steps != 0)
                        {
                            delta = (u - d) / steps;
                            deltaTexture = (tu - td) / steps;
                            deltaNormal = (nu - nd) / steps;
                        }
                        else
                        {
                            delta = Vector3.Zero;
                            deltaNormal = Vector3.Zero;
                            deltaTexture = Vector2.Zero;
                        }

                        if (down < 0)
                        {
                            pos -= delta * down;
                            tex -= deltaTexture * down;
                            norm -= deltaNormal * down;
                            steps += down;
                            down = 0;
                        }
                        int x = x1 + i;
                        for (int g = 0; g <= steps; g++)
                        {
                            int y = down + g;
                            int index = y * Width + x;
                            if (ZBuffer[index].Position.Z == 0)
                            {
                                ZBuffer[index] = new Vertex(pos, new TGAColor(), tex, norm, poly.v1.Primitive);
                                VisibleIndexes[VisibleCount] = index;
                                VisibleCount++;
                            }
                            else if (ZBuffer[index].Position.Z > pos.Z)
                            {
                                ZBuffer[index] = new Vertex(pos, new TGAColor(), tex, norm, poly.v1.Primitive); ;
                            }

                            pos += delta;
                            norm += deltaNormal;
                            tex += deltaTexture;
                        }
                    }

                    d += dd;
                    u += du;
                    pu += dpu;
                    pd += dpd;
                    nu += dnu;
                    nd += dnd;
                    tu += dtu;
                    td += dtd;
                }
                //*method for computing part of poly

            }

            if (x2 < Width && x3 > 0)
            {
                u = poly.v3.Position;
                d = poly.v3.Position;
                tu = poly.v3.TextureCoord;
                td = poly.v3.TextureCoord;
                nu = poly.v3.Normal;
                nd = poly.v3.Normal;
                pu = v3p.Y;
                pd = v3p.Y;

                if (x32 != 0)
                {
                    dpu = (v2p.Y - v3p.Y) / x32;
                    dpd = (v1p.Y - v3p.Y) / x13;

                    if (dpu > dpd)
                    {
                        du = (poly.v2.Position - poly.v3.Position) / x32;
                        dtu = (poly.v2.TextureCoord - poly.v3.TextureCoord) / x32;
                        dnu = (poly.v2.Normal - poly.v3.Normal) / x32;

                        dd = (poly.v1.Position - poly.v3.Position) / x13;
                        dtd = (poly.v1.TextureCoord - poly.v3.TextureCoord) / x13;
                        dnd = (poly.v1.Normal - poly.v3.Normal) / x13;
                    }
                    else
                    {
                        var t = dpu; dpu = dpd; dpd = t;

                        du = (poly.v1.Position - poly.v3.Position) / x13;
                        dtu = (poly.v1.TextureCoord - poly.v3.TextureCoord) / x13;
                        dnu = (poly.v1.Normal - poly.v3.Normal) / x13;

                        dd = (poly.v2.Position - poly.v3.Position) / x32;
                        dtd = (poly.v2.TextureCoord - poly.v3.TextureCoord) / x32;
                        dnd = (poly.v2.Normal - poly.v3.Normal) / x32;
                    }
                }
                else
                {
                    dpu = 0;
                    dpd = 0;
                    dtu = Vector2.Zero;
                    dtd = Vector2.Zero;
                    dnu = Vector3.Zero;
                    dnd = Vector3.Zero;
                    du = Vector3.Zero;
                    dd = Vector3.Zero;
                }

                if (x2 < 0)
                {
                    x32 += x2;
                    x2 = 0;
                }

                rDiff = Width - 1 - x3;

                if (rDiff < 0)
                {
                    x32 += rDiff;
                    u -= du * rDiff;
                    d -= dd * rDiff;
                    tu -= dtu * rDiff;
                    td -= dtd * rDiff;
                    nu -= dnu * rDiff;
                    nd -= dnd * rDiff;
                    pu -= dpu * rDiff;
                    pd -= dpd * rDiff;
                    x3 = Width - 1;
                }

                //method for computing part of poly
                for (int i = 0; i <= x32; i++)
                {
                    Vector3 pos = d;
                    Vector3 norm = nd;
                    Vector2 tex = td;
                    Vector3 delta, deltaNormal;
                    Vector2 deltaTexture;

                    if (pu > 0)
                    {
                        int steps;
                        int up = (int)pu, down = (int)pd;
                        int diffU = Height - 1 - up;

                        if (diffU < 0)
                        {
                            up = Height - 1;
                        }

                        steps = up - down;
                        if (steps != 0)
                        {
                            delta = (u - d) / steps;
                            deltaTexture = (tu - td) / steps;
                            deltaNormal = (nu - nd) / steps;
                        }
                        else
                        {
                            delta = Vector3.Zero;
                            deltaNormal = Vector3.Zero;
                            deltaTexture = Vector2.Zero;
                        }

                        if (down < 0)
                        {
                            pos -= delta * down;
                            tex -= deltaTexture * down;
                            norm -= deltaNormal * down;
                            steps += down;
                            down = 0;
                        }

                        int x = x3 - i;
                        for (int g = 0; g <= steps; g++)
                        {
                            int y = down + g;
                            int index = y * Width + x;
                            if (ZBuffer[index].Position.Z == 0)
                            {
                                ZBuffer[index] = new Vertex(pos, new TGAColor(), tex, norm, poly.v1.Primitive);
                                VisibleIndexes[VisibleCount] = index;
                                VisibleCount++;
                            }
                            else if (ZBuffer[index].Position.Z > pos.Z)
                            {
                                ZBuffer[index] = new Vertex(pos,  new TGAColor(), tex, norm, poly.v1.Primitive); ;
                            }

                            pos += delta;
                            norm += deltaNormal;
                            tex += deltaTexture;
                        }
                    }

                    d += dd;
                    u += du;
                    pu += dpu;
                    pd += dpd;
                    nu += dnu;
                    nd += dnd;
                    tu += dtu;
                    td += dtd;
                }
                //*method for computing part of poly
            }
        }
    }
}
