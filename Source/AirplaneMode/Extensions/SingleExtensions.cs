using System;

namespace AirplaneMode.Extensions
{
    internal static class SingleExtensions
    {
        public static bool IsZero(this float val)
        {
            return Math.Abs(val) < Single.Epsilon;
        }
    }
}
