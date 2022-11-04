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
// evelle
using System;
using System.Collections.Generic;
using System.Linq;
using RobotControlFramework.SSC32;

namespace RobotControlFramework.HexBot
{
	class Program
	{
		static void Main(string[] args)
        {
            try
            {
                using (var ctl = new BluetoothController(@"/dev/rfcomm0"))
                {
                    ctl.Add(new Action("F", c => {
                        var sequence = new HexpodSequence();
                        ctl.Command
                            .StartSequence(sequence)
                            .Execute();
                        Console.WriteLine("FORWARD");
                    }));

                    ctl.Add(new Action("B", c => {
                        var sequence = new HexpodSequence();
                        sequence.TravelPercentage_Left = sequence.TravelPercentage_Right = -100;
                        ctl.Command
                            .StartSequence(sequence)
                            .Execute();
                        Console.WriteLine("BACKWARD");
                    }));

                    ctl.Add(new Action("L", c => {
                        var sequence = new HexpodSequence();
                        sequence.TravelPercentage_Left = 100;
                        sequence.TravelPercentage_Right = -100;
                        ctl.Command
                            .StartSequence(sequence)
                            .Execute();
                        Console.WriteLine("LEFT");
                    }));

                    ctl.Add(new Action("R", c => {
                        var sequence = new HexpodSequence();
                        sequence.TravelPercentage_Left = -100;
                        sequence.TravelPercentage_Right = 100;
                        ctl.Command
                            .StartSequence(sequence)
                            .Execute();
                        Console.WriteLine("RIGHT");
                    }));

                    ctl.Add(new Action("STOP", c =>
                    {
                        ctl.Command.StopHexSequencer().Execute();
                        Console.WriteLine("STOP");
                    }));
                    ctl.Add(new Action("QUIT", c => { ctl.IsRunning = false; }));
                    ctl.Add(new Action("SPEED+", c => {
                        ctl.Model.Speed = Math.Min(200, ctl.Model.Speed + 10);
                        if (ctl.Model.State == MovementState.InWalkSequence)
                            ctl.Command.SetHorizontalSpeedPercentage(ctl.Model.Speed).Execute();
                        Console.WriteLine("Set speed to:" + ctl.Model.Speed);
                    }));
                    ctl.Add(new Action("SPEED-", c => {
                        ctl.Model.Speed = Math.Max(0, ctl.Model.Speed - 10);
                        if (ctl.Model.State == MovementState.InWalkSequence)
                            ctl.Command.SetHorizontalSpeedPercentage(ctl.Model.Speed).Execute();
                        Console.WriteLine("Set speed to:" + ctl.Model.Speed);
                    }));
                    ctl.Add(new Action("QUIT", c => { ctl.IsRunning = false; }));

                    ctl.OpenSSC32Connection ("/dev/ttyUSB0", SSC32_Constants.BaudGREENRED_115200);
                    const int offset = 1000;
                    ctl.Command.SingleServo(6, offset).SingleServo(7, offset).SingleServo(8, offset)
                        .SingleServo(22, offset).SingleServo(23, offset).SingleServo(24, offset).Execute();

                    ctl.Run();


/*                    ssc32.WriteCommand("XQ\r");
                    Console.WriteLine(ssc32.ReadLine());
                    Console.WriteLine("Done.");*/
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
}
