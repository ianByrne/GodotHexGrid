using Godot;
using IanByrne.HexTiles;

public class Demo3D : Spatial
{
    private const int DEFAULT_RADIUS = 3;

    private CameraGimbal _camera;
    private CSGPolygon _highlightedCell;
    private Label _mouseCoords;
    private Label _hexCoords;
    private PackedScene _cellScene;
    private Button _modeButton;
    private LineEdit _scaleLineEdit;

    [Export]
    public HexGrid HexGrid;

    public override void _Ready()
    {
        base._Ready();
        
        _camera = GetNode<CameraGimbal>("../MainCamera");
        _highlightedCell = GetNode<CSGPolygon>("HighlightedCell");
        _mouseCoords = GetNode<Label>("../HUD/Coordinates/MouseValue");
        _hexCoords = GetNode<Label>("../HUD/Coordinates/HexValue");
        _modeButton = GetNode<Button>("../HUD/Controls/ModeValue");
        _scaleLineEdit = GetNode<LineEdit>("../HUD/Controls/ScaleValue");
        _cellScene = GD.Load<PackedScene>("res://HexCell3D.tscn");

        _modeButton.Pressed = HexGrid.Mode == HexMode.FLAT;
        _modeButton.Text = HexGrid.Mode.ToString();
        _scaleLineEdit.Text = $"{HexGrid.Scale.x} {HexGrid.Scale.y}";

        DrawGrid(DEFAULT_RADIUS);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event is InputEventMouseMotion mouseMotion && !_camera.Clicking)
        {
            var spaceState = GetWorld().DirectSpaceState;
            var rayOrigin = _camera.Camera.ProjectRayOrigin(mouseMotion.Position);
            var rayEnd = _camera.Camera.ProjectRayNormal(mouseMotion.Position) * 2000;
            var intersection = spaceState.IntersectRay(rayOrigin, rayEnd, null, 0x7FFFFFFF, false, true);

            if (intersection.Count > 0)
            {
                var relativePosV3 = Transform.AffineInverse().Xform((Vector3)intersection["position"]);
                
                var relativePos = new Vector2(relativePosV3.x, relativePosV3.z);

                if (_mouseCoords != null)
                    _mouseCoords.Text = relativePos.ToString();

                if (_hexCoords != null)
                    _hexCoords.Text = HexGrid.GetPixelToHex(relativePos).CubeCoordinates.ToString();

                if (_highlightedCell != null)
                {
                    var planePos = HexGrid.GetHexToPixel(HexGrid.GetPixelToHex(relativePos));
                    _highlightedCell.Translation = new Vector3(planePos.x, 0, planePos.y);
                }
            }
        }
    }

    private void DrawGrid(int radius)
    {
        foreach (CSGPolygon cell in GetTree().GetNodesInGroup("cells"))
            cell.QueueFree();

        if (HexGrid.Mode == HexMode.POINTY)
            _highlightedCell.RotationDegrees = new Vector3(90, 30, 0);
        else
            _highlightedCell.RotationDegrees = new Vector3(90, 0, 0);

        for (int x = -radius; x <= radius; ++x)
        {
            for (int y = -radius; y <= radius; ++y)
            {
                for (int z = -radius; z <= radius; ++z)
                {
                    if (x + y + z == 0)
                        DrawCell(new HexCell(x, y, z));
                }
            }
        }
    }

    private void DrawCell(HexCell cell)
    {
        var position = HexGrid.GetHexToPixel(cell);
        var material = new SpatialMaterial();
        material.AlbedoColor = new Color(0, 255, 0);

        var node = _cellScene.Instance<CSGPolygon>();
        node.Translation = new Vector3(position.x, 0, position.y);
        node.Material = material;
        node.Scale = new Vector3(0.009f, 0.009f, 0.009f);
        node.AddToGroup("cells");

        if (HexGrid.Mode == HexMode.POINTY)
            node.RotationDegrees = new Vector3(90, 30, 0);
        else
            node.RotationDegrees = new Vector3(90, 0, 0);

        AddChild(node);
    }

    private void OnModeToggled(bool toggle)
    {
        HexGrid.Mode = toggle ? HexMode.FLAT : HexMode.POINTY;
        _modeButton.Text = HexGrid.Mode.ToString();

        DrawGrid(DEFAULT_RADIUS);
    }

    private void OnScaleTextChanged(string text)
    {
        string x = text.Split(' ')[0];
        string y = text.Split(' ')[1];

        HexGrid.Scale = new Vector2(float.Parse(x), float.Parse(y));

        DrawGrid(DEFAULT_RADIUS);
    }
}
