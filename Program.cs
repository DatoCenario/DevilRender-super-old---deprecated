using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using DevilRender.Primitives;
using System.Runtime.InteropServices;
using System.Drawing;

namespace DevilRender
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var env = new Enviroment(1000);
            CreateSylvanasScene(env);
            var render = new DevilRender(env, 1920 , 1080);
            render.Start();
        }

        public static void CreateWeaponScene(Enviroment enviroment)
        {
            var texure = FastBitmap.FromBitmap((Bitmap)Image.FromFile(@"F:\c#\DevilRender\DevilRender\Models\Cyborg-Weapon textures.png"));
            var model = ObjParser.FromObjFile(@"F:\c#\DevilRender\DevilRender\Models\Cyborg_Weapon.obj" , texure);
            model.Scale(1000f);
            model.Rotate(1.57f  , Axis.Y);
            enviroment.AddPrimitive(model);
        }

        public static void CreateSylvanasScene(Enviroment enviroment)
        {
            var model = ObjParser.FromObjFile(@"F:\c#\DevilRender\DevilRender\Models\sylvanas_obj.obj", null);
            model.Scale(1f);
            model.Rotate(3.14f + 0.5f, Axis.Y);
            model.Move(new Vector3(0, -2200, 0));
            enviroment.AddPrimitive(model);
        }
        public static void CreateCubeScene(Enviroment enviroment)
        {
            var rand = new Random();

            var cubes = Enumerable.Range(0, 3)
                .SelectMany(i1 => Enumerable.Range(0, 3)
                .SelectMany(i2 => Enumerable.Range(0, 3)
                .Select(i3 => new Cube(new Vector3(i1 * 500, i2 * 500, i3 * 500), rand.Next(50, 400)))))
                .ToArray();

            foreach (var item in cubes)
            {
                item.Rotate((float)(rand.NextDouble() * Math.PI), Axis.X);
                item.Rotate((float)(rand.NextDouble() * Math.PI), Axis.Y);
                item.Rotate((float)(rand.NextDouble() * Math.PI), Axis.Z);
                enviroment.Primitives.Add(item);
            }
        }
    }
}















