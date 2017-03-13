using System;

namespace TPLPipeLine
{
	public class Job : BaseJob
	{
		public Job()
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
