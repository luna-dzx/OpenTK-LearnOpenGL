using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Library;

public class Shader
{
    public readonly int ID;

    /// <summary>
    /// Compile and store shader
    /// </summary>
    /// <param name="text">glsl code in raw text</param>
    /// <param name="type">what type of shader this program is</param>
    public Shader(string text,ShaderType type)
    {
        ID = GL.CreateShader(type);
        GL.ShaderSource(ID,text);
        GL.CompileShader(ID);
        
        string infoLog = GL.GetShaderInfoLog(ID);
        if (!string.IsNullOrEmpty(infoLog)) throw new Exception(infoLog);
    }

    /// <summary>
    /// Alternative to using constructor to load from a file instead of plaintext
    /// </summary>
    /// <param name="path">path to a file containing glsl code</param>
    /// <param name="type">what type of shader this program is</param>
    /// <returns>the shader which was created from loading this file</returns>
    public static Shader FromFile(string path, ShaderType type) => new Shader(File.ReadAllText(path), type);

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
    protected readonly int handle;
    private Dictionary<string, int> uniforms;
    private Dictionary<string, (FieldInfo ,int)> syncedUniforms;

    /// <summary>
    /// Most common shader combination - loads from glsl files
    /// </summary>
    /// <param name="vertexPath">path to the vertex shader file</param>
    /// <param name="fragmentPath">path to the fragment shader file</param>
    public ShaderProgram(string vertexPath, string fragmentPath) : this()
    {

        var vert = Shader.FromFile(vertexPath,ShaderType.VertexShader);
        var frag = Shader.FromFile(fragmentPath,ShaderType.FragmentShader);
        
        GL.AttachShader(handle,(int)vert);
        GL.AttachShader(handle,(int)frag);
        
        GL.LinkProgram(handle);
        
        string infoLog = GL.GetProgramInfoLog(handle);
        if (!string.IsNullOrEmpty(infoLog)) throw new Exception(infoLog);

        GL.DetachShader(handle,(int)vert);vert.Delete();
        GL.DetachShader(handle,(int)frag);frag.Delete();
    }

    /// <summary>
    /// Create a new shader program based on pre-existing shaders which have already been created
    /// </summary>
    /// <param name="shaderIDs">the OpenGL handles of the shaders</param>
    public ShaderProgram(int[] shaderIDs) : this()
    {
        LoadShaders(shaderIDs);
    }

    /// <summary>
    /// Load pre-existing shaders which have already been created
    /// </summary>
    /// <param name="shaderIDs">the OpenGL handles of the shaders</param>
    protected void LoadShaders(int[] shaderIDs)
    {
        // attach to program
        foreach (int id in shaderIDs) 
        { GL.AttachShader(handle,id); }
        
        GL.LinkProgram(handle);
        
        // delete from memory
        foreach (int id in shaderIDs)
        { GL.DetachShader(handle,id); GL.DeleteShader(id); }
    }

    /// <summary>
    /// Empty constructor for handling shaders differently in classes that inherit from this
    /// </summary>
    protected ShaderProgram()
    {
        handle = GL.CreateProgram();
        uniforms = new Dictionary<string, int>();
        syncedUniforms = new Dictionary<string, (FieldInfo, int)>();
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

    /// <summary>
    /// Get the shader program's handle
    /// </summary>
    /// <returns>the OpenGL shader program handle</returns>
    public int GetHandle() => handle;
    
    
    /// <summary>
    /// Override (int) cast to return the handle
    /// </summary>
    /// <param name="program">the shader program to cast</param>
    /// <returns>the OpenGL shader program handle</returns>
    public static explicit operator int(ShaderProgram program) => program.GetHandle();

    /// <summary>
    /// Retrieve the binding of a uniform variable in the shader program
    /// </summary>
    /// <param name="name">the uniform variable's name in glsl</param>
    /// <returns>uniform location of the variable</returns>
    /// <exception cref="Exception">OpenGL exception</exception>
    public int GetUniform(string name)
    {
        this.Use();
        if (!uniforms.ContainsKey(name))uniforms.Add(name,GL.GetUniformLocation(handle,name));
        ErrorCode error = GL.GetError();
        if (error != ErrorCode.NoError) throw new Exception(error.ToString());
        return uniforms[name];
    }


    #region Uniform Functions

    #region 1D uniform
    /// <summary>
    /// Set a uniform variable's value on the gpu
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">value</param>
    public void Uniform1(string name, double v0) => GL.Uniform1(GetUniform(name), v0);
    /// <summary>
    /// Set a uniform variable's value on the gpu
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">value</param>
    public void Uniform1(string name, float v0) => GL.Uniform1(GetUniform(name), v0);
    /// <summary>
    /// Set a uniform variable's value on the gpu
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">value</param>
    public void Uniform1(string name, int v0) => GL.Uniform1(GetUniform(name), v0);
    /// <summary>
    /// Set a uniform variable's value on the gpu
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">value</param>
    public void Uniform1(string name, uint v0) => GL.Uniform1(GetUniform(name), v0);
        
    #endregion
        
    #region 2D uniform
        
    /// <summary>
    /// Set a 2d uniform variable's value on the gpu (vec2)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="vector">vector value</param>
    public void Uniform2(string name, Vector2 vector) => GL.Uniform2(GetUniform(name), vector);
    /// <summary>
    /// Set a 2d uniform variable's value on the gpu (vec2)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    public void Uniform2(string name, double v0, double v1) => GL.Uniform2(GetUniform(name), v0,v1);
    /// <summary>
    /// Set a 2d uniform variable's value on the gpu (vec2)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    public void Uniform2(string name, float v0, float v1) => GL.Uniform2(GetUniform(name), v0,v1);
    /// <summary>
    /// Set a 2d uniform variable's value on the gpu (vec2)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    public void Uniform2(string name, int v0, int v1) => GL.Uniform2(GetUniform(name), v0,v1);
    /// <summary>
    /// Set a 2d uniform variable's value on the gpu (vec2)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    public void Uniform2(string name, uint v0, uint v1) => GL.Uniform2(GetUniform(name), v0,v1);
        
    #endregion
        
    #region 3D uniform
        
    /// <summary>
    /// Set a 3d uniform variable's value on the gpu (vec3)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="vector">vector value</param>
    public void Uniform3(string name, Vector3 vector) => GL.Uniform3(GetUniform(name), vector);
    /// <summary>
    /// Set a 3d uniform variable's value on the gpu (vec3)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    /// <param name="v2">z component value</param>
    public void Uniform3(string name, double v0, double v1, double v2) => GL.Uniform3(GetUniform(name), v0,v1,v2);
    /// <summary>
    /// Set a 3d uniform variable's value on the gpu (vec3)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    /// <param name="v2">z component value</param>
    public void Uniform3(string name, float v0, float v1, float v2) => GL.Uniform3(GetUniform(name), v0,v1,v2);
    /// <summary>
    /// Set a 3d uniform variable's value on the gpu (vec3)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    /// <param name="v2">z component value</param>
    public void Uniform3(string name, int v0, int v1, int v2) => GL.Uniform3(GetUniform(name), v0,v1,v2);
    /// <summary>
    /// Set a 3d uniform variable's value on the gpu (vec3)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    /// <param name="v2">z component value</param>
    public void Uniform3(string name, uint v0, uint v1, uint v2) => GL.Uniform3(GetUniform(name), v0,v1,v2);
        
    #endregion
        
    #region 4D uniform
        
    /// <summary>
    /// Set a 4d uniform variable's value on the gpu (vec4)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="vector">vector value</param>
    public void Uniform4(string name, Vector4 vector) => GL.Uniform4(GetUniform(name), vector);
    /// <summary>
    /// Set a 4d uniform variable's value on the gpu (vec4)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    /// <param name="v2">z component value</param>
    /// <param name="v3">w component value</param>
    public void Uniform4(string name, double v0, double v1, double v2, double v3) => GL.Uniform4(GetUniform(name), v0,v1,v2,v3);
    /// <summary>
    /// Set a 4d uniform variable's value on the gpu (vec4)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    /// <param name="v2">z component value</param>
    /// <param name="v3">w component value</param>
    public void Uniform4(string name, float v0, float v1, float v2, float v3) => GL.Uniform4(GetUniform(name), v0,v1,v2,v3);
    /// <summary>
    /// Set a 4d uniform variable's value on the gpu (vec4)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    /// <param name="v2">z component value</param>
    /// <param name="v3">w component value</param>
    public void Uniform4(string name, int v0, int v1, int v2, int v3) => GL.Uniform4(GetUniform(name), v0,v1,v2,v3);
    /// <summary>
    /// Set a 4d uniform variable's value on the gpu (vec4)
    /// </summary>
    /// <param name="name">the uniform variable's name</param>
    /// <param name="v0">x component value</param>
    /// <param name="v1">y component value</param>
    /// <param name="v2">z component value</param>
    /// <param name="v3">w component value</param>
    public void Uniform4(string name, uint v0, uint v1, uint v2, uint v3) => GL.Uniform4(GetUniform(name), v0,v1,v2,v3);
        
    #endregion
        
    #endregion
    
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
    

public class MultiShaderProgram : ShaderProgram
{

    private Dictionary<ShaderType, List<string>> sections;
    private List<int> shaders;

    /// <summary>
    /// Uses the engine's custom glsl syntax to format a multi-shader
    /// </summary>
    /// <param name="shaderText">the original shader's text</param>
    /// <param name="shaderType">the type of OpenGL shader to compile this as</param>
    /// <returns>the shader formatted to be compiled as glsl</returns>
    private string FormatShader(string shaderText, ShaderType shaderType)
    {
        string[] lines = shaderText.Split('\n');
        if (shaderType == ShaderType.FragmentShader)
        {
            lines[0] += "\nuniform int active" + shaderType + "Id;\nout vec4 lx_FragColour;\n";
        }

        string outputText = "";
        string currentText = "";

        sections[shaderType] = new List<string>();
        // first section contains no main functions, this makes the first section section 0
        int currentId = -1;

        foreach (string line in lines)
        {
            int firstCharIndex = 0;
            int lastCharIndex = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ') continue;
                firstCharIndex = i; break;
            }

            for (int i = line.Length-1; i > -1; i--)
            {
                if (line[i] == ' ') continue;
                lastCharIndex = i-1; break;
            }

            if (line[firstCharIndex] == '[' && line[lastCharIndex] == ']')
            {

                string test = String.ReplaceAll(currentText, "main", "lx_program" + currentId + "_main");
                outputText += test;

                string sectionName = line.Substring(firstCharIndex+1, lastCharIndex - firstCharIndex - 1);
                currentText = "";
                sections[shaderType].Add(sectionName);

                currentId++;
            }
            else
            {
                currentText += line+"\n";
            }
        }



        if (sections[shaderType].Count > 0)
        {
            outputText += String.ReplaceAll(currentText, "main", "lx_program" + currentId + "_main");

            outputText += "\nvoid main(){";

            for (int i = 0; i < sections[shaderType].Count; i++)
            {
                outputText += "if (active" + shaderType + "Id == "+i+") {lx_program" + i + "_main(); return;}";
            }
                
            outputText += "}";
        }
        else
        {
            outputText += currentText;
        }

        return outputText;
    }

    /// <summary>
    /// Create an empty multi-shader program for configuration outside the constructor
    /// </summary>
    public MultiShaderProgram() : base()
    {
        sections = new Dictionary<ShaderType, List<string>>();
        shaders = new List<int>();
    }

    /// <summary>
    /// Create a generic multi-shader program based on a vertex and fragment shader
    /// </summary>
    /// <param name="vertexPath">the path to a glsl vertex shader file</param>
    /// <param name="fragmentPath">the path to a glsl fragment shader file</param>
    public MultiShaderProgram(string vertexPath, string fragmentPath) : this()
    {
        LoadShader(vertexPath, ShaderType.VertexShader);
        LoadShader(fragmentPath, ShaderType.FragmentShader);
        Compile();
    }

    /// <summary>
    /// Load shader from file then format as a multi-shader and load to the shader program
    /// </summary>
    /// <param name="path">path to the glsl shader file</param>
    /// <param name="shaderType">the type of shader to use this as in the shader pipeline</param>
    /// <returns>current object for ease of use</returns>
    public MultiShaderProgram LoadShader(string path,ShaderType shaderType) => LoadShaderText(File.ReadAllText(path),shaderType);
        
    /// <summary>
    /// Format a shader (in plaintext) as a multi-shader and load to the shader program
    /// </summary>
    /// <param name="shaderText">plaintext glsl shader</param>
    /// <param name="shaderType">the type of shader to use this as in the shader pipeline</param>
    /// <returns>current object for ease of use</returns>
    public MultiShaderProgram LoadShaderText(string shaderText,ShaderType shaderType)
    {
        shaderText = FormatShader(shaderText,shaderType);
        Shader shader = new Shader(shaderText, shaderType);
        shaders.Add(shader.ID);
        return this;
    }
        
    /// <summary>
    /// Add an existing shader to the shader program
    /// </summary>
    /// <param name="shaderId">the OpenGL shader handle</param>
    /// <returns>current object for ease of use</returns>
    /// <remarks>does not get interpreted as a multi-shader</remarks>
    public MultiShaderProgram AddShader(int shaderId)
    {
        shaders.Add(shaderId);
        return this;
    }

    /// <summary>
    /// Compile any shaders which were loaded before this function call
    /// </summary>
    /// <returns>current object for ease of use</returns>
    public MultiShaderProgram Compile()
    {
        LoadShaders(shaders.ToArray());
        return this;
    }

    /// <summary>
    /// Set the active shader within a certain multi-shader (based on the shader type)
    /// </summary>
    /// <param name="shader">the type of shader to set</param>
    /// <param name="sectionName">the title of the section that contains the main function of the desired shader to switch to</param>
    /// <returns>current object for ease of use</returns>
    public MultiShaderProgram SetActive(ShaderType shader, string sectionName)
    {
        GL.Uniform1(
            GL.GetUniformLocation(handle,"active" + shader + "Id"),
            sections[shader].IndexOf(sectionName)
        );

        return this;
    }
        
        
        
}