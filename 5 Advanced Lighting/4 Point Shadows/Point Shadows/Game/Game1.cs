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
    Model inverseCube;

    Objects.Light light;
    Objects.Material material;

    Texture texture;

    ShaderProgram depthMapShader;
    
    int depthMap;

    Vector3 depthMapPosition;


    int depthCubeMap;

    private Vector3 cubePosition = new Vector3(0f, 0f, -5f);
    
    const int CubeMapWidth = 2048;
    const int CubeMapHeight = 2048;

    const float DepthNear = 0.05f;
    const float DepthFar = 100f;

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

        var inverseCubeMesh = PresetMesh.Cube;
        for (int i = 0; i < inverseCubeMesh.Normals.Length; i++) { inverseCubeMesh.Normals[i] *= -1; }
        
        inverseCube = new Model(inverseCubeMesh);

        texture = new Texture("../../../../../../0 Assets/wood.png",0);

        depthMap = GL.GenFramebuffer();
        depthMapPosition = (0f,0f,0f);

        depthCubeMap = GL.GenTexture();
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
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,depthMap);
        GL.FramebufferTexture(FramebufferTarget.Framebuffer,FramebufferAttachment.DepthAttachment,depthCubeMap,0);
        
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);

        depthMapShader = new ShaderProgram()
            .LoadShader(DepthMapShaderLocation + "vertexCubeMap.glsl", ShaderType.VertexShader)
            .LoadShader(DepthMapShaderLocation + "geometryCubeMap.glsl", ShaderType.GeometryShader)
            .LoadShader(DepthMapShaderLocation + "fragmentCubeMap.glsl", ShaderType.FragmentShader)
            .Compile()
            .SetModelLocation("model");

        light = new Objects.Light().PointMode().SetPosition(depthMapPosition).SetAmbient(0.1f);
        material = PresetMaterial.Silver.SetAmbient(0.1f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);

        shader.EnableGammaCorrection();
        
        depthMapShader.Use();
        var proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 0.05f, 100f);

        Matrix4[] viewSpaceMatrices =
        {
            Matrix4.LookAt(Vector3.Zero,  Vector3.UnitX, -Vector3.UnitY) * proj,
            Matrix4.LookAt(Vector3.Zero, -Vector3.UnitX, -Vector3.UnitY) * proj,
            Matrix4.LookAt(Vector3.Zero,  Vector3.UnitY,  Vector3.UnitZ) * proj, // can't look straight up so swap the "up" direction
            Matrix4.LookAt(Vector3.Zero, -Vector3.UnitY, -Vector3.UnitZ) * proj,
            Matrix4.LookAt(Vector3.Zero,  Vector3.UnitZ, -Vector3.UnitY) * proj,
            Matrix4.LookAt(Vector3.Zero, -Vector3.UnitZ, -Vector3.UnitY) * proj,
        };

        depthMapShader.Use();
        GL.UniformMatrix4(GL.GetUniformLocation(depthMapShader.GetHandle(),"shadowMatrices[0]"),false, ref viewSpaceMatrices[0]);
        GL.UniformMatrix4(GL.GetUniformLocation(depthMapShader.GetHandle(),"shadowMatrices[1]"),false, ref viewSpaceMatrices[1]);
        GL.UniformMatrix4(GL.GetUniformLocation(depthMapShader.GetHandle(),"shadowMatrices[2]"),false, ref viewSpaceMatrices[2]);
        GL.UniformMatrix4(GL.GetUniformLocation(depthMapShader.GetHandle(),"shadowMatrices[3]"),false, ref viewSpaceMatrices[3]);
        GL.UniformMatrix4(GL.GetUniformLocation(depthMapShader.GetHandle(),"shadowMatrices[4]"),false, ref viewSpaceMatrices[4]);
        GL.UniformMatrix4(GL.GetUniformLocation(depthMapShader.GetHandle(),"shadowMatrices[5]"),false, ref viewSpaceMatrices[5]);

        texture.Use();
        
        shader.UniformMaterial("material",material,texture)
            .UniformLight("light",light);

        shader.Use();
        GL.ActiveTexture(TextureUnit.Texture0 + 1);
        GL.BindTexture(TextureTarget.TextureCubeMap,depthCubeMap);
        shader.Uniform1("depthMap", 1);
        
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
    }
    
    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Disable(EnableCap.CullFace);
        GL.Viewport(0,0,CubeMapWidth,CubeMapHeight);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,depthMap);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        depthMapShader.Use();
        
        cube.Draw(depthMapShader, cubePosition, new Vector3(0f,0.2f,0f));
        cube.Draw(depthMapShader,new Vector3(-3f,0f,3f), new Vector3(0.4f,0f,0f));

        
        GL.Enable(EnableCap.CullFace);
        shader.Use();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer,0);

        GL.Viewport(0,0,Window.Size.X,Window.Size.Y);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.SetActive(ShaderType.FragmentShader, "cube");
        shader.Uniform1("farPlane", DepthFar);
        depthMapShader.Uniform1("farPlane", DepthFar);
        
        GL.CullFace(CullFaceMode.Front);
        inverseCube.Draw(shader,scale: new Vector3(8f,8f,8f));
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

        GL.DeleteTexture(depthCubeMap);
        GL.DeleteFramebuffer(depthMap);
        depthMapShader.Delete();
    }
}