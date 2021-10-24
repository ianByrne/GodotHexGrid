using Godot;
using IanByrne.HexTiles;

public class Demo2D : Node2D
{
    private HexGrid _hexGrid;
    private Polygon2D _highlight;
    private Label _screenCoords;
    private Label _hexCoords;

    public override void _Ready()
    {
        _hexGrid = new HexGrid(HexMode.FLAT, new Vector2(50, 50));
        _highlight = GetNode<Polygon2D>("HighlightedCell");
        _screenCoords = GetNode<Label>("Coordinates/ScreenValue");
        _hexCoords = GetNode<Label>("Coordinates/HexValue");

        var cellScene = GD.Load<PackedScene>("res://HexCell.tscn");

        // Draw grid
        for (int x = 0; x <= 50; ++x)
        {
            for (int y = -50; y <= 0; ++y)
            {
                var position = _hexGrid.GetHexCenter(new HexCell(x, y));

                var cell = cellScene.Instance<Polygon2D>();
                cell.Position = position;
                cell.ZIndex = -1;
                cell.Color = new Color(0, 255, 0);
                cell.Scale = new Vector2(0.9f, 0.9f);
                AddChild(cell);
            }
        }
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

            if (_highlight != null)
                _highlight.Position = _hexGrid.GetHexCenter(_hexGrid.GetHexAt(relativePos));
        }
    }
}
