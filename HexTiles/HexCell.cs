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
            Index = CalculateIndex(rounded);
        }

        public HexCell(Vector2 axialCoordinates, int movementCost = 0) : this(axialCoordinates.ToCubeCoordinates(), movementCost) { }
        public HexCell(int q, int r, int s, int movementCost = 0) : this(new Vector3(q, r, s), movementCost) { }

        public Vector3 CubeCoordinates { get; }
        public Vector2 AxialCoordinates => CubeCoordinates.ToAxialCoordinates();
        public int Index { get; }

        /// <summary>
        /// A value of -1 is impassable
        /// </summary>
        public int MovementCost { get; set; }

        private static int CalculateIndex(Vector3 cubeCoordinates)
        {
            // Get the distance/radius from the origin (the MAX ABS of the cubeCoords)
            // Can alternatively sum the ABS of all axes and halve. TODO: Which is faster?
            var absoluteVector3 = cubeCoordinates.Abs();
            var maxAxis = absoluteVector3.MaxAxis();

            int distanceFromOrigin = (int)absoluteVector3[(int)maxAxis];

            // Index of the origin is 0
            if (distanceFromOrigin == 0)
                return 0;

            // Count the number of cells in the filled inner spiral
            int innerSpiral = 1; // Origin

            for (int i = 1; i < distanceFromOrigin; ++i)
                innerSpiral += 6 * i;

            // Count the remaining number of cells on the outer ring
            // It will be something between 1 and distanceFromOrigin * 6
            var neighbour = HexGrid.DIRECTIONS[4] * distanceFromOrigin;
            int outerRing = 1;

            for (int i = 0; i < 6; ++i)
            {
                for (int j = 0; j < distanceFromOrigin; ++j)
                {
                    if (neighbour == cubeCoordinates)
                        return innerSpiral + outerRing - 1;

                    ++outerRing;

                    neighbour = HexGrid.GetNeighbourCubeCoordinate(neighbour, i);
                }
            }

            return innerSpiral + outerRing - 1;
        }
    }
}