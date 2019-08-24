using System;
using System.Threading;
using RobotControlFramework.SSC32;
using System.Linq;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using System.Reflection;
using System.IO;

namespace HexBot.FrSky
{
    class FrSkyController : AbstractController
    {
        HidStream hidStream;

        HexpodSequenceListener listener;

        public bool Init()
        {
            var list = DeviceList.Local;
            list.Changed += (sender, e) => SearchDevices();

            listener = new HexpodSequenceListener(Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "../../sequence.xml"));
            listener.SequenceChanged += (sender, e) =>
            {
                if (this.Model.State == MovementState.InWalkSequence)
                {
                    this.Command.StopHexSequencer().Execute();
                    this.RunSequence();
                }
            };

            return SearchDevices();
        }

        void RunSequence()
        {
            if (!SSC32.IsOpen)
                return;

            if (!controllerState.LeftSwitch)
            {
                if (Model.State == MovementState.InWalkSequence)
                    Command.StopHexSequencer().Execute();
                return;
            }

            var currentSequence = listener?.Sequence?.Clone();
            if (currentSequence == null)
            {
                Log.Error("Listener does not provide a sequence.");
                return;
            }
            try
            {
                int newSpeed = (int)(200 * controllerState.RZ);
                if (newSpeed != Model.Speed || !currentSequence.Equals(oldSequence))
                {
                    Model.Speed = newSpeed;

                    if (Model.Speed <= 1)
                    {
                        Command.StopHexSequencer().Execute();
                    }
                    else
                    {
                        Command.StartSequence(currentSequence).Execute();
                    }
                    oldSequence = currentSequence;
                }
            } catch (Exception e)
            {
                Log.Error("Error while running sequence", e);
            }
        }

        void UpdateControlState(ControllerState oldState)
        {
            if (!SSC32.IsOpen)
                return;
            if (controllerState.LeftSwitch)
            {
                int value = (int)(500 + 1000 * (1 + controllerState.Z));
                var cmd = Command;
                bool exec = false;
                if (oldState.Z != controllerState.Z || !oldState.LeftSwitch) {
                 cmd.SingleServo(22, value)
                    .SingleServo(23, value)
                    .SingleServo(24, value);
                    exec = true;
                }

                if (oldState.RZ != controllerState.RZ || !oldState.LeftSwitch)
                {
                    value = (int)(500 + 1000 * (1 + controllerState.RZ));
                    cmd.SingleServo(6, value)
                        .SingleServo(7, value)
                        .SingleServo(8, value)
                        ;
                    exec = true;
                }
                if (exec)
                    cmd.Execute();
            }
            else
            {
                if (oldState.LeftSwitch)
                {
                    Command.StopServo(22).StopServo(23).StopServo(24)
                        .StopServo(6).StopServo(7).StopServo(8).Execute();
                }
            }

            RunSequence();
            if (controllerState.RightSwitch)
                IsRunning = false;
        }

        ControllerState controllerState = new ControllerState();
        private HexpodSequence oldSequence;

        public class ControllerState : IEquatable<ControllerState>
        {
            public double X, Y, Z;
            public double RX, RY, RZ;

            public double SL, SR;
            public bool LeftSwitch, RightSwitch;


            public override string ToString()
            {
                const string fmt = ",3:N1";
                return string.Format("[ControllerState: X={0"+fmt+"}, Y={1"+fmt+"}, Z={2"+fmt+"}, RX={3"+fmt+"}, RY={4"+fmt+"}, RZ={5"+fmt+"}, SL={6"+fmt+"}, SR={7"+fmt+"}, LeftSwitch={8}, RightSwitch={9}]", X, Y, Z, RX, RY, RZ, SL, SR, LeftSwitch, RightSwitch);
            }

            public ControllerState Clone() => (ControllerState)this.MemberwiseClone ();


            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public bool Equals(ControllerState other)
            {
                return X == other.X && Y == other.Y && Z == other.Z &&
                    RX == other.RX && RY == other.RY && RZ == other.RZ &&
                    SL == other.SL && SR == other.SR &&
                    LeftSwitch == other.LeftSwitch && RightSwitch == other.RightSwitch;
            }
        }

        bool SearchDevices()
        {

            if (hidStream != null)
            {
                hidStream.Dispose();
                hidStream = null;
            }

            var list = DeviceList.Local;
            var dev = list.GetHidDevices().FirstOrDefault(d => d.GetProductName().Contains("FrSky"));
            if (dev == null)
            {
                Console.WriteLine("No FrSky device found.");
                return false;
            }
            if (dev.TryOpen(out hidStream))
            {
                hidStream.ReadTimeout = Timeout.Infinite;
                var inputReportBuffer = new byte[dev.GetMaxInputReportLength()];
                var rawReportDescriptor = dev.GetRawReportDescriptor();
                var reportDescriptor = dev.GetReportDescriptor();

                var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                foreach (var deviceItem in reportDescriptor.DeviceItems)
                {
                    var inputParser = deviceItem.CreateDeviceItemInputParser();
                    inputReceiver.Received += (sender, e) =>
                    {
                        Report report;
                        var oldState = controllerState.Clone();
                        while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
                        {
                            if (inputParser.TryParseReport(inputReportBuffer, 0, report))
                                WriteDeviceItemInputParserResult(inputParser);
                        }
                        if (!controllerState.Equals (oldState))
                            UpdateControlState(oldState);
                    };
                }
                inputReceiver.Start(hidStream);
                Console.WriteLine("FrSky device found and initialized.");

            }
            else
            {
                Console.WriteLine($"Can't open device {dev.GetFriendlyName()}. Check permissions for {dev.GetFileSystemName ()}.");
                return false;
            }
            return true;
        }


        void WriteDeviceItemInputParserResult(DeviceItemInputParser parser)
        {
            while (parser.HasChanged)
            {
                int changedIndex = parser.GetNextChangedIndex();
                var previousDataValue = parser.GetPreviousValue(changedIndex);
                var dataValue = parser.GetValue(changedIndex);
                var value = dataValue.GetPhysicalValue();
                Usage usage = (Usage)dataValue.Usages.FirstOrDefault();
                value = value < 0 ? value / 127 : value / 254;
                switch (usage)
                {
                    case Usage.GenericDesktopX:
                        controllerState.X = value;
                        break;
                    case Usage.GenericDesktopY:
                        controllerState.Y = value;
                        break;
                    case Usage.GenericDesktopZ:
                        controllerState.Z = value;
                        break;
                    case Usage.GenericDesktopRx:
                        controllerState.RX = value;
                        break;
                    case Usage.GenericDesktopRy:
                        controllerState.RY = value;
                        break;
                    case Usage.GenericDesktopRz:
                        controllerState.RZ = value;
                        break;
                    case Usage.GenericDesktopSlider:
                        if (changedIndex == 30)
                            controllerState.LeftSwitch = value >= 0;
                        else
                            controllerState.RightSwitch = value >= 0;
                        break;
                    case Usage.Button1:
                        Console.WriteLine("button 1 :" + value);
                        break;
                    case Usage.Button2:
                        Console.WriteLine("button 2 :" + value);
                        break;
                    default:
                        Console.WriteLine("unknown usage:" + usage);
                        break;
                }
                Console.WriteLine(controllerState);
            }
        }
        public bool IsRunning { get; set; }

        internal void Run()
        {
            if (!SSC32.IsOpen)
            {
                Log.Error("SSC32 connection is not open. Just running in input mode.");
            }
            IsRunning = true;
            while (IsRunning)
            {
                Thread.Sleep(200);
            }
            Command.StopHexSequencer().Execute();
        }
    }
}
