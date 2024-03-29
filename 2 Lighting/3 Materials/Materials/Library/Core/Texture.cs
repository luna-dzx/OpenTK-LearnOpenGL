﻿using System.IO;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Library;

/// <summary>
/// Class for handling OpenGL texture objects
/// </summary>
public class Texture
{
    private int handle;
    private int unit;
    private readonly TextureTarget target;
    
    /// <summary>
    /// Setup an OpenGL texture for manual texture handling
    /// </summary>
    /// <param name="textureUnit">the GPU texture unit to pass this texture to</param>
    /// <param name="textureTarget">the type of texture to store</param>
    public Texture(int textureUnit, TextureTarget textureTarget = TextureTarget.Texture2D)
    {
        handle = GL.GenTexture();
        unit = textureUnit;
        target = textureTarget;
    }

    /// <summary>
    /// Load a texture directly from a local file
    /// </summary>
    /// /// <param name="path">path to the image file</param>
    /// <param name="textureUnit">the GPU texture unit to pass this texture to</param>
    /// <param name="textureTarget">the type of texture to store</param>
    public Texture(string path, int textureUnit = 0, TextureTarget textureTarget = TextureTarget.Texture2D, bool flipOnLoad = true) : this(textureUnit,textureTarget)
    {
        LoadFile(path,flipOnLoad);
    }

    public Texture LoadFile(string path, bool flipOnLoad = true)
    {
        StbImage.stbi_set_flip_vertically_on_load((flipOnLoad)?1:0);
        this.Use();
        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream,ColorComponents.RedGreenBlueAlpha);
        GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,image.Width,image.Height,0,PixelFormat.Rgba,PixelType.UnsignedByte,image.Data);
        GL.GenerateMipmap((GenerateMipmapTarget)target);
        return this;
    }

    /// <summary>
    /// Load the texture to the GPU
    /// </summary>
    /// <param name="program">the shader program you are loading the texture to</param>
    /// <param name="name">the variable name of the sampler in glsl</param>
    public Texture Uniform(int program, string name)
    {
        GL.UseProgram(program);
        GL.Uniform1(GL.GetUniformLocation(program,name),unit);
        this.Use();
        return this;
    }

    public Texture Uniform(int program) => Uniform(program,"texture"+unit);
        
    public Texture Uniform(ShaderProgram program, string name) => Uniform((int)program,name);
    public Texture Uniform(ShaderProgram program) => Uniform((int)program,"texture"+unit);

    /// <summary>
    /// Activate the correct texture unit and set up the OpenGL texture for editing properties
    /// </summary>
    public Texture Use()
    {
        GL.ActiveTexture((TextureUnit) (unit + (int)TextureUnit.Texture0));
        GL.BindTexture(target,handle);
        return this;
    }

    /// <summary>
    /// Set parameters for how textures handle texture coordinates when they lie outside of the range of 0->1
    /// </summary>
    /// <param name="paramName">set wrapping for x,y,z components of the texture separately</param>
    /// <param name="wrapMode">how to wrap the texture when texture coordinates lie outside of the range of 0->1</param>
    public Texture Wrapping(TextureParameterName paramName, TextureWrapMode wrapMode) { this.Use(); GL.TexParameter(target, paramName, (int)wrapMode); return this; }

    /// <summary>
    /// Set parameters for how textures are displayed which take up less pixels on screen than the number of pixels in the texture image
    /// </summary>
    /// <param name="filter">the filter for how surrounding pixels should be sampled (if at all)</param>
    public Texture MinFilter(TextureMinFilter filter) { this.Use(); GL.TexParameter(target,TextureParameterName.TextureMinFilter,(int)filter); return this; }

    /// <summary>
    /// Set parameters for how textures are displayed which take up more pixels on screen than the number of pixels in the texture image
    /// </summary>
    /// <param name="filter">the filter for how surrounding pixels should be sampled (if at all)</param>
    public Texture MagFilter(TextureMagFilter filter) { this.Use(); GL.TexParameter(target,TextureParameterName.TextureMagFilter,(int)filter); return this; }
    
    /// <summary>
    /// Set parameters for how mipmapping should be handled (lower res textures further away)
    /// </summary>
    /// <param name="mipmapFilter">the texture filter for blending between mipmaps and the current texture's sample</param>
    public Texture Mipmapping(TextureMinFilter mipmapFilter) { this.Use(); GL.TexParameter(target,TextureParameterName.TextureMinFilter,(int)mipmapFilter); return this; }

    /// <summary>
    /// Delete the OpenGL texture object
    /// </summary>
    public void Delete() => GL.DeleteTexture(handle);

    /// <summary>
    /// Get the OpenGL handle of the texture for manipulation beyond this class
    /// </summary>
    /// <returns>OpenGL texture handle</returns>
    public int GetHandle() => handle;
    
    /// <summary>
    /// Override (int) cast to return texture handle
    /// </summary>
    /// <param name="texture">the texture who's handle you are getting from the cast</param>
    /// <returns>texture handle</returns>
    public static explicit operator int(Texture texture) => texture.GetHandle();

}