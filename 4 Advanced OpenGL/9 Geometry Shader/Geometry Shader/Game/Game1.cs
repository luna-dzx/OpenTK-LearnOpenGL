using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Geometry_Shader.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;
    ShaderProgram normalShader;

    FirstPersonPlayer player;
    Model backpack;

    protected override void Load()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        shader = new ShaderProgram(
            ShaderLocation + "vertex.glsl", 
            ShaderLocation + "fragment.glsl",
            true);

        
        normalShader = new ShaderProgram()
            .LoadShader(ShaderLocation + "normals/vertex.glsl", ShaderType.VertexShader)
            .LoadShader(ShaderLocation + "normals/geometry.glsl", ShaderType.GeometryShader)
            .LoadShader(ShaderLocation + "normals/fragment.glsl", ShaderType.FragmentShader)
            .Compile()
            .EnableAutoProjection()
            ;


        player = new FirstPersonPlayer(Window.Size)
            .SetPosition(new Vector3(0, 0, 3))
            .SetDirection(new Vector3(0, 0, -1))
            .UpdateProjection(shader).UpdateProjection(normalShader)
            ;
        

        backpack = Model.FromFile(
                    "../../../../../../0 Assets/backpack/", "backpack.obj",
                    out var textures,
                    shader.DefaultModel,
                    new [] { TextureType.Diffuse}
                )
                .Transform(Vector3.UnitX*5f, new Vector3(0.3f,0.2f,0.8f), Vector3.One)
                .UpdateTransform(shader).UpdateTransform(normalShader)
            ;


        Texture texture = textures[TextureType.Diffuse][0].Use();
        shader.UniformTexture("texture0", texture);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(args,Window.KeyboardState,GetRelativeMouse())
            .UpdateView(shader)
            .UpdateView(normalShader);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.Use();
        shader.SetActive(ShaderType.FragmentShader, "alt");
        backpack.Draw();

        normalShader.Use();
        backpack.Draw();

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        backpack.Delete();
        
        shader.Delete();
        normalShader.Delete();
    }
}