using System;

namespace SignalSimulatorYT
{
    public static class ModLogger
    {
        public static void Log(object input)
        {
            Main.Log.LogInfo(DateTime.Now.ToString("HH:mm:ss:fff | ") + (input?.ToString() ?? "NULL"));
        }
    }
}
