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

    CubeDepthMap depthMap;
    

    private Vector3 cubePosition = new Vector3(0f, 0f, -5f);

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


        depthMap = new CubeDepthMap(DepthMapShaderLocation, (2048, 2048), Vector3.Zero).UpdateMatrices();

        light = new Objects.Light().PointMode().SetPosition(depthMap.Position).SetAmbient(0.1f);
        material = PresetMaterial.Silver.SetAmbient(0.1f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);

        shader.EnableGammaCorrection();

        texture.Use();
        
        shader.UniformMaterial("material",material,texture)
            .UniformLight("light",light);

        shader.Use();

        depthMap.UniformTexture(shader,"depthMap", 1);

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
        depthMap.DrawMode();
        
        cube.Draw(depthMap.Shader, cubePosition, new Vector3(0f,0.2f,0f));
        cube.Draw(depthMap.Shader,new Vector3(-3f,0f,3f), new Vector3(0.4f,0f,0f));

        
        GL.Enable(EnableCap.CullFace);
        shader.Use();
        depthMap.ReadMode();

        GL.Viewport(0,0,Window.Size.X,Window.Size.Y);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.SetActive(ShaderType.FragmentShader, "cube");
        depthMap.UniformClipFar(shader, "farPlane");

        GL.CullFace(CullFaceMode.Front);
        inverseCube.Draw(shader,scale: new Vector3(8f,8f,8f));
        GL.CullFace(CullFaceMode.Back);
        cube.Draw(shader,cubePosition, new Vector3(0f,0.2f,0f));
        cube.Draw(shader,new Vector3(-3f,0f,3f), new Vector3(0.4f,0f,0f));
        
        shader.SetActive(ShaderType.FragmentShader, "light");
        cube.Draw(shader,depthMap.Position,scale: new Vector3(0.2f,0.2f,0.2f));

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        cube.Delete();

        shader.Delete();

        depthMap.Delete();
    }
}