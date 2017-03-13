using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeLine
{
	public static class PipelineBlockFactory
	{
		public static TransformManyBlock<IPipelineJob, IPipelineJobElement> StartBlock()
		{
			return new TransformManyBlock<IPipelineJob, IPipelineJobElement>(
				job =>
				{
					job.OnJobStart();
					
					return job.Elements();
				});
		}

		public static TransformBlock<IPipelineJobElement, IPipelineJobElement> TransformBlock<Tjob, Tin, Tout>(Func<Tjob, Tin, Tout> action, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			return new TransformBlock<IPipelineJobElement, IPipelineJobElement>(
				element =>
				{
					var job = (Tjob)element.Job;
					var data = element.GetData<Tin>();

					element.BeginStep();
					//Logger.Log(element, $"Begin step {element.CurrentStep}");

					element.SetData(action(job,data));

					element.CompleteStep();
					//Logger.Log(element, $"Completed step {element.CurrentStep}");

					return element;
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static TransformBlock<IPipelineJobElement, IPipelineJobElement> TransformBlock<Tjob, Tin, Tout>(Func<Tjob, Tin, Task<Tout>> action, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			return new TransformBlock<IPipelineJobElement, IPipelineJobElement>(
				async element =>
				{
					var job = (Tjob)element.Job;
					var data = element.GetData<Tin>();

					element.BeginStep();
					//Logger.Log(element, $"Begin step {element.CurrentStep}");

					element.SetData(await action(job,data));

					element.CompleteStep();
					//Logger.Log(element, $"Completed step {element.CurrentStep}");

					return element;
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement> MergeTransformBlock<Tjob, Tin, Tout>(Func<Tjob, IEnumerable<Tin>, Tout> action, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			return new TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement>(
				elements =>
				{
					var job = elements.GetJob<Tjob>();
					var data = elements.GetData<Tin>();

					foreach (var element in elements)
					{
						element.BeginStep();
						//Logger.Log(element, $"Begin step {element.CurrentStep}");
					}

					var newData = action(job, data);
					
					foreach (var element in elements)
					{
						element.CompleteStep();

						//Logger.Log(element, $"Completed step {element.CurrentStep}");
					}

					var newElement = job.MergeToSingleElement();
					newElement.SetData(newData);
					return newElement;
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement> MergeTransformBlock<Tjob, Tin, Tout>(Func<Tjob, IEnumerable<Tin>, Task<Tout>> action, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			return new TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement>(
				async elements =>
				{
					var job = elements.GetJob<Tjob>();
					var data = elements.GetData<Tin>();

					foreach (var element in elements)
					{
						element.BeginStep();
						//Logger.Log(element, $"Begin step {element.CurrentStep}");
					}

					var newData = await action(job, data);
					
					foreach (var element in elements)
					{
						element.CompleteStep();

						//Logger.Log(element, $"Completed step {element.CurrentStep}");
					}

					var newElement = job.MergeToSingleElement();
					newElement.SetData(newData);
					return newElement;
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static ActionBlock<IPipelineJobElement> ActionBlock<Tjob, Tin>(Action<Tjob, Tin> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			return new ActionBlock<IPipelineJobElement>(
				element =>
				{
					var job = (Tjob)element.Job;
					var data = element.GetData<Tin>();

					element.BeginStep();
					//Logger.Log(element, $"Begin step {element.CurrentStep}");
					
					action(job, data);

					element.CompleteStep();
					//Logger.Log(element, $"Completed step {element.CurrentStep}");

					if(isLastStep && element.Job.IsCompleted(element.CurrentStep))
					{
						element.Job.Complete();
					}
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static ActionBlock<IPipelineJobElement> ActionBlock<Tjob, Tin>(Func<Tjob, Tin, Task> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			return new ActionBlock<IPipelineJobElement>(
				async element =>
				{
					var job = (Tjob)element.Job;
					var data = element.GetData<Tin>();

					element.BeginStep();
					//Logger.Log(element, $"Begin step {element.CurrentStep}");
					
					await action(job, data);

					element.CompleteStep();
					//Logger.Log(element, $"Completed step {element.CurrentStep}");

					if (isLastStep && element.Job.IsCompleted(element.CurrentStep))
					{
						element.Job.Complete();
					}
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static ActionBlock<IEnumerable<IPipelineJobElement>> MergeActionBlock<Tjob, Tin>(Action<Tjob, IEnumerable<Tin>> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			return new ActionBlock<IEnumerable<IPipelineJobElement>>(
				elements =>
				{
					var job = elements.GetJob<Tjob>();
					var data = elements.GetData<Tin>();

					foreach (var element in elements)
					{
						element.BeginStep();
						//Logger.Log(element, $"Begin step {element.CurrentStep}");
					}

					action(job, data);
					
					foreach (var element in elements)
					{
						element.CompleteStep();
						//Logger.Log(element, $"Completed step {element.CurrentStep}");
					}

					var firstItem = elements.First();

					if (isLastStep && firstItem.Job.IsCompleted(firstItem.CurrentStep))
					{
						firstItem.Job.Complete();
					}
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static ActionBlock<IEnumerable<IPipelineJobElement>> MergeActionBlock<Tjob, Tin>(Func<Tjob, IEnumerable<Tin>, Task> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			return new ActionBlock<IEnumerable<IPipelineJobElement>>(
				async elements =>
				{
					var job = elements.GetJob<Tjob>();
					var data = elements.GetData<Tin>();

					foreach (var element in elements)
					{
						element.BeginStep();
						//Logger.Log(element, $"Begin step {element.CurrentStep}");
					}

					await action(job, data);

					foreach (var element in elements)
					{
						element.CompleteStep();
						//Logger.Log(element, $"Completed step {element.CurrentStep}");
					}

					var firstItem = elements.First();

					if (isLastStep && firstItem.Job.IsCompleted(firstItem.CurrentStep))
					{
						firstItem.Job.Complete();
					}
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static void LinkTo(this ISourceBlock<IPipelineJobElement> element, ITargetBlock<IEnumerable<IPipelineJobElement>> target)
		{
			new PipelineBatchLinker(element, target);
		}
	}
}
