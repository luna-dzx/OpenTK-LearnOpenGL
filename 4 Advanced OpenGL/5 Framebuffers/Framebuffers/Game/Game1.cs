using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace Face_Culling.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model cube;
    Model quad;

    Texture texture;

    int fboHandle;
    TextureBuffer fboTexture;

    //int renderBufferHandle;

    protected override void Load()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        shader = new ShaderProgram(
            ShaderLocation + "vertex.glsl", 
            ShaderLocation + "fragment.glsl",
            true);

        player = new FirstPersonPlayer(shader.DefaultProjection, shader.DefaultView, Window.Size)
            .SetPosition(new Vector3(0, 0, 3))
            .SetDirection(new Vector3(0, 0, -1));

        texture = new Texture("../../../../../../0 Assets/awesomeface.png",0);
        

        shader.UniformTexture("texture0", texture);
        cube = new Model(PresetMesh.Cube, shader.DefaultModel);
        quad = new Model(PresetMesh.Square, shader.DefaultModel);


        fboHandle = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,fboHandle);
        

        fboTexture = new TextureBuffer(PixelFormat.Rgb, Window.Size)
            .Wrapping(TextureWrapMode.ClampToEdge);

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,FramebufferAttachment.ColorAttachment0,TextureTarget.Texture2D,fboTexture.Handle,0);

        /*
        TextureBuffer fboDepthTexture = new TextureBuffer(PixelInternalFormat.Depth24Stencil8, PixelFormat.DepthStencil, Window.Size,
            pixelType: PixelType.UnsignedInt248);

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,FramebufferAttachment.DepthStencilAttachment,TextureTarget.Texture2D,fboDepthTexture.Handle,0);
        */
        
        RenderBuffer renderBuffer = new RenderBuffer(RenderbufferStorage.Depth24Stencil8, Window.Size);
        
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,FramebufferAttachment.DepthStencilAttachment,RenderbufferTarget.Renderbuffer,renderBuffer.Handle);

        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            Console.WriteLine("Incomplete Fbo -> "+status);
        }
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);

        texture.Use();

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
        Window.UpdateFrame += args => player.Update(args,Window.KeyboardState,GetRelativeMouse());
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        
        GL.Enable(EnableCap.CullFace);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer,fboHandle);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.Enable(EnableCap.DepthTest);

        texture.Use();
        
        shader.SetActive(ShaderType.VertexShader,"scene");
        shader.SetActive(ShaderType.FragmentShader,"scene");
        cube.ResetTransform();
        cube.Draw();    
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.Disable(EnableCap.DepthTest);


        fboTexture.Use();
        
        shader.SetActive(ShaderType.VertexShader,"quad");
        shader.SetActive(ShaderType.FragmentShader,"quad");
        quad.ResetTransform();
        quad.Draw();
        

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        cube.Delete();
        
        GL.DeleteFramebuffer(fboHandle);

        shader.Delete();
    }
}