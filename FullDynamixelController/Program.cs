using System;
using System.Runtime.InteropServices;
using dynamixel_sdk;

namespace read_write
{
    class ReadWrite
    {

        public const int DXL_MINIMUM_POSITION_VALUE = 300;                 // Dynamixel will rotate between this value
        public const int DXL_MAXIMUM_POSITION_VALUE = 4000;                // and this value (note that the Dynamixel would not move when the position value is out of movable range. Check e-manual about the range of the Dynamixel you use.)
        public const int DXL_MOVING_STATUS_THRESHOLD = 10;                  // Dynamixel moving status threshold

        public const byte ESC_ASCII_VALUE = 0x1b;

        public const int COMM_SUCCESS = 0;                   // Communication Success result value
        public const int COMM_TX_FAIL = -1001;               // Communication Tx Failed

        static void Main(string[] args)
        {
            testControllerClass();
        }

        private static void testControllerClass()
        {
            using (var controller = new DynamixelController(DynamixelController.defaultDeviceName,
                                                            1, DynamixelController.defaultDxlId,
                                                            DynamixelController.defaultBauldRate))
            {
                Console.WriteLine("Dynamixel has been successfully connected");
                int index = 0;
                UInt16[] dxl_goal_position = new UInt16[2] { DXL_MINIMUM_POSITION_VALUE, DXL_MAXIMUM_POSITION_VALUE };         // Goal position

                while (true)
                {
                    Console.WriteLine("Press any key to continue! (or press ESC to quit!)");
                    if (Console.ReadKey().KeyChar == ESC_ASCII_VALUE)
                        break;

                    // Write goal position
                    controller.setGoalPosition(dxl_goal_position[index]);
                    UInt16 dxl_present_position = 0;
                    do
                    {
                        // Read present position
                        dxl_present_position = controller.getCurrentPosition();

                        Console.WriteLine("[ID: {0}] GoalPos: {1}  PresPos: {2}", 3, dxl_goal_position[index], dxl_present_position);

                    } while ((Math.Abs(dxl_goal_position[index] - dxl_present_position) > DXL_MOVING_STATUS_THRESHOLD));

                    // Change goal position
                    if (index == 0)
                    {
                        index = 1;
                    }
                    else
                    {
                        index = 0;
                    }
                }
            }
        }
    }
}
