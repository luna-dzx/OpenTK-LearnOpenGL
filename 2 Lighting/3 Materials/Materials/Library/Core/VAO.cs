using System;
using OpenTK.Graphics.OpenGL4;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace Library;

/// <summary>
/// Simplifying VAOs and VBOs (for managing large amounts of data on the GPU)
/// </summary>
public class VertexArray
{
    private readonly int handle;
    private BufferUsageHint _bufferUsageHint;

    /// <summary>
    /// Self-handled VAO for multiple or dynamic VBOs
    /// </summary>
    /// <param name="usage">how frequently this data is to be used</param>
    public VertexArray(BufferUsageHint usage = BufferUsageHint.StaticDraw)
    {
        _bufferUsageHint = usage;
        handle = GL.GenVertexArray();
        this.Use();
    }

    /// <summary>
    /// Setup a VAO for static loading of standard vertices
    /// </summary>
    /// <param name="layoutLocation">shader layout location of vertex input</param>
    /// <param name="vertices">array of vertices to load</param>
    /// <param name="usage">how frequently this data is to be used</param>
    public VertexArray(int layoutLocation, float[] vertices, BufferUsageHint usage = BufferUsageHint.StaticDraw) : this(usage)
    {
        LoadData(layoutLocation, vertices);
    }

    /// <summary>
    /// Setup a VAO for loading standard elements (vertices + indices)
    /// </summary>
    /// <param name="layoutLocation">shader layout location of vertex input</param>
    /// <param name="vertices">array of vertices to load</param>
    /// <param name="indices">array of indices connecting the vertices as triangles</param>
    /// <param name="usage">how frequently this data is to be used</param>
    public VertexArray(int layoutLocation, float[] vertices, int[] indices, BufferUsageHint usage = BufferUsageHint.StaticDraw) : this(layoutLocation,vertices, usage)
    {
        StoreData(indices, BufferTarget.ElementArrayBuffer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data">the data to send to the GPU</param>
    /// <param name="target">the type of data we are sending</param>
    /// <param name="buffer">what VBO to load this data to (-1 means create new buffer)</param>
    /// <typeparam name="T">general type for loading up any variable type</typeparam>
    /// <returns>the VBO id that this data was loaded to</returns>
    public int StoreData<T>(T[] data,BufferTarget target, int buffer = -1) where T : struct
    {
        if (buffer == -1) { buffer = GL.GenBuffer(); }
        
        // bind buffer for storing data
        GL.BindBuffer(target,buffer);

        // copy vertex data to buffer memory
        GL.BufferData(target,data.Length*OpenGL.GetSizeInBytes(data[0]),data,_bufferUsageHint);

        return buffer;
    }

    /// <summary>
    /// Stores data in memory along with setting up memory for data reading/writing
    /// </summary>
    /// <param name="layoutLocation">shader layout location of data input</param>
    /// <param name="data">the data to send to the GPU</param>
    /// <param name="target">the type of data we are sending</param>
    /// <param name="buffer">what VBO to load this data to (-1 means create new buffer)</param>
    /// <param name="dataSize">number of variables per one group of data</param>
    /// <param name="stride">number of variable between each group of data</param>
    /// <param name="offset">number of variables to offset the start of the reading from</param>
    /// <param name="normalized">sets all data to length 1</param>
    /// <typeparam name="T">general type for loading up any variable type</typeparam>
    /// <returns>the VBO id that this data was loaded to</returns>
    /// <exception cref="Exception">length of data must be > 0</exception>
    public int LoadData<T>(int layoutLocation, T[] data, BufferTarget target = BufferTarget.ArrayBuffer, int buffer = -1, int dataSize=3, int stride = 3, int offset=0, bool normalized = false) where T : struct
    {
        if (data.Length < 1) throw new Exception("Invalid Input Data (data length must be > 0)");
        if (buffer == -1) { buffer = GL.GenBuffer(); }
        
        StoreData(data,target,buffer);

        GL.VertexAttribPointer(
            layoutLocation, // shader layout location
            dataSize, // size (num values)
            OpenGL.GetAttribPointerType(data[0]), // variable type
            normalized, // normalize data (set to "length 1")
            stride*OpenGL.GetSizeInBytes(data[0]), // space in bytes between each vertex attrib
            offset*OpenGL.GetSizeInBytes(data[0]) // data offset
        );

        GL.EnableVertexAttribArray(layoutLocation);

        return buffer;
    }

    /// <summary>
    /// Add new VBO from data
    /// </summary>
    /// <param name="data">the data to send to the GPU</param>
    /// <param name="target">the type of data we are sending</param>
    /// <param name="dataSize">number of variables per one group of data</param>
    /// <param name="stride">number of variable between each group of data</param>
    /// <param name="offset">number of variables to offset the start of the reading from</param>
    /// <param name="normalized">sets all data to length 1</param>
    /// <typeparam name="T">general type for loading up any variable type</typeparam>
    /// <returns>the new VBO id that the data was loaded to</returns>
    public int Add<T>(int layoutLocation, T[] data, BufferTarget target = BufferTarget.ArrayBuffer, int dataSize = 3, int stride = 3, int offset = 0, bool normalized = false) where T : struct
    {
        int buffer = LoadData(layoutLocation, data, target, -1, dataSize, stride, offset, normalized);
        return buffer;
    }
    /// <summary>
    /// Add new VBO from data
    /// </summary>
    /// <param name="data">the data to send to the GPU</param>
    /// <param name="target">the type of data we are sending</param>
    /// <typeparam name="T">general type for loading up any variable type</typeparam>
    /// <returns>the new VBO id that the data was loaded to</returns>
    public int Add<T>(T[] data,BufferTarget target) where T : struct
    {
        int buffer = StoreData(data, target);
        return buffer;
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

    /// <summary>
    /// Get the VAOs handle
    /// </summary>
    /// <returns>the OpenGL VAO handle</returns>
    public int GetHandle() => handle;
    

}