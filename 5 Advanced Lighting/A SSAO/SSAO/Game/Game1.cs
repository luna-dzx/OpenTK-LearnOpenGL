using System.Runtime.InteropServices;
using Assimp;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace SSAO.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";

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

    GeometryBuffer gBuffer;
    Vector3 rotation = Vector3.Zero;

    const int SampleNum = 64;
    const int NoiseWidth = 4;
    Vector3[] ssaoKernel;
    float[] ssaoNoise;

    int noiseTexture;

    // TODO: rework my postprocessing code - can't seem to get it to work here with so many framebuffers :(
    FrameBuffer blurBuffer;
    ShaderProgram blurShader;


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

        texture = new Texture(BackpackDir+"diffuse.bmp",0);
        specular = new Texture(BackpackDir+"specular.bmp",1);
        normalMap = new Texture(BackpackDir+"normal.bmp",2);
    
        material = PresetMaterial.Silver.SetAmbient(0.01f);
        
        cube = new Model(PresetMesh.Cube.FlipNormals());
        
    
        // TODO: make resizing this better, requires re-doing all the framebuffer objects though
        gBuffer = new GeometryBuffer(Window.Size)
            .AddTexture(PixelInternalFormat.Rgb16f)  // position
            .AddTexture(PixelInternalFormat.Rgb16f)  // normal
            .Construct();
    
    
        gBufferShader = new ShaderProgram(ShaderLocation+"lightingFragment.glsl");
        light = new Objects.Light().PointMode().SetPosition(3f,5f,6f);

        Random r = new Random();
        ssaoKernel = new Vector3[SampleNum];
        ssaoNoise = new float[NoiseWidth*NoiseWidth*3];

        // random vectors in positive Z and any X,Y direction, of length 0.0 to 1.0
        // (random positions in hemisphere of positive Z)
        for (int i = 0; i < SampleNum; i++)
        {
            // scale for favouring points closer to the centre of the hemisphere
            float scale = (float)i/64f;
            // interpolate
            scale = Maths.Lerp(0.1f, 1.0f, scale * scale);

            ssaoKernel[i] = (
                new Vector3(
                    (float)r.NextDouble() * 2f - 1f,
                    (float)r.NextDouble() * 2f - 1f,
                    (float)r.NextDouble()
                ).Normalized()
            ) * (float)r.NextDouble() * scale;
        }

        for (int i = 0; i < NoiseWidth*NoiseWidth*3; i+=3)
        {
            ssaoNoise[i] = (float)r.NextDouble() * 2f - 1f;
            ssaoNoise[i + 1] = (float)r.NextDouble() * 2f - 1f;
        }

        noiseTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D,noiseTexture);
        GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba16f,NoiseWidth,NoiseWidth,0,PixelFormat.Rgb,PixelType.Float,ssaoNoise);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapS,(int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureWrapT,(int)TextureWrapMode.Repeat);

        blurBuffer = new FrameBuffer(Window.Size,TextureTarget.Texture2D);
        blurShader = new ShaderProgram(ShaderLocation + "blurFragment.glsl");
    }

    protected override void Load()
    {
        player.UpdateProjection(sceneShader);

        shader.Use();
        GL.UniformMatrix4(shader.DefaultProjection,false,ref player.Camera.ProjMatrix);

        cube.UpdateTransform(shader,new Vector3(10f,10f,10f),Vector3.Zero,0.2f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.DepthMask(true);

        sceneShader.EnableGammaCorrection();

        texture.Use();

        sceneShader.Use().UniformMaterial("material",material,texture,specular).UniformTexture("normalMap",normalMap);
        gBufferShader.UniformVec3Array("samples", ssaoKernel);
        gBufferShader.Uniform2("noiseScale", new Vector2(Window.Size.X / 4f, Window.Size.Y / 4f));

        gBuffer.UniformTextures((int)gBufferShader, new[] { "gPosition", "gNormal", "gAlbedoSpec", "tbnColumn0", "tbnColumn1", "tbnColumn2" });
    }

    protected override void Resize(ResizeEventArgs newWin)
    {
        player.Camera.Resize(sceneShader, newWin.Size);
            
        shader.Use();
        GL.UniformMatrix4(shader.DefaultProjection,false,ref player.Camera.ProjMatrix);
        
        gBuffer.Delete();
        gBuffer = new GeometryBuffer(Window.Size)
            .AddTexture(PixelInternalFormat.Rgb16f)  // position
            .AddTexture(PixelInternalFormat.Rgb16f)  // normal
            .Construct();

        blurBuffer = new FrameBuffer(newWin.Size, TextureTarget.Texture2D);
        
        gBufferShader.Uniform2("noiseScale", new Vector2(newWin.Size.X / 4f, newWin.Size.Y / 4f));
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

            backpack.Draw(sceneShader);
       
            GL.CullFace(CullFaceMode.Front);
            cube.UpdateTransform(sceneShader,Vector3.Zero,Vector3.Zero,3f);
            cube.Draw();

            GL.CullFace(CullFaceMode.Back);
            
            GL.ClearColor(0.01f, 0.01f, 0.01f, 1.0f);
            
        
        gBuffer.ReadMode();
        #endregion

        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        #region Colour Render

        blurBuffer.WriteMode();
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        gBufferShader.Use();
        gBufferShader.UniformMat4("proj", ref player.Camera.ProjMatrix);
        gBuffer.UseTexture();
        
        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D,noiseTexture);
        
        PostProcessing.Draw();


        blurBuffer.ReadMode();

        blurBuffer.UseTexture();
        blurShader.Use();
        PostProcessing.Draw();
        
        #endregion
        
        #region blur
        
        
        GL.CullFace(CullFaceMode.Back);
        
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