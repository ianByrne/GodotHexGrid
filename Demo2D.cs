using Godot;
using IanByrne.HexTiles;
using System;

public class Demo2D : Node2D
{
    private const int DEFAULT_RADIUS = 5;
    
    private Polygon2D _highlightedCell;
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
        
        _highlightedCell = GetNode<Polygon2D>("HighlightedCell");
        _mouseCoords = GetNode<Label>("../HUD/Coordinates/MouseValue");
        _hexCoords = GetNode<Label>("../HUD/Coordinates/HexValue");
        _modeButton = GetNode<Button>("../HUD/Controls/ModeValue");
        _scaleLineEdit = GetNode<LineEdit>("../HUD/Controls/ScaleValue");
        _cellScene = GD.Load<PackedScene>("res://HexCell2D.tscn");

        _modeButton.Pressed = HexGrid.Mode == HexMode.FLAT;
        _modeButton.Text = HexGrid.Mode.ToString();
        _scaleLineEdit.Text = $"{HexGrid.Scale.x} {HexGrid.Scale.y}";

        DrawGrid(DEFAULT_RADIUS);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event is InputEventMouseMotion)
        {
            var relativePos = Transform.AffineInverse().Multiply(GetGlobalMousePosition());
            var hex = HexGrid.GetPixelToHex(relativePos);

            if (_mouseCoords != null)
                _mouseCoords.Text = relativePos.ToString();

            if (_hexCoords != null)
                _hexCoords.Text = hex.CubeCoordinates.ToString();

            if (_highlightedCell != null)
                _highlightedCell.Position = HexGrid.GetHexToPixel(hex);
        }

        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == (int)ButtonList.Right && mouseButton.Pressed)
        {
            // Toggle obstacle at selected hex
            var relativePos = Transform.AffineInverse().Multiply(GetGlobalMousePosition());
            var cell = HexGrid.GetPixelToHex(relativePos);

            // Remove old cell
            foreach (Polygon2D hex in GetTree().GetNodesInGroup(cell.CubeCoordinates.ToString()))
                hex.QueueFree();

            cell.MovementCost = cell.MovementCost == 0 ? -1 : 0;
            var colour = cell.MovementCost == -1 ? new Color(255, 255, 0) : new Color(0, 255, 0);

            // Add new cell
            DrawCell(cell, colour);
        }
    }

    private void DrawGrid(int radius)
    {
        foreach (Polygon2D cell in GetTree().GetNodesInGroup("cells"))
            cell.QueueFree();

        if (HexGrid.Mode == HexMode.POINTY)
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
        node.Position = HexGrid.GetHexToPixel(cell);
        node.ZIndex = -1;
        node.Color = colour;
        node.Scale = new Vector2(0.9f, 0.9f);
        node.AddToGroup("cells");
        node.AddToGroup(cell.CubeCoordinates.ToString());

        if (HexGrid.Mode == HexMode.POINTY)
            node.RotationDegrees = 30;
        else
            node.RotationDegrees = 0;

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
