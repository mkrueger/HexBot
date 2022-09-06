//
//  Copyright 2019 Microsoft Corporation. All rights reserved.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;




namespace RobotControlFramework.SSC32
{
    public enum ServoSide { Left, Right }

    public enum LegValue { High, Mid, Low }

    public enum FrontRear { Front, Rear }

    public static class Command_HexapodSequencer
    {
        /// <summary>
        /// Set the value for the servos on the sides of the hexapod.
        /// SSC-32U Commands: LH, LM, LL, RH, RM, RL
        /// </summary>
        /// <param name="side">Specifies the side of the hexapod.</param>
        /// <param name="legValue">Leg Value :  High - maximum height, Low - minimum h.</param>
        /// <param name="value">Value.</param>
        public static Command SetVerticalServo(this Command command, ServoSide side, LegValue legValue, int value)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (value < 500 || value > 2500)
                throw new ArgumentOutOfRangeException(nameof(value), "The valid range is between 500 to 2500uS.");
            var sb = command.Builder;
            sb.Append(side == ServoSide.Left ? 'L' : 'R');
            switch (legValue)
            {
                case LegValue.High:
                    sb.Append('H');
                    break;
                case LegValue.Mid:
                    sb.Append('M');
                    break;
                case LegValue.Low:
                    sb.Append('L');
                    break;
            }
            sb.Append(' ');
            sb.Append(value);
            return command;
        }

        /// <summary>
        /// Sets the speed for movement of vertical servos. All vertical servo moves use this speed.
        /// SSC-32U Command: VS
        /// </summary>
        /// <param name="speed">Valid range is 0 to 65535uS/Sec.</param>
        public static Command SetVerticalServoMovementSpeed(this Command command, ushort speed)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            var sb = command.Builder;
            sb.Append("VS ");
            sb.Append(speed);
            return command;
        }

        /// <summary>
        /// Set the value for the horizontal servos on the left side of the robot. sets the pulse width to move the leg to the maximum forward/rear position; 
        /// SSC-32U Commands: LF, LR, RF, RR
        /// </summary>
        /// <param name="side">Specifies the side of the hexapod.</param>
        /// <param name="servoValue">Specifies front/rear servos.</param>
        /// <param name="value"> The valid range for the arguments is 500 to 2500uS.</param>
        public static Command SetHorizontalServo(this Command command, ServoSide side, FrontRear servoValue, int value)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (value < 500 || value > 2500)
                throw new ArgumentOutOfRangeException(nameof(value), "The valid range is between 500 to 2500uS.");
            var sb = command.Builder;
            sb.Append(side == ServoSide.Left ? 'L' : 'R');
            sb.Append(servoValue == FrontRear.Front ? "F " : "R ");
            sb.Append(value);
            return command;
        }

        /// <summary>
        /// Sets the time to move between horizontal front and rear positions. 
        /// SSC-32U Command: HT
        /// </summary>
        /// <param name="speed">The valid range for the argument is 1 to 65535uS.</param>
        public static Command SetHoizontalServoMovementTime(this Command command, ushort speed)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (speed < 1)
                throw new ArgumentOutOfRangeException(nameof(speed), "The valid range for the\nargument is 1 to 65535uS.");
            var sb = command.Builder;
            sb.Append("HT ");
            sb.Append(speed);
            return command;
        }

        /// <summary>
        /// Set the travel percentage for left and right legs. The valid range is -100% to 100%.
        /// Negative values cause the legs on the side to move in reverse. With a value of 100%, the legs will move
        /// between the front and rear positions.Lower values cause the travel to be proportionally less, but 
        /// always centered.The speed for horizontal moves is adjusted based on the XL and XR
        /// commands, so the move time remains the same.
        /// SSC-32U Commands: XL, XR
        /// </summary>
        /// <param name="side">Specifies the side of the hexapod.</param>
        /// <param name="percentage"> The valid range is -100% to 100%.</param>
        public static Command SetTravelPercentage(this Command command, ServoSide side, int percentage)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (percentage < -100 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage), "The valid range is between -100% to 100%");
            var sb = command.Builder;
            sb.Append(side == ServoSide.Left ? "XL" : "XR");
            sb.Append(percentage);
            return command;
        }

        /// <summary>
        /// Set the horizontal speed percentage for all legs. With a value of
        /// 100%, the horizontal travel time will be the value programmed using the HT command.Higher
        /// values proportionally reduce the travel time; lower values increase it.A value of 0% will stop the
        /// robot in place.The hex sequencer will not be started until the XS command is received.
        /// SSC-32U Commands: XS
        /// </summary>
        /// <param name="percentage"> The valid range is 0% to 200%.</param>
        public static Command SetHorizontalSpeedPercentage(this Command command, int percentage)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (percentage < 0 || percentage > 200)
                throw new ArgumentOutOfRangeException(nameof(percentage), "The valid range is between 0% to 200%");
            var sb = command.Builder;
            sb.Append("XS ");
            sb.Append(percentage);
            command.Controller.Model.State = MovementState.InWalkSequence;
            return command;
        }

        /// <summary>
        /// Stop the hex sequencer. Return all servos to normal operation.
        /// SSC-32U Commands: XSTOP
        /// </summary>
        public static Command StopHexSequencer(this Command command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (command.Controller.Model.State == MovementState.Stopped)
                return command;
            var sb = command.Builder;
            sb.Append("XSTOP");
            command.Controller.Model.State = MovementState.Stopped;
            return command;
        }

        public static Command StartSequence(this Command command, HexpodSequence sequence)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (command.Controller.Model.State == MovementState.InWalkSequence)
                command.StopHexSequencer().Execute();
            return command.SetVerticalServo(ServoSide.Left, LegValue.High, sequence.VerticalServo_Left_HighValue).SetVerticalServo(ServoSide.Left, LegValue.Mid, sequence.VerticalServo_Left_MidValue).SetVerticalServo(ServoSide.Left, LegValue.Low, sequence.VerticalServo_Left_LowValue)
                      .SetVerticalServo(ServoSide.Right, LegValue.High, sequence.VerticalServo_Right_HighValue).SetVerticalServo(ServoSide.Right, LegValue.Mid, sequence.VerticalServo_Right_MidValue).SetVerticalServo(ServoSide.Right, LegValue.Low, sequence.VerticalServo_Right_LowValue)
                      .SetVerticalServoMovementSpeed(sequence.VerticalServo_MovementSpeed)
                      .SetHorizontalServo(ServoSide.Left, FrontRear.Front, sequence.HorizontalServo_Left_FrontValue).SetHorizontalServo(ServoSide.Left, FrontRear.Rear, sequence.HorizontalServo_Left_RearValue)
                      .SetHorizontalServo(ServoSide.Right, FrontRear.Front, sequence.HorizontalServo_Right_FrontValue).SetHorizontalServo(ServoSide.Right, FrontRear.Rear, sequence.HorizontalServo_Right_RearValue)
                      .SetHoizontalServoMovementTime(sequence.HorizontalServo_MovementTime)
                      .SetTravelPercentage(ServoSide.Left, sequence.TravelPercentage_Left).SetTravelPercentage(ServoSide.Right, sequence.TravelPercentage_Right)
                      .SetHorizontalSpeedPercentage(command.Controller.Model.Speed);
        }

        public static Command UpdateSequence(this Command command, HexpodSequence oldSequence, int oldSpeed, HexpodSequence sequence)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (oldSequence == null)
                throw new ArgumentNullException(nameof(oldSequence));
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (oldSequence.VerticalServo_Left_HighValue != sequence.VerticalServo_Left_HighValue)
                command = command.SetVerticalServo(ServoSide.Left, LegValue.High, sequence.VerticalServo_Left_HighValue);
            if (oldSequence.VerticalServo_Left_MidValue != sequence.VerticalServo_Left_MidValue)
                command = command.SetVerticalServo(ServoSide.Left, LegValue.Mid, sequence.VerticalServo_Left_MidValue);
            if (oldSequence.VerticalServo_Left_LowValue != sequence.VerticalServo_Left_LowValue)
                command = command.SetVerticalServo(ServoSide.Left, LegValue.Low, sequence.VerticalServo_Left_LowValue);

            if (oldSequence.VerticalServo_Right_HighValue != sequence.VerticalServo_Right_HighValue)
                command = command.SetVerticalServo(ServoSide.Right, LegValue.High, sequence.VerticalServo_Right_HighValue);
            if (oldSequence.VerticalServo_Right_MidValue != sequence.VerticalServo_Right_MidValue)
                command = command.SetVerticalServo(ServoSide.Right, LegValue.Mid, sequence.VerticalServo_Right_MidValue);
            if (oldSequence.VerticalServo_Right_LowValue != sequence.VerticalServo_Right_LowValue)
                command = command.SetVerticalServo(ServoSide.Right, LegValue.Low, sequence.VerticalServo_Right_LowValue);

            if (oldSequence.VerticalServo_MovementSpeed != sequence.VerticalServo_MovementSpeed)
                command = command.SetVerticalServoMovementSpeed(sequence.VerticalServo_MovementSpeed);

            if (oldSequence.HorizontalServo_Left_FrontValue != sequence.HorizontalServo_Left_FrontValue)
                command = command.SetHorizontalServo(ServoSide.Left, FrontRear.Front, sequence.HorizontalServo_Left_FrontValue);
            if (oldSequence.HorizontalServo_Left_RearValue != sequence.HorizontalServo_Left_RearValue)
                command = command.SetHorizontalServo(ServoSide.Left, FrontRear.Rear, sequence.HorizontalServo_Left_RearValue);

            if (oldSequence.HorizontalServo_Right_FrontValue != sequence.HorizontalServo_Right_FrontValue)
                command = command.SetHorizontalServo(ServoSide.Right, FrontRear.Front, sequence.HorizontalServo_Right_FrontValue);
            if (oldSequence.HorizontalServo_Right_RearValue != sequence.HorizontalServo_Right_RearValue)
                command = command.SetHorizontalServo(ServoSide.Right, FrontRear.Rear, sequence.HorizontalServo_Right_RearValue);

            if (oldSequence.HorizontalServo_MovementTime != sequence.HorizontalServo_MovementTime)
                command = command.SetHoizontalServoMovementTime(sequence.HorizontalServo_MovementTime);

            if (oldSequence.TravelPercentage_Left != sequence.TravelPercentage_Left)
                command = command.SetTravelPercentage(ServoSide.Left, sequence.TravelPercentage_Left);
            if (oldSequence.TravelPercentage_Right != sequence.TravelPercentage_Right)
                command = command.SetTravelPercentage(ServoSide.Right, sequence.TravelPercentage_Right);
            if (command.Controller.Model.State == MovementState.Stopped || command.Controller.Model.Speed != oldSpeed)
            command = command.SetHorizontalSpeedPercentage(command.Controller.Model.Speed);

            return command;
        }
    }
}