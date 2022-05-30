using System.Diagnostics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace Hello_Triangle.Game;
public class Game1 : Library.Game
{
    private float[] triangleVertices;
    private float[] quadVertices;
    private int[] quadIndices;
    private int vbo;
    private int ebo;
    private int vao1;
    private int vao2;

    private const string VertexShaderSource = 
        "#version 330 core\n"+
        "layout (location = 0) in vec3 aPos;\n"+
        "void main()\n"+
        "{\n"+
        "   gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);\n"+
        "}\0";
    private const string FragmentShaderSource1 = 
        "#version 330 core\n"+
        "out vec4 FragColor;\n"+
        "void main()\n"+
        "{\n"+
        "   FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);\n"+
        "}\0";
    private const string FragmentShaderSource2 = 
        "#version 330 core\n"+
        "out vec4 FragColor;\n"+
        "void main()\n"+
        "{\n"+
        "   FragColor = vec4(1.0f, 1.0f, 0.0f, 1.0f);\n"+
        "}\0";
    
    

    private int vertexShader;
    private int fragmentShader;
    private int shaderProgram;
    private int shaderProgram2;
    
    protected override void Load()
    {
        GL.ClearColor(0.2f,0.3f,0.3f,1.0f);
        
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
        
        vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader,VertexShaderSource);
        GL.CompileShader(vertexShader);

        string errorInfo = GL.GetShaderInfoLog(vertexShader);
        if (errorInfo != string.Empty)
        {
            // TODO: fancy error stuff
            throw new ArgumentException(errorInfo);
        }


        fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader,FragmentShaderSource1);
        GL.CompileShader(fragmentShader);
        
        errorInfo = GL.GetShaderInfoLog(fragmentShader);
        if (errorInfo != string.Empty)
        {
            // TODO: fancy error stuff
            throw new ArgumentException(errorInfo);
        }


        shaderProgram = GL.CreateProgram();
        GL.AttachShader(shaderProgram,vertexShader);
        GL.AttachShader(shaderProgram,fragmentShader);
        
        
        GL.LinkProgram(shaderProgram);
        
        // delete the shader objects on the CPU side
        //GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
        

        fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader,FragmentShaderSource2);
        GL.CompileShader(fragmentShader);
        
        errorInfo = GL.GetShaderInfoLog(fragmentShader);
        if (errorInfo != string.Empty)
        {
            // TODO: fancy error stuff
            throw new ArgumentException(errorInfo);
        }


        shaderProgram2 = GL.CreateProgram();
        GL.AttachShader(shaderProgram2,vertexShader);
        GL.AttachShader(shaderProgram2,fragmentShader);
        
        
        GL.LinkProgram(shaderProgram2);
        
        // delete the shader objects on the CPU side
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
        
        

        errorInfo = GL.GetProgramInfoLog(shaderProgram);
        if (errorInfo != string.Empty)
        {
            // TODO: fancy error stuff
            throw new ArgumentException(errorInfo);
        }
        
        GL.UseProgram(shaderProgram);


        vao1 = GL.GenVertexArray();
        GL.BindVertexArray(vao1);
        
        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer,vbo);
        // copy vertex data to buffer memory
        GL.BufferData(BufferTarget.ArrayBuffer,triangleVertices.Length*sizeof(float),triangleVertices,BufferUsageHint.StaticDraw);


        GL.VertexAttribPointer(
            0, // shader layout location
            3, // size (num values)
            VertexAttribPointerType.Float, // variable type
            false, // normalize data (set to "length 1")
            3*sizeof(float), // space in bytes between each vertex attrib
            IntPtr.Zero // data offset
            );
        
        GL.EnableVertexAttribArray(0);


        
        vao2 = GL.GenVertexArray();
        GL.BindVertexArray(vao2);
        
        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer,vbo);
        GL.BufferData(BufferTarget.ArrayBuffer,quadVertices.Length*sizeof(float),quadVertices,BufferUsageHint.StaticDraw);
        
        ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer,ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer,quadIndices.Length*sizeof(int),quadIndices,BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(
            0, // shader layout location
            3, // size (num values)
            VertexAttribPointerType.Float, // variable type
            false, // normalize data (set to "length 1")
            3*sizeof(float), // space in bytes between each vertex attrib
            IntPtr.Zero // data offset
        );
        
        GL.EnableVertexAttribArray(0);

        
        

    }

    protected override void KeyDown(KeyboardKeyEventArgs keyInfo)
    {
        if (keyInfo.Key == Keys.Escape) Window.Close();
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        
        GL.UseProgram(shaderProgram2);
        
        GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Fill);
        GL.BindVertexArray(vao1);
        GL.DrawArrays(PrimitiveType.Triangles,0,triangleVertices.Length);
        
        GL.UseProgram(shaderProgram);
        
        GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Line);
        GL.BindVertexArray(vao2);
        GL.DrawElements(PrimitiveType.Triangles,quadIndices.Length,DrawElementsType.UnsignedInt,0);
        
        GL.BindVertexArray(0);
        
        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.DeleteBuffer(vbo);
        GL.DeleteBuffer(ebo);
        GL.DeleteVertexArray(vao1);
        GL.DeleteVertexArray(vao2);
        GL.DeleteProgram(shaderProgram);

        // TODO: make this only check for errors in debug mode and make error checks fancier
        #region Error Check
        var error = GL.GetError();
        if (error == ErrorCode.NoError) Debug.WriteLine("Successfully cleared memory");
        else throw new ArgumentException(error.ToString());
        #endregion
    }
}
