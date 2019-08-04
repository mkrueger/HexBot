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
using System.IO.Ports;
using System.Threading;

namespace RobotControlFramework.SSC32
{
    public class SSC32 : IDisposable
    {
        SerialPort serialPort = new SerialPort();

        public bool IsOpen => serialPort.IsOpen;

        public void Open(string portName, int baud)
        {
            serialPort.BaudRate = baud;
            serialPort.PortName = portName;
            serialPort.ReadTimeout = 100;
            serialPort.Open();
        }

        public void WriteCommand(string cmd)
        {
            serialPort.Write(cmd);
        }

        public string ReadLine()
        {
            int timeOut = 0;
            while (timeOut++ < 5)
            {
                try
                {
                    var result = serialPort.ReadExisting();
                    Console.WriteLine("Result:" + result);
                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
                catch (TimeoutException) { }
                Thread.Sleep(10);
            }
            return null;
        }

        string version;
        public string GetVersion()
        {
            if (version != null)
                return version;
            WriteCommand("VER\r");
            return version = ReadLine();
        }

        public MovementStatus QueryMovementStatus()
        {
            WriteCommand("Q\r");
            string line = ReadLine();
            switch (line)
            {
                case ".":
                    return MovementStatus.Complete;
                case "+":
                    return MovementStatus.Complete;
                default:
                    Log.Error("Error while querying movement status ");
                    return MovementStatus.Unknown;
            }
        }

        public int QueryPulseWidth(int channel)
        {
            ValidateServoChannel(channel);
            WriteCommand("QP " + channel + "\r");
            return 10 * (int)serialPort.ReadByte();
        }

        internal static void ValidateServoChannel(int channel)
        {
            if (channel < 0 || channel > 31)
                throw new InvalidOperationException("Servo channel needs to be between 0 and 31 was " + channel);
        }

        #region IDisposable Support
        bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            if (disposing)
            {
                serialPort.Close();
            }
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void WaitTillMovementComplete()
        {
            while (true)
            {
                var status = QueryMovementStatus();
                if (status != MovementStatus.InProgress)
                    return;
                Thread.Sleep(100);
            }
        }
        #endregion
    }

}
