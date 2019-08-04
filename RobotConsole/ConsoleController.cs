using System;
using RobotControlFramework.SSC32;

namespace RobotControlFramework.MaintainanceConsole
{
    class ConsoleController : AbstractController
    {
        internal void Run()
        {
            Console.WriteLine("Awaiting commands.");
            while (true)
            {
                string cmd = Console.ReadLine();
                var split = cmd.Split(' ');
                if (split == null || split.Length == 0)
                    continue;
                switch (split[0].ToUpper())
                {
                    case "Q":
                    case "QUIT":
                        return;
                    case "S":
                        try
                        {
                            int channel = int.Parse(split[1]);
                            int pulse = int.Parse(split[2]);
                            Command.SingleServo(channel, pulse).Execute();
                        } catch (Exception e)
                        {
                            Log.Error(e.ToString());
                            Console.WriteLine("Usage: S [Channel] [Pulse]");
                        }
                        break;
                }
            }
        }
    }
}
