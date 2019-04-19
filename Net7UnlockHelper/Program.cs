namespace Net7UnlockHelper
{
    using System;
    using System.Diagnostics;

    public class Program
    {
        private static readonly bool IsDebug = Debugger.IsAttached;

        public static void Main(string[] args)
        {
            var failed = false;

            try
            {
                LogInfo("Start");
                if (args.Length != 1)
                {
                    LogInfo("Wrong number of arguments");
                    Environment.Exit(1);
                }

                LogInfo("Converting processid");
                var processId = Convert.ToInt32(args[0]);
                LogInfo("Found ProcessID : " + processId);
                var clientProcess = Process.GetProcessById(processId);
                LogInfo("Attached to process");
                var handles = Win32Processes.GetHandles(clientProcess, "Mutant");
                LogInfo("Got Handles : " + handles.Count);

                var currentHandle = 0;

                foreach (var handle in handles)
                {
                    currentHandle++;
                    LogInfo("Handling : " + currentHandle);
                    // ReSharper disable once NotAccessedVariable
                    IntPtr mutexHandle;
                    LogInfo("Before duplicate call");
                    if (Win32API.DuplicateHandle(clientProcess.Handle, handle.Handle, Win32API.GetCurrentProcess(), out mutexHandle, 0, false, Win32API.DUPLICATE_CLOSE_SOURCE))
                    {
                        continue;
                    }

                    LogInfo("Duplication failed.");
                    failed = true;
                }
            }
            catch (Exception)
            {
                Environment.Exit(1);
            }

            if (failed)
            {
                Environment.Exit(2);
            }

            Environment.Exit(3);
        }

        private static void LogInfo(string info)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (IsDebug)
#pragma warning disable 162
            {
                Console.WriteLine(info);
            }
#pragma warning restore 162
        }
    }
}
