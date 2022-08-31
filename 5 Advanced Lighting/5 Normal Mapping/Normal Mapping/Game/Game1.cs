﻿using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Normal_Mapping.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";

    ShaderProgram shader;

    FirstPersonPlayer player;
    Model quad;

    Objects.Light light;
    Objects.Material material;

    Texture texture;
    Texture normalMap;
    
    bool normalMapping;

    private Vector3 rotation = Vector3.Zero; //  new Vector3(0f,MathHelper.DegreesToRadians(59f),0f);

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
        

        quad = new Model(PresetMesh.Square);

        texture = new Texture("../../../../../../0 Assets/brickwall.jpg",0);
        normalMap = new Texture("../../../../../../0 Assets/brickwall_normal.jpg",1);

        light = new Objects.Light().PointMode().SetPosition(new Vector3(-2f,2f,5f)).SetAmbient(0.1f);
        material = PresetMaterial.Silver.SetAmbient(0.1f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);

        shader.EnableGammaCorrection();

        texture.Use();
        
        shader.UniformMaterial("material",material,texture)
            .UniformLight("light",light)
            .UniformTexture("normalMap",normalMap);

        shader.Use();

        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(shader,newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(shader, args, Window.KeyboardState, GetRelativeMouse());
        shader.Uniform3("cameraPos", player.Position);
        //rotation += Vector3.UnitX * (float)(args.Time * 2f);
    }

    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyPressed(Keys.Enter))
        {
            normalMapping = !normalMapping;
            shader.Uniform1("normalMapping",normalMapping?1:0);
        }
        
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        quad.Draw(shader,rotation:rotation,scale:5f);

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        quad.Delete();

        shader.Delete();
    }
}