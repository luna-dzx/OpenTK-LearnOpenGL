using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Deferred_Shading.Game;

public class Game1 : Library.Game
{
    private const int NUM_LIGHTS = 32;
    private const int NUM_BACKPACKS = 32;
    const string ShaderLocation = "../../../Game/Shaders/";

    ShaderProgram shader;
    ShaderProgram sceneShader;
    ShaderProgram gBufferShader;

    FirstPersonPlayer player;
    Model backpack;
    Model cube;

    Objects.Light[] lights;
    Objects.Material material;

    Texture texture;
    Texture normalMap;
    Texture specular;

    GeometryBuffer gBuffer;

    private Vector3 rotation = Vector3.Zero;
    private Matrix4[] backpackTransforms;

    Random r = new Random();
    void RandomizeLights()
    {
        for (int i = 0; i < NUM_LIGHTS; i++)
        {
            var tempColour = Color4.FromHsv(new Vector4((float)r.NextDouble(),1f,1f,1f));
            Vector3 colour = new Vector3(tempColour.R, tempColour.G, tempColour.B);
            
            lights[i] = new Objects.Light().PointMode()
                .SetPosition(new Vector3(15f*(float)r.NextDouble() - 7.5f,15f*(float)r.NextDouble() - 7.5f,15f*(float)r.NextDouble() - 7.5f))
                .SetAmbient(1f)
                .SetDiffuse(colour)
                .SetAttenuation(0.5f,0.22f,0.2f);
        }
    }
    void RandomizeBackpacks()
    {
        backpackTransforms = new Matrix4[NUM_BACKPACKS];
        float positionRange = 2f * MathF.Sqrt(NUM_BACKPACKS);
        
        for (int i = 0; i < NUM_BACKPACKS; i++)
        {
            backpackTransforms[i] = Maths.CreateTransformation(
                
                new Vector3(
                    positionRange * (float)r.NextDouble() - positionRange/2f, 
                    positionRange * (float)r.NextDouble() - positionRange/2f,
                    positionRange * (float)r.NextDouble() - positionRange/2f
                ),
                
                Vector3.Zero,
                new Vector3(0.8f));
        }
    }
    
    protected override void Initialize()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

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
        
        const string BackpackDir = "../../../../../../0 Assets/backpack/";
        backpack = Model.FromFile(BackpackDir,"backpack.obj",out _ , postProcessFlags: PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.CalculateTangentSpace);
        
        RandomizeBackpacks();
        
        texture = new Texture(BackpackDir+"diffuse.bmp",0);
        specular = new Texture(BackpackDir+"specular.bmp",1);
        normalMap = new Texture(BackpackDir+"normal.bmp",2);
        
        material = PresetMaterial.Silver.SetAmbient(0.01f);
        
        cube = new Model(PresetMesh.Cube);
        
        // TODO: make resizing this better, requires re-doing all the framebuffer objects though
        gBuffer = new GeometryBuffer(Window.Size)
            .AddTexture(PixelInternalFormat.Rgba16f) // position
            .AddTexture(PixelInternalFormat.Rgb16f)  // normal
            .AddTexture(PixelInternalFormat.Rgba8)   // albedo & specular
            .AddTexture(PixelInternalFormat.Rgb16f)  // TBN column 1
            .AddTexture(PixelInternalFormat.Rgb16f)  // TBN column 2
            .AddTexture(PixelInternalFormat.Rgb16f)  // TBN column 3
            .Construct();
        
        
        gBufferShader = new ShaderProgram
        (
            ShaderLocation+"lightingFragment.glsl",
            numLights: NUM_LIGHTS
        );
        
        lights = new Objects.Light[NUM_LIGHTS];
        RandomizeLights();
    }
    
    protected override void Load()
    {
        player.UpdateProjection(sceneShader);

        backpack.LoadMatrix(4, backpackTransforms, 4, 4,countPerInstance:1);

        shader.Use();
        GL.UniformMatrix4(shader.DefaultProjection,false,ref player.Camera.ProjMatrix);

        cube.UpdateTransform(shader,new Vector3(10f,10f,10f),Vector3.Zero,0.2f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);

        sceneShader.EnableGammaCorrection();

        texture.Use();

        sceneShader.Use().UniformMaterial("material",material,texture,specular).UniformTexture("normalMap",normalMap);
        gBufferShader.Use().UniformMaterial("material", material, texture, specular).UniformLightArray("lights", lights);

        gBuffer.UniformTextures((int)gBufferShader, new[] { "gPosition", "gNormal", "gAlbedoSpec", "tbnColumn0", "tbnColumn1", "tbnColumn2" });
    }

    protected override void Resize(ResizeEventArgs newWin)
    {
        player.Camera.Resize(sceneShader, newWin.Size);
            
        shader.Use();
        GL.UniformMatrix4(shader.DefaultProjection,false,ref player.Camera.ProjMatrix);
        
        gBuffer.Delete();
        gBuffer = new GeometryBuffer(newWin.Size)
            .AddTexture(PixelInternalFormat.Rgba16f) // position
            .AddTexture(PixelInternalFormat.Rgb16f)  // normal
            .AddTexture(PixelInternalFormat.Rgba8)   // albedo & specular
            .AddTexture(PixelInternalFormat.Rgb16f)  // TBN column 1
            .AddTexture(PixelInternalFormat.Rgb16f)  // TBN column 2
            .AddTexture(PixelInternalFormat.Rgb16f)  // TBN column 3
            .Construct();
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(sceneShader, args, Window.KeyboardState, GetRelativeMouse()*2f);
        shader.Use();
        GL.UniformMatrix4(shader.DefaultView,false,ref player.Camera.ViewMatrix);

        gBufferShader.Uniform3("cameraPos", player.Position);
    }

    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState k)
    {
        if (k.IsKeyPressed(Keys.Enter))
        {
            RandomizeLights();
            gBufferShader.UniformLightArray("lights", lights);
        }

        if (k.IsKeyPressed(Keys.Backspace))
        {
            RandomizeBackpacks();
            backpack.LoadMatrix(4, backpackTransforms, 4, 4,countPerInstance:1);
        }

        if (k.IsKeyDown(Keys.Right)) rotation+=Vector3.UnitY*(float)args.Time;
        if (k.IsKeyDown(Keys.Left))  rotation-=Vector3.UnitY*(float)args.Time;
        if (k.IsKeyDown(Keys.Up))    rotation+=Vector3.UnitX*(float)args.Time;
        if (k.IsKeyDown(Keys.Down))  rotation-=Vector3.UnitX*(float)args.Time;
        
        backpack.UpdateTransform(sceneShader, Vector3.Zero, rotation, new Vector3(0.8f));
    }


    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        #region Geometry Render
        gBuffer.WriteMode();
        
            sceneShader.Use();
            gBuffer.SetDrawBuffers();
            
            // since we're also using texture channels 0 and 1 for the framebuffer
            // we need to bind our normal textures back to channels 0 and 1 with this
            texture.Use();
            specular.Use();
            normalMap.Use();
            
            GL.ClearColor(0f,0f,0f,0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            backpack.Draw(sceneShader,instanceCount: NUM_BACKPACKS);
            
            GL.ClearColor(0.01f, 0.01f, 0.01f, 1.0f);
            
        
        gBuffer.ReadMode();
        #endregion

        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        #region Colour Render
        // draw gBuffer to screen
        gBufferShader.Use();
        gBuffer.UseTexture();
        PostProcessing.Draw();

        // use depth of gBuffer and draw other objects
        gBuffer.BlitDepth(Window.Size,true);
        
        
        shader.Use();

        for (int i = 0; i < lights.Length; i++)
        {
            shader.Uniform3("colour", lights[i].Diffuse);
            cube.UpdateTransform(shader,lights[i].Position,Vector3.Zero,0.2f);
            cube.Draw();
        }
        #endregion

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