using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TPLPipeline.TestApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var r = new Random();
			var jobList = new List<Job>();

			var pipeline = new Pipeline();

			for (int l = 0; l < 10; l++)
			{
				var data = new List<object>();

				for (int i = 0; i < 10; i++)
				{
					data.Add("http://thomas-ict.nl");
				}

				jobList.Add(new Job { Data = data });
			}

			var tasks = new List<Task>();

			foreach (var job in jobList)
			{
				tasks.Add(pipeline.PostAsync(job));
			}

			Task.WaitAll(tasks.ToArray());

			Console.WriteLine("*** Done ***");

			Console.ReadLine();
		}
	}
}
