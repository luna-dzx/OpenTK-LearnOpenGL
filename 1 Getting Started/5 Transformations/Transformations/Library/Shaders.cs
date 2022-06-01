using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Transformations.Library;

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
    private Dictionary<string, int> uniforms;
    private Dictionary<string, (FieldInfo ,int)> syncedUniforms;

    /// <summary>
    /// Most common shader combination - loads from glsl files
    /// </summary>
    /// <param name="vertexPath">path to the vertex shader file</param>
    /// <param name="fragmentPath">path to the fragment shader file</param>
    public ShaderProgram(string vertexPath, string fragmentPath)
    {
        handle = GL.CreateProgram();
        uniforms = new Dictionary<string, int>();
        syncedUniforms = new Dictionary<string, (FieldInfo, int)>();
        
        var vert = new Shader(vertexPath,ShaderType.VertexShader);
        var frag = new Shader(fragmentPath,ShaderType.FragmentShader);
        
        GL.AttachShader(handle,(int)vert);
        GL.AttachShader(handle,(int)frag);
        
        GL.LinkProgram(handle);
        
        string infoLog = GL.GetProgramInfoLog(handle);
        if (!string.IsNullOrEmpty(infoLog)) throw new Exception(infoLog);

        GL.DetachShader(handle,(int)vert);vert.Delete();
        GL.DetachShader(handle,(int)frag);frag.Delete();
    }

    /// <summary>
    /// Load pre-existing shaders which have already been created
    /// </summary>
    /// <param name="shaderIDs">the OpenGL handles of the shaders</param>
    public ShaderProgram(int[] shaderIDs)
    {
        
        handle = GL.CreateProgram();
        uniforms = new Dictionary<string, int>();
        syncedUniforms = new Dictionary<string, (FieldInfo, int)>();

        // attach to program
        foreach (int id in shaderIDs) 
        { GL.AttachShader(handle,id); }
        
        GL.LinkProgram(handle);
        
        // delete from memory
        foreach (int id in shaderIDs)
        { GL.DetachShader(handle,id); GL.DeleteShader(id); }
        
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

    public int GetHandle() => handle;
    
    
    /// <summary>
    /// Override (int) cast to return the handle
    /// </summary>
    /// <param name="program">the shader program to cast</param>
    /// <returns>the OpenGL shader program handle</returns>
    public static explicit operator int(ShaderProgram program) => program.GetHandle();

    public void Uniform(string name)
    {
        uniforms.Add(name,GL.GetUniformLocation(handle,name));
        ErrorCode error = GL.GetError();
        if (error != ErrorCode.NoError) throw new Exception(error.ToString());
    }

    public int GetUniform(string name)
    {
        this.Use();
        return uniforms[name];
    }
    
    
    #region Synced Uniforms
    
    // NOTE: using these isn't very good practice and isn't very efficient, however for small projects
    // where maximum efficiency isn't necessary they can sometimes slightly reduce the programming workload

    
    /// <summary>
    /// Use reflections to sync variables between C# and glsl based on their names - only supports simple vectors and scalars
    /// </summary>
    /// <param name="name">variable name</param>
    /// <param name="game">game class containing the variable</param>
    public void SyncUniform(string name, Game game)
    {
        syncedUniforms[name] = (
            game.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance),
            GL.GetUniformLocation(handle,name)
        )!;
    }

    /// <summary>
    /// Loads all synced uniforms to the GPU
    /// </summary>
    /// <param name="game">game class containing the synced uniforms</param>
    public void UpdateSyncedUniforms(Game game)
    {
        foreach (string name in syncedUniforms.Keys)
        {
            UpdateSyncedUniform(game, name);
        }
    }

    /// <summary>
    /// Loads specific synced uniform to the GPU
    /// </summary>
    /// <param name="game">game class containing the synced uniform</param>
    /// <param name="name">the name of the specific synced uniform</param>
    /// <exception cref="Exception">unsupported synced uniform type</exception>
    public void UpdateSyncedUniform(Game game, string name)
    {
        this.Use();
        var (variable, uniformId) = syncedUniforms[name];
        object value = variable.GetValue(game)!;
        Type t = value.GetType();
        
        #region scalars
        // Uniform 1
        if (t == typeof(float)) { GL.Uniform1(uniformId,(float)value); return;}
        if (t == typeof(int)) { GL.Uniform1(uniformId,(int)value); return;}
        if (t == typeof(uint)) { GL.Uniform1(uniformId,(uint)value); return;}
        if (t == typeof(double)) { GL.Uniform1(uniformId,(double)value); return;}
        #endregion
        
        #region vectors
        // Uniform 2
        if (t == typeof(Vector2)) { GL.Uniform2(uniformId,(Vector2)value); return;}
        // Uniform 3
        if (t == typeof(Vector3)) { GL.Uniform3(uniformId,(Vector3)value); return;}
        // Uniform 4
        if (t == typeof(Vector4)) { GL.Uniform4(uniformId,(Vector4)value); return;}
        #endregion

        throw new Exception("Invalid synced uniform type");
    }
    
    #endregion
    
}