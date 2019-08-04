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
using System.Collections.Generic;
using System.IO.Ports;
using RobotControlFramework.SSC32;

namespace RobotControlFramework.HexBot
{
    class BluetoothController : AbstractController
    {
        SerialPort port = new SerialPort();
        Dictionary<string, Action> registeredActions = new Dictionary<string, Action>();

        public bool IsRunning { get; set; }

        public BluetoothController(string bluetoothPortNamea)
        {
            port.PortName = bluetoothPortNamea;
            port.BaudRate = 115200;
        }

        public void Add(Action action)
        {
            registeredActions[action.Command] = action;
        }

        public void Run()
        {
            port.Open();
            IsRunning = true;
            while (IsRunning)
            {
                string line = null;
                while (string.IsNullOrEmpty(line))
                    line = port.ReadExisting();
                if (registeredActions.TryGetValue(line, out var action))
                {
                    action.Execute(this);
                }
                else
                {
                    Log.Error("Received unknown action:" + line);
                }
            }
            port.Close();
        }

      
    }

}
