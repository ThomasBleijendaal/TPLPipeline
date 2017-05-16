using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TPLPipeline
{
	public abstract class BaseJob : IPipelineJob
	{
		private List<IJobElement> Elements = new List<IJobElement>();
		private TaskCompletionSource<bool> CompletionTcs = new TaskCompletionSource<bool>();

		// TODO: remove locks
		private object MergeLock { get; set; } = new object();
		private object CompletionLock { get; set; } = new object();

		private bool Completed { get; set; } = false;

		private bool Merged { get; set; }

		public Task<bool> Completion => CompletionTcs.Task;
		public string Id { get; private set; }

		bool IPipelineJob.IsCompleted<T>(string stepName)
		{
			Console.WriteLine("IsCompleted");
			return Elements
				?.Where(e => !e.Disabled && (e.CurrentStepName?.EndsWith(stepName) ?? false))
				.All(e => e.CompletedStepName?.EndsWith(stepName) ?? false) ?? false;
		}
		bool IPipelineJob.IsCompleted<T>(string stepName, Predicate<IPipelineJobElement<T>> predicate)
		{
			Console.WriteLine("IsCompleted");
			return ((IPipelineJob)this).Elements<T>()
				?.Where(e => predicate(e) && !e.Disabled && (e.CurrentStepName?.EndsWith(stepName) ?? false))
				.All(e => e.CompletedStepName?.EndsWith(stepName) ?? false) ?? false;
		}

		public abstract void OnJobStart();
		public abstract void OnJobComplete();

		public BaseJob()
		{
			Id = Guid.NewGuid().ToString();
		}

		void IPipelineJob.Complete(string stepName)
		{
			Console.WriteLine("Complete");
			//lock (CompletionLock)
			//{

				if ((Elements.TrueForAll(e => e.Disabled || (e.CompletedStepName?.EndsWith(stepName) ?? false))) && !Completed)
				{
					Completed = true;

					CompletionTcs.TrySetResult(true);
					OnJobComplete();
				}
			//}
		}

		IEnumerable<IJobElement> IPipelineJob.Elements()
		{
			return Elements;
		}

		IEnumerable<IPipelineJobElement<T>> IPipelineJob.Elements<T>()
		{
			if (Elements.TrueForAll(x => x as IPipelineJobElement<T> != null))
			{
				return Elements.Select(x => x as IPipelineJobElement<T>);
			}
			else
			{
				return null;
			}
		}

		void IPipelineJob.UpdateElement(IJobElement newElement)
		{
			Elements[newElement.Nr] = newElement;
		}

		IEnumerable<IPipelineJobElement<T>> IPipelineJob.MergeElements<T>()
		{
			return (this as IPipelineJob).MergeElements<T>(e => true);
		}
		IEnumerable<IPipelineJobElement<T>> IPipelineJob.MergeElements<T>(Predicate<IPipelineJobElement<T>> predicate)
		{
			lock (MergeLock)
			{
				if (!Merged)
				{
					Merged = true;
					var elements = Elements.Select(x => (IPipelineJobElement<T>)x).Where(e => predicate(e)).ToList();

					elements.GetRange(1, elements.Count - 1).ForEach(element => element.Disable());

					return elements;
				}
				else
				{
					return null;
				}
			}
		}

		IPipelineJobElement<T> IPipelineJob.MergeToSingleElement<T>(IEnumerable<IPipelineJobElement<T>> elements)
		{
			var elementList = elements.ToList();

			foreach (var element in elementList.GetRange(1, elementList.Count - 1))
			{
				element.Disable();
			}
			return elementList.First();
		}

		IPipelineJobElement<Tuple<T1, T2>> IPipelineJob.MergeToSingleElement<T1, T2>(Tuple<IPipelineJobElement<T1>, IPipelineJobElement<T2>> elements)
		{
			var newData = Tuple.Create(elements.Item1.GetData(), elements.Item2.GetData());

			elements.Item2.Disable();
			var mergedElement = elements.Item1.SetData(newData);

			return mergedElement;
		}

		public void AddData<T>(T value, DataProperty[] properties = null)
		{
			Elements.Add(new JobElement<T>(this, Elements.Count, value, properties));
		}

	}
}
