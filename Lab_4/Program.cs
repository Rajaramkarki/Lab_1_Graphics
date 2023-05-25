using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;


namespace Lab_4
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

        protected override void OnLoad()
        {
            GL.ClearColor(new Color4(1f, 1f, 1f, 1f));

            float[] vertices = new float[]
            {
                0.8f, 0.8f, 1f, // top
                0.2f, 0.8f, 1f, // bottom left
                0.8f, 0.2f, 1f, // bottom right
            };

            // 2D Translation
            float[][] translationMatrix = new float[][] 
            {   
                new float[] { 1.0f, 0.0f, -0.5f }, 
                new float[] { 0.0f, 1.0f, -0.5f }, 
                new float[] { 0.0f, 0.0f, 1.0f } 
            };
            float[] translatedVertices = transformVertices(vertices, translationMatrix);

            // 2D Rotation
            float[][] rotationMatrix = new float[][]
            {
                new float[] {(float)Math.Cos(-45.0f), (float) -Math.Sin(-45.0f), 0f },
                new float[] {(float) Math.Sin(-45.0f),(float) Math.Cos(-45.0f), 0f },
                new float[] { 0f, 0f, 1f },
            };
            float[] rotatedVertices = transformVertices(vertices, rotationMatrix);

            // 2D Scaling
            float[][] scalingMatrix = new float[][]
            {
                new float[] { 1.7f, 0f, 0f },
                new float[] { 0f, 1.7f, 0f },
                new float[] { 0f, 0f, 1f },
            };
            float[] scaledVertices = transformVertices(vertices, scalingMatrix);

            // 2D Reflection
            float[][] reflectionMatrix = new float[][]
            {
                new float[] { 1f, 0f, 0f },
                new float[] { 0f, -1f, 0f },
                new float[] { 0f, 0f, 1f },
            };
            float[] reflectedVertices = transformVertices(vertices, reflectionMatrix);

            // 2D Shearing
            float[][] shearingMatrix = new float[][]
            {
                new float[] { 1f, 0.5f, 0f },
                new float[] { 0f, 1f, 0f },
                new float[] { 0f, 0f, 1f },
            };
            float[] shearedVertices = transformVertices(vertices, shearingMatrix);

            // Helper function to transform vertices using a transformation matrix
             float[] transformVertices(float[] vertices, float[][] transformationMatrix)
            {
                float[] transformedVertices = new float[vertices.Length];
                for (int i = 0; i < vertices.Length; i += 3)
                {
                    float x = vertices[i];
                    float y = vertices[i + 1];
                    float w = vertices[i + 2];
                    float tx = transformationMatrix[0][0] * x + transformationMatrix[0][1] * y + transformationMatrix[0][2] * w;
                    float ty = transformationMatrix[1][0] * x + transformationMatrix[1][1] * y + transformationMatrix[1][2] * w;
                    float tw = transformationMatrix[2][0] * x + transformationMatrix[2][1] * y + transformationMatrix[2][2] * w;
                    transformedVertices[i] = tx;
                    transformedVertices[i + 1] = ty;
                    transformedVertices[i + 2] = tw;
                }
                return transformedVertices;
            }

            this.vertexbufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexbufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, translatedVertices.Length * sizeof(float), translatedVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            this.vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(this.vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexbufferObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);

            string vertexShaderCode =
                @"
                 #version 330 core

                 layout (location=0) in vec3 aPosition;
                 
                 void main(){
                 
                    gl_Position = vec4(aPosition, 1.0f);

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

            GL.Uniform4(uniformLocations, 0.078f, 0.114f, 0.420f, 1.0f);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3); 


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