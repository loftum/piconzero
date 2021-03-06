using System;

namespace Maths
{
    public struct Quatf
    {
        public static readonly Quatf Identity = new Quatf(0, 0, 0, 1);
        
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        
        public float Length => Float.Sqrt(W * W + X * X + Y * Y + Z * Z);

        public Quatf(float w, float x, float y, float z)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        public Quatf(float rotationX, float rotationY, float rotationZ)
        {
            rotationX *= 0.5f;
            rotationY *= 0.5f;
            rotationZ *= 0.5f;

            var c1 = Float.Cos(rotationX);
            var c2 = Float.Cos(rotationY);
            var c3 = Float.Cos(rotationZ);
            var s1 = Float.Sin(rotationX);
            var s2 = Float.Sin(rotationY);
            var s3 = Float.Sin(rotationZ);

            W = (c1 * c2 * c3) - (s1 * s2 * s3);
            X = (s1 * c2 * c3) + (c1 * s2 * s3);
            Y = (c1 * s2 * c3) - (s1 * c2 * s3);
            Z = (c1 * c2 * s3) + (s1 * s2 * c3);
        }

        public override string ToString()
        {
            return $"[{W},{X},{Y},{Z}]";
        }

        public Float4 ToAxisAngle()
        {
            var q = this;
            if (Math.Abs(q.W) > 1.0f)
            {
                q.Normalize();
            }

            var result = new Float4
            {
                W = 2.0f * Float.Acos(q.W) // angle
            };

            var den = Float.Sqrt(1.0 - (q.W * q.W));
            if (den > 0.0001f)
            {
                result.X = q.X / den;
                result.Y = q.Y / den;
                result.Z = q.Z / den;
            }
            else
            {
                // This occurs when the angle is zero.
                // Not a problem: just set an arbitrary normalized axis.
                result.X = 1;
                result.Y = 0;
                result.Z = 0;
            }

            return result;
        }

        private void Normalize()
        {
            var scale = 1.0f / Length;
            W *= scale;
            X *= scale;
            Y *= scale;
            Z *= scale;
        }
        
        public static Quatf operator /(Quatf vector, float f)
        {
            return new Quatf(vector.W / f, vector.X / f, vector.Y / f, vector.Z / f);
        }
        
        public static bool TryParse(string stringValue, out Quatf result)
        {
            result = Identity;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return false;
            }

            var parts = stringValue.TrimStart('[').TrimEnd(']').Split(',');
            if (parts.Length != 4)
            {
                return false;
            }

            if (float.TryParse(parts[0], out var w) &&
                float.TryParse(parts[1], out var x) &&
                float.TryParse(parts[2], out var y) &&
                float.TryParse(parts[3], out var z))
            {
                result = new Quatf(w, x, y, z);
                return true;
            }
            return false;
        }
    }
}