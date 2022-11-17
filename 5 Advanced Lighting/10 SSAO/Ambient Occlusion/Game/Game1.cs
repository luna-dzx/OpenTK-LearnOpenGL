using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Ambient_Occlusion.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    const string DepthMapShaderLocation = "../../../Library/Shaders/DepthMap/";
    
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model cube;
    Model square;
    Model inverseCube;
    Model backpack;

    Objects.Light light;
    Objects.Material material;
    Objects.Material cubeMaterial;

    Texture marbleTexture;
    Texture brickTexture;
    Texture brickNormal;
    Texture brickDisplace;
    Texture texture;
    Texture specular;
    Texture normalMap;
    Texture toyboxNormal;

    CubeDepthMap depthMap;
    

    private Vector3 backpackPosition = new Vector3(0f, 0f, -5f);
    private Vector3 backpackRotation = new Vector3(0f, 0f, 0f);
    
    float heightScale = 0.1f;

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
        
        
        const string AssetsDir = "../../../../../../0 Assets/";
        const string BackpackDir = "../../../../../../0 Assets/backpack/";
        backpack = Model.FromFile(BackpackDir,"backpack.obj",out _,
            postProcessFlags: PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.CalculateTangentSpace);
        
        texture = new Texture(BackpackDir+"diffuse.bmp",0);
        specular = new Texture(BackpackDir+"specular.bmp",1);
        normalMap = new Texture(BackpackDir+"normal.bmp",2);
        
        brickTexture = new Texture(AssetsDir+"bricks2.bmp",0);
        brickNormal = new Texture(AssetsDir+"bricks2_normal.bmp",2);
        brickDisplace = new Texture(AssetsDir+"bricks2_disp.bmp",4);
        
        toyboxNormal = new Texture(AssetsDir+"toy_box_normal.bmp",2);

        shader.UniformTexture("normalMap", normalMap)
            .UniformTexture("displaceMap", brickDisplace)
            .Uniform1("height_scale",heightScale);;
        

        cube = new Model(PresetMesh.Cube);
        square = new Model(PresetMesh.Square);

        var inverseCubeMesh = PresetMesh.Cube;
        for (int i = 0; i < inverseCubeMesh.Normals.Length; i++) { inverseCubeMesh.Normals[i] *= -1; }

        inverseCube = new Model(inverseCubeMesh);

        marbleTexture = new Texture(AssetsDir+"metal.png",0);


        depthMap = new CubeDepthMap(DepthMapShaderLocation, (2048, 2048), Vector3.Zero).UpdateMatrices();

        light = new Objects.Light().PointMode().SetPosition(depthMap.Position).SetAmbient(0.1f);
        material = PresetMaterial.Silver.SetAmbient(0.1f);
        cubeMaterial = new Objects.Material(0.1f,0.01f,0.1f,64f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);

        shader.EnableGammaCorrection();

        marbleTexture.Use();

        shader.UniformMaterial("material", material, texture, specular)
            .UniformMaterial("cubeMaterial",cubeMaterial,marbleTexture,marbleTexture)
            .UniformLight("light", light);

        shader.Use();

        depthMap.UniformTexture(shader,"depthMap", 3);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(shader,newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(shader, args, Window.KeyboardState, GetRelativeMouse());
        shader.Uniform3("cameraPos", player.Position);
    }
    
    protected override void MouseHandling(FrameEventArgs args, MouseState mouseState)
    {
        heightScale += mouseState.ScrollDelta.Y*((float)args.Time);
        shader.Uniform1("height_scale",heightScale);
    }

    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        Vector3 direction = Vector3.Zero;
        if (keyboardState.IsKeyDown(Keys.Up)) direction -= Vector3.UnitZ;
        if (keyboardState.IsKeyDown(Keys.Down)) direction += Vector3.UnitZ;
        if (keyboardState.IsKeyDown(Keys.Left)) direction -= Vector3.UnitX;
        if (keyboardState.IsKeyDown(Keys.Right)) direction += Vector3.UnitX;

        backpackPosition += direction * (float)args.Time * 5f;
        
        Vector3 rotation = Vector3.Zero;
        if (keyboardState.IsKeyDown(Keys.KeyPad8)) rotation -= Vector3.UnitX;
        if (keyboardState.IsKeyDown(Keys.KeyPad2)) rotation += Vector3.UnitX;
        if (keyboardState.IsKeyDown(Keys.KeyPad4)) rotation -= Vector3.UnitY;
        if (keyboardState.IsKeyDown(Keys.KeyPad6)) rotation += Vector3.UnitY;

        backpackRotation += rotation * (float)args.Time;
        
        
        
    }
    
    protected override void RenderFrame(FrameEventArgs args)
    {
        depthMap.DrawMode();
        
        backpack.Draw(depthMap.Shader, backpackPosition, backpackRotation);
        square.Draw(depthMap.Shader,new Vector3(-3f,0f,3f), new Vector3(0f,MathF.PI/2f + 0.3f,0f), 2f);

        
        GL.Enable(EnableCap.CullFace);
        shader.Use();
        depthMap.ReadMode();

        GL.Viewport(0,0,Window.Size.X,Window.Size.Y);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        
        marbleTexture.Use();

        shader.SetActive(ShaderType.FragmentShader, "cube");
        depthMap.UniformClipFar(shader, "farPlane");
        


        GL.CullFace(CullFaceMode.Front);
        inverseCube.Draw(shader,scale: 8f);
        GL.CullFace(CullFaceMode.Back);

        brickTexture.Use();
        brickNormal.Use();
        brickDisplace.Use();
        shader.SetActive(ShaderType.FragmentShader, "brick");
        square.Draw(shader,new Vector3(-3f,0f,3f), new Vector3(0f,MathF.PI/2f + 0.3f,0f), 2f);

        texture.Use();
        normalMap.Use();
        shader.SetActive(ShaderType.FragmentShader, "backpack");
        backpack.Draw(shader,backpackPosition, backpackRotation);
        
        
        
        shader.SetActive(ShaderType.FragmentShader, "light");
        toyboxNormal.Use();
        cube.Draw(shader,depthMap.Position,scale: 0.2f);

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        cube.Delete();
        square.Delete();
        inverseCube.Delete();
        backpack.Delete();

        shader.Delete();
        
        marbleTexture.Delete();
        brickTexture.Delete();
        brickNormal.Delete();
        brickDisplace.Delete();
        texture.Delete();
        specular.Delete();
        normalMap.Delete();
        toyboxNormal.Delete();

        depthMap.Delete();
    }
}