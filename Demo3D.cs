using Godot;
using IanByrne.HexTiles;
using System.Collections.Generic;

public class Demo3D : Spatial
{
    private CameraGimbal _camera;
    private CSGPolygon _highlightedCell;
    private Label _mouseCoords;
    private Label _cellCoords;
    private PackedScene _cellScene;
    private Button _modeButton;
    private RigidBody _player;

    [Export]
    public HexGrid HexGrid;
    [Export]
    public int GridRadius = 5;

    public override void _Ready()
    {
        base._Ready();
        
        _camera = GetNode<CameraGimbal>("../MainCamera");
        _highlightedCell = GetNode<CSGPolygon>("HighlightedCell");
        _mouseCoords = GetNode<Label>("../HUD/Coordinates/MouseValue");
        _cellCoords = GetNode<Label>("../HUD/Coordinates/CellValue");
        _modeButton = GetNode<Button>("../HUD/Controls/ModeValue");
        _player = GetNode<RigidBody>("../Player");
        _cellScene = GD.Load<PackedScene>("res://HexCell3D.tscn");

        _modeButton.Pressed = HexGrid.Mode == HexMode.FLAT;
        _modeButton.Text = HexGrid.Mode.ToString();

        DrawGrid(GridRadius);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event is InputEventMouse mouse)
        {
            var spaceState = GetWorld().DirectSpaceState;
            var rayOrigin = _camera.Camera.ProjectRayOrigin(mouse.Position);
            var rayEnd = _camera.Camera.ProjectRayNormal(mouse.Position) * 2000;
            var intersection = spaceState.IntersectRay(rayOrigin, rayEnd);

            if (intersection.Contains("position"))
            {
                var relativePosV3 = Transform.AffineInverse().Xform((Vector3)intersection["position"]);
                var relativePos = new Vector2(relativePosV3.x, relativePosV3.z);
                var cell = HexGrid.GetCellAtPixel(relativePos);

                if (@event is InputEventMouseMotion && !_camera.Clicking)
                {
                    if (_mouseCoords != null)
                        _mouseCoords.Text = relativePos.ToString();

                    if (_cellCoords != null && cell.HasValue)
                        _cellCoords.Text = cell.Value.Index.ToString() + " " + cell.Value.CubeCoordinates.ToString();

                    if (_highlightedCell != null && cell.HasValue)
                    {
                        var planePos = HexGrid.GetPixelAtCell(cell.Value);
                        _highlightedCell.Translation = new Vector3(planePos.x, 0, planePos.y);
                    }
                }

                if (@event is InputEventMouseButton mouseButton && cell.HasValue)
                {
                    if (mouseButton.ButtonIndex == (int)ButtonList.Right && mouseButton.Pressed)
                    {
                        // Toggle obstacle at selected hex
                        // Undraw old cell
                        foreach (CSGPolygon polygon in GetTree().GetNodesInGroup(cell.Value.CubeCoordinates.ToString()))
                            polygon.QueueFree();

                        var newCell = cell.Value;

                        newCell.MovementCost = newCell.MovementCost == 0 ? -1 : 0;
                        var colour = newCell.MovementCost == -1 ? new Color(255, 255, 0) : new Color(0, 255, 0);

                        // Add new cell
                        HexGrid.SetCell(newCell);
                        DrawCell(newCell, colour);
                    }

                    if (mouseButton.ButtonIndex == (int)ButtonList.Left && mouseButton.Pressed)
                    {
                        // Move player to selected hex
                        var path = HexGrid.GetAStarPath(new HexCell(), cell.Value);

                        //// Draw ring?
                        //var cells = HexGrid.GetSpiral(cell.Value, 3);

                        // Undraw old cells
                        DrawGrid(GridRadius);
                        foreach (var vec2 in path)
                        {
                            var thisCell = new HexCell(vec2);

                            foreach (CSGPolygon polygon in GetTree().GetNodesInGroup(thisCell.CubeCoordinates.ToString()))
                                polygon.QueueFree();

                            var colour = new Color(255, 0, 0);

                            // Add new cell
                            HexGrid.SetCell(thisCell);
                            DrawCell(thisCell, colour);
                        }
                    }
                }
            }
        }
    }

    private void DrawGrid(int radius)
    {
        foreach (CSGPolygon polygon in GetTree().GetNodesInGroup("cells"))
            polygon.QueueFree();

        if (HexGrid.Mode == HexMode.POINTY)
            _highlightedCell.RotationDegrees = new Vector3(90, 30, 0);
        else
            _highlightedCell.RotationDegrees = new Vector3(90, 0, 0);

        var cells = new Dictionary<Vector2, HexCell>();

        for (int x = -radius; x <= radius; ++x)
        {
            for (int y = -radius; y <= radius; ++y)
            {
                for (int z = -radius; z <= radius; ++z)
                {
                    if (x + y + z == 0)
                    {
                        var cell = new HexCell(x, y, z);
                        cells.Add(cell.AxialCoordinates, cell);
                        DrawCell(cell, new Color(0, 255, 0));
                    }
                }
            }
        }

        HexGrid.SetCells(cells);
    }

    private void DrawCell(HexCell cell, Color colour)
    {
        var position = HexGrid.GetPixelAtCell(cell);
        var material = new SpatialMaterial();
        material.AlbedoColor = colour;

        var node = _cellScene.Instance<CSGPolygon>();
        node.Translation = new Vector3(position.x, 0, position.y);
        node.Material = material;
        node.Scale = new Vector3(0.009f, 0.009f, 0.009f);
        node.AddToGroup("cells");
        node.AddToGroup(cell.CubeCoordinates.ToString());

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

        DrawGrid(GridRadius);
    }
}
