using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;


namespace Hello_Window;

internal static class Program
{
    
    private static GameWindow window;
    private const int FPS = 60;
    
    public static void Main(string[] args)
    {
        var gameSettings = GameWindowSettings.Default;
        gameSettings.RenderFrequency = FPS;
        gameSettings.UpdateFrequency = FPS;
            
        var uiSettings = NativeWindowSettings.Default;
        uiSettings.APIVersion = Version.Parse("4.1.0");
        uiSettings.Size = new Vector2i(800,600);
        uiSettings.Title = "LearnOpenGL";
        uiSettings.NumberOfSamples = 4;

        uiSettings.WindowState = WindowState.Normal;
        uiSettings.WindowBorder = WindowBorder.Resizable;
        uiSettings.IsEventDriven = false;
        uiSettings.StartFocused = true;
            
        window = new GameWindow(gameSettings,uiSettings);

        window.Load += OnLoad;
        window.UpdateFrame += Update;
        window.RenderFrame += Render;
        window.Unload += OnExit;
        window.KeyDown += OnKeyDown;
        
        window.Run();


    }

    private static void OnKeyDown(KeyboardKeyEventArgs state)
    {
        if (state.Key == Keys.Escape) window.Close();
    }


    private static void OnLoad()
    {
        GL.ClearColor(0.2f,0.3f,0.3f,1.0f);
    }

    private static void Update(FrameEventArgs frameEventArgs) { }

    private static void Render(FrameEventArgs frameEventArgs)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        window.SwapBuffers();
    }
    
    private static void OnExit(){}
    
    
    
}