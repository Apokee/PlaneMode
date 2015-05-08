using System;

namespace PlaneMode
{
    internal static class Log
    {
        public static LogLevel Level { get; set; }

        static Log()
        {
            Level = LogLevel.Info;
        }

        public static void Error(string format, params object[] args)
        {
            if ((byte)LogLevel.Error <= (byte)Level)
            {
                UnityEngine.Debug.LogError(Prefix(LogLevel.Error) + String.Format(format, args));
            }
        }

        public static void Warning(string format, params object[] args)
        {
            if ((byte)LogLevel.Warning <= (byte)Level)
            {
                UnityEngine.Debug.LogWarning(Prefix(LogLevel.Warning) + String.Format(format, args));
            }
        }

        public static void Info(string format, params object[] args)
        {
            if ((byte)LogLevel.Info <= (byte)Level)
            {
                UnityEngine.Debug.Log(Prefix(LogLevel.Info) + String.Format(format, args));
            }
        }

        public static void Debug(string format, params object[] args)
        {
            if ((byte)LogLevel.Debug <= (byte)Level)
            {
                UnityEngine.Debug.Log(Prefix(LogLevel.Debug) + String.Format(format, args));
            }
        }

        public static void Trace(string format, params object[] args)
        {
            if ((byte)LogLevel.Trace <= (byte)Level)
            {
                UnityEngine.Debug.Log(Prefix(LogLevel.Trace) + String.Format(format, args));
            }
        }

        private static string Prefix(LogLevel level)
        {
            return String.Format("[PlaneMode] [{0}]: ", level);
        }
    }
}
