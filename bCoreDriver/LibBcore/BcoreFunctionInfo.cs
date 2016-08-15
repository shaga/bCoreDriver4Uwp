using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibBcore
{
    public class BcoreFunctionInfo
    {
        private const int ValueLength = 2;
        private const int IdxMotor = 0;
        private const int IdxServoPortOut = 1;

        private const int OffsetMotor = 0;
        private const int OffsetServo = 0;
        private const int OffsetPortOut = 4;

        public int MotorPortInfo { get; }
        public int ServoPortInfo { get; }
        public int PortOutInfo { get; }

        public int MotorPortCount => GetPortCount(MotorPortInfo);
        public int ServoPortCount => GetPortCount(ServoPortInfo);
        public int PortOutCount => GetPortCount(PortOutInfo);

        public BcoreFunctionInfo(byte[] value)
        {
            if (value.Length != ValueLength) return;

            MotorPortInfo = (value[IdxMotor] >> OffsetMotor) & 0x0f;
            ServoPortInfo = (value[IdxServoPortOut] >> OffsetServo) & 0x0f;
            PortOutInfo = (value[IdxServoPortOut] >> OffsetPortOut) & 0x0f;
        }

        public bool IsMotorPortEnable(int idx)
        {
            return IsEnablePort(idx, MotorPortInfo);
        }

        public bool IsServoPortEnable(int idx)
        {
            return IsEnablePort(idx, ServoPortInfo);
        }

        public bool IsPortOutEnable(int idx)
        {
            return IsEnablePort(idx, PortOutInfo);
        }

        private bool IsEnablePort(int idx, int info)
        {
            if (idx < 0 || Bcore.MaxFunctionCount <= idx) return false;

            return ((info >> idx) & 0x01) == 0x01;
        }

        private int GetPortCount(int info)
        {
            var count = 0;

            for (var i = 0; i < Bcore.MaxFunctionCount; i++)
            {
                if (((info >> i) & 0x01) == 0) continue;

                count++;
            }

            return count;
        }
    }
}
