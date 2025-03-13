using System;
using System.IO;

namespace pdpditxx
{
    internal class CleanupProcess
    {
        public static void Cleanup(string zipWorkDir)
        {
            try
            {
                Directory.Delete(zipWorkDir, true);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failure trying to cleanup. Message : {e.Message}  Stacktrace : {e.StackTrace}");
                Environment.Exit(1);
            }
        }
    }
}
