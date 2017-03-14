using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TPLPipeline
{
	public abstract class BaseJob : IPipelineJob
	{
		private List<IPipelineJobElement> JobElements = new List<IPipelineJobElement>();
		private TaskCompletionSource<bool> CompletionTcs = new TaskCompletionSource<bool>();

		private bool Merged { get; set; }

		public Task<bool> Completion => CompletionTcs.Task;
		public string Id { get; private set; }

		bool IPipelineJob.IsFullyBegun(int stepNr) => !JobElements.Any(e => e.CurrentStep <= stepNr - 1);

		bool IPipelineJob.IsCompleted(int stepNr) => !JobElements.Any(e => e.CompletedStep <= stepNr - 1);
		
		public abstract void OnJobStart();
		public abstract void OnJobComplete();

		public BaseJob()
		{
			Id = Guid.NewGuid().ToString();
		}

		void IPipelineJob.Complete()
		{
			CompletionTcs.TrySetResult(true);
			OnJobComplete();
		}

		IEnumerable<IPipelineJobElement> IPipelineJob.Elements()
		{
			return JobElements;
		}

		IEnumerable<IPipelineJobElement> IPipelineJob.MergeElements()
		{
			// TODO: this code can suffer from races
			if (!Merged)
			{
				Merged = true;
				JobElements.GetRange(1, JobElements.Count - 1).ForEach(element => element.Disable());

				return JobElements;
			}
			else
			{
				return null;
			}
		}

		IPipelineJobElement IPipelineJob.MergeToSingleElement()
		{
			JobElements.RemoveRange(1, JobElements.Count - 1);
			return JobElements.First();
		}

		public IEnumerable<object> Data
		{
			set
			{
				int i = 0;
				foreach (var initData in value)
				{
					JobElements.Add(new JobElement(this, i++, initData));
				}
			}
		}

	}
}
