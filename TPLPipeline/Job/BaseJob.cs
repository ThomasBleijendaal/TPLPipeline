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

		bool IPipelineJob.IsFullyBegun(string stepName) => JobElements.All(e => e.CurrentStepName == stepName);

		bool IPipelineJob.IsCompleted(string stepName) => JobElements.All(e => e.CompletedStepName == stepName);
		bool IPipelineJob.IsCompleted(string stepName, Predicate<IPipelineJobElement> predicate) => JobElements.Where(e => predicate(e)).All(e => e.CompletedStepName == stepName);

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
			return ((IPipelineJob)this).MergeElements(e => true);
		}
		IEnumerable<IPipelineJobElement> IPipelineJob.MergeElements(Predicate<IPipelineJobElement> predicate)
		{
			// TODO: this code can suffer from races
			if (!Merged)
			{
				Merged = true;
				var elements = JobElements.Where(e => predicate(e)).ToList();
				
				elements.GetRange(1, elements.Count - 1).ForEach(element => element.Disable());

				return elements;
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

		public void AddData(object value)
		{
			JobElements.Add(new JobElement(this, JobElements.Count, value));
		}
		public void AddDataRange(IEnumerable<object> value)
		{
			JobElements.Add(new JobElement(this, JobElements.Count, value));
		}

	}
}
