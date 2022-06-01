using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Transformations.Library;

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
    /// <param name="textureUnit">the GPU texture unit to pass this texture to</param>
    /// <param name="path">path to the image file</param>
    /// <param name="textureTarget">the type of texture to store</param>
    public Texture(int textureUnit, string path, TextureTarget textureTarget = TextureTarget.Texture2D) : this(textureUnit,textureTarget)
    {
        this.Use();
        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream,ColorComponents.RedGreenBlueAlpha);
        GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,image.Width,image.Height,0,PixelFormat.Rgba,PixelType.UnsignedByte,image.Data);
        GL.GenerateMipmap((GenerateMipmapTarget)target);
    }

    /// <summary>
    /// Load the texture to the GPU
    /// </summary>
    /// <param name="program">the shader program you are loading the texture to</param>
    /// <param name="name">the variable name of the sampler in glsl</param>
    public void LoadUniform(int program, string name)
    {
        GL.UseProgram(program);
        GL.Uniform1(GL.GetUniformLocation(program,name),unit);
        this.Use();
    }

    /// <summary>
    /// Activate the correct texture unit and set up the OpenGL texture for editing properties
    /// </summary>
    public void Use()
    {
        GL.ActiveTexture((TextureUnit) (unit + (int)TextureUnit.Texture0));
        GL.BindTexture(target,handle);
    }

    public void Wrapping(TextureParameterName paramName, TextureWrapMode wrapMode) { this.Use(); GL.TexParameter(target, paramName, (int)wrapMode); }

    /// <summary>
    /// Set parameters for how textures are displayed which take up less pixels on screen than the number of pixels in the texture image
    /// </summary>
    /// <param name="filter">the filter for how surrounding pixels should be sampled (if at all)</param>
    public void MinFilter(TextureMinFilter filter) { this.Use(); GL.TexParameter(target,TextureParameterName.TextureMinFilter,(int)filter); }

    /// <summary>
    /// Set parameters for how textures are displayed which take up more pixels on screen than the number of pixels in the texture image
    /// </summary>
    /// <param name="filter">the filter for how surrounding pixels should be sampled (if at all)</param>
    public void MagFilter(TextureMagFilter filter) { this.Use(); GL.TexParameter(target,TextureParameterName.TextureMagFilter,(int)filter); }
    
    /// <summary>
    /// Set parameters for how mipmapping should be handled (lower res textures further away)
    /// </summary>
    /// <param name="mipmapFilter">the texture filter for blending between mipmaps and the current texture's sample</param>
    public void Mipmapping(TextureMinFilter mipmapFilter) { this.Use(); GL.TexParameter(target,TextureParameterName.TextureMinFilter,(int)mipmapFilter); }

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