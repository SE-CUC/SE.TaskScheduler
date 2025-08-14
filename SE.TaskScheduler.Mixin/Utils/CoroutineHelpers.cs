using System;
using System.Collections;

namespace IngameScript
{
    public static class CoroutineHelpers
    {
        public static IEnumerator Wait(double seconds)
        {
            var waitUntil = DateTime.Now.AddSeconds(seconds);
            while (DateTime.Now < waitUntil)
            {
                yield return true;
            }
        }
    }
}
