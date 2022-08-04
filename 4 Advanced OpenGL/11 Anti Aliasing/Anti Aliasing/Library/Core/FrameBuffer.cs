using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Library;

public class FrameBuffer
{
    private int handle = -1;
    
    // optional extras for simplification
    private bool usingPreset = false;
    private TextureBuffer colourAttachment;


    public FrameBuffer()
    {
        handle = GL.GenFramebuffer();
    }

    /// <summary>
    /// Create a common FBO with a texture bound to colour attachment 0 and a render buffer bound to the depth/stencil buffer
    /// </summary>
    /// <param name="pixelFormat"></param>
    /// <param name="size"></param>
    public FrameBuffer(PixelFormat pixelFormat, Vector2i size, TextureTarget target = TextureTarget.Texture2D) : this()
    {
        usingPreset = true;

        colourAttachment = new TextureBuffer(pixelFormat, size, target)
            .Wrapping(TextureWrapMode.ClampToEdge);
        
        AttachTexture(colourAttachment, FramebufferAttachment.ColorAttachment0);
        
        RenderBuffer depthStencilAttachment = new RenderBuffer(RenderbufferStorage.Depth24Stencil8, size);
        
        AttachRenderBuffer(depthStencilAttachment, FramebufferAttachment.DepthStencilAttachment);

        CheckCompletion();

        ReadMode();

    }


    public FrameBuffer UseTexture()
    {
        if (usingPreset)
        {
            colourAttachment.Use();
            return this;
        }

        throw new Exception("Texture isn't handled by this FrameBuffer");
    }

    public FrameBuffer UniformTexture(int shader, string name, int unit)
    {
        GL.UseProgram(shader);
        UseTexture();
        GL.Uniform1(GL.GetUniformLocation(shader,name),unit);
        return this;
    }


    /// <summary>
    /// Does nothing if complete, throws error if incomplete
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void CheckCompletion()
    {
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new Exception("Incomplete Fbo -> " + status);
        }
    }


    public FrameBuffer AttachTexture(TextureTarget textureTarget, int textureHandle, FramebufferAttachment attachment, FramebufferTarget target = FramebufferTarget.Framebuffer, int mipmap = 0)
    {
        WriteMode();
        GL.FramebufferTexture2D(target,attachment,textureTarget,textureHandle,mipmap);
        ReadMode();
        return this;
    }

    public FrameBuffer AttachTexture(TextureBuffer textureBuffer, FramebufferAttachment attachment,
        FramebufferTarget target = FramebufferTarget.Framebuffer, int mipmap = 0)
    {
        return AttachTexture(textureBuffer.Target, textureBuffer.Handle,attachment,target,mipmap);
    }

    public FrameBuffer AttachRenderBuffer(RenderbufferTarget renderBufferTarget, int renderBufferHandle, FramebufferAttachment attachment, FramebufferTarget target = FramebufferTarget.Framebuffer)
    {
        WriteMode();
        GL.FramebufferRenderbuffer(target,attachment,renderBufferTarget,renderBufferHandle);
        ReadMode();
        return this;
    }

    public FrameBuffer AttachRenderBuffer(RenderBuffer renderBuffer,
        FramebufferAttachment attachment, FramebufferTarget target = FramebufferTarget.Framebuffer)
    {
        return AttachRenderBuffer(renderBuffer.Target, renderBuffer.Handle, attachment, target);
    }

    public FrameBuffer WriteMode()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,handle);
        return this;
    }
    
    public FrameBuffer ReadMode()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);
        return this;
    }

    public void Delete()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);
        GL.DeleteFramebuffer(handle);
    }


}