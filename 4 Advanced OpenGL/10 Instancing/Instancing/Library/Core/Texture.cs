﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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
    public Texture(string path, int textureUnit, TextureTarget textureTarget = TextureTarget.Texture2D, bool flipOnLoad = true) : this(textureUnit,textureTarget)
    {
        LoadFile(path,flipOnLoad);
    }


    public static Texture LoadCubeMap(string filePath, string fileExtension, int textureUnit, bool flipOnLoad = false)
    {
        return new Texture(textureUnit,TextureTarget.TextureCubeMap)
                .LoadFile(filePath+"right"+fileExtension,TextureTarget.TextureCubeMapPositiveX,flipOnLoad)
                .LoadFile(filePath+"left"+fileExtension,TextureTarget.TextureCubeMapNegativeX,flipOnLoad)
                .LoadFile(filePath+"top"+fileExtension,TextureTarget.TextureCubeMapPositiveY,flipOnLoad)
                .LoadFile(filePath+"bottom"+fileExtension,TextureTarget.TextureCubeMapNegativeY,flipOnLoad)
                .LoadFile(filePath+"back"+fileExtension,TextureTarget.TextureCubeMapNegativeZ,flipOnLoad)
                .LoadFile(filePath+"front"+fileExtension,TextureTarget.TextureCubeMapPositiveZ,flipOnLoad)
                
                .MagFilter(TextureMagFilter.Linear)
                .MinFilter(TextureMinFilter.Linear)
                .Wrapping(TextureParameterName.TextureWrapS,TextureWrapMode.ClampToEdge)
                .Wrapping(TextureParameterName.TextureWrapT,TextureWrapMode.ClampToEdge)
                .Wrapping(TextureParameterName.TextureWrapR,TextureWrapMode.ClampToEdge)
            ;
    }
    

    public Texture LoadFile(string path, bool flipOnLoad = true)
    {
        StbImage.stbi_set_flip_vertically_on_load((flipOnLoad)?1:0);
        this.Use();
        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream,ColorComponents.RedGreenBlueAlpha);
        GL.TexImage2D(target,0,PixelInternalFormat.Rgba,image.Width,image.Height,0,PixelFormat.Rgba,PixelType.UnsignedByte,image.Data);
        GL.GenerateMipmap((GenerateMipmapTarget)target);
        return this;
    }
    
    public Texture LoadFile(string path, TextureTarget textureTarget, bool flipOnLoad = true)
    {
        StbImage.stbi_set_flip_vertically_on_load((flipOnLoad)?1:0);
        this.Use();
        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream,ColorComponents.RedGreenBlueAlpha);
        GL.TexImage2D(textureTarget,0,PixelInternalFormat.Rgba,image.Width,image.Height,0,PixelFormat.Rgba,PixelType.UnsignedByte,image.Data);
        return this;
    }

    /// <summary>
    /// Load the texture to the GPU
    /// </summary>
    /// <param name="program">the shader program you are loading the texture to</param>
    /// <param name="name">the variable name of the sampler in glsl</param>
    public Texture Uniform(int program, string name)
    {
        this.Use();
        GL.UseProgram(program);
        GL.Uniform1(GL.GetUniformLocation(program,name),unit);
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
    /// Set parameters for how textures handle texture coordinates when they lie outside of the range of 0->1 (for specifically x and y wrapping)
    /// </summary>
    /// <param name="wrapMode">how to wrap the texture when texture coordinates lie outside of the range of 0->1</param>
    public Texture Wrapping(TextureWrapMode wrapMode)
    {
        Wrapping(TextureParameterName.TextureWrapS,wrapMode);
        Wrapping(TextureParameterName.TextureWrapT,wrapMode);
        return this;
    }
    
    
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
    /// Get the OpenGL texture unit of the texture
    /// </summary>
    /// <returns>OpenGL texture unit</returns>
    public int GetUnit() => unit;
    
    /// <summary>
    /// Override (int) cast to return texture handle
    /// </summary>
    /// <param name="texture">the texture who's handle you are getting from the cast</param>
    /// <returns>texture handle</returns>
    public static explicit operator int(Texture texture) => texture.GetHandle();

}

/// <summary>
/// Textures which the CPU can read from, but doesn't write to
/// </summary>
public class TextureBuffer
{
    public readonly int Handle;
    public readonly PixelFormat Format;
    public readonly PixelInternalFormat InternalFormat;
    public readonly TextureTarget Target;
    public readonly PixelType PixelType;
    public readonly Vector2i Size;

    public TextureBuffer(PixelInternalFormat internalFormat, PixelFormat format, (int,int) size,
        TextureTarget target = TextureTarget.Texture2D, PixelType pixelType = PixelType.UnsignedByte, int mipmap = 0, int border = 0)
    {
        (Size.X, Size.Y) = size;
        
        Handle = GL.GenTexture();
        Format = format;
        Target = target;
        PixelType = pixelType;
        InternalFormat = internalFormat;
        Format = format;

        this.Use();
        
        GL.TexImage2D(Target,mipmap,InternalFormat,Size.X,Size.Y,border,Format,PixelType,IntPtr.Zero);
        MinFilter(TextureMinFilter.Linear);
        
        GL.BindTexture(Target,0);
        
    }

    public TextureBuffer(PixelInternalFormat internalFormat, PixelFormat format, Vector2i size,
        TextureTarget target = TextureTarget.Texture2D, PixelType pixelType = PixelType.UnsignedByte, int mipmap = 0,
        int border = 0) :
        this(internalFormat, format, (size.X,size.Y),target,pixelType,mipmap,border) { }
    public TextureBuffer(PixelFormat format, Vector2i size,
        TextureTarget target = TextureTarget.Texture2D, PixelType pixelType = PixelType.UnsignedByte, int mipmap = 0,
        int border = 0) :
        this((PixelInternalFormat)format, format, (size.X,size.Y),target,pixelType,mipmap,border) { }

    public static explicit operator int(TextureBuffer textureBuffer) => textureBuffer.Handle;

    public TextureBuffer Use() { GL.BindTexture(Target,Handle); return this; }
    
    
    
    // have to repeat these instead of using inheritance to allow for interfacing
    
    /// <summary>
    /// Set parameters for how textures handle texture coordinates when they lie outside of the range of 0->1
    /// </summary>
    /// <param name="paramName">set wrapping for x,y,z components of the texture separately</param>
    /// <param name="wrapMode">how to wrap the texture when texture coordinates lie outside of the range of 0->1</param>
    public TextureBuffer Wrapping(TextureParameterName paramName, TextureWrapMode wrapMode) { this.Use(); GL.TexParameter(Target, paramName, (int)wrapMode); return this; }

    /// <summary>
    /// Set parameters for how textures handle texture coordinates when they lie outside of the range of 0->1 (for specifically x and y wrapping)
    /// </summary>
    /// <param name="wrapMode">how to wrap the texture when texture coordinates lie outside of the range of 0->1</param>
    public TextureBuffer Wrapping(TextureWrapMode wrapMode)
    {
        Wrapping(TextureParameterName.TextureWrapS,wrapMode);
        Wrapping(TextureParameterName.TextureWrapT,wrapMode);
        return this;
    }

    /// <summary>
    /// Set parameters for how textures are displayed which take up less pixels on screen than the number of pixels in the texture image
    /// </summary>
    /// <param name="filter">the filter for how surrounding pixels should be sampled (if at all)</param>
    public TextureBuffer MinFilter(TextureMinFilter filter) { this.Use(); GL.TexParameter(Target,TextureParameterName.TextureMinFilter,(int)filter); return this; }

    /// <summary>
    /// Set parameters for how textures are displayed which take up more pixels on screen than the number of pixels in the texture image
    /// </summary>
    /// <param name="filter">the filter for how surrounding pixels should be sampled (if at all)</param>
    public TextureBuffer MagFilter(TextureMagFilter filter) { this.Use(); GL.TexParameter(Target,TextureParameterName.TextureMagFilter,(int)filter); return this; }
    

}


/// <summary>
/// Textures which the CPU cannot read from
/// </summary>
public class RenderBuffer
{
    public readonly int Handle;
    public readonly RenderbufferStorage Format;
    public readonly RenderbufferTarget Target;
    public readonly Vector2i Size;

    public RenderBuffer(RenderbufferStorage format, (int,int) size, RenderbufferTarget target = RenderbufferTarget.Renderbuffer)
    {
        (Size.X, Size.Y) = size;
        
        Handle = GL.GenRenderbuffer();
        Format = format;
        Target = target;

        GL.BindRenderbuffer(Target,Handle);
        GL.RenderbufferStorage(Target,Format,Size.X,Size.Y);
        GL.BindRenderbuffer(Target,0);
    }

    public RenderBuffer(RenderbufferStorage internalformat, Vector2i resolution, RenderbufferTarget bufferTarget = RenderbufferTarget.Renderbuffer) : 
        this(internalformat, (resolution.X, resolution.Y), bufferTarget) { }

    public static explicit operator int(RenderBuffer renderBuffer) => renderBuffer.Handle;

    public RenderBuffer Use()
    {
        GL.BindRenderbuffer(Target,Handle);
        return this;
    }

}