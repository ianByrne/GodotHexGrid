using Godot;
using System;
using System.Collections.Generic;

namespace IanByrne.HexTiles
{
    public class HexGrid : Reference
    {
        private static readonly Vector2 DEFAULT_SIZE = new Vector2(1, (float)Math.Sqrt(3) / 2);

        private static readonly Vector3[] DIRECTIONS_FLAT = new Vector3[]
        {
            new Vector3(0, 1, -1),  // N
            new Vector3(1, 0, -1),  // NE
            new Vector3(1, -1, 0),  // SE
            new Vector3(0, -1, 1),  // S
            new Vector3(-1, 0, 1),  // SW
            new Vector3(-1, 1, 0)   // NW
        };

        private static readonly Vector3[] DIRECTIONS_POINTY = new Vector3[]
        {
            new Vector3(1, 0, -1),  // NE
            new Vector3(1, -1, 0),  // E
            new Vector3(0, -1, 1),  // SE
            new Vector3(-1, 0, 1),  // SW
            new Vector3(-1, 1, 0),  // W
            new Vector3(0, 1, -1)   // NW
        };

        private Vector3[] DIRECTIONS;

        private IDictionary<Vector2, HexCell> _cells; // AxialCoordinates => HexCell
        private Vector2 _scale;
        private HexMode _mode;
        private Transform2D _transform;
        private Transform2D _inverseTransform;
        private AStar2D _aStar;

        public HexGrid(HexMode mode, Vector2 scale)
        {
            _cells = new Dictionary<Vector2, HexCell>();
            _mode = mode;
            _scale = scale;
            _aStar = new AStar2D();

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

        public void SetCells(IDictionary<Vector2, HexCell> cells)
        {
            var newCells = new Dictionary<Vector2, HexCell>();

            foreach (var cell in cells)
            {
                var newCell = cell.Value;
                CalculateIndex(ref newCell);
                newCells.Add(newCell.AxialCoordinates, newCell);
            }

            _cells = newCells;
        }

        public void SetCell(HexCell cell)
        {
            if (_cells.ContainsKey(cell.AxialCoordinates))
            {
                CalculateIndex(ref cell);
                _cells[cell.AxialCoordinates] = cell;
            }
        }

        public HexCell? GetCell(Vector2 axialCoordinates)
        {
            if (_cells.TryGetValue(axialCoordinates, out HexCell hexCell))
                return hexCell;
            else
                return null;
        }

        public HexCell? GetCell(Vector3 cubeCoordinates) => GetCell(cubeCoordinates.ToAxialCoordinates());

        public Vector2 GetPixelAtCell(HexCell cell)
        {
            return _transform.Multiply(cell.AxialCoordinates);
        }

        public HexCell? GetCellAtPixel(Vector2 coordinates)
        {
            return GetCell(_inverseTransform.Multiply(coordinates).RoundAxialCoordinates());
        }

        public Vector3 GetNeighbourCubeCoordinate(Vector3 from, int direction, int offset = 1)
        {
            var directionVec3 = DIRECTIONS[direction] * offset;

            return (from + directionVec3).RoundCubeCoordinates();
        }

        public HexCell? GetNeighbourCell(HexCell from, int direction, int offset = 1)
        {
            return GetCell(GetNeighbourCubeCoordinate(from.CubeCoordinates, direction, offset));
        }

        public Vector3[] GetAllNeighbourCubeCoordinates(Vector3 from)
        {
            var neighbours = new List<Vector3>();

            foreach (var direction in DIRECTIONS)
            {
                neighbours.Add((from + direction).RoundCubeCoordinates());
            }

            return neighbours.ToArray();
        }

        public HexCell[] GetAllNeighbourCells(HexCell from)
        {
            var neighbours = new List<HexCell>();

            foreach (var direction in DIRECTIONS)
            {
                var cell = GetCell((from.CubeCoordinates + direction).RoundCubeCoordinates());

                if (cell.HasValue)
                    neighbours.Add(cell.Value);
            }

            return neighbours.ToArray();
        }

        public int GetDistance(Vector3 from, Vector3 target)
        {
            return (int)(
                Math.Abs(from.x - target.x) +
                Math.Abs(from.y - target.y) +
                Math.Abs(from.z - target.z)) / 2;
        }

        public Vector3[] GetCubeCoordinateRing(Vector3 from, int radius)
        {
            var cubeCoordinates = new List<Vector3>();

            var cubeCoordinate = GetNeighbourCubeCoordinate(from, 4, radius);

            for (int i = 0; i < 6; ++i)
            {
                for (int j = 0; j < radius; ++j)
                {
                    cubeCoordinates.Add(cubeCoordinate);

                    cubeCoordinate = GetNeighbourCubeCoordinate(cubeCoordinate, i);
                }
            }

            return cubeCoordinates.ToArray();
        }

        public HexCell[] GetCellRing(HexCell from, int radius)
        {
            var cells = new List<HexCell>();

            var cubeCoordinates = GetCubeCoordinateRing(from.CubeCoordinates, radius);

            foreach (var cubeCoordinate in cubeCoordinates)
            {
                var cell = GetCell(cubeCoordinate);

                if (cell.HasValue)
                    cells.Add(cell.Value);
            }

            return cells.ToArray();
        }

        public Vector3[] GetCubeCoordinateSpiral(Vector3 from, int radius)
        {
            var cubeCoordinates = new List<Vector3>();

            cubeCoordinates.Add(from);

            for (int i = 1; i <= radius; ++i)
            {
                cubeCoordinates.AddRange(GetCubeCoordinateRing(from, i));
            }

            return cubeCoordinates.ToArray();
        }

        public HexCell[] GetCellSpiral(HexCell from, int radius)
        {
            var cells = new List<HexCell>();

            var cubeCoordinates = GetCubeCoordinateSpiral(from.CubeCoordinates, radius);

            foreach (var cubeCoordinate in cubeCoordinates)
            {
                var cell = GetCell(cubeCoordinate);

                if (cell.HasValue)
                    cells.Add(cell.Value);
            }

            return cells.ToArray();
        }

        public Vector2[] GetAStarPath(HexCell from, HexCell to)
        {
            // TODO: Add obstacles, add movement range
            // https://www.gdquest.com/tutorial/godot/2d/tactical-rpg-movement/lessons/04.pathfinding-and-path-drawing/
            _aStar.Clear();

            // Add all the nodes
            foreach (var cell in _cells.Values)
                _aStar.AddPoint(cell.Index, cell.AxialCoordinates);

            // Add all their neighbours
            foreach (var cell in _cells.Values)
                foreach (var neighbour in GetAllNeighbourCells(cell))
                    _aStar.ConnectPoints(cell.Index, neighbour.Index);

            return _aStar.GetPointPath(from.Index, to.Index);
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

        private void CalculateIndex(ref HexCell cell)
        {
            // TODO: Put this into HexCell somehow...

            // Get the distance/radius from the origin (the MAX ABS of the cubeCoords)
            // Can alternatively sum the ABS of all axes and halve. TODO: Which is faster?
            var absoluteVector3 = cell.CubeCoordinates.Abs();
            var maxAxis = absoluteVector3.MaxAxis();

            int distanceFromOrigin = (int)absoluteVector3[(int)maxAxis];

            // Index of the origin is 0
            if (distanceFromOrigin == 0)
            {
                cell.Index = 0;
                return;
            }

            // Count the number of cells in the filled inner spiral
            var spiralCells = GetCubeCoordinateSpiral(cell.CubeCoordinates, distanceFromOrigin - 1);
            int fill = spiralCells.Length;

            // Count the remaining number of cells on the outer ring
            // It will be something between 1 and distanceFromOrigin * 6
            var neighbour = DIRECTIONS[4] * distanceFromOrigin;
            int outerRing = 1;

            for (int i = 0; i < 6; ++i)
            {
                for (int j = 0; j < distanceFromOrigin; ++j)
                {
                    if (neighbour == cell.CubeCoordinates)
                    {
                        cell.Index = fill + outerRing - 1;
                        return;
                    }

                    ++outerRing;

                    neighbour = GetNeighbourCubeCoordinate(neighbour, i);
                }
            }

            cell.Index = fill + outerRing - 1;
        }
    }
}