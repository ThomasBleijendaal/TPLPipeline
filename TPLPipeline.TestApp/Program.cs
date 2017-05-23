using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TPLPipeline.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(MainAsync).GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var sw = new Stopwatch();
            var result = new List<long>();

            var i = 0;
            var n = 0;

            var pipeline = new Implementation.Simple.Pipeline();
            
            do
            {
                i = 0;

                sw.Start();

                while (++i <= 5)
                {
                    var job = new Implementation.Simple.Job();

                    //job.Completed += (sender, e) => { Console.WriteLine(++c); };

                    await pipeline.PostAsync(job);
                }

                sw.Stop();
                result.Add(sw.ElapsedMilliseconds);
                sw.Reset();

                Console.WriteLine(n);
            }
            while (++n < 10);
            
            Console.WriteLine("*** Simple Done ***");
            Console.WriteLine(result.Average());



            Console.WriteLine("*** Done ***");
            Console.ReadLine();
        }
    }
}
