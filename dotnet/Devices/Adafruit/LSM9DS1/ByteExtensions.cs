﻿namespace Devices.Adafruit.LSM9DS1
{
    public static class ByteExtensions
    {
        public static Vector3 ToVector3(this byte[] buffer)
        {
            return new Vector3(buffer[1] << 8 | buffer[0], buffer[3] << 8 | buffer[2], buffer[5] << 8 | buffer[4]);
        }

        public static int ToUshort(this byte[] buffer)
        {
            return buffer[1] << 8 | buffer[0];
        }
    }
}