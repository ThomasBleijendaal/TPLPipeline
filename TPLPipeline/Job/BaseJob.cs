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

		public Task<bool> Completion => CompletionTcs.Task;
		public string Id => Guid.NewGuid().ToString();

		bool IPipelineJob.IsFullyBegun(int stepNr) => !JobElements.Any(e => e.CurrentStep <= stepNr - 1);

		bool IPipelineJob.IsCompleted(int stepNr) => !JobElements.Any(e => e.CompletedStep <= stepNr - 1);
		bool IPipelineJob.IsMerged => JobElements.Any(e => e.Disabled);

		public abstract void OnJobStart();
		public abstract void OnJobComplete();

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
			JobElements.GetRange(1, JobElements.Count - 1).ForEach(element => element.Disable());

			return JobElements;
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
