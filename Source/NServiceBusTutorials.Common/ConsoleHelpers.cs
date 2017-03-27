using System;

namespace NServiceBusTutorials.Common
{
    public static class ConsoleHelpers
    {
        public static bool TryReadKeyAsync(int timeout, out ConsoleKeyInfo consoleKeyInfo)
        {
            bool result;

            // Get a delegate for Console.ReadKey
            ReadKeyDelegate d = Console.ReadKey;

            // Start waiting to read a key.
            var readKeyresult = d.BeginInvoke(null, null);
            readKeyresult.AsyncWaitHandle.WaitOne(timeout);

            // At this point, we either read a key, or we timed out.
            if (readKeyresult.IsCompleted)
            {
                // SUCCESS:  Get the ConsoleKeyInfo and set the out variable.
                consoleKeyInfo = d.EndInvoke(readKeyresult);
                result = true;
            }
            else
            {
                // FAILURE:  Set the out variable to a default instance.
                consoleKeyInfo = default(ConsoleKeyInfo);
                result = false;
            }

            return result;
        }

        private delegate ConsoleKeyInfo ReadKeyDelegate();
    }
}