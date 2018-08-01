﻿namespace Devices.Adafruit.LSM9DS1
{
    public struct Vector3
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 operator +(Vector3 first, Vector3 second)
        {
            return new Vector3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }

        public static Vector3 operator *(Vector3 vector, double f)
        {
            return new Vector3(vector.X * f, vector.Y * f, vector.Z * f);
        }

        public static Vector3 operator *(double f, Vector3 vector)
        {
            return vector * f;
        }

        public static Vector3 operator /(Vector3 vector, double f)
        {
            return new Vector3(vector.X / f, vector.Y / f, vector.Z / f);
        }

        public static Vector3 Empty => new Vector3();

        public override string ToString()
        {
            return $"[{X}, {Y}, {Z}]";
        }
    }
}