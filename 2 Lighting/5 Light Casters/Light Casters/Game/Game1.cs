using System.Runtime.InteropServices;
using Library;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Light_Casters.Game;

public class Game1 : Library.Game
{
    const string ShaderLocation = "../../../Game/Shaders/";
    ShaderProgram shader;

    FirstPersonPlayer player;
    Model cube;

    Objects.Light light;
    Objects.Material material;
        
    Texture texture;
    Texture textureSpecular;

    Matrix4[] cubeTransforms;

    private static void DebugCallback(DebugSource source,
        DebugType type,
        int id,
        DebugSeverity severity,
        int length,
        IntPtr message,
        IntPtr userParam)
    {
        string messageString = Marshal.PtrToStringAnsi(message, length);

        Console.WriteLine($"{severity} {type} | {messageString}");

        if (type == DebugType.DebugTypeError)
        {
            throw new Exception(messageString);
        }
    }
    
    private static DebugProc _debugProcCallback = DebugCallback;
    private static GCHandle _debugProcCallbackHandle;
    
    
    protected override void Load()
    {
        
        _debugProcCallbackHandle = GCHandle.Alloc(_debugProcCallback);

        GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);



        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        shader = new ShaderProgram(
            ShaderLocation + "vertex.glsl", 
            ShaderLocation + "fragment.glsl",
            true);

        player = new FirstPersonPlayer(shader.DefaultProjection, shader.DefaultView, Window.Size)
            .SetPosition(new Vector3(0, 0, 3))
            .SetDirection(new Vector3(0, 0, -1));

        texture = new Texture("../../../../../../0 Assets/container2.png",0);
        textureSpecular = new Texture("../../../../../../0 Assets/container2_specular.png",1);

        cube = new Model(PresetMesh.Cube, shader.DefaultModel);

        light = new Objects.Light()
            //    .SetAttenuation(1,0.09f,0.032f);
            .SetPosition(0,-4,0)
            .SetDirection(Vector3.UnitY)
            .SpotlightMode(MathHelper.DegreesToRadians(20f),MathHelper.DegreesToRadians(25f));

        material = PresetMaterial.Silver;

        shader
            .Uniform3("objectColour", 1.0f, 0.5f, 0.31f)
            .UniformLight("light", light)
            .UniformMaterial("material", material, texture, textureSpecular);

        Random random = new Random(1);

        float FloatRand(float lower, float upper)
        {
            float diff = upper - lower;
            return ((float)random.NextDouble() * diff) + lower;
        }

        Vector3 VecRand(float lower, float upper)
        {
            return new Vector3(FloatRand(lower, upper), FloatRand(lower, upper), FloatRand(lower, upper));
        }
        
        cubeTransforms = new Matrix4[16];
        for (int i = 0; i < 16; i++)
        {
            cubeTransforms[i] = Maths.CreateTransformation(
                VecRand(-6,6),
                VecRand(-MathF.PI,MathF.PI),
                Vector3.One
            );
        }

    }
        
    protected override void Resize(ResizeEventArgs newWin) => player.Camera.Resize(newWin.Size);

    private float angle;
    protected override void UpdateFrame(FrameEventArgs args)
    {
        player.Update(args,Window.KeyboardState,GetRelativeMouse());

        //light.Position = 4 * (Matrix3.CreateRotationZ(0.05f*angle) * Matrix3.CreateRotationY(0.5f*angle) * Vector3.UnitZ);
        //angle += (float)args.Time;


        shader.Uniform3("cameraPos", player.Camera.Position);
    }

    protected override void RenderFrame(FrameEventArgs args)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            
        shader.SetActive(ShaderType.FragmentShader, "lightShader");
        cube.Transform(light.Position, Vector3.Zero, 0.2f);
        cube.Draw();

        shader.SetActive(ShaderType.FragmentShader, "shader");
        //cube.ResetTransform();
        foreach (var transform in cubeTransforms)
        {
            cube.UpdateTransformation(transform);
            cube.Draw();
        }
        
        Window.SwapBuffers();
    }

    protected override void Unload()
    {
        GL.BindVertexArray(0);
        GL.UseProgram(0);
    
        cube.Delete();
        texture.Delete();
        textureSpecular.Delete();
        shader.Delete();
    }
    
}