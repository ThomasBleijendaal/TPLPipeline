using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline
{
	public class PipelineBatchLinker<T>
	{
		private ActionBlock<IJobElement> Input { get; set; }

		public PipelineBatchLinker(ISourceBlock<IPipelineJobElement<T>> from, ITargetBlock<IEnumerable<IPipelineJobElement<T>>> to) : this(from, to, null) { }

		public PipelineBatchLinker(ISourceBlock<IPipelineJobElement<T>> from, ITargetBlock<IEnumerable<IPipelineJobElement<T>>> to, Predicate<IPipelineJobElement<T>> predicate)
		{
			if (predicate == null)
			{
				predicate = e => true;
			}

			Input = new ActionBlock<IJobElement>(
				element =>
				{
					var job = element.Job;

					if (job.IsCompleted<T>(element.CurrentStepName, predicate))
					{
						var elements = job.MergeElements<T>(predicate);

						if (elements != null)
						{
							to.Post(elements);
						}
					}
				});

			from.LinkTo(Input, predicate);
		}
	}
}
