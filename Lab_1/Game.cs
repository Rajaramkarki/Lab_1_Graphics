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

namespace Lab_1
{


    public class Game : GameWindow
    {
        int circleSides = 250; // number of sides of the circle polygon


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

        void DrawCircle(float centerX, float centerY,float radius, int circleSides, ref float[] vertices)
        {
            float angleIncrement = (float)(2.0f * Math.PI / circleSides);
            float currentAngle = 0.0f;

            int originalLength = vertices.Length;
            Array.Resize(ref vertices, originalLength + circleSides * 3);

            for (int i = 0; i < circleSides; i++)
            {
                float x = centerX + (float)(radius * Math.Cos(currentAngle));
                float y = centerY + (float)(radius * Math.Sin(currentAngle));
                vertices[originalLength + i * 3] = x;
                vertices[originalLength + i * 3 + 1] = y;
                vertices[originalLength + i * 3 + 2] = 0.0f;
                currentAngle += angleIncrement;
            }
        }

        void DrawCrescent(float centerX, float centerY, float radius, int circleSides, ref float[] vertices)
        {
            float angleIncrement = (float)(Math.PI / circleSides);
            float currentAngle = 0.0f;

            int originalLength = vertices.Length;
            Array.Resize(ref vertices, originalLength + circleSides * 2*3);

            for (int i = 0; i < circleSides; i++)
            {
                float x = centerX + (float)(radius * Math.Cos(currentAngle));
                float y = centerY + (float)(radius * Math.Sin(currentAngle));
                vertices[originalLength + i * 3] = x;
                vertices[originalLength + i * 3 + 1] = y;
                vertices[originalLength + i * 3 + 2] = 0.0f;
                currentAngle += angleIncrement;
            }

            // Mirror the circle horizontally to create the crescent shape
            for (int i = 0; i < circleSides - 1; i++)
            {
                float x = centerX - (float)(radius * Math.Cos(currentAngle));
                float y = centerY + (float)(radius * Math.Sin(currentAngle));
                vertices[originalLength + circleSides * 3 + (circleSides - i - 1) * 3] = x;
                vertices[originalLength + circleSides * 3 + (circleSides - i - 1) * 3 + 1] = y;
                vertices[originalLength + circleSides * 3 + (circleSides - i - 1) * 3 + 2] = 0.0f;
                currentAngle -= angleIncrement;
            }
        }



        protected override void OnLoad()
        {
            GL.ClearColor(new Color4(1f, 1f, 1f, 1f));

            float[] vertices = new float[]
            {
                // Triangle 1 - bottom
                -0.5f, 0.35f, 0.0f, // top
                -0.5f, -0.5f, 0.0f, // bottom left
                0.5f, -0.5f, 0.0f, // bottom right

                // Triangle 2 - top
                -0.5f, 0.6f, 0.0f, // top
                -0.5f, 0.0f, 0.0f, // bottom right
                0.5f, 0.0f, 0.0f, // bottom left

                //Inner Triangle Bot
                -0.47f, -0.47f, 0.0f,   //bottom left
                0.42f, -0.47f, 0.0f,    //bottom right
                -0.47f, 0.28f, 0.0f,       //top

                //Inner triangle top
                -0.47f, 0.03f, 0.0f,   //bottom left
                -0.47f, 0.54f, 0.0f,     //top
                0.38f, 0.03f, 0.0f       //bottom right

            };

            DrawCircle(-0.2f, -0.25f, 0.07f, 250, ref vertices);
            DrawCrescent(-0.22f, 0.23f, 0.07f, 250, ref vertices);

            this.vertexbufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexbufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
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
            if(vertexShaderInfo != String.Empty)
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

        //public float[] TransformVertex(float[] arr, float x, float y)
        //{
        //    float[] final_vertices = new float[arr.Length];

        //    for (int i =0; i<arr.Length; i= i+3)
        //    {
        //        final_vertices[i] = arr[i] + x;
        //        final_vertices[i + 1] = arr[i + 1] + y;
        //        final_vertices[i + 2] = 0;
        //    }

        //    return final_vertices;
        //}

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(this.shaderProgramObject);
            int uniformLocations = GL.GetUniformLocation(this.shaderProgramObject, "vColor");
            
            GL.BindVertexArray(this.vertexArrayObject);

            GL.Uniform4(uniformLocations, 0.078f, 0.114f, 0.420f, 1.0f);
            ///for outer two triangles that are blue
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3); // left triangle
            GL.DrawArrays(PrimitiveType.Triangles, 3, 3); // right triangle

            ///for inner two triangles that are crimson
            GL.Uniform4(uniformLocations, 0.8627f, 0.0784f, 0.2353f, 1.0f);     //crimson color
            GL.DrawArrays(PrimitiveType.Triangles, 6, 3);
            GL.DrawArrays(PrimitiveType.Triangles, 9, 3);

            ///for the sun and moon
            GL.Uniform4(uniformLocations, 1.0f, 1.0f, 1.0f, 1.0f); // white
            GL.DrawArrays(PrimitiveType.TriangleFan, 12, this.circleSides);

            
            GL.Uniform4(uniformLocations, 1.0f, 1.0f, 1.0f, 1.0f); // white
            GL.DrawArrays(PrimitiveType.TriangleFan, 387, circleSides );


            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

    }
}