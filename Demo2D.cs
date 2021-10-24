using Godot;
using IanByrne.HexTiles;

public class Demo2D : Node2D
{
    private HexGrid _hexGrid;
    private Polygon2D _highlightedCell;
    private Label _screenCoords;
    private Label _hexCoords;
    private PackedScene _cellScene;

    public override void _Ready()
    {
        _hexGrid = new HexGrid(new Vector2(50, 50));
        _highlightedCell = GetNode<Polygon2D>("HighlightedCell");
        _screenCoords = GetNode<Label>("../Coordinates/ScreenValue");
        _hexCoords = GetNode<Label>("../Coordinates/HexValue");
        _cellScene = GD.Load<PackedScene>("res://HexCell.tscn");

        DrawGrid(3);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            var relativePos = Transform.AffineInverse().Multiply(mouseMotion.Position);

            if (_screenCoords != null)
                _screenCoords.Text = relativePos.ToString();

            if (_hexCoords != null)
                _hexCoords.Text = _hexGrid.GetHexAt(relativePos).AxialCoordinates.ToString();

            if (_highlightedCell != null)
                _highlightedCell.Position = _hexGrid.GetHexCenter(_hexGrid.GetHexAt(relativePos));
        }
    }

    private void DrawGrid(int radius)
    {
        if (_hexGrid.Mode == HexMode.POINTY)
            _highlightedCell.Rotate(0.5235988f);
        else
            _highlightedCell.Rotate(0f);

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
        var node = _cellScene.Instance<Polygon2D>();
        node.Position = _hexGrid.GetHexCenter(cell);
        node.ZIndex = -1;
        node.Color = new Color(0, 255, 0);
        node.Scale = new Vector2(0.9f, 0.9f);

        if (_hexGrid.Mode == HexMode.POINTY)
            node.Rotate(0.5235988f);
        else
            node.Rotate(0f);

        AddChild(node);
    }
}
