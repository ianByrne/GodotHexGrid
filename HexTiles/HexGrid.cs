using Godot;
using System;

namespace IanByrne.HexTiles
{
    public class HexGrid
    {
        private static readonly Vector3[] DIRECTIONS_FLAT = new Vector3[]
        {
            new Vector3(0, 1, -1),  // N
            new Vector3(1, 0, -1),  // NE
            Vector3.Inf,            // E (there's no eastern hex)
            new Vector3(1, -1, 0),  // SE
            new Vector3(0, -1, 1),  // S
            new Vector3(-1, 0, 1),  // SW
            Vector3.Inf,            // W (there's no western hex)
            new Vector3(-1, 1, 0)   // NW
        };

        private static readonly Vector3[] DIRECTIONS_POINTY = new Vector3[]
        {
            Vector3.Inf,            // N (there's no northern hex)
            new Vector3(1, 0, -1),  // NE
            new Vector3(1, -1, 0),  // E
            new Vector3(0, -1, 1),  // SE
            Vector3.Inf,            // S (there's no southern hex)
            new Vector3(-1, 0, 1),  // SW
            new Vector3(-1, 1, 0),  // W
            new Vector3(0, 1, -1)   // NW
        };

        private static readonly Vector2 DEFAULT_SIZE = new Vector2(1, (float)Math.Sqrt(3) / 2);

        private readonly Vector3[] DIRECTIONS;

        private Vector2 _scale;
        private Transform2D _transform;
        private Transform2D _inverseTransform;

        public HexGrid(HexMode mode, Vector2 scale)
        {
            Scale = scale;

            DIRECTIONS = mode == HexMode.FLAT ? DIRECTIONS_FLAT : DIRECTIONS_POINTY;
        }

        public HexGrid(HexMode mode) : this(mode, Vector2.One) { }

        public Vector2 Scale
        {
            get
            {
                return _scale;
            }

            set
            {
                _scale = value;

                var size = DEFAULT_SIZE * _scale;

                _transform = new Transform2D(
                    new Vector2(size.x * 3 / 4, -size.y / 2),
                    new Vector2(0, -size.y),
                    Vector2.Zero);

                _inverseTransform = _transform.AffineInverse();
            }
        }

        public Vector2 GetHexCenter(HexCell cell)
        {
            return _transform.Multiply(cell.AxialCoordinates);
        }

        public HexCell GetHexAt(Vector2 coordinates)
        {
            var hexCell = new HexCell(_inverseTransform.Multiply(coordinates));

            return hexCell;
        }

        public HexCell GetNeighbour(HexCell cell, Direction direction)
        {
            var directionVec3 = DIRECTIONS[(int)direction];

            if (directionVec3 == Vector3.Inf)
                throw new InvalidOperationException($"Invalid direction {direction} for mode");

            return new HexCell(cell.CubeCoordinates + directionVec3);
        }

        public int GetDistance(HexCell from, Vector3 target)
        {
            return (int)(
                Math.Abs(from.CubeCoordinates.x - target.x) +
                Math.Abs(from.CubeCoordinates.y - target.y) +
                Math.Abs(from.CubeCoordinates.z - target.z)) / 2;
        }

        public int GetDistance(HexCell from, HexCell target)
        {
            return GetDistance(from, target.CubeCoordinates);
        }

        public int GetDistance(HexCell from, Vector2 target)
        {
            return GetDistance(from, target.ToCubeCoordinates());
        }
    }
}