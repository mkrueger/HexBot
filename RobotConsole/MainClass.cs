using System;
using RobotControlFramework.SSC32;

namespace RobotControlFramework.MaintainanceConsole
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            try
            {
                using (var ctl = new ConsoleController())
                {
                    ctl.OpenSSC32Connection("/dev/ttyUSB0", SSC32_Constants.BaudGREENRED_115200);
                    ctl.Run();
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
}
