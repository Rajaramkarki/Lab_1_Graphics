using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Drawing;
using System.ComponentModel;


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

        void clipLineCohenSutherland(float[] vertices)
        {
            const int INSIDE = 0; // Bit code for inside region
            const int LEFT = 1;   // Bit code for left region
            const int RIGHT = 2;  // Bit code for right region
            const int BOTTOM = 4; // Bit code for bottom region
            const int TOP = 8;    // Bit code for top region

            float xMin = vertices[0];
            float yMin = vertices[1];
            float xMax = vertices[6];
            float yMax = vertices[7];

            float x0 = vertices[15];
            float y0 = vertices[16];
            float x1 = vertices[18];
            float y1 = vertices[19];

            // Compute the bit codes for the two endpoints of the line
            int code0 = computeOutCode(x0, y0, xMin, yMin, xMax, yMax);
            int code1 = computeOutCode(x1, y1, xMin, yMin, xMax, yMax);

            // Initialize the variables to store the clipped points
            float x0Clip = x0;
            float y0Clip = y0;
            float x1Clip = x1;
            float y1Clip = y1;

            bool accept = false;

            while (true)
            {
                if ((code0 == 0) && (code1 == 0))
                {
                    // Both endpoints are inside the region
                    accept = true;
                    break;
                }
                else if ((code0 & code1) != 0)
                {
                    // Both endpoints are outside the same region (one of the four regions)
                    break;
                }
                else
                {
                    // Some part of the line lies inside the region
                    int outCode = (code0 != 0) ? code0 : code1;

                    float x, y;

                    // Find the intersection point
                    if ((outCode & TOP) != 0)
                    {
                        x = x0 + (x1 - x0) * (yMax - y0) / (y1 - y0);
                        y = yMax;
                    }
                    else if ((outCode & BOTTOM) != 0)
                    {
                        x = x0 + (x1 - x0) * (yMin - y0) / (y1 - y0);
                        y = yMin;
                    }
                    else if ((outCode & RIGHT) != 0)
                    {
                        y = y0 + (y1 - y0) * (xMax - x0) / (x1 - x0);
                        x = xMax;
                    }
                    else
                    {
                        y = y0 + (y1 - y0) * (xMin - x0) / (x1 - x0);
                        x = xMin;
                    }

                    if (outCode == code0)
                    {
                        // Update the coordinates of the first endpoint
                        x0Clip = x;
                        y0Clip = y;
                        code0 = computeOutCode(x0Clip, y0Clip, xMin, yMin, xMax, yMax);
                    }
                    else
                    {
                        // Update the coordinates of the second endpoint
                        x1Clip = x;
                        y1Clip = y;
                        code1 = computeOutCode(x1Clip, y1Clip, xMin, yMin, xMax, yMax);
                    }
                }
            }

            if (accept)
            {
                // Update the coordinates in the vertices array
                vertices[15] = x0Clip;
                vertices[16] = y0Clip;
                vertices[18] = x1Clip;
                vertices[19] = y1Clip;
            }
        }

        // Helper function to compute the outcode for a point
        int computeOutCode(float x, float y, float xMin, float yMin, float xMax, float yMax)
        {
            const int INSIDE = 0; // Bit code for inside region
            const int LEFT = 1;   // Bit code for left region
            const int RIGHT = 2;  // Bit code for right region
            const int BOTTOM = 4; // Bit code for bottom region
            const int TOP = 8;    // Bit code for top region

            int code = INSIDE;

            if (x < xMin)
                code |= LEFT;
            else if (x > xMax)
                code |= RIGHT;

            if (y < yMin)
                code |= BOTTOM;
            else if (y > yMax)
                code |= TOP;

            return code;
        }

        protected override void OnLoad()
        {
            GL.ClearColor(new Color4(0.0f, 0.5f, 0.5f, 1.0f));

            float[] vertices = new float[]
            {
                //rectangle for the window/viewport
                150f, 350f, 1f,
                150f, 750f, 1f,
                1150f, 750f, 1f,
                1150f, 350f, 1f,
                150f, 350f, 1f,

                //for line
                100f, 600, 1f,
                850f, 850f, 1f,
            };


            clipLineCohenSutherland(vertices);

            this.vertexbufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexbufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            this.vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(this.vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexbufferObject);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
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

            //for the rectangle to represent viewport
            GL.Uniform4(uniformLocations, 0f, 0f, 0f, 1.0f);
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            GL.DrawArrays(PrimitiveType.Lines, 1, 2);
            GL.DrawArrays(PrimitiveType.Lines, 2, 2);
            GL.DrawArrays(PrimitiveType.Lines, 3, 2);

            GL.DrawArrays(PrimitiveType.Lines, 5, 2);

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