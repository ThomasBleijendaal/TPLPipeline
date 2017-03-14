using System;

namespace TPLPipeline.TestApp
{
	public class Job : BaseJob
	{
		public Job() : base()
		{

		}

		public override void OnJobStart()
		{
			Console.WriteLine("Job started");
		}

		public override void OnJobComplete()
		{
			Console.WriteLine("Job completed");
		}

		public string FileName => $"data\\{Id}.txt";
	}
}
