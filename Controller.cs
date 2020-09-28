using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevilRender
{
    public class Controller
    {
        public HashSet<Keys> DownKeys;
        public int Speed { get; set; } = 40;

        public DevilRender DevilRender;
        public Point LastMousePos { get; private set; }
        public Camera CurrentCamera => DevilRender.Preparer.CurrentRaterizer.Camera;
        public Controller(DevilRender render)
        {
            DownKeys = new HashSet<Keys>();
            DevilRender = render;
        }
        public void KeyDown(KeyEventArgs e)
        {
            if (!DownKeys.Contains(e.KeyCode))
            {
                DownKeys.Add(e.KeyCode);
            }
        }

        public void KeyUp(KeyEventArgs e)
        {
            if (DownKeys.Contains(e.KeyCode))
            {
                DownKeys.Remove(e.KeyCode);
            }
        }


        public void ComputeKeys()
        {
            foreach (var key in DownKeys)
            {
                switch (key)
                {
                    case Keys.Q:
                        CurrentCamera.Rotate(-0.05f, Axis.Y);
                        break;
                    case Keys.S:
                        var v = -CurrentCamera.Pivot.ZAxis * Speed;
                        CurrentCamera.Move(v);
                        break;
                    case Keys.E:
                        CurrentCamera.Rotate(0.05f, Axis.Y);
                        break;
                    case Keys.W:
                        v = CurrentCamera.Pivot.ZAxis * Speed;
                        CurrentCamera.Move(v);
                        break;
                    case Keys.D:
                        v = CurrentCamera.Pivot.XAxis * Speed;
                        CurrentCamera.Move(v);
                        break;
                    case Keys.A:
                        v = -CurrentCamera.Pivot.XAxis * Speed;
                        CurrentCamera.Move(v);
                        break;
                    case Keys.R:
                        CurrentCamera.Rotate(-0.05f, Axis.X);
                        break;
                    case Keys.F:
                        CurrentCamera.Rotate(0.05f, Axis.X);
                        break;
                    case Keys.Y:
                        DevilRender.Preparer.MoveNextRasterizer();
                        break;
                    case Keys.M:
                        DevilRender.Timer.Stop();
                        var camera = new Camera(CurrentCamera.Pivot.Center , 1 , (float)Math.PI / 2 ,1920 * 7 , 1080 * 7);
                        camera.Pivot.XAxis = CurrentCamera.Pivot.XAxis;
                        camera.Pivot.YAxis = CurrentCamera.Pivot.YAxis;
                        camera.Pivot.ZAxis = CurrentCamera.Pivot.ZAxis;
                        var preparer = new BufferPreparer(new List<Camera>() { camera}, DevilRender.Enviroment);
                        preparer.PrepareNewBuffer();
                        var buffer = preparer.Buffers.Dequeue();
                        buffer.Save(@"F:\c#\DevilRender\DevilRender\Images\img.png");
                        DevilRender.Timer.Start();
                        break;
                }
            }
        }
        public void HanleMouse(MouseEventArgs e)
        {
            //float deltaX = e.X - LastMousePos.X;
            //float deltaY = e.Y - LastMousePos.Y;
            //CurrentCamera.Rotate(-deltaX / 100, Axis.Y);
            //CurrentCamera.Rotate(-deltaY / 100, Axis.X);
            //LastMousePos = e.Location;
        }
    }
}
