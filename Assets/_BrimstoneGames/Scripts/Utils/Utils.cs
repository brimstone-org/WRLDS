using UnityEngine;

namespace _DPS
{
    public static class Utils
    {
        /// <summary>
        /// This will return a random int between start and end.
        /// p parameter will be used as the odds mdifier
        /// a value of 0.5 for example will favor hingher numbers 
        /// while a value of 2 will return a quadratic behaviour favouring lower numbers
        /// p=2 means ^2 less odds that the next number in range will show up
        /// </summary>
        /// <param name="start">INCLUSIVE</param>
        /// <param name="end">INCLUSIVE!!!</param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static int SkewedRandomRange(int start, int end, float p)
        {
            var rnd = Mathf.Pow(Random.value, p);
            var index = Mathf.FloorToInt(start + rnd * (end - start + 1));
            var result = Mathf.Clamp(index, start, end);
            return result;
        }

        public static bool Contains<T>(this T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        public static T Add<T>(this T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
            return flags;
        }

        public static T Remove<T>(this T flags, T flag) where T : struct
        {
            int flagsValue = (int)(object)flags;
            int flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
            return flags;
        }
    }
}
