using Godot;
using System;
using System.Collections.Generic;

namespace IanByrne.HexTiles
{
    public class HexGrid : Reference
    {
        private static readonly Vector2 DEFAULT_SIZE = new Vector2(1, (float)Math.Sqrt(3) / 2);

        private static readonly Vector3?[] DIRECTIONS_FLAT = new Vector3?[]
        {
            new Vector3(0, 1, -1),  // N
            new Vector3(1, 0, -1),  // NE
            null,                   // E (there's no eastern hex)
            new Vector3(1, -1, 0),  // SE
            new Vector3(0, -1, 1),  // S
            new Vector3(-1, 0, 1),  // SW
            null,                   // W (there's no western hex)
            new Vector3(-1, 1, 0)   // NW
        };

        private static readonly Vector3?[] DIRECTIONS_POINTY = new Vector3?[]
        {
            null,                   // N (there's no northern hex)
            new Vector3(1, 0, -1),  // NE
            new Vector3(1, -1, 0),  // E
            new Vector3(0, -1, 1),  // SE
            null,                   // S (there's no southern hex)
            new Vector3(-1, 0, 1),  // SW
            new Vector3(-1, 1, 0),  // W
            new Vector3(0, 1, -1)   // NW
        };

        private Vector3?[] DIRECTIONS;

        private Dictionary<Vector2, HexCell> _cells; // AxialCoordinates => HexCell
        private Vector2 _scale;
        private HexMode _mode;
        private Transform2D _transform;
        private Transform2D _inverseTransform;

        public HexGrid(HexMode mode, Vector2 scale)
        {
            _cells = new Dictionary<Vector2, HexCell>();
            _mode = mode;
            _scale = scale;

            SetMembers();
        }

        public HexGrid() : this(HexMode.FLAT, Vector2.One) { }

        [Export]
        public Vector2 Scale
        {
            get
            {
                return _scale;
            }

            set
            {
                _scale = value;

                SetMembers();
            }
        }

        [Export]
        public HexMode Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                _mode = value;

                SetMembers();
            }
        }

        public void SetCellAtAxialCoords(HexCell cell)
        {
            _cells[cell.AxialCoordinates] = cell;
        }

        public HexCell? GetCellAtAxialCoords(Vector2 coordinates)
        {
            if (_cells.TryGetValue(coordinates, out HexCell hexCell))
                return hexCell;
            else
                return null;
        }

        public Vector2 GetPixelAtCell(HexCell cell)
        {
            return _transform.Multiply(cell.AxialCoordinates);
        }

        public HexCell? GetCellAtPixel(Vector2 coordinates)
        {
            var cell = new HexCell(_inverseTransform.Multiply(coordinates));

            return GetCellAtAxialCoords(cell.AxialCoordinates);
        }

        public HexCell? GetNeighbourCell(HexCell homeCell, Direction direction)
        {
            var directionVec3 = DIRECTIONS[(int)direction];

            if (!directionVec3.HasValue)
                return null;

            var neighbourCell = new HexCell(homeCell.CubeCoordinates + directionVec3.Value);

            return GetCellAtAxialCoords(neighbourCell.AxialCoordinates);
        }

        public int GetDistanceFromCell(HexCell from, Vector3 target)
        {
            return (int)(
                Math.Abs(from.CubeCoordinates.x - target.x) +
                Math.Abs(from.CubeCoordinates.y - target.y) +
                Math.Abs(from.CubeCoordinates.z - target.z)) / 2;
        }

        public int GetDistanceFromCell(HexCell from, HexCell target)
        {
            return GetDistanceFromCell(from, target.CubeCoordinates);
        }

        public int GetDistanceFromCell(HexCell from, Vector2 target)
        {
            return GetDistanceFromCell(from, target.ToCubeCoordinates());
        }

        private void SetMembers()
        {
            var size = DEFAULT_SIZE * _scale;

            if (_mode == HexMode.FLAT)
            {
                DIRECTIONS = DIRECTIONS_FLAT;

                _transform = new Transform2D(
                    new Vector2(size.x * 3 / 4, size.y / 2),
                    new Vector2(0, size.y),
                    Vector2.Zero);
            }
            else
            {
                DIRECTIONS = DIRECTIONS_POINTY;

                _transform = new Transform2D(
                    new Vector2(size.y, 0),
                    new Vector2(size.y / 2, size.x * 3 / 4),
                    Vector2.Zero);
            }

            _inverseTransform = _transform.AffineInverse();
        }
    }
}