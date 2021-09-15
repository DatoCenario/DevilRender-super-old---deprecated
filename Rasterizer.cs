using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;


namespace DevilRender
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class FragmentMap
    {
        public int[] NotEmptyPositions { get; set; }
        
        public FragmentInfo[] Fragments { get; set; }
        
        public int LastPosIndex { get; set; }
    }

    public class DepthInfo
    {

        public DepthInfo(int screenX, int screenY, float depth)
        {
            ScreenX = screenX;
            ScreenY = screenY;
            Depth = depth;
        }

        public int ScreenX { get; set; }
        
        public int ScreenY { get; set; }
        
        public float Depth { get; set; }

        public override int GetHashCode()
        {
            return ((this.ScreenX * 37) + this.ScreenY) * 37;
        }
    }
    
    public class FragmentInfo
    {
        public FragmentInfo(Vector3 coordinate, Vector3 normal, Vector2 textureCoordinate, Vector2 screenCoordinate)
        {
            Coordinate = coordinate;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            ScreenCoordinate = screenCoordinate;
        }

        public Vector3 Coordinate { get; set; }
        
        public Vector3 Normal { get; set; }
        
        public Vector2 TextureCoordinate { get; set; }
        
        public Vector2 ScreenCoordinate { get; set; }
        
        public Int32 Color { get; set; }
    }

    public interface IFragmentShader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FragmentInfo[] ComputeFragments(FragmentInfo[] polysFragments, Camera camera);
    }

    public class RasterizerService
    {
        private readonly object locker = new object();
        private readonly List<IFragmentShader> fragmentShaders;
        
        public RasterizerService(List<IFragmentShader> fragmentShaders)
        {
            this.fragmentShaders = fragmentShaders;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<FastBitmap> GetSnapshot(Enviroment enviroment, Camera camera)
        {
            var map = await this.GetFragmentMap(enviroment, camera);
            //var computedShaderFrags = await this.ComputeShaders(frags, camera);
            var newBitmap = new FastBitmap(camera.ScreenWidth, camera.ScreenHeight);
            for (int i = 0; i < map.LastPosIndex; i++)
            {
                var index = map.NotEmptyPositions[i];
                var frag = map.Fragments[index];
                newBitmap.SetPixel((int)frag.ScreenCoordinate.X, (int)frag.ScreenCoordinate.Y, frag.Color);
            }

            return newBitmap;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<FragmentMap> GetFragmentMap(Enviroment enviroment, Camera camera)
        {
            var fragmentMap = new FragmentMap()
            {
                Fragments = new FragmentInfo[camera.ScreenWidth * camera.ScreenHeight],
                LastPosIndex = 0,
                NotEmptyPositions = new int[camera.ScreenWidth * camera.ScreenHeight]
            };
            
            var allPolys = enviroment.GetPrimitives()
                .SelectMany(p => p.GetLocalPolys(camera))
                .Where(p => this.Observed(p ,camera));

            await Task.Run(() =>
            {
                Parallel.ForEach(allPolys, (p) => this.GetAllFragmentsFromPoly(p, camera, fragmentMap));
            });

            return fragmentMap;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float[] GetDepthMap(Enviroment environment, Camera camera)
        {
           var depthInfos = environment.GetPrimitives()
                .SelectMany(p => p.GetLocalPolys(camera))
                .Where(p => this.Observed(p, camera))
                .AsParallel()
                .SelectMany(p => this.GetDepthMapFromPoly(p, camera))
                .GroupBy(p => p)
                .Select(g => g
                    .Aggregate((f1, f2) => f1.Depth <= f2.Depth ? f1 : f2));
           var map = new float[camera.ScreenWidth * camera.ScreenHeight];
           foreach (var depthInfo in depthInfos)
           {
               map[camera.ScreenWidth * depthInfo.ScreenY + depthInfo.ScreenX] = depthInfo.Depth;
           }

           return map;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetAllFragmentsFromPoly(Poly poly, Camera camera, FragmentMap fragmentMap)
        {
            var v1p = camera.ScreenProection(poly.v1.Position);
            var v2p = camera.ScreenProection(poly.v2.Position);
            var v3p = camera.ScreenProection(poly.v3.Position);
            
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

            if (x2 > 0 && x1 < camera.ScreenWidth)
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

                rDiff = camera.ScreenWidth - 1 - x2;

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

                this.InterpolationStep(x1, x12, 1,
                                                                u, d, tu, td, nu, nd, pu, pd, du, dd, dtu,
                                                                dtd, dnu, dnd, dpu,
                                                                dpd, camera, fragmentMap);
            }

            if (x2 < camera.ScreenWidth && x3 > 0)
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

                rDiff = camera.ScreenWidth - 1 - x3;

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
                    x3 = camera.ScreenWidth - 1;
                }

                this.InterpolationStep(x3, x32, -1, u, d,
                                            tu, td, nu, nd,
                                            pu, pd, du, dd, dtu, dtd, dnu, dnd,
                                            dpu, dpd, camera, fragmentMap);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<DepthInfo> GetDepthMapFromPoly(Poly poly, Camera camera)
        {
            var map = new List<DepthInfo>();
            var v1p = camera.ScreenProection(poly.v1.Position);
            var v2p = camera.ScreenProection(poly.v2.Position);
            var v3p = camera.ScreenProection(poly.v3.Position);
            
            if (v1p.X > v2p.X) { var w = v1p; v1p = v2p; v2p = w; var v = poly.v1; poly.v1 = poly.v2; poly.v2 = v; }
            if (v2p.X > v3p.X) { var w = v2p; v2p = v3p; v3p = w; var v = poly.v2; poly.v2 = poly.v3; poly.v3 = v; }
            if (v1p.X > v2p.X) { var w = v1p; v1p = v2p; v2p = w; var v = poly.v1; poly.v1 = poly.v2; poly.v2 = v; }

            int x1 = (int)v1p.X;
            int x2 = (int)v2p.X;
            int x3 = (int)v3p.X;

            int x12 = x2 - x1;
            int x13 = x3 - x1;
            int x32 = x3 - x2;

            float u, d, du, dd;
            float pu, pd, dpu, dpd;
            int rDiff;

            if (x2 > 0 && x1 < camera.ScreenWidth)
            {
                u = poly.v1.Position.Z;
                d = poly.v1.Position.Z;
                pu = v1p.Y;
                pd = v1p.Y;

                if (x12 != 0)
                {
                    dpu = (v2p.Y - v1p.Y) / x12;
                    dpd = (v3p.Y - v1p.Y) / x13;

                    if (dpu > dpd)
                    {
                        du = (poly.v2.Position.Z - poly.v1.Position.Z) / x12;
                        dd = (poly.v3.Position.Z - poly.v1.Position.Z) / x13;
                    }
                    else
                    {
                        var t = dpu; dpu = dpd; dpd = t;
                        du = (poly.v3.Position.Z - poly.v1.Position.Z) / x13;
                        dd = (poly.v2.Position.Z - poly.v1.Position.Z) / x12;
                    }
                }
                else
                {
                    dpu = 0;
                    dpd = 0;
                    du = 0;
                    dd = 0;
                }

                rDiff = camera.ScreenWidth - 1 - x2;

                if (rDiff < 0)
                {
                    x12 += rDiff;
                }

                if (x1 < 0)
                {
                    x12 += x1;
                    u -= du * x1;
                    d -= dd * x1;
                    pu -= dpu * x1;
                    pd -= dpd * x1;
                    x1 = 0;
                }

                map.AddRange(this.InterpolationStepForDepthMap(x1, x2, u, d, pu, pd, du, dd, dpu, dpd, camera));
            }

            if (x2 < camera.ScreenWidth && x3 > 0)
            {
                u = poly.v3.Position.Z;
                d = poly.v3.Position.Z;
                pu = v3p.Y;
                pd = v3p.Y;

                if (x32 != 0)
                {
                    dpu = (v2p.Y - v3p.Y) / x32;
                    dpd = (v1p.Y - v3p.Y) / x13;

                    if (dpu > dpd)
                    {
                        du = (poly.v2.Position.Z - poly.v3.Position.Z) / x32;
                        dd = (poly.v1.Position.Z - poly.v3.Position.Z) / x13;
                    }
                    else
                    {
                        var t = dpu; dpu = dpd; dpd = t;
                        du = (poly.v1.Position.Z - poly.v3.Position.Z) / x13;
                        dd = (poly.v2.Position.Z - poly.v3.Position.Z) / x32;
                    }
                }
                else
                {
                    dpu = 0;
                    dpd = 0;
                    du = 0;
                    dd = 0;
                }

                if (x2 < 0)
                {
                    x32 += x2;
                    x2 = 0;
                }

                rDiff = camera.ScreenWidth - 1 - x3;

                if (rDiff < 0)
                {
                    x32 += rDiff;
                    u -= du * rDiff;
                    d -= dd * rDiff;
                    pu -= dpu * rDiff;
                    pd -= dpd * rDiff;
                    x3 = camera.ScreenWidth - 1;
                }

                map.AddRange(this.InterpolationStepForDepthMap(x3, x32, u, d, pu, pd, du, dd, dpu, dpd, camera));
            }

            return map;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<DepthInfo> InterpolationStepForDepthMap(
            int xStart,
            int xEnd,
            float startUpZ,
            float startDownZ,
            float startProectionUp,
            float startProectionDown,
            float startUpZDelta,
            float startDownZDelta,
            float startProectionUpDelta,
            float startProectionDownDelta,
            Camera camera)
        {
            var map = new List<DepthInfo>();

            for (int i = 0; i <= xEnd; i++)
            {
                var posZ = startDownZ;

                if (startProectionUp > 0)
                {
                    int steps;
                    int up = (int) startProectionUp, down = (int) startProectionDown;
                    int diffU = camera.ScreenHeight - 1 - up;

                    if (diffU < 0)
                    {
                        up = camera.ScreenHeight - 1;
                    }

                    steps = up - down;
                    float delta;
                    if (steps != 0)
                    {
                        delta = (startUpZ - startDownZ) / steps;
                    }
                    else
                    {
                        delta = 0;
                    }

                    if (down < 0)
                    {
                        posZ -= delta * down;
                        steps += down;
                        down = 0;
                    }

                    int x = xStart + i;
                    for (int g = 0; g <= steps; g++)
                    {
                        int y = down + g;
                        map.Add(new DepthInfo(x, y, posZ));
                        posZ += delta;
                    }
                }

                startDownZ+= startDownZDelta;
                startUpZ += startUpZDelta;
                startProectionDown += startProectionDownDelta;
                startProectionUp += startProectionUpDelta;
            }

            return map;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InterpolationStep(
            int xStart,
            int xSteps,
            int xDir,
            Vector3 startUp,
            Vector3 startDown,
            Vector2 startTexUp,
            Vector2 startTexDown,
            Vector3 startNormalUp,
            Vector3 startNormalDown,
            float startProectionUp,
            float startProectionDown,
            Vector3 startUpDelta,
            Vector3 startDownDelta,
            Vector2 startTexUpDelta,
            Vector2 startTexDownDelta,
            Vector3 startNormalUpDelta,
            Vector3 startNormalDownDelta,
            float startProectionUpDelta,
            float startProectionDownDelta,
            Camera camera,
            FragmentMap fragmentMap)
        {
            for (int i = 0; i <= xSteps; i++)
            {
                Vector3 pos = startDown;
                Vector3 norm = startNormalDown;
                Vector2 tex = startTexDown;
                Vector3 delta, deltaNormal;
                Vector2 deltaTexture;

                if (startProectionUp > 0)
                {
                    int steps;
                    int up = (int) startProectionUp, down = (int) startProectionDown;
                    int diffU = camera.ScreenHeight - 1 - up;

                    if (diffU < 0)
                    {
                        up = camera.ScreenHeight - 1;
                    }

                    steps = up - down;
                    if (steps != 0)
                    {
                        delta = (startUp - startDown) / steps;
                        deltaTexture = (startTexUp - startTexDown) / steps;
                        deltaNormal = (startNormalUp - startNormalDown) / steps;
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

                    int x = xStart + (i * xDir);
                    for (int g = 0; g <= steps; g++)
                    {
                        int y = down + g;
                        int index = y * camera.ScreenWidth + x;

                        if (fragmentMap.Fragments[index] == null)
                        {
                            var frag = new FragmentInfo(
                                pos,
                                norm,
                                tex,
                                new Vector2(x, y)) {Color = int.MaxValue};
                            fragmentMap.Fragments[index] = frag;
                            Monitor.Enter(this.locker);
                            fragmentMap.NotEmptyPositions[fragmentMap.LastPosIndex] = index;
                            fragmentMap.LastPosIndex++;
                            Monitor.Exit(this.locker);
                        }
                        else if(fragmentMap.Fragments[index].Coordinate.Z > pos.Z)
                        {
                            var frag = new FragmentInfo(
                                pos,
                                norm,
                                tex,
                                new Vector2(x, y)) {Color = int.MaxValue};
                            fragmentMap.Fragments[index] = frag;
                        }

                        pos += delta;
                        norm += deltaNormal;
                        tex += deltaTexture;
                    }
                }

                startDown += startDownDelta;
                startUp += startUpDelta;
                startTexDown += startTexDownDelta;
                startTexUp += startTexUpDelta;
                startNormalDown += startNormalDownDelta;
                startNormalUp += startNormalUpDelta;
                startProectionDown += startProectionDownDelta;
                startProectionUp += startProectionUpDelta;
            }
        }

        private async Task<IEnumerable<FragmentInfo>> ComputeShaders(FragmentInfo[] fragmentInfos, Camera camera)
        {
            foreach (var shader in this.fragmentShaders)
            { 
                fragmentInfos = shader.ComputeFragments(fragmentInfos, camera);
            }

            return fragmentInfos;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Observed(Poly p, Camera camera)
        {
            return camera.InObserve(p.v1.Position)
                   || camera.InObserve(p.v2.Position) || camera.InObserve(p.v3.Position);
        }
    }
}
