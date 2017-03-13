using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeLine
{
	public abstract class BasePipeline<Tjob> : IPipeline<Tjob> where Tjob : IPipelineJob
	{
		public abstract void Post(Tjob job);
		public abstract Task PostAsync(Tjob job);

		protected TransformBlock<IPipelineJobElement, IPipelineJobElement> TransformBlock<Tin, Tout>(Func<Tjob, Tin, Tout> action, ExecutionDataflowBlockOptions options = null)
		{
			return PipelineBlockFactory.TransformBlock(action, options);
		}

		protected TransformBlock<IPipelineJobElement, IPipelineJobElement> TransformBlock<Tin, Tout>(Func<Tjob, Tin, Task<Tout>> action, ExecutionDataflowBlockOptions options = null)
		{
			return PipelineBlockFactory.TransformBlock(action, options);
		}

		protected TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement> MergeTransformBlock<Tin, Tout>(Func<Tjob, IEnumerable<Tin>, Tout> action, ExecutionDataflowBlockOptions options = null)
		{
			return PipelineBlockFactory.MergeTransformBlock(action, options);
		}

		protected TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement> MergeTransformBlock<Tin, Tout>(Func<Tjob, IEnumerable<Tin>, Task<Tout>> action, ExecutionDataflowBlockOptions options = null)
		{
			return PipelineBlockFactory.MergeTransformBlock(action, options);
		}

		protected ActionBlock<IPipelineJobElement> ActionBlock<Tin>(Action<Tjob, Tin> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
		{
			return PipelineBlockFactory.ActionBlock(action, isLastStep, options);
		}

		protected ActionBlock<IPipelineJobElement> ActionBlock<Tin>(Func<Tjob, Tin, Task> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
		{
			return PipelineBlockFactory.ActionBlock(action, isLastStep, options);
		}

		protected ActionBlock<IEnumerable<IPipelineJobElement>> MergeActionBlock<Tin>(Action<Tjob, IEnumerable<Tin>> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
		{
			return PipelineBlockFactory.MergeActionBlock(action, isLastStep, options);
		}

		protected ActionBlock<IEnumerable<IPipelineJobElement>> MergeActionBlock<Tin>(Func<Tjob, IEnumerable<Tin>, Task> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
		{
			return PipelineBlockFactory.MergeActionBlock(action, isLastStep, options);
		}
	}
}
