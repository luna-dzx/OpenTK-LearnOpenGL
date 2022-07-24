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
    int fboTexture;

    int renderBufferHandle;

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

        fboTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D,fboTexture);
        
        GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgb,800,600,0,PixelFormat.Rgb,PixelType.UnsignedByte,IntPtr.Zero);
        
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        
        GL.BindTexture(TextureTarget.Texture2D,0);
        
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,FramebufferAttachment.ColorAttachment0,TextureTarget.Texture2D,fboTexture,0);

        /*
        GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Depth24Stencil8,800,600,0,
            PixelFormat.DepthStencil,PixelType.UnsignedInt248,IntPtr.Zero);
        
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,FramebufferAttachment.DepthStencilAttachment,TextureTarget.Texture2D,fboTexture,0);
        */

        renderBufferHandle = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer,renderBufferHandle);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer,RenderbufferStorage.Depth24Stencil8,800,600);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer,0);
        
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,FramebufferAttachment.DepthStencilAttachment,RenderbufferTarget.Renderbuffer,renderBufferHandle);

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

        
        GL.BindTexture(TextureTarget.Texture2D,fboTexture);
        
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