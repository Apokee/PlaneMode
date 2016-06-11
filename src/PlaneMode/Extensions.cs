using System;

namespace PlaneMode
{
    internal static class Extensions
    {
        public static bool IsZero(this float val)
        {
            return Math.Abs(val) < Single.Epsilon;
        }
    }
}
