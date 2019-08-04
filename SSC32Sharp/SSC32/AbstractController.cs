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
    public abstract class AbstractController : IDisposable
    {
        public SSC32 SSC32 { get; } = new SSC32();
        public RobotModel Model { get; } = new RobotModel();

        public Command Command => new Command(this);

        public void OpenSSC32Connection(string portName, int baud)
        {
            SSC32.Open(portName, baud);
        }

        #region IDisposable Support
        bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    SSC32.Dispose();
                }

                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
