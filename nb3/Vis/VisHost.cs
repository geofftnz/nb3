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
using nb3.Player.Analysis;
using OpenTKExtensions.Resources;
using nb3.Common;

namespace nb3.Vis
{
    public class VisHost : GameWindow
    {
        private const string SHADERPATH = @"../../Res/Shaders;Res/Shaders";

        private GameComponentCollection components = new GameComponentCollection();
        private Font font;
        private TextManager text;
        private KeyboardActionManager keyboardActions;

        private TextBlock title = new TextBlock("t0", "NeuralBeat3 ©2016-2020 Geoff Thornburrow", new Vector3(0.0f, 0.05f, 0f), 0.0005f, new Vector4(1f, 0.8f, 0.2f, 1f));
        private Matrix4 overlayProjection;
        private Matrix4 overlayModelview;

        private FrameData frameData = new FrameData();
        private GlobalTextures globalTextures = new GlobalTextures();
        private Stopwatch timer = new Stopwatch();
        private OpenTKExtensions.Components.FrameCounter frameCounter;
        private FileSystemPoller shaderUpdatePoller = new FileSystemPoller(SHADERPATH.Split(';')[0]);
        private double lastShaderPollTime = 0.0;

        private ConcurrentQueue<AudioAnalysisSample> sampleQueue = new ConcurrentQueue<AudioAnalysisSample>();

        private float[] tempSpectrum = new float[Globals.SPECTRUMRES]; // this will be coming in from the analysis side.

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
                {
                    //TODO: unhook existing events?
                    _player = value;
                    
                    if (_player != null)
                    {
                        _player.SpectrumReady += (s, e) => { AddSample(e.Sample); };
                        //_player.PlayerStart += OnPlayerStart;
                    }

                }
            }
        }

        public ConcurrentQueue<AudioAnalysisSample> SampleQueue
        {
            get
            {
                return sampleQueue;
            }

            set
            {
                sampleQueue = value;
            }
        }

        private int lastTracksPlayed = 0;


        public VisHost(Player.Player player)
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
            Player = player;

            VSync = VSyncMode.On;

            UpdateFrame += VisHost_UpdateFrame;
            RenderFrame += VisHost_RenderFrame;
            Load += VisHost_Load;
            Unload += VisHost_Unload;
            Resize += VisHost_Resize;
            Closed += VisHost_Closed;
            Closing += VisHost_Closing;

            Keyboard.KeyDown += Keyboard_KeyDown;
            Keyboard.KeyUp += Keyboard_KeyUp;

            // set default shader loader
            ShaderProgram.DefaultLoader = new OpenTKExtensions.Loaders.MultiPathFileSystemLoader(SHADERPATH);


            // framedata setup
            frameData.GlobalTextures = globalTextures;

            // create components
            //components.Add(font = new Font(@"res\font\calibrib.ttf_sdf.2048.png", @"res\font\calibrib.ttf_sdf.2048.txt"), 1);
            components.Add(font = new Font(@"res\font\lucon.ttf_sdf.1024.png", @"res\font\lucon.ttf_sdf.1024.txt"), 1);
            components.Add(text = new TextManager(), 2);
            components.Add(keyboardActions = new KeyboardActionManager(), 1);
            components.Add(globalTextures);
            components.Add(frameCounter = new OpenTKExtensions.Components.FrameCounter(font));
            //components.Add(new Renderers.Components.DebugSpectrumWaterfall());
            components.Add(new Renderers.AnalysisDebugRenderer(font, Player.FilterOutputNames));
            //components.Add(new Renderers.BasicShaderRenderer());

            font.Loaded += (s, e) => { text.Font = font; };

        }

        private void Keyboard_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            components.ProcessKeyUp(e);
        }

        private void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            //this.keyboardActions.ProcessKeyDown(e.Key, e.Modifiers);
            components.ProcessKeyDown(e);
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
            keyboardActions.Add(Key.X, 0, () => { Player?.Play(); });
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

            //text.AddOrUpdate(title);

            GL.ClearColor(0.0f, 0.1f, 0.4f, 1.0f);
            GL.ClearDepth(1.0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            components.Render(frameData);



            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            //text.Render();

            SwapBuffers();
            Thread.Sleep(0);
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

            if (Player != null && Player.TracksPlayed != lastTracksPlayed)
            {
                globalTextures.Reset();
                Player?.WaveFormat.Maybe(wf => globalTextures.SampleRate = wf.SampleRate);
                lastTracksPlayed = Player.TracksPlayed;
            }

            AudioAnalysisSample sample;

            while (SampleQueue.TryDequeue(out sample))
            {
                globalTextures.PushSample(sample);
            }

            components.Update(frameData);

            Thread.Sleep(0);
        }

        private void SetProjection()
        {
            GL.Viewport(ClientRectangle);
            SetOverlayProjection();

            //SetGBufferCombineProjection();
        }

        private void SetOverlayProjection()
        {
            float aspect = ClientRectangle.Height > 0 ? ((float)ClientRectangle.Width / (float)ClientRectangle.Height) : 1f;

            overlayProjection = Matrix4.CreateOrthographicOffCenter(0.0f, aspect, 1.0f, 0.0f, 0.001f, 10.0f);
            overlayModelview = Matrix4.Identity * Matrix4.CreateTranslation(0.0f, 0.0f, -1.0f);

            text.Projection = overlayProjection;
            text.Modelview = overlayModelview;
        }

        public void AddSample(AudioAnalysisSample sample)
        {
            SampleQueue.Enqueue(sample);
        }

        //private void OnPlayerStart(object sender, Player.PlayerStartEventArgs e)
        //{
        //    globalTextures.SampleRate = e.SampleRate;
        //}

    }
}
