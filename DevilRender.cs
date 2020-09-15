using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace DevilRender
{
    public class DevilRender
    {
        public Timer Timer { get; private set; }
        public BufferPreparer Preparer { get; private set; }
        public Enviroment Enviroment { get; private set; }
        public List<Camera> Cameras { get; private set; }
        public Form RenderForm { get; private set; }
        public Controller Controller { get; private set; }

        int Width;
        int Height;
        public DevilRender(Enviroment enviroment, int winWidth, int winHeight)
        {
            Width = winHeight;
            Height = winHeight;
            Enviroment = enviroment;
            Cameras = new List<Camera>() 
            {
                new Camera(new Vector3(0, 0, -500), 1, (float)Math.PI / 2, winWidth, winHeight) ,
                new Camera(new Vector3(100, 100, -500), 1, (float)Math.PI / 2, winWidth, winHeight) 
            };
            Preparer = new BufferPreparer(Cameras , enviroment);
            RenderForm = new Form1()
            {
                BackColor = Color.Black,
                Width = winWidth,
                Height = winHeight
            };
            Controller = new Controller(this);
            RenderForm.KeyDown += (args, e) => Controller.KeyDown(e);
            RenderForm.KeyUp += (args, e) => Controller.KeyUp(e);
            RenderForm.MouseMove += (args, e) => Controller.HanleMouse(e);
            Timer = new Timer();
            Timer.Interval = 100;
            Timer.Tick += (args, e) =>
            {
                Controller.ComputeKeys();
                Preparer.PrepareNewBuffer();
                var buffer = Preparer.GetBuffer();
                if (buffer.Width != 1)
                {
                    RenderForm.BackgroundImage = new Bitmap(buffer , RenderForm.Size);
                }
            };
        }
        public void Start()
        {
            Timer.Start();
            Application.Run(RenderForm);
        }
    }
}