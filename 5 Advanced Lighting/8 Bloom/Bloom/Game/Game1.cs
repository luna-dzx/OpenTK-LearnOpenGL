using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bloom.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    const string LibraryShaderLocation = "../../../Library/Shaders/";

    ShaderProgram sceneShader;
    ShaderProgram HdrShader;

    FirstPersonPlayer player;
    Model backpack;
    Model cube;
    Model quad;

    Objects.Light light;
    Objects.Material material;

    Texture texture;
    Texture normalMap;
    Texture specular;
    
    
    
    bool bloomEnabled;

    float exposure = 1f;

    private Vector3 rotation = Vector3.Zero;

    private DrawBuffersEnum[] colourAttachments;
    private DrawBuffersEnum[] brightColourAttachment;

    private PostProcessing postProcessor;
    
    protected override void Load()
    {
        GL.ClearColor(0.01f, 0.01f, 0.01f, 1.0f);
        
        colourAttachments = new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 };
        brightColourAttachment = new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment1 };

        sceneShader = new ShaderProgram()
            .LoadShader(ShaderLocation + "vertex.glsl", ShaderType.VertexShader)
            .LoadShader(ShaderLocation + "fragment.glsl", ShaderType.FragmentShader)
            .Compile()
            .EnableAutoProjection();
        
        player = new FirstPersonPlayer(Window.Size)
            .SetPosition(new Vector3(0,0,6))
            .SetDirection(new Vector3(0, 0, 1));
        player.UpdateProjection(sceneShader);


        const string BackpackDir = "../../../../../../0 Assets/backpack/";
        backpack = Model.FromFile(BackpackDir,"backpack.obj",out var textures,
            Array.Empty<TextureType>(),
            PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.CalculateTangentSpace);

        quad = new Model(PresetMesh.Square);
        
        
        texture = new Texture(BackpackDir+"diffuse.jpg",0);
        specular = new Texture(BackpackDir+"specular.jpg",1);
        normalMap = new Texture(BackpackDir+"normal.png",2);
        
        light = new Objects.Light().PointMode().SetPosition(new Vector3(-2f,2f,5f))
            .SetAmbient(0.01f).SetSpecular(Vector3.One*12f).SetDiffuse(Vector3.One*12f);
        material = PresetMaterial.Silver.SetAmbient(0.01f);
        
        
        cube = new Model(PresetMesh.Cube)
            .UpdateTransform(sceneShader,light.Position,Vector3.Zero,0.2f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);

        sceneShader.EnableGammaCorrection();

        texture.Use();
        
        
        HdrShader = new ShaderProgram(
            LibraryShaderLocation + "PostProcessing/vertex.glsl",
            ShaderLocation + "PostProcess/fragment.glsl"
        );
        
        postProcessor = new PostProcessing(LibraryShaderLocation, PostProcessing.PostProcessShader.GaussianBlur, Window.Size, PixelInternalFormat.Rgba16f, colourAttachments);
        postProcessor.UniformTextures(HdrShader, new []{"sampler", "brightSample"});

        
        sceneShader.UniformMaterial("material",material,texture,specular)
            .UniformLight("light",light)
            .UniformTexture("normalMap",normalMap);

        sceneShader.Use();


        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(sceneShader,newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(sceneShader, args, Window.KeyboardState, GetRelativeMouse()*3f);
        sceneShader.Uniform3("cameraPos", player.Position);
    }

    protected override void MouseHandling(FrameEventArgs args, MouseState mouseState)
    {
        exposure += mouseState.ScrollDelta.Y * (float)args.Time;
        HdrShader.Uniform1("exposure", exposure);
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

        postProcessor.StartSceneRender(colourAttachments);
        #region render scene

        GL.DepthMask(true);
        sceneShader.Use();
        
        // since we're also using texture channels 0 and 1 for the framebuffer
        // we need to bind our normal textures back to channels 0 and 1 with this
        texture.Use();
        specular.Use();
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        sceneShader.SetActive(ShaderType.FragmentShader, "scene");
        backpack.Draw(sceneShader,rotation:rotation,scale:5f);

        sceneShader.SetActive(ShaderType.FragmentShader, "light");
        cube.Draw(sceneShader);

        #endregion
        postProcessor.EndSceneRender();

        if (bloomEnabled) { postProcessor.RenderEffect(PostProcessing.PostProcessShader.GaussianBlur, brightColourAttachment); } // blur the bright component 

        
        #region render to screen with HDR
        
        // combine scene with blurred bright component using HDR
        
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        HdrShader.Use();
        postProcessor.DrawFbo();
        
        #endregion

        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        backpack.Delete();
        cube.Delete();
        quad.Delete();
        
        
        postProcessor.Delete();
        
        HdrShader.Delete();
        sceneShader.Delete();
    }
}