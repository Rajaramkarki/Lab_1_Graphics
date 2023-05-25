using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace Lab5
{
    public sealed class MainWindow : GameWindow
    {
        private readonly string title;
        private int program;
        private double time;
        private List<RenderObject> renderObjects = new List<RenderObject>();
        private Color4 bgColor = new Color4(0.1f, 0.1f, 0.3f, 1.0f);

        public MainWindow()
            : base(750, // initial width
                500, // initial height
                GraphicsMode.Default,
                "",  // initial title
                GameWindowFlags.Fullscreen,
                DisplayDevice.Default,
                4, // OpenGL major version
                5, // OpenGL minor version
                GraphicsContextFlags.ForwardCompatible)
        {
            title += "dreamstatecoding.blogspot.com: OpenGL Version: " + GL.GetString(StringName.Version);
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }


        protected override void OnLoad(EventArgs e)
        {
            VSync = VSyncMode.Off;
            Vertex[] vertices = ObjectFactory.CreateSolidCube(0.2f, Color4.HotPink);
            renderObjects.Add(new RenderObject(vertices));

            CursorVisible = true;

            program = CreateProgram();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            GL.Enable(EnableCap.DepthTest);
            Closed += OnClosed;
        }
        
        private void OnClosed(object sender, EventArgs eventArgs)
        {
            Exit();
        }

        public override void Exit()
        {
            Debug.WriteLine("Exit called");
            foreach(var obj in renderObjects)
                obj.Dispose();
            GL.DeleteProgram(program);
            base.Exit();
        }
        
        private int CreateProgram()
        {
            try
            {
                var program = GL.CreateProgram();
                var shaders = new List<int>();
                shaders.Add(CompileShader(ShaderType.VertexShader, @"Components\Shaders\1Vert\simplePipeVert.c"));
                shaders.Add(CompileShader(ShaderType.FragmentShader, @"Components\Shaders\5Frag\simplePipeFrag.c"));

                foreach (var shader in shaders)
                    GL.AttachShader(program, shader);
                GL.LinkProgram(program);
                var info = GL.GetProgramInfoLog(program);
                if (!string.IsNullOrWhiteSpace(info))
                    throw new Exception($"CompileShaders ProgramLinking had errors: {info}");

                foreach (var shader in shaders)
                {
                    GL.DetachShader(program, shader);
                    GL.DeleteShader(shader);
                }
                return program;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        private int CompileShader(ShaderType type, string path)
        {
            var shader = GL.CreateShader(type);
            var src = File.ReadAllText(path);
            GL.ShaderSource(shader, src);
            GL.CompileShader(shader);
            var info = GL.GetShaderInfoLog(shader);
            if (!string.IsNullOrWhiteSpace(info))
                throw new Exception($"CompileShader {type} had errors: {info}");
            return shader;
        }

        private Matrix4 _modelView;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            time += e.Time;
            var k = (float)time*0.05f;
            var t1 = Matrix4.CreateTranslation(
                (float) (Math.Sin(k * 5f) * 0.5f),
                (float) (Math.Cos(k * 5f) * 0.5f),
                0f);
            var r1 = Matrix4.CreateRotationX(k * 13.0f);
            var r2 = Matrix4.CreateRotationY(k * 13.0f);
            var r3 = Matrix4.CreateRotationZ(k * 3.0f);
            var s1 = Matrix4.CreateScale(k * 2.0f);
            _modelView = r1*r2*r3*t1*s1;

            HandleKeyboard();
        }
        private void HandleKeyboard()
        {
            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Key.Escape))
            {
                Exit();
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"{title}: (Vsync: {VSync}) FPS: {1f / e.Time:0}";
            GL.ClearColor(bgColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(program);
            GL.UniformMatrix4(20, false, ref _modelView);
            foreach (var renderObject in renderObjects)
            {
                renderObject.Render();
            }
            GL.PointSize(10);
            SwapBuffers();
        }
        
    }
}
