using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Library;

/// <summary>
/// Simplification of movement and camera handling - dependent on Camera class
/// </summary>
public abstract class Player
{
    public Vector3 Position;
    public Camera Camera;
    
    /// <summary>
    /// Create new player and create camera based off window size
    /// </summary>
    /// <param name="projectionBinding">the uniform location of the projection matrix</param>
    /// <param name="viewBinding">the uniform location of the view matrix</param>
    /// <param name="windowSize">the screen's size</param>
    /// <param name="fov">the camera's field of view in radians</param>
    public Player(int projectionBinding, int viewBinding, Vector2i windowSize, float fov = MathHelper.PiOver3)
    {
        Camera = new Camera(projectionBinding, viewBinding, windowSize, fov);
    }    
    
    /// <summary>
    /// Create new player and create camera based off aspect ratio
    /// </summary>
    /// <param name="projectionBinding">the uniform location of the projection matrix</param>
    /// <param name="viewBinding">the uniform location of the view matrix</param>
    /// <param name="aspectRatio">the screen's aspect ratio</param>
    /// <param name="fov">the camera's field of view in radians</param>
    public Player(int projectionBinding, int viewBinding, float aspectRatio, float fov = MathHelper.PiOver3)
    {
        Camera = new Camera(projectionBinding, viewBinding, aspectRatio, fov);
    }    
    
    /// <summary>
    /// Create a new player with a pre-existing camera
    /// </summary>
    /// <param name="camera">the camera to attach to the player</param>
    public Player(ref Camera camera)
    {
        Camera = camera;
    }
    
    /// <summary>
    /// Update the camera's view
    /// </summary>
    /// <param name="flipCamera">if ture, the camera will be upside down</param>
    /// <param name="renderingPaused">setting this to true exclusively updates the player and not the camera</param>
    protected void Update(bool flipCamera = false, bool renderingPaused = false)
    {
        if (!renderingPaused) Camera.UpdateView(flipCamera);
    }

}




public class FirstPersonPlayer : Player
{
    public Vector3 Velocity;
    public float Sensitivity;
    public float Speed;

    private Vector3 unitGravity = -Vector3.UnitY;
    public void SetGravity(Vector3 direction) => unitGravity = direction;

    /// <summary>
    /// Create first person player with [wasd + space/ctrl] controls
    /// </summary>
    /// <param name="projectionBinding">the uniform location of the projection matrix</param>
    /// <param name="viewBinding">the uniform location of the view matrix</param>
    /// <param name="windowSize">the screen's size</param>
    /// <param name="fov">the camera's field of view in radians</param>
    /// <param name="sensitivity">the mouse sensitivity</param>
    /// <param name="speed">player's speed</param>
    public FirstPersonPlayer(int projectionBinding, int viewBinding, Vector2i windowSize, float fov = MathHelper.PiOver3, float sensitivity = 1/20f, float speed = 5f)
        : base(projectionBinding,viewBinding,windowSize,fov)
    {
        Sensitivity = sensitivity;
        Speed = speed;
    }

    /// <summary>
    /// Manually set the player's position
    /// </summary>
    /// <param name="position">new player position</param>
    /// <returns>current object for ease of use</returns>
    public FirstPersonPlayer SetPosition(Vector3 position)
    {
        Position = position;
        return this;
    }
        
    /// <summary>
    /// Manually set the camera's direction
    /// </summary>
    /// <param name="direction">new camera direction</param>
    /// <returns>current object for ease of use</returns>
    public FirstPersonPlayer SetDirection(Vector3 direction)
    {
        Direction = direction;
        return this;
    }

    private readonly Matrix3 rightTransform = Matrix3.CreateRotationY(MathHelper.PiOver2);

    private Vector2 lastMousePos;
    private float yaw;
    private float pitch;

    private bool capPitch = true;
    
    /// <summary>
    /// Move and rotate the player and camera, as well as updating the camera's view
    /// </summary>
    /// <param name="args">args from the window's update function</param>
    /// <param name="keyboardState">the keyboard state to check inputs from</param>
    /// <param name="relativeMousePos">the relative mouse pos from the last call of SetMouseOrigin()</param>
    public void Update(FrameEventArgs args, KeyboardState keyboardState, Vector2 relativeMousePos)
    {
        var input = Input.DirectionWASD(keyboardState) * Speed * (float)args.Time;
        yaw += (relativeMousePos.X - lastMousePos.X) * Sensitivity;
        pitch += (relativeMousePos.Y - lastMousePos.Y) * Sensitivity;

        yaw %= 360;
        pitch %= 360;

        // 90 degrees gives gimbal locking so lock to 89
        if (capPitch) pitch = Math.Clamp(pitch, -89f, 89f);

        Camera.Direction = Matrix3.CreateRotationY(MathHelper.DegreesToRadians(yaw)) * Matrix3.CreateRotationX(MathHelper.DegreesToRadians(pitch)) * -Vector3.UnitZ;
        
        Vector3 up = ((keyboardState.IsKeyDown(Keys.Space) ?1:0) - (keyboardState.IsKeyDown(Keys.LeftControl) ?1:0)) * Speed * (float)args.Time * Vector3.UnitY;
        
        Vector3 directionFlat = Camera.Direction;
        directionFlat.Y = 0;
        directionFlat.Normalize();

        Velocity = input.Z * directionFlat + input.X * (rightTransform * directionFlat) + up;
        
        Position += Velocity;
        Camera.Position = Position;

        base.Update((Math.Abs(pitch)+90)%360 >= 180);
        lastMousePos = relativeMousePos;
    }

    /// <summary>
    /// Direction handled in the player's camera
    /// </summary>
    public Vector3 Direction
    {
        get => Camera.Direction;
        set => Camera.Direction=value;
    }
    
}