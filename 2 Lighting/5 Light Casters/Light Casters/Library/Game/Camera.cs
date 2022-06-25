using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Library;

public class Camera
{
    // projection matrix vars
    private float aspect;
    private float fov;
    private float depthNear;
    private float depthFar;

    // camera vars
    public Vector3 Position;
    public Vector3 Direction;
    private readonly Vector3 up = Vector3.UnitY;

    // OpenGL
    private int uProj;
    private int uView;

    private Matrix4 proj;
    private Matrix4 view;

    /// <summary>
    /// Create a new camera object for handling 3D projection
    /// </summary>
    /// <param name="projectionBinding">the uniform location of the projection matrix</param>
    /// <param name="viewBinding">the uniform location of the view matrix</param>
    /// <param name="aspectRatio">the screen's aspect ratio</param>
    /// <param name="fieldOfView">the camera's field of view</param>
    /// <param name="clipNear">the closest distance to render</param>
    /// <param name="clipFar">the furthest distance to render</param>
    public Camera(int projectionBinding, int viewBinding, float aspectRatio, float fieldOfView = MathHelper.PiOver3,float clipNear = 0.1f, float clipFar = 100f)
    {
        aspect = aspectRatio;
        fov = fieldOfView;
        depthNear = clipNear;
        depthFar = clipFar;
    
        uProj = projectionBinding;
        uView = viewBinding;

        UpdateProjection();
        UpdateView();
    }

    /// <summary>
    /// Create a new camera object for handling 3D projection
    /// </summary>
    /// <param name="projectionBinding">the uniform location of the projection matrix</param>
    /// <param name="viewBinding">the uniform location of the view matrix</param>
    /// <param name="windowSize">the screen's size</param>
    /// <param name="fieldOfView">the camera's field of view in radians</param>
    /// <param name="clipNear">the closest distance to render</param>
    /// <param name="clipFar">the furthest distance to render</param>
    public Camera(int projectionBinding, int viewBinding, Vector2i windowSize, float fieldOfView = MathHelper.PiOver3, float clipNear = 0.1f, float clipFar = 100f):this(projectionBinding,viewBinding,(float)windowSize.X/windowSize.Y,fieldOfView,clipNear,clipFar) { }
    

    /// <summary>
    /// Create a new perspective projection matrix and load to the uniform projection matrix binding
    /// </summary>
    public void UpdateProjection()
    {
        proj = Matrix4.CreatePerspectiveFieldOfView(fov, aspect, depthNear, depthFar);
        GL.UniformMatrix4(uProj,false,ref proj);
    }

    /// <summary>
    /// Update matrices according to a new aspect ratio
    /// </summary>
    /// <param name="newAspect">the new aspect ratio of the screen</param>
    public void Resize(float newAspect)
    {
        aspect = newAspect;
        UpdateProjection();
    }
        
    /// <summary>
    /// Update matrices according to a new screen size
    /// </summary>
    /// <param name="newSize">the new size of the screen</param>
    public void Resize(Vector2i newSize)
    {
        Resize((float)newSize.X / newSize.Y);
    }

    /// <summary>
    /// Set the camera's field of view
    /// </summary>
    /// <param name="fieldOfView">field of view in radians</param>
    public void SetFov(float fieldOfView)
    {
        fov = fieldOfView;
        UpdateProjection();
    }

    /// <summary>
    /// Change the near and far clip distances
    /// </summary>
    /// <param name="near">the closest distance to render</param>
    /// <param name="far">the furthest distance to render</param>
    public void SetDepth(float near, float far)
    {
        depthNear = near;
        depthFar = far;
        UpdateProjection();
    }

    /// <summary>
    /// Update the view matrix relative to the camera's current position
    /// </summary>
    /// <param name="flipCamera">if ture, the camera will be upside down</param>
    public void UpdateView(bool flipCamera = false)
    {
        view = Matrix4.LookAt(Position, Position + Direction, ((flipCamera)?-1:1) * up);
        GL.UniformMatrix4(uView,false,ref view);
    }

}