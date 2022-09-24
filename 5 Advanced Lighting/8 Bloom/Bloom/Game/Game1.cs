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

    ShaderProgram shader;
    ShaderProgram frameBufferShader;
    ShaderProgram gaussianShader;
    ShaderProgram blitShader;

    FirstPersonPlayer player;
    Model backpack;
    Model cube;
    Model quad;

    Objects.Light light;
    Objects.Material material;

    Texture texture;
    Texture normalMap;
    Texture specular;
    
    //FrameBuffer frameBuffer;
    //FrameBuffer[] blurFrameBuffer;

    FrameBuffer postProcessFbo;
    FrameBuffer storageFbo;
    
    
    bool highDynamicRange;

    float exposure = 1f;

    private Vector3 rotation = Vector3.Zero; //  new Vector3(0f,MathHelper.DegreesToRadians(59f),0f);

    private DrawBuffersEnum[] colourAttachments;
    private DrawBuffersEnum[] brightColourAttachment;
    
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
            .UpdateTransform(shader,light.Position,Vector3.Zero,0.2f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);

        shader.EnableGammaCorrection();

        texture.Use();
        
        
        //frameBuffer = new FrameBuffer(Window.Size,internalFormat: PixelInternalFormat.Rgba16f,numColourAttachments:2);
        postProcessFbo = new FrameBuffer(Window.Size,internalFormat: PixelInternalFormat.Rgba16f,numColourAttachments:2);
        storageFbo = new FrameBuffer(Window.Size,internalFormat: PixelInternalFormat.Rgba16f,numColourAttachments:2);
        
        frameBufferShader = new ShaderProgram(
            ShaderLocation + "PostProcess/vertex.glsl",
            ShaderLocation + "PostProcess/fragment.glsl"
        );
        
        gaussianShader = new ShaderProgram(
            ShaderLocation + "PostProcess/vertex.glsl",
            ShaderLocation + "PostProcess/gaussianFragment.glsl"
        );      
        
        blitShader = new ShaderProgram(
            ShaderLocation + "PostProcess/vertex.glsl",
            ShaderLocation + "PostProcess/blitFragment.glsl"
        );

        //blurFrameBuffer = new FrameBuffer[2];
        //blurFrameBuffer[0] = new FrameBuffer(Window.Size,internalFormat: PixelInternalFormat.Rgba16f,numColourAttachments:2);
        //blurFrameBuffer[1] = new FrameBuffer(Window.Size,internalFormat: PixelInternalFormat.Rgba16f,numColourAttachments:2);
        
        postProcessFbo.UniformTexture((int)gaussianShader, "texture0", 1);

        postProcessFbo.UniformTexture((int)frameBufferShader, "sampler", 0);
        postProcessFbo.UniformTexture((int)frameBufferShader, "brightSample", 1);
        
        postProcessFbo.UniformTexture((int)blitShader, "sampler", 0);
        postProcessFbo.UniformTexture((int)blitShader, "brightSample", 1);


        shader.UniformMaterial("material",material,texture,specular)
            .UniformLight("light",light)
            .UniformTexture("normalMap",normalMap);
        

        
        shader.Use();

        colourAttachments = new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 };
        brightColourAttachment = new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment1 };


        // attach player functions to window
        Window.Resize += newWin => player.Camera.Resize(shader,newWin.Size);
    }

    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(shader, args, Window.KeyboardState, GetRelativeMouse()*3f);
        shader.Uniform3("cameraPos", player.Position);
        //rotation += Vector3.UnitX * (float)(args.Time * 2f);
    }

    protected override void MouseHandling(FrameEventArgs args, MouseState mouseState)
    {
        exposure += mouseState.ScrollDelta.Y * (float)args.Time;
        frameBufferShader.Uniform1("exposure", exposure);
    }

    protected override void KeyboardHandling(FrameEventArgs args, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyPressed(Keys.Enter))
        {
            highDynamicRange = !highDynamicRange;
            frameBufferShader.Uniform1("highDynamicRange",highDynamicRange?1:0);
        }
        
        if (keyboardState.IsKeyDown(Keys.Right)) rotation+=Vector3.UnitY*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Left))  rotation-=Vector3.UnitY*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Up))    rotation+=Vector3.UnitX*(float)args.Time;
        if (keyboardState.IsKeyDown(Keys.Down))  rotation-=Vector3.UnitX*(float)args.Time;
        
        
    }



    void BlitFbo()
    {
        blitShader.Use();
        storageFbo.WriteMode();
        
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        postProcessFbo.UseTexture();
        PostProcessing.Draw();
        
        storageFbo.ReadMode();
    }
    

    protected override void RenderFrame(FrameEventArgs args)
    {
        
        GL.DepthMask(true);
        
        shader.Use();
        
        // since we're also using texture channels 0 and 1 for the framebuffer
        // we need to bind our normal textures back to channels 0 and 1 with this
        texture.Use();
        specular.Use();

        postProcessFbo.WriteMode();
        GL.DrawBuffers(2,colourAttachments );
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        
        shader.SetActive(ShaderType.FragmentShader, "scene");
        backpack.Draw(shader,rotation:rotation,scale:5f);

        shader.SetActive(ShaderType.FragmentShader, "light");
        cube.Draw(shader);

        
        postProcessFbo.ReadMode();
        
        GL.DepthMask(false);
        
        
        storageFbo.WriteMode();
        GL.DrawBuffers(2,colourAttachments );
        storageFbo.ReadMode();
        
        BlitFbo();

        gaussianShader.Use();

        gaussianShader.Uniform1("blurDirection", 0);
        postProcessFbo.WriteMode();
        GL.DrawBuffers(1,brightColourAttachment );
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        storageFbo.UseTexture();
        PostProcessing.Draw();
        
        postProcessFbo.ReadMode();
        
        
        BlitFbo();
        

        
        gaussianShader.Uniform1("blurDirection", 1);
        postProcessFbo.WriteMode();
        GL.DrawBuffers(1,brightColourAttachment );
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        storageFbo.UseTexture();
        PostProcessing.Draw();
        
        postProcessFbo.ReadMode();
        
        BlitFbo();
        
        

        
        
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        frameBufferShader.Use();
        frameBufferShader.SetActive(ShaderType.FragmentShader, "hdr");
        
        storageFbo.UseTexture();

        PostProcessing.Draw();


        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        backpack.Delete();
        cube.Delete();
        quad.Delete();

        frameBufferShader.Delete();
        gaussianShader.Delete();
        shader.Delete();
    }
}