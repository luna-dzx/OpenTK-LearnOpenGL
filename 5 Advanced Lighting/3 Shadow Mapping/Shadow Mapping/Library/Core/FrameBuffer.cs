using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Library;

public class FrameBuffer
{
    private int handle = -1;
    
    // optional extras for simplification
    private bool usingPreset = false;
    private TextureBuffer colourAttachment;
    private TextureBuffer depthStencilAttachment;
    private RenderBuffer depthStencilRenderBuffer;
    private bool usingRenderBuffer = true;


    public FrameBuffer()
    {
        handle = GL.GenFramebuffer();
    }

    /// <summary>
    /// Create a common FBO with a texture bound to colour attachment 0 and a render buffer bound to the depth/stencil buffer
    /// </summary>
    /// <param name="pixelFormat"></param>
    /// <param name="size"></param>
    public FrameBuffer(Vector2i size, TextureTarget target = TextureTarget.Texture2D, PixelFormat pixelFormat= PixelFormat.Rgb,
        PixelInternalFormat internalFormat = PixelInternalFormat.Rgba8, int numSamples = 4) : this()
    {
        usingPreset = true;

        colourAttachment = new TextureBuffer(internalFormat, pixelFormat, (size.X,size.Y), target,samples:numSamples);

        if (37120 <= (int)target && (int)target <= 37123) // MSAA
        {
            AttachTexture(colourAttachment, FramebufferAttachment.ColorAttachment0);

            depthStencilAttachment = new TextureBuffer(PixelInternalFormat.Depth24Stencil8, PixelFormat.DepthStencil,
                (size.X, size.Y), target, samples: numSamples);
            
            AttachTexture(depthStencilAttachment, FramebufferAttachment.DepthStencilAttachment);

            usingRenderBuffer = false;
        }
        else // NO MSAA
        {
            
            colourAttachment.Wrapping(TextureWrapMode.ClampToEdge);
            AttachTexture(colourAttachment, FramebufferAttachment.ColorAttachment0);
        
            depthStencilRenderBuffer = new RenderBuffer(RenderbufferStorage.Depth24Stencil8, size);
            AttachRenderBuffer(depthStencilRenderBuffer, FramebufferAttachment.DepthStencilAttachment);
        }
        


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

        if (usingPreset)
        {
            GL.DeleteTexture(colourAttachment.Handle);
            if (usingRenderBuffer)
            {
                GL.DeleteRenderbuffer(depthStencilRenderBuffer.Handle);
            }
            else
            {
                GL.DeleteTexture(depthStencilAttachment.Handle);
            }
        }

        GL.DeleteFramebuffer(handle);
    }


}


public class DepthMap
{
    public readonly int Handle;
    public readonly int TextureHandle;

    public readonly Vector2i Size;
    
    public Matrix4 ViewSpaceMatrix;
    
    //Vector3 Position = new Vector3(-3.5f,8.5f,20f);
    //Vector3 Direction = new Vector3(1f,-4f,-5f);
    public Vector3 Position { get; private set; }
    public Vector3 Direction { get; private set; }


    public DepthMap(Vector2i size, Vector3 position, Vector3 direction)
    {
        Handle = GL.GenFramebuffer();
        TextureHandle = GL.GenTexture();
        Size = size;
        Position = position;
        Direction = direction;
        
        GL.BindTexture(TextureTarget.Texture2D,TextureHandle);
        GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.DepthComponent,Size.X,Size.Y,0,PixelFormat.DepthComponent,PixelType.Float,IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapS,(int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapT,(int)TextureWrapMode.Repeat);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,Handle);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,FramebufferAttachment.DepthAttachment,TextureTarget.Texture2D,TextureHandle,0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);
    }

    public DepthMap DrawMode(int x = 0, int y = 0, int width = 0, int height = 0, CullFaceMode cullFaceMode = CullFaceMode.Front)
    {
        if (width == 0) width = Size.X;
        if (height == 0) height = Size.Y;
        
        GL.CullFace(cullFaceMode);
        GL.Viewport(x,y,width,height);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,Handle);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        return this;
    }

    public DepthMap ReadMode()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);
        return this;
    }

    public DepthMap ProjectOrthographic(float orthoWidth = 24f, float orthoHeight = 24f, float clipNear = 0.05f, float clipFar = 50f, Vector3 up = default)
    {
        if (up == default) up = Vector3.UnitY;
        Matrix4 viewMatrix = Matrix4.LookAt(Position, Position + Direction, up);
        Matrix4 projMatrix = Matrix4.CreateOrthographic(orthoWidth,orthoHeight, clipNear, clipFar);
        ViewSpaceMatrix = viewMatrix * projMatrix;
        
        return this;
    }

    public DepthMap ProjectPerspective(float fieldOfView = MathHelper.PiOver3, float clipNear = 0.1f, float clipFar = 100f, Vector3 up = default)
    {
        if (up == default) up = Vector3.UnitY;
        Matrix4 viewMatrix = Matrix4.LookAt(Position, Position + Direction, up);
        Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView(fieldOfView, (float) Size.X / Size.Y, clipNear, clipFar);
        ViewSpaceMatrix = viewMatrix * projMatrix;
        
        return this;
    }

    public DepthMap UniformMatrix(int shaderProgram, string name)
    {
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram,name),false, ref ViewSpaceMatrix);
        return this;
    }

}