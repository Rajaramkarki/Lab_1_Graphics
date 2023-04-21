using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Xml.Linq;
using System.Drawing;

namespace Lab_2
{

    public class Game : GameWindow
    {

        private int vertexbufferObject;
        private int shaderProgramObject;
        private int vertexArrayObject;
        

        public Game()
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.CenterWindow(new Vector2i(1270, 1200));
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        public void lineBresenham(int x, int y, int x2, int y2, ref float[] vertices)
        {
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            int index = 0;
            for (int i = 0; i <= longest; i++)
            {
                if (index >= vertices.Length)
                {
                    Array.Resize(ref vertices, vertices.Length + 2);
                }
                vertices[index] = x;
                vertices[index + 1] = y;
                index += 2;

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        public void lineDDA(int x1, int y1, int x2, int y2, ref float[] vertices)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;

            int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));

            float xIncrement = (float)dx / (float)steps;
            float yIncrement = (float)dy / (float)steps;

            Array.Resize(ref vertices, (steps + 1) * 2);

            float x = x1;
            float y = y1;

            for (int i = 0; i <= steps; i++)
            {
                int index = i * 2;
                vertices[index] = x;
                vertices[index + 1] = y;

                x += xIncrement;
                y += yIncrement;
            }
        }


        protected override void OnLoad()
        {
            GL.ClearColor(new Color4(1f, 1f, 1f, 1f));

            float[] vertices = new float[20];
            lineDDA(900, 500, 200, 200, ref vertices);

            this.vertexbufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexbufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            this.vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(this.vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexbufferObject);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);

            string vertexShaderCode =
                @"
                 #version 330 core
                 
                 uniform vec2 ViewportSize;
                 layout (location=0) in vec3 aPosition;
                 
                 void main(){
                    
                    float nx = aPosition.x / ViewportSize.x * 2f - 1f;
                    float ny = aPosition.y / ViewportSize.y * 2f - 1f;
                    gl_Position = vec4(nx, ny, 0f, 1f);

                 }";

            string pixelShaderCode =
                @"
                 #version 330 core
                 out vec4 fragColor;
                 uniform vec4 vColor;

                 void main(){
                    fragColor = vColor;
                 }
                 ";


            int vertexShaderObject = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexbufferObject, vertexShaderCode);
            GL.CompileShader(vertexShaderObject);

            string vertexShaderInfo = GL.GetShaderInfoLog(vertexShaderObject);
            if (vertexShaderInfo != String.Empty)
            {
                Console.WriteLine(vertexShaderInfo);
            }

            int pixelShaderObject = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(pixelShaderObject, pixelShaderCode);
            GL.CompileShader(pixelShaderObject);

            string pixelShaderInfo = GL.GetShaderInfoLog(pixelShaderObject);
            if (pixelShaderInfo != String.Empty)
            {
                Console.WriteLine(pixelShaderInfo);
            }

            this.shaderProgramObject = GL.CreateProgram();

            GL.AttachShader(this.shaderProgramObject, vertexShaderObject);
            GL.AttachShader(this.shaderProgramObject, pixelShaderObject);

            GL.LinkProgram(this.shaderProgramObject);

            GL.DetachShader(this.shaderProgramObject, vertexShaderObject);
            GL.DetachShader(this.shaderProgramObject, pixelShaderObject);

            GL.DeleteShader(vertexShaderObject);
            GL.DeleteShader(pixelShaderObject);

            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            GL.UseProgram(this.shaderProgramObject);
            int viewportSizeUniformLocation = GL.GetUniformLocation(this.shaderProgramObject, "ViewportSize");
            GL.Uniform2(viewportSizeUniformLocation, (float)viewport[2], (float)viewport[3]);
            GL.UseProgram(0);

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(this.vertexbufferObject);

            GL.UseProgram(0);
            GL.DeleteProgram(this.shaderProgramObject);


            base.OnUnload();
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(this.shaderProgramObject);
            int uniformLocations = GL.GetUniformLocation(this.shaderProgramObject, "vColor");

            GL.BindVertexArray(this.vertexArrayObject);

            GL.Uniform4(uniformLocations, 0f, 0f, 0f, 1.0f);
            GL.DrawArrays(PrimitiveType.Points, 0, 2000);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        static void Main(string[] args)
        {
            using (Game game = new Game())
            {
                game.Run();
            }
        }

    }
}