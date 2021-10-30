using Godot;

public class Camera2DPan : Node2D
{
    [Export]
    private float _minZoom = 0.4f;
    [Export]
    private float _maxZoom = 3.0f;
    [Export]
    private float _zoomSpeed = 0.09f;

    private float _zoom = 1.0f;
    private Camera2D _camera2D;

    public bool Clicking { get; private set; } = false;

    public override void _Ready()
    {
        base._Ready();

        _camera2D = GetNode<Camera2D>("Camera2D");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        float scale = Mathf.Lerp(_camera2D.Zoom.x, _zoom, _zoomSpeed);

        _camera2D.Zoom = new Vector2(scale, scale);
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
            Translate(-mouseMotion.Relative);

        _zoom = Mathf.Clamp(_zoom, _minZoom, _maxZoom);
    }
}