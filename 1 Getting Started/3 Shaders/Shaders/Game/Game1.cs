using System.Diagnostics;
using Shaders.Library;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace Shaders.Game;
public class Game1 : Library.Game
{
    private float[] triangleVertices;
    private float[] quadVertices;
    private int[] quadIndices;

    private VertexArray vao1;
    private VertexArray vao2;

    private ShaderProgram shaderProgram;
    private ShaderProgram shaderProgram2;
    
    private const string ShaderLocation = "../../../Game/Shaders/";

    private int vertexColorLocation;
    private int vertexColorLocation2;
    
    protected override void Load()
    {
        GL.ClearColor(0.2f,0.3f,0.3f,1.0f);
        
        #region Shape Data
        
        triangleVertices = new float[]
        {
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f,  0.5f, 0.0f
        };
        
        quadVertices = new float[]
        {
            0.5f,  0.5f, 0.0f,  // top right
            0.5f, -0.5f, 0.0f,  // bottom right
            -0.5f, -0.5f, 0.0f,  // bottom left
            -0.5f,  0.5f, 0.0f   // top left 
        };

        quadIndices = new int[]
        {
            0, 1, 3,   // first triangle
            1, 2, 3    // second triangle
        };
        
        #endregion
        
        vao1 = new VertexArray(quadVertices,quadIndices,0);
        vao2 = new VertexArray(triangleVertices,0);

        shaderProgram = new ShaderProgram(ShaderLocation+"vertex.glsl",ShaderLocation+"fragment.glsl");
        shaderProgram2 = new ShaderProgram(ShaderLocation+"vertex2.glsl",ShaderLocation+"fragment2.glsl");
        
     
        vertexColorLocation = GL.GetUniformLocation((int)shaderProgram, "inputColour");
        ErrorCode error = GL.GetError();
        if (error != ErrorCode.NoError) throw new Exception(error.ToString());
        
        vertexColorLocation2 = GL.GetUniformLocation((int)shaderProgram2, "inputColour2");
        error = GL.GetError();
        if (error != ErrorCode.NoError) throw new Exception(error.ToString());   
        
    }

    protected override void KeyDown(KeyboardKeyEventArgs keyInfo)
    {
        if (keyInfo.Key == Keys.Escape) Window.Close();
    }

    private double totalTime = 0.0;
    private float greenValue;
    
    protected override void UpdateFrame(FrameEventArgs args)
    {
        totalTime += args.Time;
        greenValue = (float)((Math.Sin(totalTime) / 2.0) + 0.5);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        
        // draw orange wireframe rect
        shaderProgram.Use(); GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Line);
        GL.Uniform3(vertexColorLocation,1f,0.5f,0.2f);
        vao1.Use();
        GL.DrawElements(PrimitiveType.Triangles,quadIndices.Length,DrawElementsType.UnsignedInt,0);
        
        // draw green triangle
        shaderProgram2.Use(); GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Fill);
        GL.Uniform3(vertexColorLocation2,0f,greenValue,0f);
        vao2.Use();
        GL.DrawArrays(PrimitiveType.Triangles,0,triangleVertices.Length);
        
        
        // unbind
        GL.BindVertexArray(0);
        
        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        // TODO: make this only check for errors in debug mode and make error checks fancier
        
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        vao1.Delete();
        vao2.Delete();

        shaderProgram.Delete();

        Debug.WriteLine("Successfully cleared memory");

    }
}
