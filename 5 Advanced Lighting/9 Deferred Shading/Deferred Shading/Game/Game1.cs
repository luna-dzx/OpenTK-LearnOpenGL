using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Deferred_Shading.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    const string LibraryShaderLocation = "../../../Library/Shaders/";

    ShaderProgram shader;
    ShaderProgram sceneShader;
    ShaderProgram gBufferShader;

    FirstPersonPlayer player;
    Model backpack;
    Model cube;

    Objects.Light light;
    Objects.Material material;

    Texture texture;
    Texture normalMap;
    Texture specular;

    FrameBuffer gBuffer;

    DrawBuffersEnum[] colourAttachments;

    bool bloomEnabled;

    float exposure = 1f;

    private Vector3 rotation = Vector3.Zero;
    
    
    protected override void Load()
    {
        GL.ClearColor(0.01f, 0.01f, 0.01f, 1.0f);


        shader = new ShaderProgram
        (
            ShaderLocation + "vertex.glsl",
            ShaderLocation + "fragment.glsl",
            true
        );
        
        sceneShader = new ShaderProgram()
            .LoadShader(ShaderLocation + "geometryVertex.glsl", ShaderType.VertexShader)
            .LoadShader(ShaderLocation + "geometryFragment.glsl", ShaderType.FragmentShader)
            .Compile()
            .EnableAutoProjection();
        
        player = new FirstPersonPlayer(Window.Size)
            .SetPosition(new Vector3(0,0,6))
            .SetDirection(new Vector3(0, 0, 1));
        player.UpdateProjection(sceneShader);
        
                    
        shader.Use();
        var matrix = player.Camera.GetProjMatrix();
        GL.UniformMatrix4(shader.DefaultProjection,false,ref matrix);
        
        Console.WriteLine(matrix);


        const string BackpackDir = "../../../../../../0 Assets/backpack/";
        backpack = Model.FromFile(BackpackDir,"backpack.obj",out var textures,
            Array.Empty<TextureType>(),
            PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.CalculateTangentSpace);

        
        
        texture = new Texture(BackpackDir+"diffuse.jpg",0);
        specular = new Texture(BackpackDir+"specular.jpg",1);
        normalMap = new Texture(BackpackDir+"normal.png",2);
        
        light = new Objects.Light().PointMode().SetPosition(new Vector3(-2f,2f,5f)).SetAmbient(0.1f);
        material = PresetMaterial.Silver.SetAmbient(0.1f);
        
        
        cube = new Model(PresetMesh.Cube)
            .UpdateTransform(shader,light.Position,Vector3.Zero,0.2f);


        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);

        sceneShader.EnableGammaCorrection();

        texture.Use();
        
        gBuffer = new FrameBuffer(Window.Size, new []
            {
                PixelInternalFormat.Rgb16f, // position
                PixelInternalFormat.Rgb16f, // normal
                PixelInternalFormat.Rgba8,   // albedo & specular
                PixelInternalFormat.Rgb16f, // TBN column 1
                PixelInternalFormat.Rgb16f, // TBN column 2
                PixelInternalFormat.Rgb16f  // TBN column 3
            }
        );
        
        gBufferShader = new ShaderProgram
        (
            LibraryShaderLocation+"PostProcessing/vertex.glsl",
            ShaderLocation+"lightingFragment.glsl"
        );

        
        sceneShader.Use();
        sceneShader.UniformMaterial("material",material,texture,specular)
            .UniformLight("light", light)
            .UniformTexture("normalMap",normalMap);        
        
        gBufferShader.Use();
        gBufferShader.UniformMaterial("material", material, texture, specular)
            .UniformLight("light", light);


        gBuffer.UniformTextures((int)gBufferShader, new[] { "gPosition", "gNormal", "gAlbedoSpec", "tbnColumn0", "tbnColumn1", "tbnColumn2" });
        colourAttachments = OpenGL.GetDrawBuffers(6);

        // attach player functions to window
        Window.Resize += newWin =>
        {
            player.Camera.Resize(sceneShader, newWin.Size);
            
            shader.Use();
            var matrix = player.Camera.GetProjMatrix();
            GL.UniformMatrix4(shader.DefaultProjection,false,ref matrix);
        };
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(sceneShader, args, Window.KeyboardState, GetRelativeMouse()*2f);
        shader.Use();
        var matrix = player.Camera.GetViewMatrix();
        GL.UniformMatrix4(shader.DefaultView,false,ref matrix);

        sceneShader.Uniform3("cameraPos", player.Position);
    }

    protected override void MouseHandling(FrameEventArgs args, MouseState mouseState)
    {
        exposure += mouseState.ScrollDelta.Y * (float)args.Time;
    }

    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyPressed(Keys.Enter)) { bloomEnabled = !bloomEnabled; }
        
        if (keyboardState.IsKeyDown(Keys.Right)) rotation+=Vector3.UnitY*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Left))  rotation-=Vector3.UnitY*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Up))    rotation+=Vector3.UnitX*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Down))  rotation-=Vector3.UnitX*(float)args.Time;
    }


    protected override void RenderFrame(FrameEventArgs args)
    {
        
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        gBuffer.WriteMode();
        
            sceneShader.Use();
            gBuffer.SetDrawBuffers(colourAttachments);
            
            // since we're also using texture channels 0 and 1 for the framebuffer
            // we need to bind our normal textures back to channels 0 and 1 with this
            texture.Use();
            specular.Use();
            normalMap.Use();
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            backpack.Draw(sceneShader,rotation:rotation,scale:5f);
        
        gBuffer.ReadMode();

        // draw gBuffer to screen
        gBufferShader.Use();
        gBuffer.UseTexture();
        PostProcessing.Draw();

        // use depth of gBuffer and draw other objects
        gBuffer.BlitDepth(Window.Size,true);
        shader.Use();
        cube.Draw();

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        backpack.Delete();
        cube.Delete();
        
        texture.Delete();
        normalMap.Delete();
        specular.Delete();
        
        gBuffer.Delete();
        
        shader.Delete();
        sceneShader.Delete();
        gBufferShader.Delete();
    }
}