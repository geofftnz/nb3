using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.Threading;
using OpenTKExtensions;
using OpenTKExtensions.Framework;
using OpenTKExtensions.Text;
using System.Diagnostics;
using OpenTKExtensions.Filesystem;
using System.Collections.Concurrent;
using OpenTKExtensions.Input;
using OpenTK.Input;

namespace nb3.Vis
{
    public class VisHost : GameWindow
    {
        private const string SHADERPATH = @"../../Res/Shaders;Res/Shaders";

        private GameComponentCollection components = new GameComponentCollection();
        private Font font;
        private TextManager text;
        private KeyboardActionManager keyboardActions;

        private TextBlock title = new TextBlock("t0", "NeuralBeat3 ©2016 Geoff Thornburrow", new Vector3(0.0f, 0.05f, 0f), 0.0005f, new Vector4(1f, 0.8f, 0.2f, 1f));
        private Matrix4 overlayProjection;
        private Matrix4 overlayModelview;

        private FrameData frameData = new FrameData();
        private GlobalTextures globalTextures = new GlobalTextures();
        private Stopwatch timer = new Stopwatch();
        private FileSystemPoller shaderUpdatePoller = new FileSystemPoller(SHADERPATH.Split(';')[0]);
        private double lastShaderPollTime = 0.0;

        private ConcurrentQueue<Player.AudioAnalysisSample> sampleQueue = new ConcurrentQueue<Player.AudioAnalysisSample>();

        private float[] tempSpectrum = new float[GlobalTextures.SPECTRUMRES]; // this will be coming in from the analysis side.

        private object playerPropertyLock = new object();
        private Player.Player _player = null;
        public Player.Player Player
        {
            get
            {
                lock (playerPropertyLock)
                    return _player;
            }
            set
            {
                lock (playerPropertyLock)
                    _player = value;
            }
        }


        public VisHost()
            : base(800, 600,
                 new OpenTK.Graphics.GraphicsMode(
                     new OpenTK.Graphics.ColorFormat(8, 8, 8, 8), 24, 8),
                    "NB3",
                    GameWindowFlags.Default,
                    DisplayDevice.Default,
                    4, 0,
                    OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible
                 )
        {
            this.VSync = VSyncMode.Off;

            this.UpdateFrame += VisHost_UpdateFrame;
            this.RenderFrame += VisHost_RenderFrame;
            this.Load += VisHost_Load;
            this.Unload += VisHost_Unload;
            this.Resize += VisHost_Resize;
            this.Closed += VisHost_Closed;
            this.Closing += VisHost_Closing;

            this.Keyboard.KeyDown += Keyboard_KeyDown;



            // framedata setup
            frameData.GlobalTextures = globalTextures;

            // create components
            //components.Add(font = new Font(@"res\font\calibrib.ttf_sdf.2048.png", @"res\font\calibrib.ttf_sdf.2048.txt"), 1);
            components.Add(font = new Font(@"res\font\lucon.ttf_sdf.1024.png", @"res\font\lucon.ttf_sdf.1024.txt"), 1);
            components.Add(text = new TextManager(), 2);
            components.Add(keyboardActions = new KeyboardActionManager(), 1);
            components.Add(globalTextures);
            components.Add(new Renderers.Components.DebugSpectrumWaterfall());

            font.Loaded += (s, e) => { text.Font = font; };

            // set default shader loader
            ShaderProgram.DefaultLoader = new OpenTKExtensions.Loaders.MultiPathFileSystemLoader(SHADERPATH);
        }

        private void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            this.keyboardActions.ProcessKeyDown(e.Key, e.Modifiers);
        }

        private void VisHost_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
        }

        private void VisHost_Closed(object sender, EventArgs e)
        {

        }

        private void VisHost_Resize(object sender, EventArgs e)
        {
            SetProjection();
            components.Resize(ClientRectangle.Width, ClientRectangle.Height);
        }

        private void VisHost_Unload(object sender, EventArgs e)
        {
            components.Unload();
        }

        private void VisHost_Load(object sender, EventArgs e)
        {
            Keyboard.KeyRepeat = true;

            components.Load();
            SetProjection();
            timer.Start();
            InitKeyboard();
        }

        private void InitKeyboard()
        {
            // winamp-style controls
            keyboardActions.Add(Key.Left, 0, () => { Player?.Skip(-5); });
            keyboardActions.Add(Key.Right, 0, () => { Player?.Skip(5); });
            //keyboardActions.Add(Key.Z, 0, () => {  });  //TODO: previous in playlist
            keyboardActions.Add(Key.X, 0, () => { Player?.Play();  });
            keyboardActions.Add(Key.C, 0, () => { Player?.TogglePause(); });
            keyboardActions.Add(Key.V, 0, () => { Player?.Stop(); });
            //keyboardActions.Add(Key.B, 0, () => {  });  //TODO: next in playlist
        }

        private void VisHost_RenderFrame(object sender, FrameEventArgs e)
        {
            if (shaderUpdatePoller.HasChanges)
            {
                components.Reload();
                shaderUpdatePoller.Reset();
            }

            text.AddOrUpdate(title);

            GL.ClearColor(0.0f, 0.1f, 0.4f, 1.0f);
            GL.ClearDepth(1.0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            components.Render(frameData);



            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            text.Render();

            SwapBuffers();
        }

        private void VisHost_UpdateFrame(object sender, FrameEventArgs e)
        {
            frameData.Time = timer.Elapsed.TotalSeconds;

            // poll for shader changes
            // TODO: make poll time a parameter
            if (frameData.Time - lastShaderPollTime > 2.0)
            {
                shaderUpdatePoller.Poll();
                lastShaderPollTime = frameData.Time;
            }


            Player.AudioAnalysisSample sample;

            while (sampleQueue.TryDequeue(out sample))
            {
                globalTextures.PushSample(sample.Spectrum);
            }

            //for (int i = 0; i < GlobalTextures.SPECTRUMRES; i++)
            //{
            //    tempSpectrum[i] = (float)(Math.Sin((double)i * frameData.Time * 0.0001 + Math.Sin((double)i * frameData.Time * 3.0) * 0.5) * 0.5 + 0.5);
            //}

            //globalTextures.PushSample(tempSpectrum);

            Thread.Sleep(0);
        }

        private void SetProjection()
        {
            GL.Viewport(this.ClientRectangle);
            SetOverlayProjection();

            //SetGBufferCombineProjection();
        }

        private void SetOverlayProjection()
        {
            float aspect = ClientRectangle.Height > 0 ? ((float)this.ClientRectangle.Width / (float)this.ClientRectangle.Height) : 1f;

            overlayProjection = Matrix4.CreateOrthographicOffCenter(0.0f, aspect, 1.0f, 0.0f, 0.001f, 10.0f);
            overlayModelview = Matrix4.Identity * Matrix4.CreateTranslation(0.0f, 0.0f, -1.0f);

            this.text.Projection = this.overlayProjection;
            this.text.Modelview = this.overlayModelview;
        }

        public void AddSample(Player.AudioAnalysisSample sample)
        {
            sampleQueue.Enqueue(sample);
        }
    }
}
