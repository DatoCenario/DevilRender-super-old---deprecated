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
    using System.Threading;
    using Timer = System.Windows.Forms.Timer;

    public class RenderApplication
    {
        private object prepareLocker = new object();
        private RasterizerService rasterizerService;
        private Queue<FastBitmap> buffers;
        private CancellationTokenSource renderSessionSource;
        
        public int MaxBuffersCount { get; set; } = 1000000000;
        public Timer Timer { get; private set; }
        
        public List<Camera> Cameras { get; private set; }
        
        public Form RenderForm { get; private set; }
        
        public Enviroment Enviroment { get; private set; }
        
        public CameraController Controller { get; private set; }
        
        public int CameraIndex { get; private set; }

        public Camera CurrentCamera => this.Cameras[this.CameraIndex];

        int Width;
        int Height;
        public RenderApplication(
            Enviroment enviroment,
            int winWidth,
            int winHeight,
            List<Camera> cameras,
            List<IFragmentShader> fragmentShaders)
        {
            this.buffers = new Queue<FastBitmap>();
            this.rasterizerService = new RasterizerService(fragmentShaders);
            Width = winHeight;
            Height = winHeight;
            Enviroment = enviroment;
            Cameras = cameras;
            RenderForm = new Form1()
            {
                BackColor = Color.Black,
                Width = winWidth,
                Height = winHeight
            };
            Controller = new CameraController();
            RenderForm.KeyDown += (args, e) => Controller.KeyDown(e);
            RenderForm.KeyUp += (args, e) => Controller.KeyUp(e);
            RenderForm.MouseMove += (args, e) => Controller.HanleMouse(e);
            Timer = new Timer();
            Timer.Interval = 100;
            Timer.Tick += async (args, e) =>
            {
                if (Monitor.IsEntered(prepareLocker))
                {
                    return;
                }
                
                Monitor.Enter(prepareLocker);

                FastBitmap buffer = new FastBitmap(1, 1);
                try
                {
                    buffer = await this.rasterizerService.GetSnapshot(this.Enviroment, this.CurrentCamera);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                if (buffer.Width != 1)
                {
                    RenderForm.BackgroundImage = new Bitmap(buffer.Bitmap, RenderForm.Size);
                }
                Monitor.Exit(prepareLocker);
            };
        }
        
        public void Start()
        {
            //this.StartRenderSession();
            Timer.Start();
            Application.Run(RenderForm);
        }
        
        public void MoveCamera()
        {
            this.CameraIndex = (this.CameraIndex + 1) % this.Cameras.Count;
        }
        
        public async Task PrepareNewBuffer()
        {
            if (this.buffers.Count > this.MaxBuffersCount)
            {
                return;
            }
            
            var newBuffer = await this.rasterizerService.GetSnapshot(this.Enviroment, this.CurrentCamera);
            this.buffers.Enqueue(newBuffer);
        }
        
        public async Task StartRenderSession()
        {
            this.renderSessionSource = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                while (!this.renderSessionSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await this.PrepareNewBuffer();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("EXXX!!");
                    }
                }
            });
        }

        public void StopRenderSession()
        {
            if (this.renderSessionSource != null && !this.renderSessionSource.IsCancellationRequested)
            {
                this.renderSessionSource.Cancel();
            }
        }
        
        public FastBitmap DequeueLastBuffer()
        {
            if (buffers.Count == 0)
            {
                return new FastBitmap(1, 1);
            }
            return this.buffers.Dequeue();
        }
    }
}