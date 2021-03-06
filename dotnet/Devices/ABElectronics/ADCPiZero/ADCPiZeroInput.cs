﻿using System;
using Unosquare.PiGpio.ManagedModel;

namespace Devices.ABElectronics.ADCPiZero
{
    public class ADCPiZeroInput : IAnalogInput
    {
        public I2cDevice Device { get; }
        public int Channel { get; }
        public int Number { get; }
        private readonly byte _baseConfig;
        public Bitrate Bitrate { get; set; } = Bitrate._12;
        public Pga Pga { get; set; } = Pga._8;
        public ConversionMode ConversionMode { get; set; } = ConversionMode.Continuous;

        public ADCPiZeroInput(I2cDevice device, int input)
        {
            Device = device;
            Channel = input < 4 ? input : input - 4; // 0-based
            Number = input;
            var channelBits = (byte)(0b0110_0000 & (Channel << 5));
            _baseConfig = (byte)(0b1001_1100 | channelBits);
            /*
             * Configuration register:
             * Bit 7 (RDY): Ready bit
             * Bit 6-5 (C1-C0): Channel selection bits. (1-based)
             *   00: Channel 1 (default)
             *   01: Channel 2
             *   10: Channel 3
             *   11: Channel 4
             * Bit 4 (O/C): conversion mode bit. 1: Continuous, 0: One-shot
             * Bit 3-2 (S1-S0): Sample selection Bit
             *   00: 240 SPS (12 bits) (default)
             *   01: 60 SPS (14 bits)
             *   10: 15 SPS (16 bits)
             *   11: 3.75 SPS (18 bits)
             * Bit 1-0 (G1-G0): PGA Gain selection bits
             *   00: x1 (default)
             *   01: x2
             *   10: x4
             *   11: x8
             */
        }

        private byte GetConfig()
        {
            var config = _baseConfig;
            switch (ConversionMode)
            {
                case ConversionMode.OneShot:
                    config &= 0b1110_1111;
                    break;
                case ConversionMode.Continuous:
                    config &= 0b1111_1111;
                    break;
            }
            switch (Bitrate)
            {
                case Bitrate._12:
                    config &= 0b1111_0011;
                    break;
                case Bitrate._14:
                    config &= 0b1111_0111;
                    break;
                case Bitrate._16:
                    config &= 0b1111_1011;
                    break;
                case Bitrate._18:
                    config &= 0b1111_1111;
                    break;
            }

            switch (Pga)
            {
                case Pga._1:
                    config &= 0b1111_1100;
                    break;
                case Pga._2:
                    config &= 0b1111_1101;
                    break;
                case Pga._4:
                    config &= 0b1111_1110;
                    break;
                case Pga._8:
                    config &= 0b1111_1111;
                    break;
            }

            return config;
        }

        private double GetLsbVoltage()
        {
            return Bitrate switch
            {
                Bitrate._12 => 0.001,
                Bitrate._14 => 0.000_250,
                Bitrate._16 => 0.000_062_5,
                Bitrate._18 => 0.000_015_625,
                _ => 9999
            };
        }

        private double GetGain()
        {
            return Pga switch
            {
                Pga._1 => 1,
                Pga._2 => 2,
                Pga._4 => 4,
                Pga._8 => 8,
                _ => 1
            };
        }

        /**
         * Returns voltage between -2.048 and 2.048 V
         */
        public double ReadVoltage()
        {
            var raw = ReadRaw();
            var gain = GetGain();
            var lsbVoltage = GetLsbVoltage();
            var voltage = 2.471 * raw * lsbVoltage / gain;
            return voltage;
        }
        
        private int ReadRaw()
        {
            var config = GetConfig();
            byte hi = 0;
            byte med = 0;
            byte lo = 0;
            var attempts = 0;
            
            while (true)
            {
                Device.Write(config);
                byte status = 0;
                
                if (Bitrate == Bitrate._18)
                {
                    var readbuffer = Device.ReadRaw(4);
                    hi = readbuffer[0];
                    med = readbuffer[1];
                    lo = readbuffer[2];
                    status = readbuffer[3];
                }
                else
                {
                    var readbuffer = Device.ReadRaw(3);
                    hi = readbuffer[0];
                    med = readbuffer[1];
                    status = readbuffer[2];
                }

                // check bit 7 of s to see if the conversion result is ready
                if ((status & 0b1000_0000) == 0)
                {
                    break;
                }

                if (attempts > 1000)
                {
                    Console.WriteLine("ADC timeout");
                    // timeout occurred
                    return 0;
                }
                attempts++;
            }

            int result;
            // Supporting negative numbers, although that would be weird.
            switch (Bitrate)
            {
                case Bitrate._18:
                    result = ((hi & 0b_0000_0011) << 16) | (med << 8) | lo;
                    return result >> 17 == 1
                        ? result & ~(1 << 17)
                        : result;
                case Bitrate._16:
                    result = (hi << 8) | med;
                    return result >> 15 == 1
                        ? result & ~(1 << 15)
                        : result;
                case Bitrate._14:
                    result = ((hi & 0b_0011_1111) << 8) | med;
                    return result >> 13 == 1
                        ? result & ~(1 << 13)
                        : result;
                case Bitrate._12:
                    result = ((hi & 0b_0000_1111) << 8) | med;
                    return result >> 11 == 1
                        ? result & ~(1 << 11)
                        : result;
                default:
                    return 0;
            }
        }
    }
}
