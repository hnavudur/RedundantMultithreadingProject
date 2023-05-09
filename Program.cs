using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RedundantMultithreading
{
    class Program
    {
        static int numberOfThreads = 2000;

        static void Main(string[] args)
        {

            // Create a cancellation token source
            var cts = new CancellationTokenSource();

            // Define the start time
            DateTime startTime = DateTime.Now;
            var startCpuTime = Process.GetCurrentProcess().TotalProcessorTime;

            // Create an array of tasks
            var tasks = new Task<int>[numberOfThreads];
            int num = 5;
            for (int i = 0; i < tasks.Length; i++)
            {

                tasks[i] = Task.Run(() => Factorial(num, cts.Token));
            }

            // Wait for all tasks to complete or stop
            while (!Task.WaitAll(tasks, 100))
            {
                // Randomly select a task to cancel
                int indexToCancel = new Random().Next(0, tasks.Length);
                Task<int> taskToCancel = tasks[indexToCancel];

                if (!taskToCancel.IsCompleted)
                {
                    //Console.WriteLine("Cancelling task {0}", indexToCancel);
                    cts.Cancel();

                    // Wait for the cancelled task to complete
                    try
                    {
                        taskToCancel.Wait();
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore the cancellation exception
                    }
                }
            }

            // Define the end time
            DateTime endTime = DateTime.Now;
            var endCpuTime = Process.GetCurrentProcess().TotalProcessorTime;

            // Get the results of the completed tasks
            int[] results = new int[tasks.Length];
            for (int i = 0; i < tasks.Length; i++)
            {
                results[i] = tasks[i].Result;
            }

            // Print the results
            for (int i = 0; i < results.Length; i++)
            {
                Console.WriteLine("TaskId : {1} Factorial of {0} = {2}", num, i, results[i]);
            }

            //Cpu Utilization
            var cpuUsage = getCPUUtilization(startTime, endTime, startCpuTime, endCpuTime);
            Console.WriteLine("CPU utilization: " + cpuUsage + "%");

            //Throughput
            double throughput = getThroughput(startTime, endTime);
            Console.WriteLine("Throughput: " + throughput + " tasks per second");

            // ResponseTime
            double responseTime = (endTime - startTime).TotalMilliseconds;
            Console.WriteLine("Response time: " + responseTime + " milliseconds");

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        public static double getThroughput(DateTime startTime, DateTime endTime)
        {
            double duration = (endTime - startTime).TotalMilliseconds;
            double throughput = numberOfThreads / (duration / 1000);
            return throughput;
        }

        public static double getCPUUtilization(DateTime startTime, DateTime endTime, TimeSpan startCpuTime, TimeSpan endCpuTime)
        {
            var elapsedTime = (endTime - startTime).TotalMilliseconds;
            var elapsedCpuTime = (endCpuTime - startCpuTime).TotalMilliseconds;
            var cpuUsage = elapsedCpuTime / (Environment.ProcessorCount * elapsedTime) * 100;
            return cpuUsage;
        }

        public static int Factorial(int n, CancellationToken token)
        {
            int result = 1;

            for (int i = 1; i <= n; i++)
            {
                // Check if cancellation is requested
                if (token.IsCancellationRequested)
                {
                    return -1; // Return -1 to indicate cancellation
                }

                result *= i;
                Thread.Sleep(10); // Simulate some work
            }

            return result;
        }
    }

}



