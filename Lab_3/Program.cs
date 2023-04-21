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

namespace Lab_3
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

        public void circleMidPoint(int centerX, int centerY, int radius, ref float[] vertices)
        {
            int x = radius;
            int y = 0;
            int decisionOver2 = 1 - x;

            List<Vector2> points = new List<Vector2>();

            while (y <= x)
            {
                points.Add(new Vector2(centerX + x, centerY + y));
                points.Add(new Vector2(centerX + y, centerY + x));
                points.Add(new Vector2(centerX - x, centerY + y));
                points.Add(new Vector2(centerX - y, centerY + x));
                points.Add(new Vector2(centerX + x, centerY - y));
                points.Add(new Vector2(centerX + y, centerY - x));
                points.Add(new Vector2(centerX - x, centerY - y));
                points.Add(new Vector2(centerX - y, centerY - x));

                y++;
                if (decisionOver2 <= 0)
                {
                    decisionOver2 += 2 * y + 1;
                }
                else
                {
                    x--;
                    decisionOver2 += 2 * (y - x) + 1;
                }
            }



            // Resize vertices array to hold all the points
            Array.Resize(ref vertices, points.Count * 2);

            // Copy all the points into vertices array
            for (int i = 0; i < points.Count; i++)
            {
                vertices[i * 2] = points[i].X;
                vertices[i * 2 + 1] = points[i].Y;
            }
        }


        public void DrawEllipse(int x0, int y0, int a, int b, ref float[] vertices)
        {
            int sqrA = a * a;
            int sqrB = b * b;
            int twoSqrA = 2 * sqrA;
            int twoSqrB = 2 * sqrB;
            int x = 0;
            int y = b;
            int dx = sqrB * (1 - 2 * b);
            int dy = sqrA;
            int error = 0;
            int index = 0;

            while (y >= 0)
            {
                // Octant 1
                vertices[index++] = x0 + x;
                vertices[index++] = y0 + y;

                // Octant 2
                vertices[index++] = x0 - x;
                vertices[index++] = y0 + y;

                // Octant 3
                vertices[index++] = x0 - x;
                vertices[index++] = y0 - y;

                // Octant 4
                vertices[index++] = x0 + x;
                vertices[index++] = y0 - y;

                int e2 = 2 * error;

                if (dx < 0 && e2 <= dy)
                {
                    x++;
                    dx += twoSqrB;
                    error += dx;
                }
                else if (dy < 0 && e2 <= dx)
                {
                    y--;
                    dy += twoSqrA;
                    error += dy;
                }
                else if (dy > 0 && e2 > dx)
                {
                    x--;
                    dx -= twoSqrB;
                    error += dx;
                }
                else if (dx > 0 && e2 > dy)
                {
                    y++;
                    dy -= twoSqrA;
                    error += dy;
                }
            }

            // Resize the array to fit all the generated vertices
            Array.Resize(ref vertices, index);
        }



        protected override void OnLoad()
        {
            GL.ClearColor(new Color4(0.0f, 0.5f, 0.5f, 1.0f));

            float[] vertices = new float[20];
            //circleMidPoint(500, 500, 100, ref vertices);
            circleMidPoint(500, 500, 300, ref vertices);

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