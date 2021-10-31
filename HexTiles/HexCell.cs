using Godot;
using System;

namespace IanByrne.HexTiles
{
    public struct HexCell
    {
        public HexCell(Vector3 cubeCoordinates, int movementCost = 0)
        {
            var rounded = cubeCoordinates.RoundCubeCoordinates();

            if (rounded.x + rounded.y + rounded.z != 0)
                throw new ArgumentException("q + r + s must be 0");

            CubeCoordinates = rounded;
            MovementCost = movementCost;
        }

        public HexCell(Vector2 axialCoordinates, int movementCost = 0) : this(axialCoordinates.ToCubeCoordinates(), movementCost) { }
        public HexCell(int q, int r, int s, int movementCost = 0) : this(new Vector3(q, r, s), movementCost) { }

        /// <summary>
        /// A value of -1 is impassable
        /// </summary>
        public int MovementCost { get; set; }
        public Vector3 CubeCoordinates { get; set; }
        public Vector2 AxialCoordinates => CubeCoordinates.ToAxialCoordinates();
    }
}