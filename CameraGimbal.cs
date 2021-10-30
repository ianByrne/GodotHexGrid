using Godot;

public class CameraGimbal : Spatial
{
    [Export]
    private float _mouseSensitivity = 0.005f;
    [Export]
    private float _minZoom = 0.4f;
    [Export]
    private float _maxZoom = 3.0f;
    [Export]
    private float _zoomSpeed = 0.09f;

    private float _zoom = 1.5f;
    private Spatial _innerGimbal;

    public bool Clicking { get; private set; } = false;
    public Camera Camera { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        _innerGimbal = GetNode<Spatial>("InnerGimbal");
        Camera = GetNode<Camera>("InnerGimbal/Camera");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        float xRotation = Mathf.Clamp(_innerGimbal.Rotation.x, -1.4f, -0.01f);

        _innerGimbal.Rotation = new Vector3(xRotation, 0, 0);

        float scale = Mathf.Lerp(Scale.x, _zoom, _zoomSpeed);

        Scale = new Vector3(scale, scale, scale);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event.IsActionPressed("cam_zoom_in"))
            _zoom -= _zoomSpeed;

        if (@event.IsActionPressed("cam_zoom_out"))
            _zoom += _zoomSpeed;

        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == (int)ButtonList.Middle)
            Clicking = mouseButton.Pressed;

        if (@event is InputEventMouseMotion mouseMotion && Clicking)
        {
            if (mouseMotion.Relative.x != 0)
                RotateObjectLocal(Vector3.Up, mouseMotion.Relative.x * _mouseSensitivity);

            if (mouseMotion.Relative.y != 0)
            {
                float yRotation = Mathf.Clamp(mouseMotion.Relative.y, -30, 30);
                _innerGimbal.RotateObjectLocal(Vector3.Right, yRotation * _mouseSensitivity);
            }
        }

        _zoom = Mathf.Clamp(_zoom, _minZoom, _maxZoom);
    }
}
