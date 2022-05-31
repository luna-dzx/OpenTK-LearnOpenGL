using OpenTK.Graphics.OpenGL4;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace Object_Oriented.Library;

/// <summary>
/// Simplifying VAOs and VBOs (for managing large amounts of data on the GPU)
/// </summary>
public class VertexArray
{
    private readonly int handle;
    private int shaderLayoutLocation;

    private BufferUsageHint _bufferUsageHint;
    private int vertexBuffer;
    private int indexBuffer;

    /// <summary>
    /// General VAO with a shader binding location - for specific use cases
    /// that don't use these, just call the OpenTK functions directly
    /// </summary>
    /// <param name="layoutLocation"></param>
    public VertexArray(int layoutLocation, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
    {
        handle = GL.GenVertexArray();
        shaderLayoutLocation = layoutLocation;
        _bufferUsageHint = bufferUsage;
    }

    /// <summary>
    /// Setup a VAO for static loading of standard vertices
    /// </summary>
    /// <param name="vertices">array of vertices to load</param>
    /// <param name="layoutLocation">shader layout location of vertex input</param>
    public VertexArray(float[] vertices, int layoutLocation, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw) : this(layoutLocation,bufferUsage)
    {
        _bufferUsageHint = bufferUsage;
        this.Use();
        StoreVertices(vertices);
        SetupMemory(layoutLocation);
        GL.EnableVertexAttribArray(shaderLayoutLocation);
    }

    /// <summary>
    /// Setup a VAO for static loading of standard elements (vertices + indices)
    /// </summary>
    /// <param name="vertices">array of vertices to load</param>
    /// <param name="indices">array of indices connecting the vertices as triangles</param>
    /// <param name="layoutLocation">shader layout location of vertex input</param>
    /// <param name="bufferUsage">specifies how frequently data is written to</param>
    public VertexArray(float[] vertices, int[] indices, int layoutLocation, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw) : this(layoutLocation)
    {
        _bufferUsageHint = bufferUsage;
        this.Use();
        StoreVertices(vertices);
        StoreIndices(indices);
        SetupMemory(layoutLocation);
        GL.EnableVertexAttribArray(shaderLayoutLocation);
    }

    /// <summary>
    /// Store standard VBO for static writing
    /// </summary>
    /// <param name="vertices">polygon vertices</param>
    private void StoreVertices(float[] vertices)
    {
        // create and bind vertex buffer for storing data
        vertexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer,vertexBuffer);
        
        // copy vertex data to buffer memory
        GL.BufferData(BufferTarget.ArrayBuffer,vertices.Length*sizeof(float),vertices,_bufferUsageHint);
    }

    /// <summary>
    /// Store standard VBO for dynamic writing
    /// </summary>
    /// <param name="vertices">polygon vertices</param>
    /// <param name="buffer">the vertex buffer object handle</param>
    public void StoreVertices(float[] vertices, int buffer)
    {
        if (_bufferUsageHint != BufferUsageHint.DynamicDraw) throw new Exception("Incorrect VBO usage - VAO must be dynamic");
        
        // ind vertex buffer for storing data
        GL.BindBuffer(BufferTarget.ArrayBuffer,buffer);
        
        // copy vertex data to buffer memory
        GL.BufferData(BufferTarget.ArrayBuffer,vertices.Length*sizeof(float),vertices,_bufferUsageHint);
    }
    
    /// <summary>
    /// Store standard EBO for static writing
    /// </summary>
    /// <param name="indices">polygon connection indices</param>
    private void StoreIndices(int[] indices)
    {
        // create and bind element buffer for storing index data
        indexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer,indexBuffer);
        
        // copy index data to buffer memory
        GL.BufferData(BufferTarget.ElementArrayBuffer,indices.Length*sizeof(int),indices,_bufferUsageHint);
    }
    
    /// <summary>
    /// Store standard EBO for dynamic writing
    /// </summary>
    /// <param name="indices">polygon connection indices</param>
    /// <param name="buffer">the element buffer object handle</param>
    public void StoreIndices(int[] indices, int buffer)
    {
        if (_bufferUsageHint != BufferUsageHint.DynamicDraw) throw new Exception("Incorrect VBO usage - VAO must be dynamic");
        
        // bind element buffer for storing index data
        GL.BindBuffer(BufferTarget.ElementArrayBuffer,buffer);
        
        // copy index data to buffer memory
        GL.BufferData(BufferTarget.ElementArrayBuffer,indices.Length*sizeof(int),indices,_bufferUsageHint);
    }

    /// <summary>
    /// Tell OpenGL how to interpret the data in memory (for standard vertex passing)
    /// </summary>
    /// <param name="location">shader layout location of vertex input</param>
    private void SetupMemory(int location)
    {
        GL.VertexAttribPointer(
            location, // shader layout location
            3, // size (num values)
            VertexAttribPointerType.Float, // variable type
            false, // normalize data (set to "length 1")
            3*sizeof(float), // space in bytes between each vertex attrib
            IntPtr.Zero // data offset
        );
    }
    
    /// <summary>
    /// Activate this VAO for reading/writing
    /// </summary>
    public void Use()
    {
        GL.BindVertexArray(handle);
    }
    

    /// <summary>
    /// Remove VAO from video memory
    /// </summary>
    public void Delete()
    {
        GL.DeleteVertexArray(handle);
        ErrorCode error = GL.GetError();
        if (error != ErrorCode.NoError) throw new Exception(error.ToString());
    }

    public int GetHandle() => handle;
    

}