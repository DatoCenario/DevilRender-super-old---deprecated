using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using DevilRender.Primitives;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace DevilRender
{
    static class Program
    {
        [STAThread]
        static async Task Main()
        {

            StartWeaponScene();
        }

        // public static void CreateThorScene(Enviroment enviroment, Camera bindingCamera, Camera original)
        // {
        //     var thor = new Function(Vector3.Zero, (x, y) => (float)(Math.Sin(x) * x * Math.Cos(y) * y), 0, 0, 1000, 1000, 10);
        //     var ps = new PhongModelShader(new Light() { Pos = original.Pivot.Center, Intensivity = 10f });
        //     var sm = new ShadowMappingShader(enviroment, new Rasterizer(bindingCamera), 3f);
        //     thor.Shaders = new IShader[] { ps, sm};
        //     thor.Scale(10f);
        //     enviroment.AddPrimitive(thor);
        // }

        public static void StartKitchenScene(Enviroment enviroment, Camera bindingCamera, Camera original)
        {
            var ps = new PhongModelShader(new Light() { Pos = original.Pivot.Center, Intensivity = 3 });
            var sm = new ShadowMappingShader( 10f, bindingCamera, enviroment);
            var model = ObjParser.FromObjFile(@"F:\c#\DevilRender\DevilRender\Models\Lancer_Evolution_model.obj",
               null );
            
            model.Scale(40f);
            enviroment.AddPrimitive(model);
            var devilRender = new RenderApplication(
                enviroment,
                800,
                600,
                new List<Camera>
                {
                    original,
                    bindingCamera
                },
                new List<IFragmentShader>() {sm, ps});
            devilRender.Start();
        }
        public static void StartWeaponScene()
        {
            var enviroment = new Enviroment(10000);
            var originCam = new Camera(new Vector3(0, 0, -500), 100, (float) (Math.PI / 2), 800, 600);
            var shadowMapCam = new Camera(new Vector3(0, 0, -500), 100, (float) (Math.PI / 2), 800, 600);
            var model = ObjParser.FromObjFile(@"C:\Users\Dato\Desktop\DevilRender\Models\Cyborg_Weapon.obj",
                @"C:\Users\Dato\Desktop\DevilRender\Models\Cyborg-Weapon textures.png");
            var sm = new ShadowMappingShader(2, shadowMapCam, enviroment);
            var ps = new PhongModelShader(new Light(new Vector3(0, 0, -1000), 2));
            model.Scale(1000f);
            model.Rotate(1.57f, Axis.Y);
            enviroment.AddPrimitive(model);

            //var devilRender = new RenderApplication(
            //    enviroment,
            //    800,
            //    600,
            //    new List<Camera>
            //    {
            //        originCam,
            //        shadowMapCam
            //    },
            //    new List<IFragmentShader>() {sm, ps});
            //devilRender.Start();

            var r = new RasterizerService(new List<IFragmentShader> { sm, ps });
            while(true)
            {
                var sw = new Stopwatch();
                sw.Start();
                r.GetSnapshot(enviroment, originCam).Wait();
                sw.Stop();
                Console.WriteLine($"{sw.ElapsedMilliseconds}");
            }
        }

        // public static void CreateSylvanasScene(Enviroment enviroment, Camera bindingCamera)
        // {
        //     var model = ObjParser.FromObjFile(@"F:\c#\DevilRender\DevilRender\Models\sylvanas_obj.obj", null);
        //     var smShader = new ShadowMappingShader(enviroment, new Rasterizer(bindingCamera), 50000f);
        //     var pShader = new PhongModelShader(new Light(bindingCamera.Pivot.Center, 2f));
        //     model.Shaders = new IShader[] { smShader, pShader };
        //     model.Scale(10f);
        //     model.Rotate(3.14f + 0.5f, Axis.Y);
        //     model.Move(new Vector3(0, -2200, 0));
        //     enviroment.AddPrimitive(model);
        // }
        // public static void CreateCubeScene(Enviroment enviroment, Camera bindingCamera, int x, int y, int z)
        // {
        //     var rand = new Random();
        //
        //     var ps = new PhongModelShader(new Light(Vector3.Zero, 10f));
        //     var sm = new ShadowMappingShader(enviroment, new Rasterizer(bindingCamera), 100000f);
        //     var cubes = Enumerable.Range(0, x)
        //         .SelectMany(i1 => Enumerable.Range(0, y)
        //         .SelectMany(i2 => Enumerable.Range(0, z)
        //         .Select(i3 => new Cube(new Vector3(i1 * 200 * x, i2 * 200 * y, i3 * 200 * z), rand.Next(50, 800))
        //         {
        //             Shaders =
        //         new IShader[] { sm, ps }
        //         })))
        //         .ToArray();
        //
        //     foreach (var item in cubes)
        //     {
        //         item.Rotate((float)(rand.NextDouble() * Math.PI), Axis.X);
        //         item.Rotate((float)(rand.NextDouble() * Math.PI), Axis.Y);
        //         item.Rotate((float)(rand.NextDouble() * Math.PI), Axis.Z);
        //         enviroment.AddPrimitive(item);
        //     }
        // }
    }
}















