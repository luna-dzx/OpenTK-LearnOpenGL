using System.ComponentModel;
using System.Diagnostics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Object_Oriented.Library;

public abstract class Game : IDisposable
{
    protected GameWindow? Window;
    
    protected Game() { }

    public void InitWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    {
        Window = new GameWindow(gameWindowSettings, nativeWindowSettings);
        SetFunctions();
    }
    
    public void Run()
    {
        if (Window == null)
        {
            Window = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
            Debug.WriteLine("No window set, defaulting to preset window");
        }
        Window.Run();
    }

    public void Dispose()
    {
        Window?.Dispose();
        GC.SuppressFinalize(this);
    }
    
    #region Functions To Override
    
    /// <summary>
    /// Called before the first frame is rendered
    /// <para>initialize -> <b>Load()</b> -> run</para>
    /// </summary>
    protected virtual void Load(){}
    
    /// <summary>
    /// Called before destroying the window
    /// <para><b>Unload()</b> -> destroy window -> end program</para>
    /// </summary>
    protected virtual void Unload(){}
    
    /// <summary>
    /// Called during the update loop after updating to render using OpenGL
    /// <para>update frame -> <b>RenderFrame()</b></para>
    /// </summary>
    /// <param name="args">contains delta time</param>
    protected virtual void RenderFrame(FrameEventArgs args){}
    
    /// <summary>
    /// Called during the update loop before rendering for calculations
    /// <para><b>UpdateFrame()</b> -> render frame</para>
    /// </summary>
    /// <param name="args">contains delta time</param>
    protected virtual void UpdateFrame(FrameEventArgs args){}
    
    /// <summary>
    /// If using multithreading, called on creation of new threads
    /// <para>Handle other threads in this function</para>
    /// </summary>
    protected virtual void RenderThreadStarted(){}

    /// <summary>
    /// Called during final execution before ending the program
    /// <para>unload -> <b>DestroyWindow()</b> -> end program</para>
    /// </summary>
    /// <param name="cancelInfo">get/set whether this event should be cancelled</param>
    protected virtual void DestroyWindow(CancelEventArgs cancelInfo) {}

    /// <summary>
    /// Called upon maximising the window
    /// </summary>
    /// <param name="args">contains whether the window is maximized</param>
    protected virtual void Maximized(MaximizedEventArgs args) {}
    
    /// <summary>
    /// Called upon minimizing the window
    /// </summary>
    /// <param name="args">contains whether the window is minimized</param>
    protected virtual void Minimized(MinimizedEventArgs args){}
    
    /// <summary>
    /// Called upon moving the window
    /// </summary>
    /// <param name="newPosition">the new window position after moving</param>
    protected virtual void Move(WindowPositionEventArgs newPosition){}
    
    /// <summary>
    /// Called when the window refreshes
    /// </summary>
    protected virtual void Refresh(){}
    
    /// <summary>
    /// Called upon resizing the window
    /// </summary>
    /// <param name="newSize">the new window size after moving</param>
    protected virtual void Resize(ResizeEventArgs newSize){}
    
    /// <summary>
    /// Called when files are dropped onto the window
    /// </summary>
    /// <param name="files">contains the file names of the files dropped onto the window</param>
    protected virtual void FileDrop(FileDropEventArgs files){}
    
    /// <summary>
    /// Called when clicking on/off the game window
    /// </summary>
    /// <param name="args">contains whether or not this window is focussed</param>
    protected virtual void FocusedChanged(FocusedChangedEventArgs args){}
    
    /// <summary>
    /// Called upon connecting a joystick to the computer
    /// </summary>
    /// <param name="args">information about the connected joystick</param>
    protected virtual void JoystickConnected(JoystickEventArgs args){}
    
    /// <summary>
    /// Activates similar to typing when holding a key
    /// <para>start -> pause -> repeat</para>
    /// </summary>
    /// <param name="keyInfo">information about which key was pressed</param>
    protected virtual void KeyDown(KeyboardKeyEventArgs keyInfo){}

    /// <summary>
    /// Called every frame for processing keyboard game inputs
    /// </summary>
    /// <param name="keyboardState">information about which keys are pressed</param>
    protected virtual void KeyboardHandling(KeyboardState keyboardState) {}

    /// <summary>
    /// Called only on the frame when a key is released
    /// </summary>
    /// <param name="args">information about which key was released</param>
    protected virtual void KeyUp(KeyboardKeyEventArgs args) {}
    
    /// <summary>
    /// Called only on the frame when a mouse button is pressed
    /// </summary>
    /// <param name="args">information about which mouse button was pressed</param>
    protected virtual void MouseButton(MouseButtonEventArgs args){}

    /// <summary>
    /// Called every frame for processing mouse game inputs
    /// </summary>
    /// <param name="mouseState">information about the mouse</param>
    protected virtual void MouseHandling(MouseState mouseState) {}
    
    /// <summary>
    /// Called only on the frame where the mouse moves from being off the screen to being on the screen
    /// </summary>
    protected virtual void MouseEnter(){}
    
    /// <summary>
    /// Called only on the frame where the mouse moves from being on the screen to being off the screen
    /// </summary>
    protected virtual void MouseLeave(){}
    
    /// <summary>
    /// Called any frame where the mouse is in a different location from the last
    /// </summary>
    /// <param name="moveInfo">the mouse's new position and what vector it moved by</param>
    protected virtual void MouseMove(MouseMoveEventArgs moveInfo){}
    
    /// <summary>
    /// Called only on the frame when the mouse is released
    /// </summary>
    /// <param name="args">information about which mouse button was released</param>
    protected virtual void MouseUp(MouseButtonEventArgs args){}
    
    /// <summary>
    /// Called only on the frame when a scroll action is inputted
    /// </summary>
    /// <param name="scroll">information about the scroll input</param>
    /// <remarks>accounts for scrolling by >1 per frame (in <b>scroll</b>)</remarks>
    protected virtual void MouseScroll(MouseWheelEventArgs scroll){}
    
    /// <summary>
    /// Called on typing event, formats the text for you
    /// </summary>
    /// <param name="text">formatted input from this frame</param>
    protected virtual void TextInput(TextInputEventArgs text){}

    /// <summary>
    /// Called every frame to add additional functions besides the standard OpenTK functions
    /// </summary>
    /// <param name="args">contains delta time</param>
    private void ExtraPerFrameFunctions(FrameEventArgs args)
    {
        KeyboardHandling(Window.KeyboardState);
        MouseHandling(Window.MouseState);
    }

    /// <summary>
    /// Adds the functions from the <b>Game</b> class to the OpenTK window
    /// </summary>
    private void SetFunctions()
    {
        Window.Load += Load;
        Window.Unload += Unload;
        Window.RenderFrame += RenderFrame;
        Window.UpdateFrame += ExtraPerFrameFunctions;
        Window.UpdateFrame += UpdateFrame;
        Window.RenderThreadStarted += RenderThreadStarted;
        Window.Closing += DestroyWindow;
        Window.Maximized += Maximized;
        Window.Minimized += Minimized;
        Window.Move += Move;
        Window.Refresh += Refresh;
        Window.Resize += Resize;
        Window.FileDrop += FileDrop;
        Window.FocusedChanged += FocusedChanged;
        Window.JoystickConnected += JoystickConnected;
        Window.KeyDown += KeyDown;
        Window.KeyUp += KeyUp;
        Window.MouseDown += MouseButton;
        Window.MouseEnter += MouseEnter;
        Window.MouseLeave += MouseLeave;
        Window.MouseMove += MouseMove;
        Window.MouseUp += MouseUp;
        Window.MouseWheel += MouseScroll;
        Window.TextInput += TextInput;
    }
    
    #endregion

}