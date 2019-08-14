using System;
using RobotControlFramework.SSC32;

namespace HexBot.FrSky
{
    class MainClass
    {
        public static void Main(string[] args)
        {

            try
            {
                using (var ctl = new FrSkyController())
                {
                    ctl.OpenSSC32Connection("/dev/ttyUSB0", SSC32_Constants.BaudGREENRED_115200);
                    ctl.Init();
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
