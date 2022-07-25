using OpenTK.Graphics.OpenGL4;

namespace Library;

public class FrameBuffer
{
    private int handle = -1;
    
    public FrameBuffer()
    {
        handle = GL.GenFramebuffer();
    }


    public FrameBuffer AttachTexture()
    {
        return this;
    }

    public FrameBuffer AttachRenderBuffer()
    {
        return this;
    }

    public FrameBuffer Enable()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,handle);
        return this;
    }
    
    public FrameBuffer Disable()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);
        return this;
    }
   
    
}