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
    public static class Command_Basic
    {
        /// <summary>
        /// This command controls a single servo.
        /// </summary>
        /// <param name="channel">Pin / channel to which the servo is connected (0 to 31).</param>
        /// <param name="pulseWidth">Desired pulse width (normally 500 to 2500) in microseconds.</param>
        /// <param name="speed">Servo movement speed in microseconds per second.</param>
        /// <param name="time">Time in microseconds to travel from the current position to the desired position. (65535 max)</param>
        public static Command SingleServo(this Command command, int channel, int pulseWidth, int? speed = null, ushort? time = null)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            SSC32.ValidateServoChannel(channel);
            var sb = command.Builder;
            sb.Append('#');
            sb.Append(channel);
            sb.Append('P');
            sb.Append(pulseWidth);
            if (speed != null)
            {
                sb.Append('S');
                sb.Append(speed);
            }
            if (time != null)
            {
                sb.Append('T');
                sb.Append(time);
            }
            return command;
        }

        /// <summary>
        /// The position offset command allows you to change the center position (associated with 1500us) 
        /// of one or more servos via software within 15 degrees of the absolute center.
        /// </summary>
        /// <returns>The position offset.</returns>
        /// <param name="channel">Pin / channel to which the servo is connected (0 to 31).</param>
        /// <param name="positionOffset">The position offset is restricted to -100us to 100us (around 15°)</param>
        public static Command ServoPositionOffset(this Command command, int channel, int positionOffset)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            SSC32.ValidateServoChannel(channel);
            if (positionOffset < -100 || positionOffset > 100)
                throw new ArgumentOutOfRangeException(nameof(positionOffset), "The position offset is restricted to -100us to 100us (around 15°)");
            var sb = command.Builder;
            sb.Append('#');
            sb.Append(channel);
            sb.Append("PO");
            sb.Append(positionOffset);
            return command;
        }

        /// <summary>
        /// Immediately stops the specified servo at its current position.
        /// </summary>
        /// <returns>The position offset.</returns>
        /// <param name="channel">Pin / channel to which the servo is connected (0 to 31).</param>
        public static Command StopServo(this Command command, int channel)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            SSC32.ValidateServoChannel(channel);
            var sb = command.Builder;
            sb.Append("STOP");
            sb.Append(channel);
            return command;
        }
    }
}
