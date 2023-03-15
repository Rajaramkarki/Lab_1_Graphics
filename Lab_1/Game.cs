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

namespace Lab_1
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
                // Triangle 1 - top
                -1.0f, 0.4f, 0.0f, // top
                -1.0f, -0.5f, 0.0f, // bottom left
                0.0f, -0.5f, 0.0f, // bottom right

                // Triangle 2 - bottom
                -1.0f, 0.5f, 0.0f, // bottom
                -1.0f, 0.0f, 0.0f, // top right
                0.0f, 0.0f, 0.0f, // top left

                //vertices for 1st border
                -1.0f, 0.5f, 0.0f,

            };

            int[] indices = new int[]
            {
                0, 1, 2,
                3, 4, 5,

            };

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
                 vec3 translatedPosition = vec3(aPosition.x + 0.5f, aPosition.y, aPosition.z);
                 gl_Position = vec4(translatedPosition, 1.0f);

                 }";

            string pixelShaderCode =
                @"
                 #version 330 core

                 out vec4 pixelColor;

                 void main(){

                    pixelColor = vec4(0.8627f, 0.0784f, 0.2353f, 1.0f);
                 }
                 ";



    //        string vertexShaderCode =
    //@"
    //             #version 330 core

    //             layout (location=0) in vec3 aPosition;
    //             out int vertexIndex;
     
    //             void main(){
    //                 vertexIndex = gl_VertexID;
    //                 vec3 translatedPosition = vec3(aPosition.x + 0.5f, aPosition.y, aPosition.z);
    //                 gl_Position = vec4(translatedPosition, 1.0f);
    //             }";

    //        string pixelShaderCode =
    //            @"
    //             #version 330 core
     
    //             in int vertexIndex;
    //             out vec4 pixelColor;

    //             void main(){
    //                if(vertexIndex >= 6)
    //                {
    //                    pixelColor = vec4(0.0f, 0.0f, 1.0f, 1.0f); // Blue color for line
    //                }
    //                else
    //                {
    //                    pixelColor = vec4(0.8627f, 0.0784f, 0.2353f, 1.0f); // Red color for triangles
    //                }
    //             }";

            int vertexShaderObject = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexbufferObject, vertexShaderCode);
            GL.CompileShader(vertexShaderObject);


            int pixelShaderObject = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(pixelShaderObject, pixelShaderCode);
            GL.CompileShader(pixelShaderObject);

            this.shaderProgramObject = GL.CreateProgram();

            GL.AttachShader(this.shaderProgramObject, vertexShaderObject);
            GL.AttachShader(this.shaderProgramObject, pixelShaderObject);

            GL.LinkProgram(this.shaderProgramObject);

            GL.DetachShader(this.shaderProgramObject, vertexShaderObject);
            GL.DetachShader(this.shaderProgramObject, pixelShaderObject);

            GL.DeleteShader(vertexShaderObject);//Check this place
            GL.DeleteShader(pixelShaderObject);//relook

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(this.vertexbufferObject);//check this in case

            GL.UseProgram(0);//check this too
            GL.DeleteProgram(this.shaderProgramObject);//Problem identified


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

            GL.BindVertexArray(this.vertexArrayObject);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3); // left triangle
            GL.DrawArrays(PrimitiveType.Triangles, 3, 3); // right triangle
            //GL.DrawArrays(PrimitiveType.LineLoop, 2, 6);

            this.Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

    }
}