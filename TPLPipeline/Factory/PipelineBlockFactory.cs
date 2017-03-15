using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline
{
	public static class PipelineBlockFactory
	{
		public static string NewBlockName()
		{
			return Guid.NewGuid().ToString();
		}

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
			var blockName = NewBlockName();

			return new TransformBlock<IPipelineJobElement, IPipelineJobElement>(
				element =>
				{
					var job = (Tjob)element.Job;
					var data = element.GetData<Tin>();

					element.BeginStep(blockName);
					element.SetData(action(job,data));
					element.CompleteStep();
					
					return element;
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static TransformBlock<IPipelineJobElement, IPipelineJobElement> TransformBlock<Tjob, Tin, Tout>(Func<Tjob, Tin, Task<Tout>> action, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			var blockName = NewBlockName();

			return new TransformBlock<IPipelineJobElement, IPipelineJobElement>(
				async element =>
				{
					var job = (Tjob)element.Job;
					var data = element.GetData<Tin>();

					element.BeginStep(blockName);
					element.SetData(await action(job, data));
					element.CompleteStep();

					return element;
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement> MergeTransformBlock<Tjob, Tin, Tout>(Func<Tjob, IEnumerable<Tin>, Tout> action, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			var blockName = NewBlockName();

			return new TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement>(
				elements =>
				{
					var job = elements.GetJob<Tjob>();
					var data = elements.GetData<Tin>();

					foreach (var element in elements)
					{
						element.BeginStep(blockName);
					}

					var newData = action(job, data);
					
					foreach (var element in elements)
					{
						element.CompleteStep();
					}

					var newElement = job.MergeToSingleElement();
					newElement.SetData(newData);
					return newElement;
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement> MergeTransformBlock<Tjob, Tin, Tout>(Func<Tjob, IEnumerable<Tin>, Task<Tout>> action, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			var blockName = NewBlockName();

			return new TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement>(
				async elements =>
				{
					var job = elements.GetJob<Tjob>();
					var data = elements.GetData<Tin>();

					foreach (var element in elements)
					{
						element.BeginStep(blockName);
					}

					var newData = await action(job, data);
					
					foreach (var element in elements)
					{
						element.CompleteStep();
					}

					var newElement = job.MergeToSingleElement();
					newElement.SetData(newData);
					return newElement;
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static ActionBlock<IPipelineJobElement> ActionBlock<Tjob, Tin>(Action<Tjob, Tin> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			var blockName = NewBlockName();

			return new ActionBlock<IPipelineJobElement>(
				element =>
				{
					var job = (Tjob)element.Job;
					var data = element.GetData<Tin>();

					element.BeginStep(blockName);
					action(job, data);
					element.CompleteStep();

					if(isLastStep && element.Job.IsCompleted(element.CurrentStepName))
					{
						element.Job.Complete();
					}
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static ActionBlock<IPipelineJobElement> ActionBlock<Tjob, Tin>(Func<Tjob, Tin, Task> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			var blockName = NewBlockName();

			return new ActionBlock<IPipelineJobElement>(
				async element =>
				{
					var job = (Tjob)element.Job;
					var data = element.GetData<Tin>();

					element.BeginStep(blockName);
					await action(job, data);
					element.CompleteStep();

					if (isLastStep && element.Job.IsCompleted(element.CurrentStepName))
					{
						element.Job.Complete();
					}
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static ActionBlock<IEnumerable<IPipelineJobElement>> MergeActionBlock<Tjob, Tin>(Action<Tjob, IEnumerable<Tin>> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			var blockName = NewBlockName();

			return new ActionBlock<IEnumerable<IPipelineJobElement>>(
				elements =>
				{
					var job = elements.GetJob<Tjob>();
					var data = elements.GetData<Tin>();

					foreach (var element in elements)
					{
						element.BeginStep(blockName);
					}

					action(job, data);
					
					foreach (var element in elements)
					{
						element.CompleteStep();
					}

					var firstItem = elements.First();

					if (isLastStep && firstItem.Job.IsCompleted(firstItem.CurrentStepName))
					{
						firstItem.Job.Complete();
					}
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static ActionBlock<IEnumerable<IPipelineJobElement>> MergeActionBlock<Tjob, Tin>(Func<Tjob, IEnumerable<Tin>, Task> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
			where Tjob : IPipelineJob
		{
			var blockName = NewBlockName();

			return new ActionBlock<IEnumerable<IPipelineJobElement>>(
				async elements =>
				{
					var job = elements.GetJob<Tjob>();
					var data = elements.GetData<Tin>();

					foreach (var element in elements)
					{
						element.BeginStep(blockName);
					}

					await action(job, data);

					foreach (var element in elements)
					{
						element.CompleteStep();
					}

					var firstItem = elements.First();

					if (isLastStep && firstItem.Job.IsCompleted(firstItem.CurrentStepName))
					{
						firstItem.Job.Complete();
					}
				}, options ?? new ExecutionDataflowBlockOptions());
		}

		public static void LinkTo(this ISourceBlock<IPipelineJobElement> element, ITargetBlock<IEnumerable<IPipelineJobElement>> target)
		{
			new PipelineBatchLinker(element, target);
		}

		public static void LinkTo(this ISourceBlock<IPipelineJobElement> element, ITargetBlock<IEnumerable<IPipelineJobElement>> target, Predicate<IPipelineJobElement> predicate)
		{
			new PipelineBatchLinker(element, target, predicate);
		}
	}
}
