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
using System.IO;

namespace RobotControlFramework.SSC32
{
    public class HexpodSequenceListener : IDisposable
    {
        string fileName;

        private HexpodSequence _sequence;
        FileSystemWatcher watcher;

        public HexpodSequence Sequence
        {
            get => _sequence; 

            private set
            {
                _sequence = value;
                SequenceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler SequenceChanged;

        public HexpodSequenceListener(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException(fileName + " not found.");
            this.fileName = fileName;
            fileName = Path.GetFullPath(fileName);
            Sequence = HexpodSequence.ReadSequence(fileName);
            watcher = new FileSystemWatcher(Path.GetDirectoryName (fileName));
            watcher.Changed += delegate (object sender, FileSystemEventArgs e)
            {
                if (e.FullPath == fileName)
                    Sequence = HexpodSequence.ReadSequence(fileName);
            };
            watcher.EnableRaisingEvents = true;
        }

        public void Dispose ()
        {
            watcher.EnableRaisingEvents = false;

            watcher.Dispose();
        }
    }

}