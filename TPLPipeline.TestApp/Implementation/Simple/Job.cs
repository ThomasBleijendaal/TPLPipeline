using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPLPipeline.TestApp.Implementation.Simple
{
	public delegate void Completed(object sender, EventArgs e);
	
	class Job : BaseJob
	{
		public event Completed Completed;

		public override void OnJobComplete()
		{
			//Console.WriteLine($"{this.Id} completed");

			Completed.Invoke(this, null);
		}

		public override void OnJobStart()
		{
			//Console.WriteLine($"{this.Id} started");
		}
	}
}
