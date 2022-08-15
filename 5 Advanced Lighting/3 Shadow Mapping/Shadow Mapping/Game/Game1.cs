using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Gamma_Correction.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model cube;
    Model quad;

    Objects.Light light;
    Objects.Material material;

    int depthMapFbo;
    int depthMap;
    Vector2i depthMapSize = new Vector2i(1024,1024);

    Texture texture;


    Vector3 shadowMapOrigin = new Vector3(-1.5f,8.5f,20f);
    Vector3 shadowMapDirection = new Vector3(1f,-4f,-5f);

    Matrix4 lightSpaceMatrix;

    protected override void Load()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        shader = new ShaderProgram(
            ShaderLocation + "vertex.glsl", 
            ShaderLocation + "fragment.glsl",
            true);
        
        player = new FirstPersonPlayer(Window.Size)
            .SetPosition(new Vector3(0,0,6))
            .SetDirection(new Vector3(0, 0, 1));
        player.UpdateProjection(shader);

        cube = new Model(PresetMesh.Cube);
            
        quad = new Model(PresetMesh.Square).Transform(new Vector3(0f,-5f,0f), new Vector3(MathHelper.DegreesToRadians(-90f),0f,0f),10f);

        texture = new Texture("../../../../../../0 Assets/wood.png",0)
            .Mipmapping(TextureMinFilter.Linear)
            .MinFilter(TextureMinFilter.Linear)
            .MagFilter(TextureMagFilter.Linear)
            .Wrapping(TextureWrapMode.ClampToEdge);
        
        light = new Objects.Light().SunMode().SetDirection(shadowMapDirection).SetAmbient(0.1f);
        material = PresetMaterial.Silver.SetAmbient(0.1f);


        depthMapFbo = GL.GenFramebuffer();

        depthMap = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D,depthMap);
        GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.DepthComponent,depthMapSize.X,depthMapSize.Y,0,PixelFormat.DepthComponent,PixelType.Float,IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapS,(int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapT,(int)TextureWrapMode.Repeat);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,depthMapFbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,FramebufferAttachment.DepthAttachment,TextureTarget.Texture2D,depthMap,0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        shader.EnableGammaCorrection();

        var lightSpaceView = Matrix4.LookAt(shadowMapOrigin, shadowMapOrigin + shadowMapDirection, Vector3.UnitY);
        var lightSpaceProjection = Matrix4.CreateOrthographic(20f,20f, 0.05f, 50f);// * lightView;
        
        lightSpaceMatrix = lightSpaceView * lightSpaceProjection;
        
        
        texture.Use();
        
        shader.UniformMaterial("material",material,texture)
            .UniformLight("light",light);
        
        
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D,depthMap);
        GL.Uniform1(GL.GetUniformLocation((int)shader,"depthMap"),1);


        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(shader,newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(shader, args, Window.KeyboardState, GetRelativeMouse());
        shader.Uniform3("cameraPos", player.Position);
    }

    void RenderScene()
    {
        //texture.Use();
        quad.UpdateTransform(shader);
        quad.Draw();
        
        cube.Transform(new Vector3(1f,-4f,-5f), new Vector3(0f,0.2f,0f), 1f);
        cube.UpdateTransform(shader);
        cube.Draw();
        
        cube.Transform(new Vector3(-3f,-3f,3f), new Vector3(0.4f,0f,0f), 1f);
        cube.UpdateTransform(shader);
        cube.Draw();
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        shader.Use();


        shader.SetActive(ShaderType.FragmentShader, "depthMap");
        GL.CullFace(CullFaceMode.Front);
        GL.Viewport(0,0,depthMapSize.X,depthMapSize.Y);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,depthMapFbo);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        
        var mat2 = Matrix4.Identity;
        
        GL.UniformMatrix4(shader.DefaultView,false,ref lightSpaceMatrix);
        GL.UniformMatrix4(shader.DefaultProjection,false,ref mat2);
        
        RenderScene();
        

        GL.CullFace(CullFaceMode.Back);
        GL.Viewport(0,0,Window.Size.X,Window.Size.Y);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        player.Camera.UpdateView((int)shader,shader.DefaultView);
        player.Camera.UpdateProjection((int)shader,shader.DefaultProjection);
        
        GL.UniformMatrix4(GL.GetUniformLocation((int)shader,"lightSpaceMatrix"),false, ref lightSpaceMatrix);

        shader.SetActive(ShaderType.FragmentShader, "scene");
        RenderScene();
        
        /*shader.SetActive(ShaderType.FragmentShader, "light");
        
        cube.Transform(Vector3.Zero, Vector3.Zero, 0.2f);
        cube.UpdateTransform(shader);
        cube.Draw();*/

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        quad.Delete();
        cube.Delete();

        shader.Delete();
    }
}