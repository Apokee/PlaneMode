using System;

namespace PlaneMode.Extensions
{
    internal static class SingleExtensions
    {
        public static bool IsZero(this float val)
        {
            return Math.Abs(val) < Single.Epsilon;
        }
    }
}
