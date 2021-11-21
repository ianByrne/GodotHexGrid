using Godot;
using IanByrne.HexTiles;
using System;
using System.Collections.Generic;

public class Demo2D : Node2D
{
    private Polygon2D _highlightedCell;
    private Label _mouseCoords;
    private Label _cellCoords;
    private PackedScene _cellScene;
    private Button _modeButton;

    [Export]
    public HexGrid HexGrid;
    [Export]
    public int GridRadius = 5;

    public override void _Ready()
    {
        base._Ready();
        
        _highlightedCell = GetNode<Polygon2D>("HighlightedCell");
        _mouseCoords = GetNode<Label>("../HUD/Coordinates/MouseValue");
        _cellCoords = GetNode<Label>("../HUD/Coordinates/CellValue");
        _modeButton = GetNode<Button>("../HUD/Controls/ModeValue");
        _cellScene = GD.Load<PackedScene>("res://HexCell2D.tscn");

        _modeButton.Pressed = HexGrid.Mode == HexMode.FLAT;
        _modeButton.Text = HexGrid.Mode.ToString();

        DrawGrid(GridRadius);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        var relativePos = Transform.AffineInverse().Multiply(GetGlobalMousePosition());
        var cell = HexGrid.GetCellAtPixel(relativePos);

        if (@event is InputEventMouseMotion)
        {
            if (_mouseCoords != null)
                _mouseCoords.Text = relativePos.ToString();

            if (_cellCoords != null && cell.HasValue)
                _cellCoords.Text = cell.Value.CubeCoordinates.ToString();

            if (_highlightedCell != null && cell.HasValue)
                _highlightedCell.Position = HexGrid.GetPixelAtCell(cell.Value);
        }

        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == (int)ButtonList.Right && mouseButton.Pressed)
        {
            // Toggle obstacle at selected hex
            if (cell.HasValue)
            {
                // Undraw old cell
                foreach (Polygon2D polygon in GetTree().GetNodesInGroup(cell.Value.CubeCoordinates.ToString()))
                    polygon.QueueFree();

                var newCell = cell.Value;

                newCell.MovementCost = newCell.MovementCost == 0 ? -1 : 0;
                var colour = newCell.MovementCost == -1 ? new Color(255, 255, 0) : new Color(0, 255, 0);

                // Add new cell
                HexGrid.SetCell(newCell);
                DrawCell(newCell, colour);
            }
        }
    }

    private void DrawGrid(int radius)
    {
        foreach (Polygon2D polygon in GetTree().GetNodesInGroup("cells"))
            polygon.QueueFree();

        if (HexGrid.Mode == HexMode.POINTY)
            _highlightedCell.RotationDegrees = 30;
        else
            _highlightedCell.RotationDegrees = 0;

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
        var node = _cellScene.Instance<Polygon2D>();
        node.Position = HexGrid.GetPixelAtCell(cell);
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

        DrawGrid(GridRadius);
    }
}
