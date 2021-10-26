using Godot;
using System;

namespace IanByrne.HexTiles
{
    public static class Extensions
    {
        /// <summary>
        /// Convert Axial Coordinates to Cube Coordinates by constraining to x + y + z = 0
        /// </summary>
        /// <param name="coordinates">The Vector2</param>
        /// <returns>The cube coordinates</returns>
        public static Vector3 ToCubeCoordinates(this Vector2 coordinates)
        {
            return new Vector3(coordinates.x, -coordinates.x - coordinates.y, coordinates.y);
        }

        /// <summary>
        /// Convert Cube Coordinates to Axial Coordinates by discarding y value
        /// </summary>
        /// <param name="coordinates">The Vector3</param>
        /// <returns>The axial coordinates</returns>
        public static Vector2 ToAxialCoordinates(this Vector3 coordinates)
        {
            return new Vector2(coordinates.x, coordinates.z);
        }

        /// <summary>
        /// Round a Vector3 to absolute values, but maintain the x + y + z = 0 constraint
        /// </summary>
        /// <param name="vector">The Vector3</param>
        /// <returns>Rounded Vector3</returns>
        public static Vector3 RoundCubeCoordinates(this Vector3 vector)
        {
            int q = (int)Math.Round(vector.x);
            int r = (int)Math.Round(vector.y);
            int s = (int)Math.Round(vector.z);

            double q_diff = Math.Abs(q - vector.x);
            double r_diff = Math.Abs(r - vector.y);
            double s_diff = Math.Abs(s - vector.z);

            if (q_diff > r_diff && q_diff > s_diff)
            {
                q = -r - s;
            }
            else if (r_diff > s_diff)
            {
                r = -q - s;
            }
            else
            {
                s = -q - r;
            }

            return new Vector3(q, r, s);
        }

        /// <summary>
        /// Godot 3.3.4 doesn't support operator * for Transform2D and Vector2
        /// (Although it will apparently be implemented for future versions)
        /// </summary>
        /// <param name="transform">The Transform2D</param>
        /// <param name="vector">The Vector2</param>
        /// <returns>The Vector2 product</returns>
        public static Vector2 Multiply(this Transform2D transform, Vector2 vector)
        {
            return new Vector2(transform.Tdotx(vector), transform.Tdoty(vector)) + transform.origin;
        }

        private static float Tdotx(this Transform2D transform, Vector2 with)
        {
            return (transform[0, 0] * with[0]) + (transform[1, 0] * with[1]);
        }

        private static float Tdoty(this Transform2D transform, Vector2 with)
        {
            return (transform[0, 1] * with[0]) + (transform[1, 1] * with[1]);
        }
    }
}