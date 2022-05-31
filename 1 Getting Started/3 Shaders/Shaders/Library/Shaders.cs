using OpenTK.Graphics.OpenGL4;

namespace Shaders.Library;

public class Shader
{
    public readonly int ID;

    /// <summary>
    /// Load shader from file and compile
    /// </summary>
    /// <param name="path">path to file</param>
    /// <param name="type">what type of shader this program is</param>
    public Shader(string path,ShaderType type)
    {
        ID = GL.CreateShader(type);
        GL.ShaderSource(ID,File.ReadAllText(path));
        GL.CompileShader(ID);
        
        string infoLog = GL.GetShaderInfoLog(ID);
        if (!string.IsNullOrEmpty(infoLog)) throw new Exception(infoLog);
    }

    /// <summary>
    /// Override (int) cast to return the ID
    /// </summary>
    /// <param name="shader">the shader to cast</param>
    /// <returns>the shader ID</returns>
    public static explicit operator int(Shader shader) => shader.ID;

    /// <summary>
    /// Delete this shader object
    /// </summary>
    public void Delete() => GL.DeleteShader(ID);
    

}

public class ShaderProgram
{
    private readonly int handle;

    /// <summary>
    /// Most common shader combination - loads from glsl files
    /// </summary>
    /// <param name="vertexPath">path to the vertex shader file</param>
    /// <param name="fragmentPath">path to the fragment shader file</param>
    public ShaderProgram(string vertexPath, string fragmentPath)
    {
        var vert = new Shader(vertexPath,ShaderType.VertexShader);
        var frag = new Shader(fragmentPath,ShaderType.FragmentShader);

        handle = GL.CreateProgram();
        GL.AttachShader(handle,(int)vert);
        GL.AttachShader(handle,(int)frag);
        
        GL.LinkProgram(handle);
        
        string infoLog = GL.GetProgramInfoLog(handle);
        if (!string.IsNullOrEmpty(infoLog)) throw new Exception(infoLog);

        vert.Delete();
        frag.Delete();
    }

    /// <summary>
    /// Load pre-existing shaders which have already been created
    /// </summary>
    /// <param name="shaderIDs">the OpenGL handles of the shaders</param>
    public ShaderProgram(int[] shaderIDs)
    {
        handle = GL.CreateProgram();
        
        // attach to program
        foreach (int id in shaderIDs) 
        { GL.AttachShader(handle,id); }
        
        GL.LinkProgram(handle);
        
        // delete from memory
        foreach (int id in shaderIDs) 
        { GL.DeleteShader(id); }
        
    }

    /// <summary>
    /// Assign this shader pipeline for rendering
    /// </summary>
    public void Use() => GL.UseProgram(handle);

    
    /// <summary>
    /// Remove shader program from video memory
    /// </summary>
    /// <exception cref="Exception">error code for if deleting fails</exception>
    public void Delete()
    {
        GL.DeleteProgram(handle);
        ErrorCode error = GL.GetError();
        if (error != ErrorCode.NoError) throw new Exception(error.ToString());
    }
    
    
}