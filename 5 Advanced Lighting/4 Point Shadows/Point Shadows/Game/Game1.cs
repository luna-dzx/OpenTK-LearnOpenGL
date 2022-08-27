using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Point_Shadows.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    const string DepthMapShaderLocation = "../../../Library/Shaders/DepthMap/";
    
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model cube;

    Objects.Light light;
    Objects.Material material;

    Texture texture;

    ShaderProgram depthMapShader;
    
    int depthMap;
    int depthMapTexture;

    Matrix4 ViewSpaceMatrix;

    Vector3 depthMapPosition;
    Vector3 depthMapDirection;
    
    
    int depthCubeMap;
    
    bool visualiseDepthMap = false;
    
    private Vector3 cubePosition = new Vector3(1f, 0f, -5f);
    
    const int CubeMapWidth = 1024;
    const int CubeMapHeight = 1024;
    
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
        
        texture = new Texture("../../../../../../0 Assets/wood.png",0);

        depthMap = GL.GenFramebuffer();
        depthMapTexture = GL.GenTexture();
        depthMapPosition = (-3.5f, 8.5f, 20f);
        depthMapDirection = (1f,-4f,-5f);

        GL.BindTexture(TextureTarget.Texture2D,depthMapTexture);
        GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.DepthComponent,4096,4096,0,PixelFormat.DepthComponent,PixelType.Float,IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapS,(int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapT,(int)TextureWrapMode.Repeat);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,depthMap);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,FramebufferAttachment.DepthAttachment,TextureTarget.Texture2D,depthMapTexture,0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);

        depthMapShader = new ShaderProgram(
            DepthMapShaderLocation + "vertex.glsl",
            DepthMapShaderLocation + "fragment.glsl"
        ).SetModelLocation("model");
        
        
        
        light = new Objects.Light().SunMode().SetDirection(depthMapDirection).SetAmbient(0.1f);
        material = PresetMaterial.Silver.SetAmbient(0.1f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        shader.EnableGammaCorrection();

        Matrix4 viewMatrix = Matrix4.LookAt(depthMapPosition, depthMapPosition + depthMapDirection, Vector3.UnitY);
        Matrix4 projMatrix = Matrix4.CreateOrthographic(24f,24f, 0.05f, 50f);
        ViewSpaceMatrix = viewMatrix * projMatrix;
        
        depthMapShader.Use();
        GL.UniformMatrix4(GL.GetUniformLocation((int)depthMapShader,"lightSpaceMatrix"),false, ref ViewSpaceMatrix);        
        shader.Use();
        GL.UniformMatrix4(GL.GetUniformLocation((int)shader,"lightSpaceMatrix"),false, ref ViewSpaceMatrix);

        texture.Use();
        
        shader.UniformMaterial("material",material,texture)
            .UniformLight("light",light);

        shader.Use();
        GL.ActiveTexture(TextureUnit.Texture0 + 1);
        GL.BindTexture(TextureTarget.Texture2D,depthMapTexture);
        shader.Uniform1("depthMap", 1);

        /*depthCubeMap = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap,depthCubeMap);
        for (int i = 0; i < 6; i++)
        {
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                0, PixelInternalFormat.DepthComponent,
                CubeMapWidth,CubeMapHeight,0,
                PixelFormat.DepthComponent,PixelType.Float,IntPtr.Zero);
        }
        
        GL.TexParameter(TextureTarget.TextureCubeMap,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.TextureCubeMap,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.TextureCubeMap,TextureParameterName.TextureWrapS,(int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap,TextureParameterName.TextureWrapT,(int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap,TextureParameterName.TextureWrapR,(int)TextureWrapMode.ClampToEdge);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,depthMap.Handle);
        GL.FramebufferTexture(FramebufferTarget.Framebuffer,FramebufferAttachment.DepthAttachment,depthCubeMap,0);*/
        

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(shader,newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(shader, args, Window.KeyboardState, GetRelativeMouse());
        shader.Uniform3("cameraPos", player.Position);
    }

    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        Vector3 direction = Vector3.Zero;
        if (keyboardState.IsKeyDown(Keys.Up)) direction -= Vector3.UnitZ;
        if (keyboardState.IsKeyDown(Keys.Down)) direction += Vector3.UnitZ;
        if (keyboardState.IsKeyDown(Keys.Left)) direction -= Vector3.UnitX;
        if (keyboardState.IsKeyDown(Keys.Right)) direction += Vector3.UnitX;

        cubePosition += direction * (float)args.Time * 5f;


        if (keyboardState.IsKeyPressed(Keys.Enter))
        {
            visualiseDepthMap = !visualiseDepthMap;
            shader.Uniform1("visualiseDepthMap", visualiseDepthMap ? 1 : 0);
        }
    }
    
    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.CullFace(CullFaceMode.Front);
        GL.Viewport(0,0,4096,4096);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,depthMap);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        depthMapShader.Use();

        cube.Draw(depthMapShader, cubePosition, new Vector3(0f,0.2f,0f));
        cube.Draw(depthMapShader,new Vector3(-3f,0f,3f), new Vector3(0.4f,0f,0f));
        
        
        shader.Use();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);

        GL.Viewport(0,0,Window.Size.X,Window.Size.Y);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.SetActive(ShaderType.FragmentShader, "cube");
        shader.Uniform1("flipNormals", 1);
        GL.CullFace(CullFaceMode.Front);
        cube.Draw(shader,scale: new Vector3(8f,8f,8f));
        
        shader.Uniform1("flipNormals", 0);
        GL.CullFace(CullFaceMode.Back);
        cube.Draw(shader,cubePosition, new Vector3(0f,0.2f,0f));
        cube.Draw(shader,new Vector3(-3f,0f,3f), new Vector3(0.4f,0f,0f));
        
        shader.SetActive(ShaderType.FragmentShader, "light");
        cube.Draw(shader,depthMapPosition,scale: new Vector3(0.2f,0.2f,0.2f));

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        cube.Delete();

        shader.Delete();

        GL.DeleteTexture(depthMapTexture);
        GL.DeleteFramebuffer(depthMap);
        depthMapShader.Delete();
    }
}