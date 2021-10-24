using Godot;
using System;

namespace IanByrne.HexTiles
{
    public struct HexCell
    {
        public HexCell(Vector3 cubeCoordinates)
        {
            var rounded = cubeCoordinates.RoundCubeCoordinates();

            if (rounded.x + rounded.y + rounded.z != 0)
                throw new ArgumentException("q + r + s must be 0");

            CubeCoordinates = rounded;
        }

        public HexCell(Vector2 axialCoordinates) : this(axialCoordinates.ToCubeCoordinates()) { }
        public HexCell(int q, int r) : this(new Vector2(q, r)) { }
        public HexCell(int q, int r, int s) : this(new Vector3(q, r, s)) { }

        public Vector3 CubeCoordinates { get; }
        public Vector2 AxialCoordinates => CubeCoordinates.ToAxialCoordinates();
    }
}