﻿using System;
using Unosquare.RaspberryIO.Gpio;

namespace Devices._4tronix
{
    public class MotorPort
    {
        private int _speed;
        private readonly I2CDevice _device;
        public int Number { get; }

        public int Speed
        {
            get => _speed;
            set
            {
                if (Math.Abs(value) > 127)
                {
                    return;
                }
                Console.WriteLine($"Speed = {value}");
                _device.WriteAddressByte(Number, (byte)value);
                _speed = value;
            }
        }

        public MotorPort(I2CDevice device, int number)
        {
            _device = device;
            Number = number;
        }
    }
}