using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SharpFont;


namespace SkeletalAnimation.Game;

public class Game1 : Library.Game
{
    StateHandler glState;
    TextRenderer textRenderer;
    
    const string ShaderLocation = "../../../Game/Shaders/";

    ShaderProgram shader;

    FirstPersonPlayer player;
    Model man;
    Model cube;

    Objects.Light light;
    Objects.Material material;

    Texture texture;

    private Vector3 rotation = Vector3.Zero;
    

    protected override void Initialize()
    {
        glState = new StateHandler();
        glState.ClearColor = new Color4(121, 139, 189, 255);
        
        shader = new ShaderProgram(
            ShaderLocation + "vertex.glsl", 
            ShaderLocation + "fragment.glsl", 
            true);
        
        player = new FirstPersonPlayer(Window.Size)
            .SetPosition(new Vector3(0,0,6))
            .SetDirection(new Vector3(0, 0, 1));
        
        const string BackpackDir = "../../../../../../0 Assets/RumbaDancing/";
        man = Model.FromFile(BackpackDir,"model.dae",out _ ,
            postProcessFlags: PostProcessSteps.Triangulate | PostProcessSteps.CalculateTangentSpace);

        texture = new Texture(BackpackDir+"diffuse.png",0);

        light = new Objects.Light().PointMode().SetPosition(new Vector3(-3f,5f,3f)).SetAmbient(0.1f);
        material = PresetMaterial.Obsidian;
        
        cube = new Model(PresetMesh.Cube)
            .UpdateTransform(shader,light.Position,Vector3.Zero,0.2f);

        glState.DepthTest = true;
        glState.DoCulling = true;
        glState.DepthMask = true;

        shader.EnableGammaCorrection();



        shader.UniformMaterial("material", material, texture)
            .UniformLight("light", light);


        glState.Blending = true;
    }
    
    
    protected override void Load()
    {
        textRenderer = new TextRenderer(48,Window.Size);
        player.UpdateProjection(shader);
    }
    
    protected override void Resize(ResizeEventArgs newWin)
    {
        player.Camera.Resize(shader,newWin.Size);
        textRenderer.UpdateScreenSize(newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(shader, args, Window.KeyboardState, GetRelativeMouse()*3f);
        shader.Uniform3("cameraPos", player.Position);
    }
    
    
    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.Right)) rotation+=Vector3.UnitY*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Left))  rotation-=Vector3.UnitY*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Up))    rotation+=Vector3.UnitX*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Down))  rotation-=Vector3.UnitX*(float)args.Time;
    }
    


    protected override void RenderFrame(FrameEventArgs args)
    {
        glState.Clear();

        texture.Use();
        
        shader.SetActive(ShaderType.FragmentShader, "scene");
        man.Draw(shader,new Vector3(0f,-2f,0f),rotation,0.04f);

        shader.SetActive(ShaderType.FragmentShader, "light");
        cube.Draw(shader);

        textRenderer.Draw("+", Window.Size.X/2f, Window.Size.Y/2f, 0.5f, Vector3.Zero);
        textRenderer.Draw("KeyFrame: 0", 10f, Window.Size.Y - 48f, 1f, Vector3.Zero, false);

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        texture.Delete();
        
        man.Delete();
        cube.Delete();

        shader.Delete();
        
        textRenderer.Delete();
    }
}