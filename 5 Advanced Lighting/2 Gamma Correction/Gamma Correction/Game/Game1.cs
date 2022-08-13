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

    Texture texture;

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

        cube = new Model(PresetMesh.Cube).Transform(Vector3.Zero, Vector3.Zero, 0.2f);
            
        quad = new Model(PresetMesh.Square).Transform(new Vector3(0f,-5f,0f), new Vector3(MathHelper.DegreesToRadians(-90f),0f,0f),5f);

        texture = new Texture("../../../../../../0 Assets/wood.png",0)
            .Mipmapping(TextureMinFilter.Linear)
            .MinFilter(TextureMinFilter.Linear)
            .MagFilter(TextureMagFilter.Linear)
            .Wrapping(TextureWrapMode.ClampToEdge);
        
        light = new Objects.Light().PointMode();
        material = PresetMaterial.Silver.SetShininess(32f);
        
        shader.UniformMaterial("material",material,texture)
            .UniformLight("light",light);


        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        shader.EnableGammaCorrection();

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(shader,newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(shader, args, Window.KeyboardState, GetRelativeMouse());
        shader.Uniform3("cameraPos", player.Position);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        shader.Use();

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.SetActive(ShaderType.FragmentShader, "scene");
        quad.UpdateTransform(shader);
        quad.Draw();
        
        shader.SetActive(ShaderType.FragmentShader, "light");
        cube.UpdateTransform(shader);
        cube.Draw();

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