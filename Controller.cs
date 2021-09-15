using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DevilRender
{
    public class CameraController
    {
        public HashSet<Keys> DownKeys = new HashSet<Keys>();
        
        public int Speed { get; set; } = 40;
        
        public Point LastMousePos { get; private set; }

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

        public void ComputeKeys(Camera camera)
        {
            foreach (var key in DownKeys)
            {
                switch (key)
                {
                    case Keys.Q:
                        camera.Rotate(-0.05f, Axis.Y);
                        break;
                    case Keys.S:
                        var v = -camera.Pivot.ZAxis * Speed;
                        camera.Move(v);
                        break;
                    case Keys.E:
                        camera.Rotate(0.05f, Axis.Y);
                        break;
                    case Keys.W:
                        v = camera.Pivot.ZAxis * Speed;
                        camera.Move(v);
                        break;
                    case Keys.D:
                        v = camera.Pivot.XAxis * Speed;
                        camera.Move(v);
                        break;
                    case Keys.A:
                        v = -camera.Pivot.XAxis * Speed;
                        camera.Move(v);
                        break;
                    case Keys.R:
                        camera.Rotate(-0.05f, Axis.X);
                        break;
                    case Keys.F:
                        camera.Rotate(0.05f, Axis.X);
                        break;
                    case Keys.Y:
                        //DevilRender.Preparer.MoveNextRasterizer();
                        break;
                    case Keys.M:
                        break;
                        // DevilRender.Timer.Stop();
                        // var newCamera = new Camera(CurrentCamera.Pivot.Center , 1 , (float)Math.PI / 2 ,1920 * 7 , 1080 * 7);
                        // newCamera.Pivot.XAxis = CurrentCamera.Pivot.XAxis;
                        // newCamera.Pivot.YAxis = CurrentCamera.Pivot.YAxis;
                        // newCamera.Pivot.ZAxis = CurrentCamera.Pivot.ZAxis;
                        // var preparer = new BufferPreparer(new List<Camera>() { camera}, DevilRender.Enviroment);
                        // preparer.PrepareNewBuffer();
                        // var buffer = preparer.Buffers.Dequeue();
                        // buffer.Save(@"F:\c#\DevilRender\DevilRender\Images\img.png");
                        // DevilRender.Timer.Start();
                        // break;
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
