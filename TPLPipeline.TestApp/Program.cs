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

			for (int l = 0; l < 1; l++)
			{
				var job = new Job();

				for (int i = 0; i < 5; i++)
				{
					job.AddData("http://thomas-ict.nl", new DataProperty[] { new DataProperty { Name = "Type", Value = "Website" } } );
				}
				
				job.AddData("http://thomas-ict.nl/logo-groot.png", new DataProperty[] { new DataProperty { Name = "Type", Value = "Thumbnail" } });

				jobList.Add(job);
			}

			var tasks = new List<Task>();

			foreach (var job in jobList)
			{
				tasks.Add(pipeline.PostAsync(job));
			}

			Task.WaitAll(tasks.ToArray());

			foreach (var job in jobList)
			{
				foreach (var el in ((IPipelineJob)job).Elements())
				{
					Console.WriteLine($"TRACE: {el.Job.Id} {el.CompletedStepName} {(el.Disabled ? "MERGED" : "COMPLETED")}");
				}
			}

			Console.WriteLine("*** Done ***");
			Console.ReadLine();
		}
	}
}
