using System;
using System.Collections.Generic;
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
			var i = 0;
			var c = 0;
			object l = new object();

			{
				var pipeline = new Implementation.Simple.Pipeline();

				i = 0;

				while (++i <= 50)
				{
					var job = new Implementation.Simple.Job();

					job.Completed += (sender, e) => { lock (l) { Console.WriteLine(++c); } };

					//await pipeline.PostAsync(job);
					pipeline.Post(job);
				}

				await Task.Run(async () => { while (c < 50) { await Task.Delay(100); } });

				Console.WriteLine("*** Simple Done ***");
			}

			Console.WriteLine("*** Done ***");
			Console.ReadLine();
		}
	}
}
