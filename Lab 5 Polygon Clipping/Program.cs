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


        void clipPolygonSutherlandHodgman(float[] vertices)
        {
            // Extract the clipping window coordinates
            float xMin = vertices[0];
            float yMin = vertices[1];
            float xMax = vertices[6];
            float yMax = vertices[7];

            // Get the number of vertices in the polygon
            int polygonVertexCount = (vertices.Length - 10) / 3;

            // Initialize the variables to store the clipped polygon
            float[] clippedVertices = new float[vertices.Length];
            int clippedVertexCount = 0;

            // Perform the Sutherland-Hodgman polygon clipping algorithm
            float[] inputVertices = vertices;
            int inputVertexCount = polygonVertexCount;
            float[] outputVertices = clippedVertices;
            int outputVertexCount = 0;

            for (int edgeIndex = 0; edgeIndex < 4; edgeIndex++)
            {
                float edgeX0, edgeY0, edgeX1, edgeY1;

                // Define the current clipping edge
                switch (edgeIndex)
                {
                    case 0: // Left edge
                        edgeX0 = xMin;
                        edgeY0 = yMin;
                        edgeX1 = xMin;
                        edgeY1 = yMax;
                        break;
                    case 1: // Right edge
                        edgeX0 = xMax;
                        edgeY0 = yMin;
                        edgeX1 = xMax;
                        edgeY1 = yMax;
                        break;
                    case 2: // Bottom edge
                        edgeX0 = xMin;
                        edgeY0 = yMin;
                        edgeX1 = xMax;
                        edgeY1 = yMin;
                        break;
                    case 3: // Top edge
                        edgeX0 = xMin;
                        edgeY0 = yMax;
                        edgeX1 = xMax;
                        edgeY1 = yMax;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid edge index.");
                }

                outputVertexCount = 0;

                float edgeX0Prev = inputVertices[(inputVertexCount - 1) * 3];
                float edgeY0Prev = inputVertices[(inputVertexCount - 1) * 3 + 1];
                int edge0PrevInside = insideEdge(edgeX0Prev, edgeY0Prev, edgeX0, edgeY0, edgeX1, edgeY1);

                for (int i = 0; i < inputVertexCount; i++)
                {
                    float edgeX0Curr = inputVertices[i * 3];
                    float edgeY0Curr = inputVertices[i * 3 + 1];
                    int edge0CurrInside = insideEdge(edgeX0Curr, edgeY0Curr, edgeX0, edgeY0, edgeX1, edgeY1);

                    if (edge0CurrInside == 1 && edge0PrevInside == 0)
                    {
                        // Compute the intersection point
                        float intersectX, intersectY;
                        intersectEdge(edgeX0Prev, edgeY0Prev, edgeX0Curr, edgeY0Curr, edgeX0, edgeY0, edgeX1, edgeY1, out intersectX, out intersectY);

                        // Add the intersection point to the output vertices
                        outputVertices[outputVertexCount * 3] = intersectX;
                        outputVertices[outputVertexCount * 3 + 1] = intersectY;
                        outputVertices[outputVertexCount * 3 + 2] = 1f;
                        outputVertexCount++;
                    }
                    else if (edge0CurrInside == 0 && edge0PrevInside == 1)
                    {
                        // Compute the intersection point
                        float intersectX, intersectY;
                        intersectEdge(edgeX0Prev, edgeY0Prev, edgeX0Curr, edgeY0Curr, edgeX0, edgeY0, edgeX1, edgeY1, out intersectX, out intersectY);

                        // Add the intersection point and the current point to the output vertices
                        outputVertices[outputVertexCount * 3] = intersectX;
                        outputVertices[outputVertexCount * 3 + 1] = intersectY;
                        outputVertices[outputVertexCount * 3 + 2] = 1f;
                        outputVertexCount++;

                        outputVertices[outputVertexCount * 3] = edgeX0Curr;
                        outputVertices[outputVertexCount * 3 + 1] = edgeY0Curr;
                        outputVertices[outputVertexCount * 3 + 2] = 1f;
                        outputVertexCount++;
                    }
                    else if (edge0CurrInside == 1 && edge0PrevInside == 1)
                    {
                        // Add the current point to the output vertices
                        outputVertices[outputVertexCount * 3] = edgeX0Curr;
                        outputVertices[outputVertexCount * 3 + 1] = edgeY0Curr;
                        outputVertices[outputVertexCount * 3 + 2] = 1f;
                        outputVertexCount++;
                    }

                    edgeX0Prev = edgeX0Curr;
                    edgeY0Prev = edgeY0Curr;
                    edge0PrevInside = edge0CurrInside;
                }

                float[] temp = inputVertices;
                inputVertices = outputVertices;
                inputVertexCount = outputVertexCount;
                outputVertices = temp;
            }

            // Copy the clipped polygon back to the original vertices array
            Array.Copy(clippedVertices, 0, vertices, 10, outputVertexCount * 3);

            // Update the total number of vertices in the array
            int newVertexCount = outputVertexCount + 5;
            vertices[9] = newVertexCount;
        }

        // Helper function to determine if a point is inside, outside, or on an edge
        int insideEdge(float x, float y, float edgeX0, float edgeY0, float edgeX1, float edgeY1)
        {
            float edgeTest = (edgeX1 - edgeX0) * (y - edgeY0) - (x - edgeX0) * (edgeY1 - edgeY0);

            if (edgeTest > 0f)
                return 1; // Inside
            else if (edgeTest < 0f)
                return 0; // Outside
            else
                return -1; // On the edge
        }

        // Helper function to compute the intersection point of two lines
        void intersectEdge(float x0, float y0, float x1, float y1, float clipX0, float clipY0, float clipX1, float clipY1, out float intersectX, out float intersectY)
        {
            float edge0dx = x1 - x0;
            float edge0dy = y1 - y0;
            float edge1dx = clipX1 - clipX0;
            float edge1dy = clipY1 - clipY0;

            float edge0Edge1dy = edge0dx * edge1dy - edge0dy * edge1dx;

            float edge0dxClip = clipX0 - x0;
            float edge0dyClip = clipY0 - y0;

            float t = (edge0dxClip * edge1dy - edge0dyClip * edge1dx) / edge0Edge1dy;

            intersectX = x0 + t * edge0dx;
            intersectY = y0 + t * edge0dy;
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
                600f, 600f, 1f,
                850f, 850f, 1f,
                900f, 120f, 1f,
                600f, 600f, 1f,
            };

            //clipPolygonSutherlandHodgman(vertices);

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

            //for the polygon
            GL.DrawArrays(PrimitiveType.Lines, 5, 2);
            GL.DrawArrays(PrimitiveType.Lines, 6, 2);
            GL.DrawArrays(PrimitiveType.Lines, 7, 2);

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