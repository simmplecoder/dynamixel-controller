using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dynamixel_sdk;

namespace read_write
{
    class DynamixelController: IDisposable
    {
        public static readonly int defaultDxlId = 3;
        public static readonly int defaultBauldRate = 57142;
        public static readonly string defaultDeviceName = "COM7";

        public const int TORQUE_ENABLE = 1;
        public const int TORQUE_DISABLE = 0;

        public const int ADDR_MX_TORQUE_ENABLE = 24;                  // Control table address is different in Dynamixel model
        public const int ADDR_MX_GOAL_POSITION = 30;
        public const int ADDR_MX_PRESENT_POSITION = 36;

        public const int PROTOCOL_VERSION = 1;                   // See which protocol version is used in the Dynamixel

        public const int COMM_SUCCESS = 0;                   // Communication Success result value
        public const int COMM_TX_FAIL = -1001;               // Communication Tx Failed

        public const int LB_TORQUE_LIMIT_ADDRESS = 34;
        public const int HB_TORQUE_LIMIT_ADDRESS = 35;

        private string DeviceName;
        private int ProtocolVersion;
        private int DXLID;
        private int BauldRate;
        private int portnum;

        public DynamixelController(string deviceName, int protocolVersion, int dxlid, int bauldRate)
        {
            DeviceName = deviceName;
            ProtocolVersion = protocolVersion;
            DXLID = dxlid;
            BauldRate = bauldRate;

            portnum = dynamixel.portHandler(deviceName);
            dynamixel.packetHandler();
            bool portOpeningSucceeded = dynamixel.openPort(portnum);
            if (!portOpeningSucceeded)
            {
                dynamixel.write1ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
                throw new SystemException("Port opening failed");
            }

            if (!dynamixel.setBaudRate(portnum, BauldRate))
            {
                dynamixel.write1ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
                throw new SystemException("Failed to set BauldRate");
            }
            setCurrentPosition(0);

            // Enable Dynamixel Torque
            dynamixel.write1ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
            checkSuccess();
        }

        public void setLBTorqueLimit(Uint16 LBtorqueLimit) {
            dynamixel.write1ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, LB_TORQUE_LIMIT_ADDRESS, (byte)LBtorqueLimit);
        }

        public void setHBTorqueLimit(Uin16 HBtorqueLimit) {
            dynamixel.write1ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, HB_TORQUE_LIMIT_ADDRESS, (byte)HBtorqueLimit);
        }

        public void setGoalPosition(UInt16 goalPosition)
        {
            dynamixel.write2ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, ADDR_MX_GOAL_POSITION, goalPosition);
            checkSuccess();
        }

        public void setCurrentPosition(Uint16 currentPosition) {
            dynamxel.write2ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, ADDR_MX_PRESENT_POSITION, 0);
        }

        public UInt16 getCurrentPosition()
        {
            UInt16 dxl_present_position = dynamixel.read2ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, ADDR_MX_PRESENT_POSITION);
            checkSuccess();
            return dxl_present_position;
        }

        private void checkSuccess()
        {
            int dxl_comm_result = dynamixel.getLastTxRxResult(portnum, PROTOCOL_VERSION);                                   // Communication result
            byte dxl_error = dynamixel.getLastRxPacketError(portnum, PROTOCOL_VERSION);
            if (dxl_comm_result != COMM_SUCCESS)
            {
                dynamixel.write1ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
                throw new SystemException(Marshal.PtrToStringAnsi(dynamixel.getTxRxResult(PROTOCOL_VERSION, dxl_comm_result)));
            }
            else if ((dxl_error = dynamixel.getLastRxPacketError(portnum, PROTOCOL_VERSION)) != 0)
            {
                dynamixel.write1ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
                throw new SystemException(Marshal.PtrToStringAnsi(dynamixel.getRxPacketError(PROTOCOL_VERSION, dxl_error)));
            }
        }

        public void Dispose()
        {
            dynamixel.write1ByteTxRx(portnum, PROTOCOL_VERSION, (byte)DXLID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
            try
            {
                checkSuccess();
            } catch(Exception) { }

            // Close port
            dynamixel.closePort(portnum);
        }
    }
}
