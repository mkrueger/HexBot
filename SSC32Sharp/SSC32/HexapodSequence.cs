﻿//
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

namespace RobotControlFramework.SSC32
{
    public class HexpodSequence
    {
        // Walking Gait Configuration

        public int VerticalServo_Left_HighValue = 1000;
        public int VerticalServo_Left_MidValue = 1400;
        public int VerticalServo_Left_LowValue = 1800;

        public int VerticalServo_Right_HighValue = 2000;
        public int VerticalServo_Right_MidValue = 1600;
        public int VerticalServo_Right_LowValue = 1200;

        public ushort VerticalServo_MovementSpeed = 3000;

        public int HorizontalServo_Left_FrontValue = 1700;
        public int HorizontalServo_Left_RearValue = 1300;

        public int HorizontalServo_Right_FrontValue = 1300;
        public int HorizontalServo_Right_RearValue = 1700;

        public ushort HorizontalServo_MovementTime = 1500;
        public int TravelPercentage_Left = 100;
        public int TravelPercentage_Right = 100;
    }

}