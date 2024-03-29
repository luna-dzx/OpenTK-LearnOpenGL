using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Library;


public struct GlState
{
    public bool DepthTest;
    public bool DepthMask;
    public DepthFunction DepthFunc;

    public bool DoCulling;
    public CullFaceMode CullFace;

    public Color4 ClearColor;
    public ClearBufferMask ClearBuffers;


    public GlState()
    {
        DepthTest = true;
        DepthMask = true;
        DepthFunc = DepthFunction.Less;

        DoCulling = true;
        CullFace = CullFaceMode.Back;

        ClearColor = Color4.Transparent;
        ClearBuffers = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
    }
}


/// <summary>
/// A way of keeping track of the OpenGL state instead of just using the GL functions directly.
/// Also, this allows you to set the OpenGL state as if you're changing variables which feels more intuitive than calling functions.
/// </summary>
public class StateHandler
{

    private GlState _state;


    public StateHandler() : this(new GlState()) { }
    public StateHandler(GlState state) { LoadState(state); }

    public void LoadState(GlState state)
    {
        _state = state;
        DepthTest = _state.DepthTest;
        DepthMask = _state.DepthMask;
        DepthFunc = _state.DepthFunc;
        
        DoCulling = _state.DoCulling;
        CullFace = _state.CullFace;

        ClearColor = _state.ClearColor;
        ClearBuffers = _state.ClearBuffers;
    }
    
    
    
    public bool DepthTest
    {
        set
        {
            _state.DepthTest = value;
            if (value) GL.Enable(EnableCap.DepthTest);
            else GL.Disable(EnableCap.DepthTest);
        }

        get => _state.DepthTest;
    }
    
    public bool DepthMask
    {
        set
        {
            _state.DepthMask = value;
            GL.DepthMask(value);
        }

        get => _state.DepthMask;
        
    }

    public DepthFunction DepthFunc
    {
        set
        {
            _state.DepthFunc = value;
            GL.DepthFunc(value);
        }

        get => _state.DepthFunc;
    }
    
    
    public bool DoCulling
    {
        set
        {
            _state.DoCulling = value;
            if (value) GL.Enable(EnableCap.CullFace);
            else GL.Disable(EnableCap.CullFace);
        }

        get => _state.DoCulling;
    }

    public CullFaceMode CullFace
    {
        set
        {
            _state.CullFace = value;
            GL.CullFace(value);
        }
        get => _state.CullFace;
    }
    
    
    public Color4 ClearColor
    {
        set
        {
            _state.ClearColor = value;
            GL.ClearColor(_state.ClearColor);
        }

        get => _state.ClearColor;
    }    
    
    public ClearBufferMask ClearBuffers
    {
        set => _state.ClearBuffers = value;
        get => _state.ClearBuffers;
    }

    public void Clear() {GL.Clear(ClearBuffers);}
    





}