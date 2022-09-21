using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Parallax_Mapping.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";

    ShaderProgram shader;

    FirstPersonPlayer player;
    Model quad;
    Model cube;

    Objects.Light light;
    Objects.Material material;

    Texture texture;
    Texture normalMap;
    Texture displaceMap;

    bool normalMapping;

    private Vector3 rotation = Vector3.Zero;

    protected override void Load()
    {
        GL.ClearColor(0.01f, 0.01f, 0.01f, 1.0f);

        shader = new ShaderProgram()
            .LoadShader(ShaderLocation + "vertex.glsl", ShaderType.VertexShader)
            .LoadShader(ShaderLocation + "fragment.glsl", ShaderType.FragmentShader)
            .Compile()
            .EnableAutoProjection();
        
        player = new FirstPersonPlayer(Window.Size)
            .SetPosition(new Vector3(0,0,6))
            .SetDirection(new Vector3(0, 0, 1));
        player.UpdateProjection(shader);

        
        quad = new Model(PresetMesh.Square);

        texture = new Texture("../../../../../../0 Assets/toy_box_diffuse.png",0);
        normalMap = new Texture("../../../../../../0 Assets/toy_box_normal.png",1);
        displaceMap = new Texture("../../../../../../0 Assets/toy_box_disp.png",2);
        

        light = new Objects.Light().PointMode().SetPosition(new Vector3(-2f,2f,5f)).SetAmbient(0.1f);
        material = PresetMaterial.Silver.SetAmbient(0.01f);
        
        
        cube = new Model(PresetMesh.Cube)
            .UpdateTransform(shader,light.Position,Vector3.Zero,0.2f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);

        shader.EnableGammaCorrection();

        texture.Use();
        
        shader.UniformMaterial("material",material,texture)
            .UniformLight("light",light)
            .UniformTexture("normalMap",normalMap)
            .UniformTexture("displaceMap",displaceMap)
            .Uniform1("height_scale",heightScale);

        shader.Use();

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
        if (keyboardState.IsKeyPressed(Keys.Enter))
        {
            normalMapping = !normalMapping;
            shader.Uniform1("normalMapping",normalMapping?1:0);
        }
        
        if (keyboardState.IsKeyDown(Keys.Right)) rotation+=Vector3.UnitY*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Left))  rotation-=Vector3.UnitY*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Up))    rotation+=Vector3.UnitX*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Down))  rotation-=Vector3.UnitX*(float)args.Time;
        
        
    }


    float heightScale = 0.1f;
    
    protected override void MouseHandling(FrameEventArgs args, MouseState mouseState)
    {
        heightScale += mouseState.ScrollDelta.Y*((float)args.Time);
        shader.Uniform1("height_scale",heightScale);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        shader.SetActive(ShaderType.FragmentShader, "scene");
        quad.Draw(shader,rotation:rotation,scale:5f);

        shader.SetActive(ShaderType.FragmentShader, "light");
        cube.Draw(shader);

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