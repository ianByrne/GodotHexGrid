using Godot;

namespace IanByrne.HexTiles
{
    public static class Extensions
    {
        public static Vector3 ToCubeCoordinates(this Vector2 coordinates)
        {
            return new Vector3(coordinates.x, coordinates.y, -coordinates.x - coordinates.y);
        }

        public static Vector2 ToAxialCoordinates(this Vector3 coordinates)
        {
            return new Vector2(coordinates.x, coordinates.y);
        }

        public static float Tdotx(this Transform2D transform, Vector2 with)
        {
            return (transform[0, 0] * with[0]) + (transform[1, 0] * with[1]);
        }

        public static float Tdoty(this Transform2D transform, Vector2 with)
        {
            return (transform[0, 1] * with[0]) + (transform[1, 1] * with[1]);
        }

        public static Vector2 Multiply(this Transform2D transform, Vector2 vector)
        {
            return new Vector2(transform.Tdotx(vector), transform.Tdoty(vector)) + transform.origin;
        }

        public static Vector3 RoundCubeCoordinates(this Vector3 vector)
        {
            int q = (int)Mathf.Round(vector.x);
            int r = (int)Mathf.Round(vector.y);
            int s = (int)Mathf.Round(vector.z);

            double q_diff = Mathf.Abs(q - vector.x);
            double r_diff = Mathf.Abs(r - vector.y);
            double s_diff = Mathf.Abs(s - vector.z);

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
    }
}