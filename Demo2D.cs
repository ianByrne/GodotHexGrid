using Godot;
using IanByrne.HexTiles;
using System;

public class Demo2D : Node2D
{
    private const HexMode DEFAULT_MODE = HexMode.FLAT;
    private const int DEFAULT_RADIUS = 5;
    private static readonly Vector2 DEFAULT_SCALE = new Vector2(50, 50);
    
    private HexGrid _hexGrid;
    private Polygon2D _highlightedCell;
    private Label _mouseCoords;
    private Label _hexCoords;
    private Label _neHexCoords;
    private PackedScene _cellScene;
    private Button _modeButton;
    private LineEdit _scaleLineEdit;

    public override void _Ready()
    {
        base._Ready();

        _hexGrid = new HexGrid(DEFAULT_MODE, DEFAULT_SCALE);
        _highlightedCell = GetNode<Polygon2D>("HighlightedCell");
        _mouseCoords = GetNode<Label>("../HUD/Coordinates/MouseValue");
        _hexCoords = GetNode<Label>("../HUD/Coordinates/HexValue");
        _neHexCoords = GetNode<Label>("../HUD/Coordinates/NeighbourValue");
        _modeButton = GetNode<Button>("../HUD/Controls/ModeValue");
        _scaleLineEdit = GetNode<LineEdit>("../HUD/Controls/ScaleValue");
        _cellScene = GD.Load<PackedScene>("res://HexCell2D.tscn");

        _modeButton.Pressed = _hexGrid.Mode == HexMode.FLAT;
        _modeButton.Text = _hexGrid.Mode.ToString();
        _scaleLineEdit.Text = $"{_hexGrid.Scale.x} {_hexGrid.Scale.y}";

        DrawGrid(DEFAULT_RADIUS);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event is InputEventMouseMotion)
        {
            var relativePos = Transform.AffineInverse().Multiply(GetGlobalMousePosition());
            var hex = _hexGrid.GetPixelToHex(relativePos);

            if (_mouseCoords != null)
                _mouseCoords.Text = relativePos.ToString();

            if (_hexCoords != null)
                _hexCoords.Text = hex.CubeCoordinates.ToString();

            if (_neHexCoords != null)
                _neHexCoords.Text = _hexGrid.GetNeighbour(hex, Direction.NE)?.CubeCoordinates.ToString();

            if (_highlightedCell != null)
                _highlightedCell.Position = _hexGrid.GetHexToPixel(hex);
        }

        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == (int)ButtonList.Right)
        {
            // Toggle obstacle at selected hex
            var relativePos = Transform.AffineInverse().Multiply(GetGlobalMousePosition());
            var hex = _hexGrid.GetPixelToHex(relativePos);
            hex.MovementCost = hex.MovementCost == 0 ? -1 : 0;
            DrawCell(hex, new Color(255, 255, 0));
        }
    }

    private void DrawGrid(int radius)
    {
        foreach (Polygon2D cell in GetTree().GetNodesInGroup("cells"))
            cell.QueueFree();

        if (_hexGrid.Mode == HexMode.POINTY)
            _highlightedCell.RotationDegrees = 30;
        else
            _highlightedCell.RotationDegrees = 0;

        var random = new Random();

        for (int x = -radius; x <= radius; ++x)
        {
            for (int y = -radius; y <= radius; ++y)
            {
                for (int z = -radius; z <= radius; ++z)
                {
                    if (x + y + z == 0)
                        DrawCell(new HexCell(x, y, z), new Color(0, 255, 0));
                }
            }
        }
    }

    private void DrawCell(HexCell cell, Color colour)
    {
        var node = _cellScene.Instance<Polygon2D>();
        node.Position = _hexGrid.GetHexToPixel(cell);
        node.ZIndex = -1;
        node.Color = colour;
        node.Scale = new Vector2(0.9f, 0.9f);
        node.AddToGroup("cells");

        if (_hexGrid.Mode == HexMode.POINTY)
            node.RotationDegrees = 30;
        else
            node.RotationDegrees = 0;

        AddChild(node);
    }

    private void OnModeToggled(bool toggle)
    {
        _hexGrid.Mode = toggle ? HexMode.FLAT : HexMode.POINTY;
        _modeButton.Text = _hexGrid.Mode.ToString();

        DrawGrid(DEFAULT_RADIUS);
    }

    private void OnScaleTextChanged(string text)
    {
        string x = text.Split(' ')[0];
        string y = text.Split(' ')[1];

        _hexGrid.Scale = new Vector2(float.Parse(x), float.Parse(y));

        DrawGrid(DEFAULT_RADIUS);
    }
}
