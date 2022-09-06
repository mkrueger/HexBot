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
    class FrSkyCotroller : AbstractController
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
                int newSpeed = (int)(200 * Math.Abs(controllerState.Y));

                if (controllerState.X < 0.1)
                {
                    int speed = (int)(100 * Math.Abs(controllerState.X));
                    currentSequence.TravelPercentage_Left = speed;
                    currentSequence.TravelPercentage_Right = -speed;
                } else 
                if (controllerState.X > 0.1)
                {
                    int speed = (int)(100 * Math.Abs(controllerState.X));
                    currentSequence.TravelPercentage_Left = -speed;
                    currentSequence.TravelPercentage_Right = speed;
                }
                else
                {
                    currentSequence.TravelPercentage_Left = currentSequence.TravelPercentage_Right = 100;
                }

                if (controllerState.Y < 0)
                {
                    currentSequence.TravelPercentage_Left = -currentSequence.TravelPercentage_Left;
                    currentSequence.TravelPercentage_Right = -currentSequence.TravelPercentage_Right;
                }
                if (newSpeed != Model.Speed || !currentSequence.Equals(oldSequence) || Model.State != MovementState.InWalkSequence)
                {
                    Model.Speed = newSpeed;

                    if (Model.Speed <= 1)
                    {
                        Command.StopHexSequencer().Execute();
                    }
                    else
                    {
                        if (oldStartedSequence == null)
                        {
                            Command.StartSequence(currentSequence).Execute();
                        }
                        else
                        {
                            Command.UpdateSequence(oldStartedSequence, oldSpeed, currentSequence).Execute();
                        }
                        oldStartedSequence = currentSequence;
                        oldSpeed = Model.Speed;
                    }
                    oldSequence = currentSequence;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error while running sequence", e);
            }
        }

        void UpdateControlState()
        {
            if (controllerState.LeftSwitch)
            {
                int value = (int)(500 + 1000 * (1 + controllerState.Z));
                var cmd = Command;
                bool exec = false;
                if (oldState == null || Math.Abs(oldState.Z - controllerState.Z) > 0.1 || !oldState.LeftSwitch)
                {
                    cmd.SingleServo(22, value)
                       .SingleServo(23, value)
                       .SingleServo(24, value);
                    exec = true;
                }

                if (oldState == null || Math.Abs(oldState.RZ - controllerState.RZ) > 0.1 || !oldState.LeftSwitch)
                {
                    value = (int)(500 + 1000 * (1 + controllerState.RZ));
                    cmd.SingleServo(6, value)
                        .SingleServo(7, value)
                        .SingleServo(8, value);
                    exec = true;
                }
                if (exec)
                    cmd.Execute();
            }
            else
            {
                if (oldState == null || oldState.LeftSwitch)
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
        private HexpodSequence oldStartedSequence;
        private int oldSpeed;
        private ControllerState oldState;

        public class ControllerState : IEquatable<ControllerState>
        {
            public double X, Y, Z;
            public double RX, RY, RZ;

            public bool LeftSwitch, RightSwitch;


            public override string ToString()
            {
                const string fmt = ",3:N1";
                return string.Format("[ControllerState: X={0" + fmt + "}, Y={1" + fmt + "}, Z={2" + fmt + "}, RX={3" + fmt + "}, RY={4" + fmt + "}, RZ={5" + fmt + "}, LeftSwitch={6}, RightSwitch={7}]", X, Y, Z, RX, RY, RZ, LeftSwitch, RightSwitch);
            }

            public ControllerState Clone() => (ControllerState)this.MemberwiseClone();


            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public bool Equals(ControllerState other)
            {
                const double EPSILON = 0.01;
                return Math.Abs(X - other.X) < EPSILON && Math.Abs(Y - other.Y) < EPSILON && Math.Abs(Z - other.Z) < EPSILON &&
                    Math.Abs(RX - other.RX) < EPSILON && Math.Abs(RY - other.RY) < EPSILON && Math.Abs(RZ - other.RZ) < EPSILON &&
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

                        while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
                        {
                            if (inputParser.TryParseReport(inputReportBuffer, 0, report))
                                WriteDeviceItemInputParserResult(inputParser);
                        }
                        if (oldState == null || !controllerState.Equals(oldState))
                        {
                            Console.WriteLine(controllerState);
                            UpdateControlState();
                            oldState = controllerState.Clone();
                        }
                    };
                }
                inputReceiver.Start(hidStream);
                Console.WriteLine("FrSky device found and initialized.");

            }
            else
            {
                Console.WriteLine($"Can't open device {dev.GetFriendlyName()}. Check permissions for {dev.GetFileSystemName()}.");
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
                value = value < 0 ? value / 127 : (value -54) / 200;
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
