﻿using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Object_Oriented.Library;

public class Utils
{
    /// <summary>
    /// Finds the equivalent OpenGL type of an object
    /// </summary>
    /// <param name="obj">the variable to check</param>
    /// <returns>OpenGL vertex attrib pointer type for loading</returns>
    /// <exception cref="Exception">entered type not accounted for</exception>
    public static VertexAttribPointerType GetAttribPointerType(object obj)
    {
        Type t = obj.GetType();
        if (t == typeof(byte)) return VertexAttribPointerType.Byte;
        if (t == typeof(double)) return VertexAttribPointerType.Double;
        if (t == typeof(float)) return VertexAttribPointerType.Float;
        if (t == typeof(int)) return VertexAttribPointerType.Int;
        if (t == typeof(short)) return VertexAttribPointerType.Short;
        if (t == typeof(Half)) return VertexAttribPointerType.HalfFloat;
        if (t == typeof(uint)) return VertexAttribPointerType.UnsignedInt;
        if (t == typeof(ushort)) return VertexAttribPointerType.UnsignedShort;

        throw new Exception("Invalid Type");
    }

    /// <summary>
    /// Calculate the size of a variable
    /// </summary>
    /// <param name="obj">the variable to check</param>
    /// <returns>the number of bytes this object takes up as an integer</returns>
    public static int GetSizeInBytes(object obj)
    {
        Type t = obj.GetType();
        if (t == typeof(byte)) return sizeof(byte);
        if (t == typeof(double)) return sizeof(double);
        if (t == typeof(float)) return sizeof(float);
        if (t == typeof(int)) return sizeof(int);
        if (t == typeof(short)) return sizeof(short);
        if (t == typeof(Half)) return sizeof(float)/2;
        if (t == typeof(uint)) return sizeof(uint);
        if (t == typeof(ushort)) return sizeof(ushort);

        return 4;
    }
    
}