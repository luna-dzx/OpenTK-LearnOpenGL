using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace MeshProj.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model cube;

    Texture texture;

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

        texture = new Texture("../../../../../../0 Assets/container2.png",0);

        cube = new Model(PresetMesh.Cube, shader.DefaultModel);

        shader.UniformTexture("texture0",texture);
        
        Scene test;
        AssimpContext importer = new AssimpContext();
        //importer.SetConfig(new Assimp.Configs.NormalSmoothingAngleConfig(66.0f));
        test = importer.ImportFile("../../../../../../0 Assets/backpack/backpack.obj",PostProcessPreset.TargetRealTimeMaximumQuality);
        
        Console.WriteLine(test.Meshes.Count);

        // attach player functions to window
        Window.UpdateFrame += args => player.Update(args,Window.KeyboardState,GetRelativeMouse());
        Window.Resize += newWin => player.Camera.Resize(newWin.Size);
    }
    
    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        cube.ResetTransform();
        cube.Draw();
        
        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        cube.Delete();
        texture.Delete();
        shader.Delete();
    }
    
}