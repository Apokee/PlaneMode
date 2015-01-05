using System;

namespace PlaneMode
{
    internal static class Log
    {
        public static LogLevel Level { get; set; }

        private static string Prefix
        {
            get
            {
                return String.Format("[PlaneMode] [{0}]: ", Level);
            }
        }

        static Log()
        {
            Level = LogLevel.Info;
        }

        public static void Error(string format, params object[] args)
        {
            if ((byte)LogLevel.Error <= (byte)Level)
            {
                UnityEngine.Debug.LogError(Prefix + String.Format(format, args));
            }
        }

        public static void Warning(string format, params object[] args)
        {
            if ((byte)LogLevel.Warning <= (byte)Level)
            {
                UnityEngine.Debug.LogWarning(Prefix + String.Format(format, args));
            }
        }

        public static void Info(string format, params object[] args)
        {
            if ((byte)LogLevel.Info <= (byte)Level)
            {
                UnityEngine.Debug.Log(Prefix + String.Format(format, args));
            }
        }

        public static void Debug(string format, params object[] args)
        {
            if ((byte)LogLevel.Debug <= (byte)Level)
            {
                UnityEngine.Debug.Log(Prefix + String.Format(format, args));
            }
        }

        public static void Trace(string format, params object[] args)
        {
            if ((byte)LogLevel.Trace <= (byte)Level)
            {
                UnityEngine.Debug.Log(Prefix + String.Format(format, args));
            }
        }
    }
}
