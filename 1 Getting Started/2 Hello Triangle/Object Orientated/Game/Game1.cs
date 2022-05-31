using System.Diagnostics;
using Object_Orientated.Library;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Object_Orientated.Game;
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
        
        vao1 = new VertexArray(triangleVertices,0);
        vao2 = new VertexArray(quadVertices,quadIndices,0);
        
        

        var vert = new Shader(ShaderLocation+"vertex.glsl",ShaderType.VertexShader);
        var frag = new Shader(ShaderLocation+"fragment.glsl",ShaderType.FragmentShader);
        var frag2 = new Shader(ShaderLocation+"fragment2.glsl",ShaderType.FragmentShader);
        
        shaderProgram = new ShaderProgram(new int[] { vert.ID,frag.ID });
        shaderProgram2 = new ShaderProgram(new int[] { vert.ID,frag2.ID });

    }

    protected override void KeyDown(KeyboardKeyEventArgs keyInfo)
    {
        if (keyInfo.Key == Keys.Escape) Window.Close();
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        // draw yellow triangle
        shaderProgram2.Use(); GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Fill);
        vao1.Use();
        GL.DrawArrays(PrimitiveType.Triangles,0,triangleVertices.Length);
        
        // draw orange wireframe rect
        shaderProgram.Use(); GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Line);
        vao2.Use();
        GL.DrawElements(PrimitiveType.Triangles,quadIndices.Length,DrawElementsType.UnsignedInt,0);
        
        // unbind
        GL.BindVertexArray(0);
        
        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        // TODO: make this only check for errors in debug mode and make error checks fancier
        
        vao1.Delete();
        vao2.Delete();

        shaderProgram.Delete();

        Debug.WriteLine("Successfully cleared memory");

    }
}
